using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Kontur.ImageTransformer.Util;

namespace Kontur.ImageTransformer
{
    internal class AsyncHttpServer : IDisposable
    {
        private static readonly int MaxAsyncTasks = Environment.ProcessorCount * 5;
        private static int tasksExecuting;

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

                        if (tasksExecuting >= MaxAsyncTasks)
                            RespondWithServiceUnavailable(context.Response);
                        else
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
                }
            }
        }

        private static void RespondWithServiceUnavailable(HttpListenerResponse httpListenerResponse)
        {
            httpListenerResponse.StatusCode = 503;
            httpListenerResponse.Close();
        }

        private static void HandleContextAsync(HttpListenerContext listenerContext)
        {
            Interlocked.Increment(ref tasksExecuting);
            Task.Run(() => HandleContext(listenerContext));
        }

        private static void HandleContext(HttpListenerContext listenerContext)
        {
            using (var result = RequestParser.ParseRequest(listenerContext.Request))
            {
                var response = listenerContext.Response;
                if (result.NoErrors)
                    SendImageResponse(response, result.Bitmap);
                response.StatusCode = (int) result.ResultCode;
                response.Close();
            }

            Interlocked.Decrement(ref tasksExecuting);
        }

        private const string ContentTypeImagePng = "image/png";

        private static void SendImageResponse(HttpListenerResponse response, Image bitmap)
        {
            response.ContentType = ContentTypeImagePng;
            response.StatusCode = (int) HttpStatusCode.OK;
            bitmap.Save(response.OutputStream, ImageFormat.Png);
        }

        private readonly HttpListener listener;

        private Thread listenerThread;
        private bool disposed;
        private volatile bool isRunning;
    }
}