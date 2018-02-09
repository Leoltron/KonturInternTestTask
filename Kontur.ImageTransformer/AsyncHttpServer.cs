using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Kontur.ImageTransformer
{
    internal class AsyncHttpServer : IDisposable
    {
        public AsyncHttpServer()
        {
            listener = new HttpListener();
        }

        public void Start(string prefix)
        {
            lock (listener)
            {
                if (isRunning) return;
                listener.Prefixes.Clear();
                listener.Prefixes.Add(prefix);
                listener.Start();

                listenerThread = new Thread(Listen)
                {
                    IsBackground = true,
                    Priority = ThreadPriority.Highest
                };
                listenerThread.Start();

                isRunning = true;
            }
        }

        public void Stop()
        {
            lock (listener)
            {
                if (!isRunning)
                    return;

                listener.Stop();

                listenerThread.Abort();
                listenerThread.Join();

                isRunning = false;
            }
        }

        public void Dispose()
        {
            if (disposed)
                return;

            disposed = true;

            Stop();

            listener.Close();
        }

        private void Listen()
        {
            while (true)
            {
                try
                {
                    if (listener.IsListening)
                    {
                        var context = listener.GetContext();
                        Task.Run(() => HandleContextAsync(context));
                    }
                    else Thread.Sleep(0);
                }
                catch (ThreadAbortException)
                {
                    return;
                }
                catch (Exception exception)
                {
                    Logger.Error(exception);
                    // TODO: log errors
                }
            }
        }

        private static async Task HandleContextAsync(HttpListenerContext listenerContext)
        {
            await new Task(() => HandleContext(listenerContext));
        }

        private static void HandleContext(HttpListenerContext listenerContext)
        {
            var match = RequestParser.TryParseRequest(listenerContext.Request);
            if (match == null)
            {
                listenerContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return;
            }

            int i = 1;

            Func<Bitmap, Bitmap> filter = GetFilter(match, ref i);

            if (filter == null)
            {
                listenerContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return;
            }

            var x = int.Parse(match.Groups[i++].Value);
            var y = int.Parse(match.Groups[i++].Value);
            var width = int.Parse(match.Groups[i++].Value);
            var height = int.Parse(match.Groups[i].Value);

            var bitmap = new Bitmap(listenerContext.Request.InputStream).Cut(x, y, width, height);
            if (bitmap == null)
            {
                listenerContext.Response.StatusCode = (int)HttpStatusCode.NoContent;
                return;
            }

            bitmap = filter(bitmap);
            SendImageResponse(listenerContext.Response, bitmap);
        }

        private static Func<Bitmap, Bitmap> GetFilter(Match match, ref int startIndex)
        {
            var filter = match.Groups[startIndex++].Value;
            if (filter == "grayscale")
                return BitmapExtensions.GrayScale;
            if (filter.StartsWith("threshold"))
            {
                var coefficent = int.Parse(match.Groups[startIndex++].Value);
                if (coefficent < 0 || coefficent > 100)
                    return null;
                return b => b.Threshold(coefficent);
            }
            if (filter == "sepia")
                return BitmapExtensions.Sepia;

            return null;
        }

        private static void SendImageResponse(HttpListenerResponse ctxResponse, Image bitmap)
        {
            ctxResponse.ContentType = "image/png";
            ctxResponse.StatusCode = (int) HttpStatusCode.OK;
            bitmap.Save(ctxResponse.OutputStream, ImageFormat.Png);
            ctxResponse.OutputStream.Close();
        }

        private readonly HttpListener listener;

        private Thread listenerThread;
        private bool disposed;
        private volatile bool isRunning;
    }
}