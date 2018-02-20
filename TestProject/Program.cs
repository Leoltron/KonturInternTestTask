using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TestProject
{
    class Program
    {
        static void Main(string[] args)
        {
            Task.WaitAll(StartTask());
        }

        private static Task[] StartTask()
        {
            var a = new Task[10];
            for (int j = 0; j < 10; j++)
            {
                 a[j] = Tasker(j);
            }

            return a;
        }

        private static Task Tasker(int i)
        {
            return Task.Run(() =>
            {
                Console.WriteLine("Started " + i);
                for (int k = 0; k < 10; k++)
                {
                    Thread.Sleep(1000);
                    Console.WriteLine(i + ":" + k);
                }

                Console.WriteLine("Finish " + i);
            });
        }
    }
}
