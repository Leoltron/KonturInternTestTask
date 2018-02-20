using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Net;
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
                        //Task.Run(() => HandleContextAsync(context));
                        HandleContextAsync(context);
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

        private static Task HandleContextAsync(HttpListenerContext listenerContext)
        {
            return Task.Run(() => HandleContext(listenerContext));
        }

        private static void HandleContext(HttpListenerContext listenerContext)
        {
            //var s = new Stopwatch();
            //s.Start();
            using (var result = RequestParser.ParseRequest(listenerContext.Request))
            {
                var response = listenerContext.Response;
                Logger.Info(
                    $"{listenerContext.Request.HttpMethod} {listenerContext.Request.RawUrl} - {result.ResultCode}");
                if (result.NoErrors)
                    SendImageResponse(response, result.Apply());
                response.StatusCode = (int) result.ResultCode;
                response.Close();
            }
            //s.Stop();
            //Logger.Debug(s.ElapsedMilliseconds+"ms");
        }

        private static void SendImageResponse(HttpListenerResponse ctxResponse, Image bitmap)
        {
            using (bitmap)
            {
                ctxResponse.ContentType = "image/png";
                ctxResponse.StatusCode = (int) HttpStatusCode.OK;
                bitmap.Save(ctxResponse.OutputStream, ImageFormat.Png);
            }
        }

        private readonly HttpListener listener;

        private Thread listenerThread;
        private bool disposed;
        private volatile bool isRunning;
    }
}