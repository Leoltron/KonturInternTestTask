using System.Drawing;

namespace Kontur.ImageTransformer.Filters
{
    public class FlipHFilter : IImageFilter
    {
        public static readonly FlipHFilter Instance = new FlipHFilter();

        public void Apply(Image image)
        {
            image.RotateFlip(RotateFlipType.RotateNoneFlipX);
        }

        public IImageFilter TryParse(string s)
        {
            return s == "flip-h" ? Instance : null;
        }

        public Size ResultSizeFor(Size srcSize)
        {
            return srcSize;
        }
    }
}