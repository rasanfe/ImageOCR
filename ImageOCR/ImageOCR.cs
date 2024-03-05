using System.Drawing;
using System.Reflection;
using Tesseract;

namespace ImageOCR
{
    public class ImageOCR
    {
        private string ErrorText = "";
        public string ConvertImageToString(string imagePath)
        {
            string rutaEnsamblado = Assembly.GetExecutingAssembly().Location;
            string directorio = Path.GetDirectoryName(rutaEnsamblado);
            string dataPath = Path.Combine(directorio, "tessdata");
            string language = "spa";
            string text = ConvertImageToString(imagePath, dataPath, language);

            return text;
        }

        public void ConvertImageToTxt(string imagePath, string txtPath)
        {
            string rutaEnsamblado = Assembly.GetExecutingAssembly().Location;
            string directorio = Path.GetDirectoryName(rutaEnsamblado);
            string dataPath = Path.Combine(directorio, "tessdata");
            string language = "spa";

            ConvertImageToTxt(imagePath, txtPath, dataPath, language);
        }

        public string ConvertImageToString(string imagePath, string dataPath, string language)
        {
            try
            {
                var format = Path.GetExtension(imagePath);

                //Si el formato es BMP lo convierto a PNG.
                if (format == ".bmp") { imagePath = SaveBmpAsPNG(imagePath); }


                var engine = new TesseractEngine(@dataPath, language);
                var image = Pix.LoadFromFile(@imagePath);
                var page = engine.Process(image);

                var text = page.GetText();

                //Si el formato es BMP elimino el PNG temporal.
                if (format == ".bmp") { File.Delete(imagePath); }

                image.Dispose();
                page.Dispose();
                engine.Dispose();

                return text;
            }
            catch (Exception e)
            {
                //Capturamos el Error para poderlo leer en PowerBuilder
                ErrorText = e.Message;
                throw new Exception(ErrorText);
            }
        }

        public void ConvertImageToTxt(string imagePath, string txtPath, string dataPath, string language)
        {
            string text = ConvertImageToString(imagePath, dataPath, language);

            File.WriteAllText(@txtPath, text);
        }

        public string SaveBmpAsPNG(string imagePath)
        {
            string newName = Path.GetDirectoryName(imagePath) + "\\" + Path.GetFileNameWithoutExtension(imagePath) + ".png";
            Bitmap bmp1 = new Bitmap(imagePath);
            if (File.Exists(newName))
            {
                File.Delete(newName);
            }
            bmp1.Save(newName, System.Drawing.Imaging.ImageFormat.Png);
            bmp1.Dispose();
            return newName;
        }



        public string GetLastError()
        {
            return ErrorText;
        }
    }
}
