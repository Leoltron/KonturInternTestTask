using System.Drawing;

namespace Kontur.ImageTransformer.Filters
{
    public interface IImageFilter
    {
        IImageFilter TryParse(string s);

        Size ResultSizeFor(Size srcSize);

        void Apply(Image image);
    }
}