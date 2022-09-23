using ImageMagick;
using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Windows.Forms.ImageList;

namespace SimpleImageConverter
{
    internal class ImageMagickAdapter
    {
        public static void Save(string strInFilePath, string strOutFilePath, MagickFormat format)
        {
            MagickImage magickImage = new MagickImage(strInFilePath);
            try
            {
                magickImage.Write(strOutFilePath, format);
            }
            catch
            {
                throw;
            }
            finally
            {
                magickImage.Dispose();
            }
        }

        public static void Save(MagickImage magickImage, string strOutFilePath, MagickFormat format)
        {
            try
            {
                magickImage.Write(strOutFilePath, format);
            }
            catch
            {
                throw;
            }
            finally
            {

            }
        }
    }
}
