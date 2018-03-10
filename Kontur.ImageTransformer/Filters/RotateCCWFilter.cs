using System.Drawing;

namespace Kontur.ImageTransformer.Filters
{
    public class RotateCCWFilter : IBitmapFilter
    {
        public static readonly RotateCCWFilter Instance = new RotateCCWFilter();

        public void Apply(Bitmap bitmap)
        {
            bitmap.RotateFlip(RotateFlipType.Rotate270FlipNone);
        }

        public IBitmapFilter TryParse(string s)
        {
            return s == "rotate-ccw" ? Instance : null;
        }

        public Size ResultSizeFor(Size srcSize)
        {
            return new Size(srcSize.Height, srcSize.Width);
        }
    }
}