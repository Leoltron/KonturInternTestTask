using System.Drawing;
using System.Linq;

namespace Kontur.ImageTransformer
{
    public interface IBitmapFilter
    {
        Bitmap Process(Bitmap bitmap);
        IBitmapFilter TryParse(string s);
        Size GetResultSize(Size srcSize);
    }

    public static class BitmapFilters
    {
        private static readonly IBitmapFilter[] FilterInstances =
        {
            new FlipHFilter(),
            new FlipVFilter(),
            new RotateCWFilter(),
            new RotateCCWFilter()
        };

        public static IBitmapFilter TryParse(string s) => FilterInstances
            .Select(filter => filter.TryParse(s))
            .FirstOrDefault(f => f != null);
    }

    public class FlipHFilter : IBitmapFilter
    {
        public Bitmap Process(Bitmap bitmap)
        {
            bitmap.RotateFlip(RotateFlipType.RotateNoneFlipX);
            return bitmap;
        }

        public IBitmapFilter TryParse(string s)
        {
            return s == "flip-h" ? new FlipHFilter() : null;
        }

        public Size GetResultSize(Size srcSize)
        {
            return srcSize;
        }
    }

    public class FlipVFilter : IBitmapFilter
    {
        public Bitmap Process(Bitmap bitmap)
        {
            bitmap.RotateFlip(RotateFlipType.RotateNoneFlipY);
            return bitmap;
        }

        public IBitmapFilter TryParse(string s)
        {
            return s == "flip-v" ? new FlipVFilter() : null;
        }

        public Size GetResultSize(Size srcSize)
        {
            return srcSize;
        }
    }

    public class RotateCWFilter : IBitmapFilter
    {
        public Bitmap Process(Bitmap bitmap)
        {
            bitmap.RotateFlip(RotateFlipType.Rotate90FlipNone);
            return bitmap;
        }

        public IBitmapFilter TryParse(string s)
        {
            return s == "rotate-cw" ? new RotateCWFilter() : null;
        }

        public Size GetResultSize(Size srcSize)
        {
            return new Size(srcSize.Height, srcSize.Height);
        }
    }

    public class RotateCCWFilter : IBitmapFilter
    {
        public Bitmap Process(Bitmap bitmap)
        {
            bitmap.RotateFlip(RotateFlipType.Rotate270FlipNone);
            return bitmap;
        }

        public IBitmapFilter TryParse(string s)
        {
            return s == "rotate-ccw" ? new RotateCCWFilter() : null;
        }

        public Size GetResultSize(Size srcSize)
        {
            return new Size(srcSize.Height, srcSize.Height);
        }
    }

    
}