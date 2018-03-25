using System.Linq;

namespace Kontur.ImageTransformer.Filters
{
    public static class ImageFilters
    {
        private static readonly IImageFilter[] FilterInstances =
        {
            FlipHFilter.Instance,
            FlipVFilter.Instance,
            RotateCWFilter.Instance,
            RotateCCWFilter.Instance
        };

        public static IImageFilter TryParse(string s) => FilterInstances.FirstOrDefault(filter => filter.TryParse(s) != null);
    }
}