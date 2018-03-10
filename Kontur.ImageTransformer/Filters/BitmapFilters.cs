using System.Linq;

namespace Kontur.ImageTransformer.Filters
{
    public static class BitmapFilters
    {
        private static readonly IBitmapFilter[] FilterInstances =
        {
            FlipHFilter.Instance,
            FlipVFilter.Instance,
            RotateCWFilter.Instance,
            RotateCCWFilter.Instance
        };

        public static IBitmapFilter TryParse(string s) => FilterInstances.FirstOrDefault(filter => filter.TryParse(s) != null);
    }
}