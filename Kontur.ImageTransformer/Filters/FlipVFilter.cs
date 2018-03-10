using System.Drawing;

namespace Kontur.ImageTransformer.Filters
{
    public class FlipVFilter : IBitmapFilter
    {
        public static readonly FlipVFilter Instance = new FlipVFilter();

        public void Apply(Bitmap bitmap)
        {
            bitmap.RotateFlip(RotateFlipType.RotateNoneFlipY);
        }

        public IBitmapFilter TryParse(string s)
        {
            return s == "flip-v" ? Instance : null;
        }

        public Size ResultSizeFor(Size srcSize)
        {
            return srcSize;
        }
    }
}