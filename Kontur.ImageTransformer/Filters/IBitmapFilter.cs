using System.Drawing;

namespace Kontur.ImageTransformer.Filters
{
    public interface IBitmapFilter
    {
        IBitmapFilter TryParse(string s);

        Size ResultSizeFor(Size srcSize);

        void Apply(Bitmap bitmap);
    }
}