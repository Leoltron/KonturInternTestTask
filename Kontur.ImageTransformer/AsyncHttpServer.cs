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
        public static int TasksExecuting = 0;
        public static long LastTaskSpeedMS = 0;

        private readonly int maxAsyncTasks = Environment.ProcessorCount * 5;

        public void LogStats()
        {
            Logger.Debug($"Tasks Executing: {TasksExecuting}/" + maxAsyncTasks);
        }

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

                //new Thread(() =>
                //{
                //    while (true)
                //    {
                //        LogStats();
                //        Thread.Sleep(200);
                //    }
                //})
                //{ IsBackground = true }.Start();

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
                        if (TasksExecuting >= maxAsyncTasks)
                            RespondWithServiceUnavailable(context);
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

        private static void RespondWithServiceUnavailable(HttpListenerContext context)
        {
            context.Response.StatusCode = 503;
            context.Response.Close();
        }

        private static void HandleContextAsync(HttpListenerContext listenerContext)
        {
            Interlocked.Increment(ref TasksExecuting);
            Task.Run(() => HandleContext(listenerContext));
        }

        private static void HandleContext(HttpListenerContext listenerContext)
        {
            //var s = Stopwatch.StartNew();
            using (var result = RequestParser.ParseRequest(listenerContext.Request))
            {
                var response = listenerContext.Response;
                //Logger.Info(
                //    $"{listenerContext.Request.HttpMethod} {listenerContext.Request.RawUrl} - {result.ResultCode}");
                if (result.NoErrors)
                    SendImageResponse(response, result.Apply());
                response.StatusCode = (int) result.ResultCode;
                response.Close();
            }

            //s.Stop();
            //LastTaskSpeedMS = s.ElapsedMilliseconds;

            Interlocked.Decrement(ref TasksExecuting);
        }

        private const string ContentTypeImagePng = "image/png";

        private static void SendImageResponse(HttpListenerResponse ctxResponse, Image bitmap)
        {
            using (bitmap)
            {
                ctxResponse.ContentType = ContentTypeImagePng;
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