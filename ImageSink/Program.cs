﻿// SINK

using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using NetMQ;
using System.Drawing;
using System.IO;
using System.Drawing.Imaging;

namespace ImageSink
{
    class Program
    {
        static void Main(string[] args)
        {
            // Task Sink
            // Bindd PULL socket to tcp://localhost:5558
            // Collects results from workers via that socket
            Console.WriteLine("====== SINK ======");

            using (NetMQContext ctx = NetMQContext.Create())
            {
                //socket to receive messages on
                using (var receiver = ctx.CreatePullSocket())
                {
                    receiver.Bind("tcp://localhost:5558");

                    //wait for start of batch (see Ventilator.csproj Program.cs)
                    var startOfBatchTrigger = receiver.ReceiveString();
                    Console.WriteLine("Seen start of batch");

                    ////Start our clock now
                    //Stopwatch watch = new Stopwatch();
                    //watch.Start();

                    //for (int taskNumber = 0; taskNumber < 100; taskNumber++)
                    //{
                    //    var workerDoneTrigger = receiver.ReceiveString();
                    //    if (taskNumber % 10 == 0)
                    //    {
                    //        Console.Write(":");
                    //    }
                    //    else
                    //    {
                    //        Console.Write(".");
                    //    }
                    //}
                    //watch.Stop();
                    ////Calculate and report duration of batch
                    //Console.WriteLine();
                    //Console.WriteLine("Total elapsed time {0} msec", watch.ElapsedMilliseconds);

                    //Console.WriteLine(receiver.Receive());

                    byte[] input;
                    input = receiver.Receive();

                    Image bw = byteArrayToImage(input);

                    SaveFile(bw);
                    Console.WriteLine("Saved");

                    Console.ReadLine();
                }
            }
        }

        static Image byteArrayToImage(byte[] byteArrayIn)
        {
            MemoryStream ms = new MemoryStream(byteArrayIn);
            Image returnImage = Image.FromStream(ms);
            return returnImage;
        }

        static void SaveFile(Image input)
        {
            Bitmap bmp = new Bitmap(input); 
            bmp.Save("BW.jpg", ImageFormat.Jpeg);
        }
    }
}
