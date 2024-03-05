using System.Reflection;

namespace ImageFromClipboard
{
    public class ImageFromClipboard
    {

        public string ErrorText = "";
        public string GetClipboardImage()
        {
            if (Clipboard.ContainsImage())
            {
                try
                {
                    Bitmap bmp1 = (Bitmap)Clipboard.GetImage();

                    string rutaEnsamblado = Assembly.GetExecutingAssembly().Location;
                    string directorio = Path.GetDirectoryName(rutaEnsamblado);
                    //string newName = directorio + "\\" + "temp.png";
                    string newName = directorio + "\\" + "temp.bmp";
                    if (File.Exists(newName))
                    {
                        File.Delete(newName);
                    }
                    //bmp1.Save(newName, System.Drawing.Imaging.ImageFormat.Png);
                    bmp1.Save(newName);
                    bmp1.Dispose();
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

        public string GetLastError()
        {
            return ErrorText;
        }
    }

}
