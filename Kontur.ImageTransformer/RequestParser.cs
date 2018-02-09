using System.Net;
using System.Text.RegularExpressions;

namespace Kontur.ImageTransformer
{
    internal class RequestParser
    {
        private const long MaxContentLength = 100 * 1024;

        private static readonly Regex RequestRegex = new Regex(@"^/process/(grayscale|sepia|threshold\([\d]+\))/
                                              (-?[\d]+),(-?[\d]+),(-?[\d]+),(-?[\d]+)$");

        public static Match TryParseRequest(HttpListenerRequest request)
        {
            if (request.HttpMethod != "POST" || request.ContentLength64 > MaxContentLength)
                return null;

            var match = RequestRegex.Match(request.RawUrl);
            return match.Success ? match : null;
        }
    }
}