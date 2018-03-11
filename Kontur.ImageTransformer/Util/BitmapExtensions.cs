using System.Drawing;

namespace Kontur.ImageTransformer.Util
{
    public static class BitmapExtensions
    {
        public static Bitmap CutRectangle(this Image image, Rectangle intersection)
        {
            var result = new Bitmap(intersection.Width, intersection.Height);
            using (var g = Graphics.FromImage(result))
                g.DrawImage(
                    image,
                    new Rectangle(0, 0, intersection.Width, intersection.Height),
                    intersection,
                    GraphicsUnit.Pixel);
            return result;
        }
    }
}