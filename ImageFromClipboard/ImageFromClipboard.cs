using System.Reflection;
// Nota: Clipboard, Bitmap, File... llegan vía ImplicitUsings (System.Windows.Forms, System.Drawing,
// System.IO ya están en los GlobalUsings del proyecto), por eso aquí solo hace falta Reflection.

namespace ImageFromClipboard
{
    /// <summary>
    /// Vuelca a un fichero la imagen que haya en el portapapeles de Windows. Pensada para PowerBuilder:
    /// el usuario hace "Recortes"/Imprimir Pantalla, copia, y desde PB recuperamos esa imagen como fichero.
    /// </summary>
    /// <remarks>
    /// Usa el portapapeles de <c>System.Windows.Forms</c> (de ahí que el proyecto active WinForms).
    /// Uso típico desde PowerBuilder:
    /// <code>
    /// oClip = create ImageFromClipboard
    /// ls_imagen = oClip.GetClipboardImage()   // ruta del temp.bmp generado junto a la DLL
    /// </code>
    /// </remarks>
    public class ImageFromClipboard
    {

        // Último error capturado. Es público porque así PB puede leerlo directamente, además de
        // tenerlo disponible vía GetLastError() (patrón habitual en estos ejemplos para PowerBuilder).
        public string ErrorText = "";

        /// <summary>
        /// Si el portapapeles contiene una imagen, la guarda como <c>temp.bmp</c> junto a la DLL y
        /// devuelve su ruta. Si no hay imagen, lanza excepción y deja el motivo en <see cref="GetLastError"/>.
        /// </summary>
        /// <returns>Ruta del fichero de imagen generado.</returns>
        public string GetClipboardImage()
        {
            // Clipboard.ContainsImage(): comprobamos primero que de verdad hay una imagen
            // (el portapapeles podría tener texto, ficheros, nada...).
            if (Clipboard.ContainsImage())
            {
                try
                {
                    // GetImage() devuelve la imagen del portapapeles; la tratamos como Bitmap para guardarla.
                    // '!' = le decimos al compilador que aquí no será null (ya validamos con ContainsImage).
                    Bitmap bmp1 = (Bitmap)Clipboard.GetImage()!;

                    // Guardamos el temporal JUNTO a la DLL para no depender de rutas externas.
                    string rutaEnsamblado = Assembly.GetExecutingAssembly().Location;
                    string directorio = Path.GetDirectoryName(rutaEnsamblado)!;
                    //string newName = directorio + "\\" + "temp.png";
                    string newName = directorio + "\\" + "temp.bmp";
                    if (File.Exists(newName))
                    {
                        File.Delete(newName);
                    }
                    //bmp1.Save(newName, System.Drawing.Imaging.ImageFormat.Png);
                    bmp1.Save(newName); // sin formato explícito, Save() usa BMP por la extensión/uso por defecto
                    bmp1.Dispose();     // Bitmap es IDisposable (handle GDI+ nativo): lo liberamos a mano
                    return newName;
                }
                catch (Exception e)
                {
                    //Capturamos el Error para poderlo leer en PowerBuilder
                    ErrorText = e.Message;
                    throw new Exception(ErrorText);
                }
            }
            else
            {
                ErrorText = "El PortaPapeles no contiene una Imagen.";
                throw new Exception(ErrorText);
            }

        }

        //public string ConvertImageFromClipBoardToString()
        //{
        //    string imagePath = GetClipboardImage();
        //    string result = ConvertImageToString(imagePath);
        //    //File.Delete(imagePath);
        //    return result;
        //}

        /// <summary>
        /// Devuelve el último mensaje de error capturado, para leerlo desde PowerBuilder tras un fallo.
        /// </summary>
        public string GetLastError()
        {
            return ErrorText;
        }
    }

}
