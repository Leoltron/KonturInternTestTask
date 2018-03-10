using System;
using System.Drawing;
using System.Net;
using System.Text.RegularExpressions;

namespace Kontur.ImageTransformer
{
    internal static class RequestParser
    {
        private const long MaxContentLength = 1024 * 100;

        private static readonly Regex RequestRegex =
            new Regex(@"^/process/([^/]+)/(-?[\d]+),(-?[\d]+),(-?[\d]+),(-?[\d]+)$");

        public static RequestParseResult ParseRequest(HttpListenerRequest request)
        {
            if (request.HttpMethod != "POST" || request.ContentLength64 > MaxContentLength)
                return RequestParseResult.BadRequest;

            var match = RequestRegex.Match(request.RawUrl);

            if (!match.Success)
                return RequestParseResult.BadRequest;

            var filter = BitmapFilters.TryParse(match.Groups[1].Value);

            if (filter == null)
                return RequestParseResult.BadRequest;

            var x = int.Parse(match.Groups[2].Value);
            var y = int.Parse(match.Groups[3].Value);
            var width = int.Parse(match.Groups[4].Value);
            var height = int.Parse(match.Groups[5].Value);

            Bitmap bitmap;
            try
            {
                bitmap = new Bitmap(request.InputStream);
            }
            catch (ArgumentException)
            {
                return RequestParseResult.BadRequest;
            }

            if (bitmap.Height > 1000 || bitmap.Width > 1000)
                return RequestParseResult.BadRequest;

            var cuttingArea = Rectangle.Intersect(
                new Rectangle(Point.Empty, filter.GetResultSize(bitmap.Size)),
                FromAdvancedXYWH(x, y, width, height)
            );

            if (cuttingArea.Height == 0 || cuttingArea.Width == 0)
                return RequestParseResult.NoContent;

            return new RequestParseResult(bitmap, filter, cuttingArea);
        }

        private static Rectangle FromAdvancedXYWH(int x, int y, int width, int height)
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

            return new Rectangle(x, y, width, height);
        }
    }

    internal class RequestParseResult : IDisposable
    {
        public readonly HttpStatusCode ResultCode;

        public bool NoErrors => ResultCode == HttpStatusCode.OK;

        private readonly IBitmapFilter filter;
        private readonly Rectangle cuttingRectangle;
        private readonly Bitmap bitmap;

        private RequestParseResult(HttpStatusCode resultCode) : this(null, null, Rectangle.Empty)
        {
            ResultCode = resultCode;
        }

        public RequestParseResult(Bitmap bitmap, IBitmapFilter filter,
            Rectangle cuttingRectangle)
        {
            ResultCode = HttpStatusCode.OK;
            this.bitmap = bitmap;
            this.filter = filter;
            this.cuttingRectangle = cuttingRectangle;
        }

        public Bitmap Apply()
        {
            return filter.Process(bitmap).CutRectangle(cuttingRectangle);
        }

        public void Dispose()
        {
            bitmap?.Dispose();
        }

        public static readonly RequestParseResult BadRequest = new RequestParseResult(HttpStatusCode.BadRequest);
        public static readonly RequestParseResult NoContent = new RequestParseResult(HttpStatusCode.NoContent);
    }
}