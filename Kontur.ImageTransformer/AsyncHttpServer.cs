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

        private static void RespondWithServiceUnavailable(HttpListenerResponse response)
        {
            response.StatusCode = 503;
            response.Close();
        }

        private static async void HandleContextAsync(HttpListenerContext context)
        {
            Interlocked.Increment(ref tasksExecuting);
            await Task.Run(() => HandleContext(context));
            Interlocked.Decrement(ref tasksExecuting);
        }

        private static void HandleContext(HttpListenerContext context)
        {
            using (var parseResult = RequestParser.ParseRequest(context.Request))
            {
                var response = context.Response;
                response.StatusCode = (int)parseResult.ResultCode;
                if (parseResult.NoErrors)
                    SendImageResponse(response, parseResult.Image);
                response.Close();
            }
        }

        private const string ContentTypeImagePng = "image/png";

        private static void SendImageResponse(HttpListenerResponse response, Image image)
        {
            response.ContentType = ContentTypeImagePng;
            image.Save(response.OutputStream, ImageFormat.Png);
        }

        private readonly HttpListener listener;

        private Thread listenerThread;
        private bool disposed;
        private volatile bool isRunning;
    }
}