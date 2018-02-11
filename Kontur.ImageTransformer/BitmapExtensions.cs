﻿using System.Drawing;

namespace Kontur.ImageTransformer
{
    public static class BitmapExtensions
    {
        public static Bitmap CutRectangle(this Bitmap bitmap, Rectangle intersection)
        {
            var result = new Bitmap(intersection.Width, intersection.Height);
            using (var g = Graphics.FromImage(result))
                g.DrawImage(bitmap, new Rectangle(0, 0, intersection.Width, intersection.Height), intersection,
                    GraphicsUnit.Pixel);
            return result;
        }
    }
}