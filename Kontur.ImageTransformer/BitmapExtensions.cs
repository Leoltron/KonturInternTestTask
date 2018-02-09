using System;
using System.Drawing;
using System.Runtime.Remoting.Messaging;

namespace Kontur.ImageTransformer
{
    public static class BitmapExtensions
    {
        public static Bitmap Cut(this Bitmap bitmap, int x, int y, int width, int height)
        {
            if (width < 0)
            {
                x -= width;
                width = -width;
            }

            if (height < 0)
            {
                y -= height;
                height = -height;
            }

            var intersection = Rectangle.Intersect(
                new Rectangle(x, y, width, height),
                new Rectangle(Point.Empty, bitmap.Size)
            );
            return bitmap.CutRectangle(intersection);
        }

        private static Bitmap CutRectangle(this Bitmap bitmap, Rectangle intersection)
        {
            if (intersection.Width == 0 || intersection.Height == 0)
                return null;
            var result = new Bitmap(intersection.Width, intersection.Height);
            using (var g = Graphics.FromImage(result))
                g.DrawImage(bitmap, new Rectangle(0, 0, intersection.Width, intersection.Height), intersection,
                    GraphicsUnit.Pixel);
            return result;
        }

        public static Bitmap GrayScale(this Bitmap bitmap)
        {
            for (var x = 0; x < bitmap.Width; x++)
            for (var y = 0; y < bitmap.Height; y++)
            {
                var pixelColor = bitmap.GetPixel(x, y);
                var intensity = (byte) ((pixelColor.R + pixelColor.G + pixelColor.B) / 3);
                bitmap.SetPixel(x, y, Color.FromArgb(pixelColor.A, intensity, intensity, intensity));
            }

            return bitmap;
        }

        public static Bitmap Threshold(this Bitmap bitmap, int coefficent)
        {
            if (coefficent < 0 || coefficent > 100)
                throw new ArgumentOutOfRangeException(nameof(coefficent), coefficent,
                    "Coeficent must be in range 0-100");

            var border = 255 / coefficent * 100;
            for (var x = 0; x < bitmap.Width; x++)
            for (var y = 0; y < bitmap.Height; y++)
            {
                var pixelColor = bitmap.GetPixel(x, y);
                var newRgbValue = (pixelColor.R + pixelColor.G + pixelColor.B) / 3 >= border ? 255 : 0;
                bitmap.SetPixel(x, y, Color.FromArgb(pixelColor.A, newRgbValue, newRgbValue, newRgbValue));
            }

            return bitmap;
        }

        public static Bitmap Sepia(this Bitmap bitmap)
        {
            for (var x = 0; x < bitmap.Width; x++)
            for (var y = 0; y < bitmap.Height; y++)
            {
                var oldColor = bitmap.GetPixel(x, y);
                var oldR = oldColor.R;
                var oldG = oldColor.G;
                var oldB = oldColor.B;
                var newR = (byte) Math.Min(255, (int) (oldR * .393 + oldG * .769 + oldB * .189));
                var newG = (byte) Math.Min(255, (int) (oldR * .349 + oldG * .686 + oldB * .168));
                var newB = (byte) Math.Min(255, (int) (oldR * .272 + oldG * .534 + oldB * .131));
                bitmap.SetPixel(x, y, Color.FromArgb(oldColor.A, newR, newG, newB));
            }

            return bitmap;
        }
    }
}