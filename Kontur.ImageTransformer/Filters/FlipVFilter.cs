using System.Drawing;

namespace Kontur.ImageTransformer.Filters
{
    public class FlipVFilter : IImageFilter
    {
        public static readonly FlipVFilter Instance = new FlipVFilter();

        public void Apply(Image image)
        {
            image.RotateFlip(RotateFlipType.RotateNoneFlipY);
        }

        public IImageFilter TryParse(string s)
        {
            return s == "flip-v" ? Instance : null;
        }

        public Size ResultSizeFor(Size srcSize)
        {
            return srcSize;
        }
    }
}