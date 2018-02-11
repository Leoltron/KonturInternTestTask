using System;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;

namespace Kontur.ImageTransformer
{
    public interface IBitmapFilter
    {
        Bitmap Process(Bitmap bitmap);
        IBitmapFilter TryParse(string s);
    }

    public static class BitmapFilters
    {
        private static readonly IBitmapFilter[] FilterInstances =
        {
            new GrayscaleFilter(),
            new SepiaFilter(),
            new ThresholdFilter()
        };

        public static IBitmapFilter TryParse(string s) => FilterInstances
            .Select(filter => filter.TryParse(s))
            .FirstOrDefault(f => f != null);
    }

    public class GrayscaleFilter : IBitmapFilter
    {
        public Bitmap Process(Bitmap bitmap)
        {
            for (var x = 0; x < bitmap.Width; x++)
            for (var y = 0; y < bitmap.Height; y++)
            {
                var pixelColor = bitmap.GetPixel(x, y);
                var intensity = (byte) ((pixelColor.R + pixelColor.G + pixelColor.B) / 3);
                bitmap.SetPixel(x, y, Color.FromArgb(pixelColor.A, intensity, intensity, intensity));
            }

            return bitmap;
        }

        public IBitmapFilter TryParse(string s)
        {
            return s == "grayscale" ? new GrayscaleFilter() : null;
        }
    }

    public class SepiaFilter : IBitmapFilter
    {
        public Bitmap Process(Bitmap bitmap)
        {
            for (var x = 0; x < bitmap.Width; x++)
            for (var y = 0; y < bitmap.Height; y++)
            {
                var oldColor = bitmap.GetPixel(x, y);
                var oldR = oldColor.R;
                var oldG = oldColor.G;
                var oldB = oldColor.B;
                var newR = (byte) Math.Min(255, (int) (oldR * .393 + oldG * .769 + oldB * .189));
                var newG = (byte) Math.Min(255, (int) (oldR * .349 + oldG * .686 + oldB * .168));
                var newB = (byte) Math.Min(255, (int) (oldR * .272 + oldG * .534 + oldB * .131));
                bitmap.SetPixel(x, y, Color.FromArgb(oldColor.A, newR, newG, newB));
            }

            return bitmap;
        }

        public IBitmapFilter TryParse(string s)
        {
            return s == "sepia" ? new SepiaFilter() : null;
        }
    }

    public class ThresholdFilter : IBitmapFilter
    {
        private readonly byte coefficent;

        public ThresholdFilter(byte coefficent = 0)
        {
            this.coefficent = coefficent;
        }

        public Bitmap Process(Bitmap bitmap)
        {
            var border = 255 * coefficent / 100;
            for (var x = 0; x < bitmap.Width; x++)
            for (var y = 0; y < bitmap.Height; y++)
            {
                var pixelColor = bitmap.GetPixel(x, y);
                var newRgbValue = (pixelColor.R + pixelColor.G + pixelColor.B) / 3 >= border ? 255 : 0;
                bitmap.SetPixel(x, y, Color.FromArgb(pixelColor.A, newRgbValue, newRgbValue, newRgbValue));
            }

            return bitmap;
        }

        private static readonly Regex regex = new Regex(@"threshold\(([\d]{1,3})\)");

        public IBitmapFilter TryParse(string s)
        {
            var match = regex.Match(s);
            if (!match.Success)
                return null;
            var c = int.Parse(match.Groups[1].Value);
            return c <= 100 ? new ThresholdFilter((byte) c) : null;
        }
    }
}