using System.Drawing;
using System.Drawing.Imaging;
using PdfiumViewer;

namespace ImageFromPdf
{
    public class ImageFromPdf
    {
        private string ErrorText = "";

        public string PdfToBmp(string source)
        {
            int pageFrom = 1;
            int pageTo = 1;
            string outputFile = PdfToBmp(source, pageFrom, pageTo);
            return outputFile;
        }

        public string PdfToBmp(string source, int pageFrom, int pageTo)
        {

            try
            {
                var document = PdfDocument.Load(source);
                var outputFile = Path.GetDirectoryName(source) + "\\" + Path.GetFileNameWithoutExtension(source) + ".bmp";

                for (int i = pageFrom - 1; i < pageTo; i++)
                {
                    var dpi = 300;

                    using (var image = document.Render(i, dpi, dpi, PdfRenderFlags.CorrectFromDpi))
                    {
                        var encoder = ImageCodecInfo.GetImageEncoders().First(c => c.FormatID == ImageFormat.Bmp.Guid);
                        var encParams = new EncoderParameters(1);
                        encParams.Param[0] = new EncoderParameter(Encoder.Quality, 100L);

                        if (pageTo > pageFrom)
                        {
                            outputFile = Path.Combine(Path.GetDirectoryName(outputFile), Path.GetFileNameWithoutExtension(outputFile) + "_" + i + ".bmp");
                        }

                        image.Save(outputFile, encoder, encParams);
                        image.Dispose();
                    }
                }
                document.Dispose();
                return outputFile;
            }
            catch (Exception e)
            {
                ErrorText = e.Message;
                throw new Exception(ErrorText);
            }

        
        }

        public string PdfToPng(string source)
        {
            int pageFrom = 1;
            int pageTo = 1;
            string outputFile = PdfToPng(source, pageFrom, pageTo);
            return outputFile;
        }

        public string PdfToPng(string source, int pageFrom, int pageTo)
        {

            try
            {
                var document = PdfDocument.Load(source);
                var outputFile = Path.GetDirectoryName(source) + "\\" + Path.GetFileNameWithoutExtension(source) + ".png";

                for (int i = pageFrom - 1; i < pageTo; i++)
                {
                    var dpi = 300;

                    using (var image = document.Render(i, dpi, dpi, PdfRenderFlags.CorrectFromDpi))
                    {
                        var encoder = ImageCodecInfo.GetImageEncoders().First(c => c.FormatID == ImageFormat.Png.Guid);
                        var encParams = new EncoderParameters(1);
                        encParams.Param[0] = new EncoderParameter(Encoder.Quality, 100L);

                        if (pageTo > pageFrom)
                        {
                            outputFile = Path.Combine(Path.GetDirectoryName(outputFile), Path.GetFileNameWithoutExtension(outputFile) + "_" + i + ".png");
                        }

                        image.Save(outputFile, encoder, encParams);
                        image.Dispose();
                    }
                }
                document.Dispose();
                return outputFile;
            }
            catch (Exception e)
            {
                ErrorText = e.Message;
                throw new Exception(ErrorText);
            }


        }

        public string GetLastError()
        {
            return ErrorText;
        }
    }
}
