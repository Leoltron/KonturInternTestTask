using System;
using System.Drawing;
using System.Net;
using System.Text.RegularExpressions;
using Kontur.ImageTransformer.Filters;

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

            var filter = ImageFilters.TryParse(match.Groups[1].Value);
            if (filter == null)
                return RequestParseResult.BadRequest;

            Image image;
            try
            {
                image = Image.FromStream(request.InputStream, false, false);
            }
            catch (Exception)
            {
                return RequestParseResult.BadRequest;
            }
            if (image.Height > MaxImageHeight || image.Width > MaxImageWidth)
                return RequestParseResult.BadRequest;

            var cuttingArea = Rectangle.Intersect(
                new Rectangle(Point.Empty, filter.ResultSizeFor(image.Size)),
                ParseCuttingArea(match)
            );

            return IsAreaEmpty(cuttingArea) ? RequestParseResult.NoContent : new RequestParseResult(image, filter, cuttingArea);
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
}