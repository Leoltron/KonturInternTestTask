using System.Drawing;

namespace Kontur.ImageTransformer.Filters
{
    public class RotateCWFilter : IBitmapFilter
    {
        public static readonly RotateCWFilter Instance = new RotateCWFilter();

        public void Apply(Bitmap bitmap)
        {
            bitmap.RotateFlip(RotateFlipType.Rotate90FlipNone);
        }

        public IBitmapFilter TryParse(string s)
        {
            return s == "rotate-cw" ? Instance : null;
        }

        public Size ResultSizeFor(Size srcSize)
        {
            return new Size(srcSize.Height, srcSize.Width);
        }
    }
}