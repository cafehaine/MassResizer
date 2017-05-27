using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace MassResizer
{
    static class Conversion
    {
        public static Bitmap ScaleToFill(Bitmap image, Size surface)
        {
            int width = image.Width;
            int height = image.Height;

            if (width != surface.Width)
            {
                height = height * surface.Width / width;
                width = surface.Width;
            }

            if (height < surface.Height)
            {
                width = width * surface.Height / height;
                height = surface.Height;
            }

            Rectangle destRect = new Rectangle(0 - (width - surface.Width) / 2, 0 - (height - surface.Height) / 2, width, height);
            Bitmap destImage = new Bitmap(surface.Width, surface.Height);

            destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            using (var graphics = Graphics.FromImage(destImage))
            {
                graphics.CompositingMode = CompositingMode.SourceCopy;

                using (ImageAttributes wrapMode = new ImageAttributes())
                {
                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }

            return destImage;
        }

        public static Bitmap ScaleToFit(Bitmap image, Size surface)
        {
            int width = image.Width;
            int height = image.Height;

            if (height != surface.Height)
            {
                width = width * surface.Height / height;
                height = surface.Height;
            }

            if (width > surface.Width)
            {
                height = height * surface.Width / width;
                width = surface.Width;
            }

            Rectangle destRect = new Rectangle(0 - (width - surface.Width) / 2, 0 - (height - surface.Height) / 2, width, height);
            Bitmap destImage = new Bitmap(surface.Width, surface.Height);

            destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            using (var graphics = Graphics.FromImage(destImage))
            {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.Clear(Color.Black);
                using (ImageAttributes wrapMode = new ImageAttributes())
                {
                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }

            return destImage;
        }

        public static Bitmap ScaleToFitNoBrdrs(Bitmap image, Size surface)
        {
            int width = image.Width;
            int height = image.Height;

            if (height != surface.Height)
            {
                width = width * surface.Height / height;
                height = surface.Height;
            }

            if (width > surface.Width)
            {
                height = height * surface.Width / width;
                width = surface.Width;
            }

            Rectangle destRect = new Rectangle(0, 0, width, height);
            Bitmap destImage = new Bitmap(width, height);

            destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            using (var graphics = Graphics.FromImage(destImage))
            {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                using (ImageAttributes wrapMode = new ImageAttributes())
                {
                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }

            return destImage;
        }
    }
}
