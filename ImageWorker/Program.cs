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
                    receiver.Connect("tcp://10.151.12.9:5557");

                    //socket to send messages on
                    using (var sender = ctx.CreatePushSocket())
                    {
                        sender.Connect("tcp://10.151.12.9:5558");

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

                            //sender.Send(receiver.Receive());
                            byte[] input = receiver.Receive();
                            int i = input.Length;
                            //Console.WriteLine(i);

                            byte[] num = new byte[4];
                            System.Buffer.BlockCopy(input, i - 4, num, 0, 4);

                            int len = BitConverter.ToInt32(num, 0);

                            Console.WriteLine(len);

                            byte[] nameb = new byte[len];
                            System.Buffer.BlockCopy(input, i - len - 4, nameb, 0, len);

                            byte[] sendI = new byte[i - len - 4];
                            System.Buffer.BlockCopy(input, 0, sendI, 0, i - len - 4);

                            //Console.WriteLine(sendI.Length);

                            byte[] output = ToBW(sendI);

                            byte[] sendM = new byte[output.Length + len + 4];
                            System.Buffer.BlockCopy(output, 0, sendM, 0, output.Length);
                            System.Buffer.BlockCopy(nameb, 0, sendM, output.Length, len);
                            System.Buffer.BlockCopy(num, 0, sendM, output.Length + len, 4);

                            sender.Send(sendM);

                            Console.WriteLine("Sending to Sink");

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
