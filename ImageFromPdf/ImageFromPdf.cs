using System.Drawing;
using System.Drawing.Imaging;
using SkiaSharp;   // Motor gráfico 2D de Google (el mismo de Chrome/Flutter); aquí, para codificar imágenes.
using PDFtoImage;  // Rasteriza PDF→imagen apoyándose en PDFium (el render de PDF de Chrome) + SkiaSharp.

namespace ImageFromPdf
{
    /// <summary>
    /// Convierte páginas de un PDF en imágenes (BMP o PNG) para, por ejemplo, pasarlas luego por OCR.
    /// Pensada para consumirse desde PowerBuilder.
    /// </summary>
    /// <remarks>
    /// Nota de la migración a .NET 10: antes esto tiraba de <c>PdfiumViewer</c>, una librería ya
    /// ABANDONADA y atada a System.Drawing. La hemos cambiado por <c>PDFtoImage</c>, que por debajo usa
    /// <c>PDFium</c> (el motor de PDF de Chrome, muy robusto) y <c>SkiaSharp</c> para generar la imagen.
    /// Resultado: render multiplataforma y mantenido, sin depender de código viejo.
    /// <para>
    /// Uso típico desde PowerBuilder:
    /// <code>
    /// oPdf = create ImageFromPdf
    /// ls_imagen = oPdf.PdfToPng("C:\documento.pdf")   // primera página → C:\documento.png
    /// </code>
    /// Tras cualquier llamada conviene comprobar <see cref="GetLastError"/> por si hubo fallo.
    /// </para>
    /// </remarks>
    public class ImageFromPdf
    {
        // Último error capturado, para que PowerBuilder lo lea con GetLastError() (PB no atrapa
        // las excepciones .NET, así que las guardamos aquí además de relanzarlas).
        private string ErrorText = "";

        // 300 puntos por pulgada: resolución "de escáner". Suficiente para que el OCR posterior
        // lea bien el texto sin disparar el tamaño de la imagen.
        private const int Dpi = 300;

        /// <summary>
        /// Convierte SOLO la primera página del PDF a BMP (atajo cómodo desde PB).
        /// </summary>
        /// <param name="source">Ruta del PDF de origen.</param>
        /// <returns>Ruta del BMP generado.</returns>
        public string PdfToBmp(string source)
        {
            return PdfToBmp(source, 1, 1);
        }

        /// <summary>
        /// Convierte un rango de páginas del PDF a BMP. Si el rango abarca varias páginas, genera un
        /// fichero por página con sufijo <c>_n</c> (p. ej. <c>documento_2.bmp</c>).
        /// </summary>
        /// <param name="source">Ruta del PDF de origen.</param>
        /// <param name="pageFrom">Primera página a convertir (empezando en 1).</param>
        /// <param name="pageTo">Última página a convertir (inclusive).</param>
        /// <returns>Ruta del último BMP generado.</returns>
        public string PdfToBmp(string source, int pageFrom, int pageTo)
        {
            try
            {
                // PDFtoImage (PDFium + SkiaSharp) reemplaza al abandonado PdfiumViewer.
                // Leemos el PDF entero en memoria; PDFium trabaja sobre el array de bytes.
                byte[] pdfBytes = File.ReadAllBytes(source);
                string outputFile = Path.Combine(Path.GetDirectoryName(source)!, Path.GetFileNameWithoutExtension(source) + ".bmp");

                // OJO: PDFium numera las páginas desde 0, por eso 'pageFrom - 1'.
                for (int i = pageFrom - 1; i < pageTo; i++)
                {
                    // Una sola página → nombre limpio; varias → añadimos el sufijo "_indice".
                    string pageFile = (pageTo > pageFrom)
                        ? Path.Combine(Path.GetDirectoryName(outputFile)!, Path.GetFileNameWithoutExtension(outputFile) + "_" + i + ".bmp")
                        : outputFile;

                    // 'using' (declaración): el SKBitmap envuelve memoria nativa de Skia y se libera
                    // (Dispose) AUTOMÁTICAMENTE al salir del ámbito de la iteración. Sin fugas y sin try/finally.
                    using SKBitmap skBitmap = Conversion.ToImage(pdfBytes, page: i, options: new RenderOptions(Dpi: Dpi));
                    SaveAsBmp(skBitmap, pageFile);
                    outputFile = pageFile;
                }
                return outputFile;
            }
            catch (Exception e)
            {
                ErrorText = e.Message;
                throw new Exception(ErrorText);
            }
        }

        /// <summary>
        /// Convierte SOLO la primera página del PDF a PNG (atajo cómodo desde PB).
        /// </summary>
        /// <param name="source">Ruta del PDF de origen.</param>
        /// <returns>Ruta del PNG generado.</returns>
        public string PdfToPng(string source)
        {
            return PdfToPng(source, 1, 1);
        }

        /// <summary>
        /// Convierte un rango de páginas del PDF a PNG. Igual que <see cref="PdfToBmp(string, int, int)"/>
        /// pero en PNG, que SkiaSharp sabe escribir de forma nativa (más directo y sin pérdidas).
        /// </summary>
        /// <param name="source">Ruta del PDF de origen.</param>
        /// <param name="pageFrom">Primera página a convertir (empezando en 1).</param>
        /// <param name="pageTo">Última página a convertir (inclusive).</param>
        /// <returns>Ruta del último PNG generado.</returns>
        public string PdfToPng(string source, int pageFrom, int pageTo)
        {
            try
            {
                byte[] pdfBytes = File.ReadAllBytes(source);
                string outputFile = Path.Combine(Path.GetDirectoryName(source)!, Path.GetFileNameWithoutExtension(source) + ".png");

                for (int i = pageFrom - 1; i < pageTo; i++)
                {
                    string pageFile = (pageTo > pageFrom)
                        ? Path.Combine(Path.GetDirectoryName(outputFile)!, Path.GetFileNameWithoutExtension(outputFile) + "_" + i + ".png")
                        : outputFile;

                    // SkiaSharp codifica PNG de forma nativa: guardado directo, sin System.Drawing.
                    // Fijaos en lo simple que queda frente al rodeo que necesita el BMP (ver SaveAsBmp).
                    Conversion.SavePng(pageFile, pdfBytes, page: i, options: new RenderOptions(Dpi: Dpi));
                    outputFile = pageFile;
                }
                return outputFile;
            }
            catch (Exception e)
            {
                ErrorText = e.Message;
                throw new Exception(ErrorText);
            }
        }

        // SkiaSharp no exporta BMP, asi que re-codificamos via System.Drawing (solo Windows)
        // para conservar la salida BMP que esperaba la API original.
        // El rodeo: Skia codifica a PNG en memoria → lo leemos con un Bitmap de System.Drawing →
        // y ese Bitmap sí sabe volcarse como BMP. Tres 'using' encadenados liberan los recursos
        // (SKData nativo, el MemoryStream y el Bitmap GDI+) en cuanto termina el método.
        private static void SaveAsBmp(SKBitmap skBitmap, string outputFile)
        {
            using SKData data = skBitmap.Encode(SKEncodedImageFormat.Png, 100);
            using var ms = new MemoryStream();
            data.SaveTo(ms);
            ms.Position = 0; // rebobinamos el stream antes de que Bitmap lo lea desde el principio
            using var bmp = new Bitmap(ms);
            bmp.Save(outputFile, ImageFormat.Bmp);
        }

        /// <summary>
        /// Devuelve el último mensaje de error capturado, para leerlo desde PowerBuilder tras un fallo.
        /// </summary>
        public string GetLastError()
        {
            return ErrorText;
        }
    }
}
