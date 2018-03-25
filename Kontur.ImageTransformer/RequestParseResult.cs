using System;
using System.Drawing;
using System.Net;
using Kontur.ImageTransformer.Filters;
using Kontur.ImageTransformer.Util;

namespace Kontur.ImageTransformer
{
    internal class RequestParseResult : IDisposable
    {
        public static readonly RequestParseResult BadRequest = new RequestParseResult(HttpStatusCode.BadRequest);
        public static readonly RequestParseResult NoContent = new RequestParseResult(HttpStatusCode.NoContent);

        public readonly HttpStatusCode ResultCode;

        public bool NoErrors => ResultCode == HttpStatusCode.OK;
        public readonly Image Image;

        private RequestParseResult(HttpStatusCode resultCode)
        {
            ResultCode = resultCode;
        }

        public RequestParseResult(Image image, IImageFilter filter, Rectangle cuttingRectangle) : this(HttpStatusCode.OK)
        {
            filter.Apply(image);
            Image = image.CutRectangle(cuttingRectangle);
            image.Dispose();
        }

        public void Dispose()
        {
            Image?.Dispose();
        }
    }
}