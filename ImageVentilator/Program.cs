// VENTILATOR

using System;
using NetMQ;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;

namespace ImageVentilator
{
    class Program
    {
        static void Main(string[] args)
        {
            // Task Ventilator
            // Binds PUSH socket to tcp://localhost:5557
            // Sends batch of tasks to workers via that socket
            Console.WriteLine("====== VENTILATOR ======");

            using (NetMQContext ctx = NetMQContext.Create())
            {
                //socket to send messages on
                using (var sender = ctx.CreatePushSocket())
                {
                    sender.Bind("tcp://*:5557");

                    using (var sink = ctx.CreatePushSocket())
                    {
                        sink.Connect("tcp://10.151.12.9:5558");

                        Console.WriteLine("Press enter when worker are ready");
                        Console.ReadLine();

                        //the first message it "0" and signals start of batch
                        //see the Sink.csproj Program.cs file for where this is used
                        //Console.WriteLine("Sending start of batch to Sink");
                        //sink.Send("0");

                        //Console.WriteLine("Sending tasks to workers");

                        ////initialise random number generator
                        //Random rand = new Random(0);

                        ////expected costs in Ms
                        //int totalMs = 0;

                        ////send 100 tasks (workload for tasks, is just some random sleep time that
                        ////the workers can perform, in real life each work would do more than sleep
                        //for (int taskNumber = 0; taskNumber < 100; taskNumber++)
                        //{
                        //    //Random workload from 1 to 100 msec
                        //    int workload = rand.Next(0, 100);
                        //    totalMs += workload;
                        //    Console.WriteLine("Workload : {0}", workload);
                        //    sender.Send(workload.ToString());
                        //}
                        //Console.WriteLine("Total expected cost : {0} msec", totalMs);

                        int count = 0;
                        string name;
                        byte[] nameb;
                        int len;
                        byte[] image;
                        byte[] num;
                        byte[] sendM;

                        string[] filePaths = Directory.GetFiles("C:\\cygwin64\\home\\user\\coba\\SISTER\\", "*.jpg");
                        count += filePaths.Length;
                        
                        filePaths = Directory.GetFiles("C:\\cygwin64\\home\\user\\coba\\SISTER\\", "*.png");
                        count += filePaths.Length;

                        Console.WriteLine(count);

                        Console.WriteLine("Sending start of batch to Sink");
                        sink.Send(BitConverter.GetBytes(count));

                        Console.WriteLine("Sending tasks to workers");

                        int i = 0;
                        for (i = 0; i < filePaths.Length; i++)
                        {
                            name = filePaths[i].Replace("C:\\cygwin64\\home\\user\\coba\\SISTER\\","");
                            nameb = GetBytes(name);
                            len = nameb.Length;
                            num = BitConverter.GetBytes(len);

                            image = ReadImage(filePaths[i]);

                            sendM = new byte[image.Length + len + 4];

                            System.Buffer.BlockCopy(image, 0, sendM, 0, image.Length);
                            System.Buffer.BlockCopy(nameb, 0, sendM, image.Length, len);
                            System.Buffer.BlockCopy(num, 0, sendM, image.Length + len, 4);
                            
                            sender.Send(sendM);
                        }

                        filePaths = Directory.GetFiles("C:\\cygwin64\\home\\user\\coba\\SISTER\\", "*.jpg");

                        for (i = 0; i < filePaths.Length; i++)
                        {
                            name = filePaths[i].Replace("C:\\cygwin64\\home\\user\\coba\\SISTER\\", "");
                            nameb = GetBytes(name);
                            len = nameb.Length;
                            num = BitConverter.GetBytes(len);

                            image = ReadImage(filePaths[i]);

                            sendM = new byte[image.Length + len + 4];

                            System.Buffer.BlockCopy(image, 0, sendM, 0, image.Length);
                            System.Buffer.BlockCopy(nameb, 0, sendM, image.Length, len);
                            System.Buffer.BlockCopy(num, 0, sendM, image.Length + len, 4);

                            sender.Send(sendM);
                        }

                        //Image output = byteArrayToImage(image);
                        //SaveFile(output);


                        Console.WriteLine("Press Enter to quit");
                        Console.ReadLine();
                    }
                }
            }
        }

        static byte[] ReadImage(string file)
        {
            // Load file meta data with FileInfo

            FileInfo fileInfo = new FileInfo(file);

            // The byte[] to save the data in
            byte[] data = new byte[fileInfo.Length];

            // Load a filestream and put its content into the byte[]
            using (FileStream fs = fileInfo.OpenRead())
            {
                fs.Read(data, 0, data.Length);
            }

            // Delete the temporary file
            //fileInfo.Delete();

            // Post byte[] to database

            return data;
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

        static byte[] GetBytes(string str)
        {
            byte[] bytes = new byte[str.Length * sizeof(char)];
            System.Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);
            return bytes;
        }
    }
}
