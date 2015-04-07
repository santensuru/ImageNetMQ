// WORKER

using System;
using System.Threading;
using NetMQ;
using System.Drawing;
using System.IO;
using System.Drawing.Imaging;

namespace ImageWorker
{
    class Program
    {
        static void Main(string[] args)
        {
            // Task Worker
            // Connects PULL socket to tcp://localhost:5557
            // collects workload for socket from Ventilator via that socket
            // Connects PUSH socket to tcp://localhost:5558
            // Sends results to Sink via that socket
            Console.WriteLine("====== WORKER ======");

            using (NetMQContext ctx = NetMQContext.Create())
            {
                //socket to receive messages on
                using (var receiver = ctx.CreatePullSocket())
                {
                    receiver.Connect("tcp://localhost:5557");

                    //socket to send messages on
                    using (var sender = ctx.CreatePushSocket())
                    {
                        sender.Connect("tcp://localhost:5558");

                        //process tasks forever
                        while (true)
                        {
                            ////workload from the vetilator is a simple delay
                            ////to simulate some work being done, see
                            ////Ventilator.csproj Proram.cs for the workload sent
                            ////In real life some more meaningful work would be done
                            //string workload = receiver.ReceiveString();

                            ////simulate some work being done
                            //Thread.Sleep(int.Parse(workload));

                            ////send results to sink, sink just needs to know worker
                            ////is done, message content is not important, just the precence of
                            ////a message means worker is done.
                            ////See Sink.csproj Proram.cs

                            byte[] output = ToBW(receiver.Receive());

                            Console.WriteLine("Sending to Sink");
                            sender.Send(output);

                            //Image bw = byteArrayToImage(output);

                            //SaveFile(bw);
                        }
                    }

                }
            }
        }

        static byte[] ToBW(byte[] input)
        {
            MemoryStream ms = new MemoryStream(input);
            Image returnImage = Image.FromStream(ms);
            Bitmap image = new Bitmap(returnImage);

            // Do some processing
            for (int x = 0; x < image.Width; x++)
            {
                for (int y = 0; y < image.Height; y++)
                {
                    Color pixelColor = image.GetPixel(x, y);
                    int color = (pixelColor.R + pixelColor.G + pixelColor.B) / 3;
                    Color newColor = Color.FromArgb(color, color, color);
                    image.SetPixel(x, y, newColor);
                }
            }
            Bitmap output = image;
            byte[] outp = ImageToByte(output);

            return outp;
        }

        public static byte[] ImageToByte(Image img)
        {
            ImageConverter converter = new ImageConverter();
            return (byte[])converter.ConvertTo(img, typeof(byte[]));
        }

        //static Image byteArrayToImage(byte[] byteArrayIn)
        //{
        //    MemoryStream ms = new MemoryStream(byteArrayIn);
        //    Image returnImage = Image.FromStream(ms);
        //    return returnImage;
        //}

        //static void SaveFile(Image input)
        //{
        //    Bitmap bmp = new Bitmap(input);
        //    bmp.Save("BW.jpg", ImageFormat.Jpeg);
        //}
    }
}
