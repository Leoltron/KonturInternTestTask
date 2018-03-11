using System.Drawing;

namespace Kontur.ImageTransformer.Filters
{
    public class RotateCCWFilter : IImageFilter
    {
        public static readonly RotateCCWFilter Instance = new RotateCCWFilter();

        public void Apply(Image image)
        {
            image.RotateFlip(RotateFlipType.Rotate270FlipNone);
        }

        public IImageFilter TryParse(string s)
        {
            return s == "rotate-ccw" ? Instance : null;
        }

        public Size ResultSizeFor(Size srcSize)
        {
            return new Size(srcSize.Height, srcSize.Width);
        }
    }
}