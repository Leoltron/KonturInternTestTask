using System.Drawing;

namespace Kontur.ImageTransformer.Filters
{
    public class RotateCWFilter : IImageFilter
    {
        public static readonly RotateCWFilter Instance = new RotateCWFilter();

        public void Apply(Image image)
        {
            image.RotateFlip(RotateFlipType.Rotate90FlipNone);
        }

        public IImageFilter TryParse(string s)
        {
            return s == "rotate-cw" ? Instance : null;
        }

        public Size ResultSizeFor(Size srcSize)
        {
            return new Size(srcSize.Height, srcSize.Width);
        }
    }
}