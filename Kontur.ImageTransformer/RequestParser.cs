using System;
using System.Drawing;
using System.Net;
using System.Text.RegularExpressions;
using Kontur.ImageTransformer.Filters;
using Kontur.ImageTransformer.Util;

namespace Kontur.ImageTransformer
{
    internal static class RequestParser
    {
        private const long MaxContentLength = 1024 * 100;
        private const long MaxImageWidth = 1000;
        private const long MaxImageHeight = 1000;

        private static readonly Regex RequestRegex =
            new Regex(@"^/process/([^/]+)/(-?[\d]+),(-?[\d]+),(-?[\d]+),(-?[\d]+)$");

        public static RequestParseResult ParseRequest(HttpListenerRequest request)
        {
            if (!IsPostRequest(request) || request.ContentLength64 > MaxContentLength)
                return RequestParseResult.BadRequest;

            var match = RequestRegex.Match(request.RawUrl);
            if (!match.Success)
                return RequestParseResult.BadRequest;

            var filter = BitmapFilters.TryParse(match.Groups[1].Value);
            if (filter == null)
                return RequestParseResult.BadRequest;

            Bitmap bitmap;
            try
            {
                bitmap = new Bitmap(request.InputStream);
            }
            catch (ArgumentException)
            {
                return RequestParseResult.BadRequest;
            }
            if (bitmap.Height > MaxImageHeight || bitmap.Width > MaxImageWidth)
                return RequestParseResult.BadRequest;

            var cuttingArea = Rectangle.Intersect(
                new Rectangle(Point.Empty, filter.ResultSizeFor(bitmap.Size)),
                ParseCuttingArea(match)
            );

            return IsAreaEmpty(cuttingArea) ? RequestParseResult.NoContent : new RequestParseResult(bitmap, filter, cuttingArea);
        }

        private static bool IsPostRequest(HttpListenerRequest request)
        {
            return request.HttpMethod == "POST";
        }

        private static bool IsAreaEmpty(Rectangle rectangle)
        {
            return rectangle.Height == 0 || rectangle.Width == 0;
        }

        private static Rectangle ParseCuttingArea(Match match)
        {
            var x = int.Parse(match.Groups[2].Value);
            var y = int.Parse(match.Groups[3].Value);
            var width = int.Parse(match.Groups[4].Value);
            var height = int.Parse(match.Groups[5].Value);

            FixNegativeDimensions(ref x, ref y, ref width, ref height);

            return new Rectangle(x, y, width, height);
        }

        private static void FixNegativeDimensions(ref int x, ref int y, ref int width, ref int height)
        {
            if (width < 0)
            {
                x += width;
                width = -width;
            }

            if (height < 0)
            {
                y += height;
                height = -height;
            }
        }
    }

    internal class RequestParseResult : IDisposable
    {
        public static readonly RequestParseResult BadRequest = new RequestParseResult(HttpStatusCode.BadRequest);
        public static readonly RequestParseResult NoContent = new RequestParseResult(HttpStatusCode.NoContent);

        public readonly HttpStatusCode ResultCode;

        public bool NoErrors => ResultCode == HttpStatusCode.OK;
        public readonly Bitmap Bitmap;

        private RequestParseResult(HttpStatusCode resultCode)
        {
            ResultCode = resultCode;
        }

        public RequestParseResult(Bitmap bitmap, IBitmapFilter filter, Rectangle cuttingRectangle)
        {
            ResultCode = HttpStatusCode.OK;
            filter.Apply(bitmap);
            Bitmap = bitmap.CutRectangle(cuttingRectangle);
            bitmap.Dispose();
        }

        public void Dispose()
        {
            Bitmap?.Dispose();
        }
    }
}