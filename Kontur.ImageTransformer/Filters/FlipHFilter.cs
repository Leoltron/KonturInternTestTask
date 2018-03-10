using System.Drawing;

namespace Kontur.ImageTransformer.Filters
{
    public class FlipHFilter : IBitmapFilter
    {
        public static readonly FlipHFilter Instance = new FlipHFilter();

        public void Apply(Bitmap bitmap)
        {
            bitmap.RotateFlip(RotateFlipType.RotateNoneFlipX);
        }

        public IBitmapFilter TryParse(string s)
        {
            return s == "flip-h" ? Instance : null;
        }

        public Size ResultSizeFor(Size srcSize)
        {
            return srcSize;
        }
    }
}