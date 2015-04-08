// SINK

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
                    receiver.Bind("tcp://10.151.12.9:5558");

                    //wait for start of batch (see Ventilator.csproj Program.cs)
                    byte[] startOfBatchTrigger = receiver.Receive();
                    Console.WriteLine("Seen start of batch");

                    //Start our clock now
                    Stopwatch watch = new Stopwatch();
                    watch.Start();

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

                    Console.WriteLine(BitConverter.ToInt32(startOfBatchTrigger, 0));

                    int i = 0;
                    for (i = 0; i < BitConverter.ToInt32(startOfBatchTrigger, 0); i++)
                    {
                        //string name = receiver.ReceiveString();

                        byte[] input;
                        input = receiver.Receive();

                        int l = input.Length;

                        byte[] num = new byte[4];
                        System.Buffer.BlockCopy(input, l - 4, num, 0, 4);

                        int len = BitConverter.ToInt32(num, 0);

                        Console.WriteLine(len);

                        byte[] sendI = new byte[l - len - 4];
                        System.Buffer.BlockCopy(input, 0, sendI, 0, l - len - 4);

                        byte[] nameb = new byte[len];
                        System.Buffer.BlockCopy(input, l - len - 4, nameb, 0, len);

                        String name = GetString(nameb);

                        Image bw = byteArrayToImage(sendI);

                        SaveFile(bw, name);
                        Console.WriteLine("Saved " + name );
                    }

                    watch.Stop();
                    //Calculate and report duration of batch
                    Console.WriteLine();
                    Console.WriteLine("Total elapsed time {0} msec", watch.ElapsedMilliseconds);

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

        static void SaveFile(Image input, string name)
        {
            Bitmap bmp = new Bitmap(input);
            if (name.Contains(".png"))
            {
                bmp.Save("C:\\cygwin64\\home\\user\\coba\\SISTER\\BW3\\BW-" + name, ImageFormat.Png);
            }
            else
                bmp.Save("C:\\cygwin64\\home\\user\\coba\\SISTER\\BW3\\BW-" + name, ImageFormat.Jpeg);
            //bmp.Save("C:\\cygwin64\\home\\user\\coba\\SISTER\\BW3\\Image-" + name + ".png", ImageFormat.Png);
        }

        static string GetString(byte[] bytes)
        {
            char[] chars = new char[bytes.Length / sizeof(char)];
            System.Buffer.BlockCopy(bytes, 0, chars, 0, bytes.Length);
            return new string(chars);
        }

    }
}
