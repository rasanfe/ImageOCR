using System.Drawing;
using System.Reflection;
using Tesseract; // Wrapper .NET sobre el motor OCR Tesseract (https://github.com/charlesw/tesseract)

namespace ImageOCR
{
    /// <summary>
    /// Clase de OCR (reconocimiento óptico de caracteres) pensada para consumirse desde PowerBuilder.
    /// Recibe la ruta de una imagen y devuelve el texto que Tesseract es capaz de "leer" en ella.
    /// </summary>
    /// <remarks>
    /// Cómo se usa desde PowerBuilder (a través del puente .NET DLL Importer / .NET Assembly):
    /// <code>
    /// oImageOCR = create ImageOCR
    /// ls_texto = oImageOCR.ConvertImageToString("C:\factura.png")
    /// </code>
    /// Importante para el despliegue: Tesseract necesita los datos del idioma entrenado en una carpeta
    /// <c>tessdata</c> situada JUNTO a la DLL (aquí usamos el idioma "spa" = español → <c>tessdata\spa.traineddata</c>).
    /// Si esa carpeta no está, Tesseract lanza una excepción; por eso conviene usar siempre
    /// <see cref="GetLastError"/> tras una llamada para saber qué falló.
    /// </remarks>
    public class ImageOCR
    {
        // Guardamos aquí el último mensaje de error. PowerBuilder no maneja bien las excepciones .NET,
        // así que seguimos el patrón "GetLastError": ante un fallo lanzamos la excepción Y dejamos el
        // texto aquí para que PB pueda recuperarlo con GetLastError().
        private string ErrorText = "";

        /// <summary>
        /// Lee el texto de una imagen usando la configuración por defecto: carpeta <c>tessdata</c>
        /// junto a la DLL e idioma español ("spa").
        /// </summary>
        /// <param name="imagePath">Ruta completa de la imagen a procesar (PNG, JPG, TIFF, BMP…).</param>
        /// <returns>El texto reconocido en la imagen.</returns>
        public string ConvertImageToString(string imagePath)
        {
            // Localizamos la carpeta donde está esta misma DLL para colgar de ahí "tessdata".
            // Así el cliente no tiene que configurar rutas: todo viaja junto al ensamblado.
            string rutaEnsamblado = Assembly.GetExecutingAssembly().Location;
            string directorio = Path.GetDirectoryName(rutaEnsamblado)!; // '!' = sabemos que no es null (la DLL existe)
            string dataPath = Path.Combine(directorio, "tessdata");
            string language = "spa";
            string text = ConvertImageToString(imagePath, dataPath, language);

            return text;
        }

        /// <summary>
        /// Igual que <see cref="ConvertImageToString(string)"/> pero, en lugar de devolver el texto,
        /// lo vuelca a un fichero .txt. Cómodo cuando desde PB solo queremos el resultado en disco.
        /// </summary>
        /// <param name="imagePath">Ruta de la imagen de entrada.</param>
        /// <param name="txtPath">Ruta del .txt de salida (se sobrescribe si existe).</param>
        public void ConvertImageToTxt(string imagePath, string txtPath)
        {
            string rutaEnsamblado = Assembly.GetExecutingAssembly().Location;
            string directorio = Path.GetDirectoryName(rutaEnsamblado)!;
            string dataPath = Path.Combine(directorio, "tessdata");
            string language = "spa";

            ConvertImageToTxt(imagePath, txtPath, dataPath, language);
        }

        /// <summary>
        /// Sobrecarga "completa": permite indicar a mano la carpeta de datos de Tesseract y el idioma.
        /// Útil si tenéis los <c>.traineddata</c> en otra ruta o queréis OCR en otro idioma (p. ej. "eng").
        /// </summary>
        /// <param name="imagePath">Ruta de la imagen a procesar.</param>
        /// <param name="dataPath">Carpeta que contiene los ficheros <c>&lt;idioma&gt;.traineddata</c>.</param>
        /// <param name="language">Código de idioma de Tesseract (p. ej. "spa", "eng").</param>
        /// <returns>El texto reconocido.</returns>
        public string ConvertImageToString(string imagePath, string dataPath, string language)
        {
            try
            {
                var format = Path.GetExtension(imagePath);

                // Tesseract (vía la librería Leptonica que lleva por debajo) no carga bien algunos BMP.
                // Truco sencillo: si nos pasan un BMP lo reconvertimos a PNG y procesamos ese.
                if (format == ".bmp") { imagePath = SaveBmpAsPNG(imagePath); }

                // Tres piezas de Tesseract: el motor (cargado con el idioma), la imagen (Pix) y la
                // página procesada. Las tres son IDisposable porque envuelven memoria NATIVA (C++),
                // que el recolector de basura de .NET no libera solo: hay que llamar a Dispose().
                var engine = new TesseractEngine(@dataPath, language);
                var image = Pix.LoadFromFile(@imagePath);
                var page = engine.Process(image);

                var text = page.GetText();

                // Si veníamos de un BMP, borramos el PNG temporal que generamos.
                if (format == ".bmp") { File.Delete(imagePath); }

                image.Dispose();
                page.Dispose();
                engine.Dispose();

                return text;
            }
            catch (Exception e)
            {
                // Patrón "GetLastError" para PowerBuilder: guardamos el mensaje y relanzamos.
                ErrorText = e.Message;
                throw new Exception(ErrorText);
            }
        }

        /// <summary>
        /// Sobrecarga "completa" de <see cref="ConvertImageToTxt(string, string)"/>: hace el OCR con la
        /// carpeta de datos e idioma indicados y escribe el resultado en un fichero de texto.
        /// </summary>
        public void ConvertImageToTxt(string imagePath, string txtPath, string dataPath, string language)
        {
            string text = ConvertImageToString(imagePath, dataPath, language);

            File.WriteAllText(@txtPath, text);
        }

        /// <summary>
        /// Convierte un BMP a PNG (mismo nombre, misma carpeta) y devuelve la ruta del PNG generado.
        /// Es un apoyo interno para esquivar los problemas de Tesseract/Leptonica con ciertos BMP.
        /// </summary>
        /// <param name="imagePath">Ruta del BMP de origen.</param>
        /// <returns>Ruta del PNG resultante.</returns>
        public string SaveBmpAsPNG(string imagePath)
        {
            string newName = Path.GetDirectoryName(imagePath) + "\\" + Path.GetFileNameWithoutExtension(imagePath) + ".png";
            // Bitmap es IDisposable (envuelve un handle GDI+ nativo); por eso hacemos Dispose() abajo.
            Bitmap bmp1 = new Bitmap(imagePath);
            if (File.Exists(newName))
            {
                File.Delete(newName);
            }
            bmp1.Save(newName, System.Drawing.Imaging.ImageFormat.Png);
            bmp1.Dispose();
            return newName;
        }



        /// <summary>
        /// Devuelve el último mensaje de error capturado. Patrón pensado para PowerBuilder: tras una
        /// llamada que falle, PB invoca este método para leer el motivo (porque no atrapa la excepción .NET).
        /// </summary>
        public string GetLastError()
        {
            return ErrorText;
        }
    }
}
