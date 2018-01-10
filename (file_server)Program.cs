using System;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Text;
using System.Threading;
using System.Collections.Generic;

namespace FILE_Server
{
    public class TcpEchoServer
    {
        #region Variables
        private Socket socket;
        public static int cno = 1, tot = 1, c = 0, smsposition = 1, update_done = 0, file_to_send = 0, down = 0;
        public static string[] sms = new string[1000];
        public static TcpClient[] clients = new TcpClient[20];
        public static int[,] smsindex = new int[100, 100];
        public static int[] msgno = new int[100];       //who gets how many messages
        public static int[] current_users = new int[100];
        public static string file_info = "", prev_info = "";
        public static Dictionary<string, int> client_id = new Dictionary<string, int>();
        public string SendingFilePath = string.Empty;
        private const int BufferSize = 1024;

        public static string[] fileNames = new string[100];
        public static string[] filePaths = new string[100];

        #endregion

        public static void Main()
        {
            Console.Title = "File Server";
            Console.WriteLine("Starting File server...");

            int port = 1234;

            TcpListener listener = new TcpListener(IPAddress.Loopback, port);
            listener.Start();
            Console.WriteLine("Address: " + IPAddress.Loopback.ToString() + "      Port: " + port);

            while (true)
            {

                c = c;

                clients[cno] = listener.AcceptTcpClient();
                TcpClient clt = clients[cno];
                Console.WriteLine("Starting session for client " + cno + " from address " +
                                             IPAddress.Parse(((IPEndPoint)listener.LocalEndpoint).Address.ToString()) +
                                                " on port number " + ((IPEndPoint)listener.LocalEndpoint).Port.ToString());

                client_id.Add("client " + cno.ToString(), cno);

                Thread upd = new Thread(update);
                upd.Start();


                ThreadPool.QueueUserWorkItem(ThreadProc, clt);       //send client what server have

                cno++;

            }


        }


        public static string Reverse(string s)
        {
            char[] charArray = s.ToCharArray();
            Array.Reverse(charArray);
            return new string(charArray);
        }


        private static void ThreadProc(object obj)          //sends all file names when a client connects
        {
            
            var client = (TcpClient)obj;

            Console.WriteLine(client.ToString());
            NetworkStream stream = client.GetStream();
            StreamWriter writer2 = new StreamWriter(stream, Encoding.ASCII) { AutoFlush = true };
            StreamReader reader = new StreamReader(stream, Encoding.ASCII);

            NetworkStream networkStream = client.GetStream();


            c++;
            int clno = c;
            int j; //number of messages i have
            tot++;

                string inputLine = "";
                try
                {

                    
                    byte[] bytes = new byte[1024];
                    string data = "";

                        prev_info = "";
                        prev_info += file_info;

                        Byte[] bytes1 = new Byte[2048];
                        bytes1 = Encoding.ASCII.GetBytes(file_info);
                        Console.WriteLine("Server sent message: {0}", file_info);
                        networkStream.Write(bytes1, 0, bytes1.Length);


                }
                catch (Exception ex)
                {
                    return;
                }


                while (true)
                {
                    string s = "";
                    if (stream.DataAvailable || down == 1)           //got a request to send file 
                    {
                        string SaveFileName = "";
                        Byte[] b = new Byte[1024];

                        if (down != 1)
                        {
                            stream.Read(b, 0, 1024);
                            s += Encoding.UTF8.GetString(b).TrimEnd('\0');
                        }

                        if (s.Contains("-rec-") || down == 1)
                        {
                            down = 1;
                            if (stream.DataAvailable)       //servers respond
                            {

                                byte[] RecData = new byte[BufferSize];
                                int RecBytes;
                                string fin = "";

                                stream.Read(b, 0, b.Length);
                                fin = Encoding.UTF8.GetString(b).TrimEnd('\0');

                                try
                                {
                                    //SaveFileName = "";
                                    SaveFileName += @"C:\Users\sid\Desktop\SERVER\" + fin;

                                    int totalrecbytes = 0;
                                    FileStream Fs = new FileStream(SaveFileName, FileMode.OpenOrCreate, FileAccess.Write);
                                    while ((RecBytes = stream.Read(RecData, 0, RecData.Length)) > 0)
                                    {

                                        Fs.Write(RecData, 0, RecBytes);
                                        totalrecbytes += RecBytes;



                                        fin = Encoding.UTF8.GetString(RecData).TrimEnd('\0');
                                        if (fin.Contains("-end-"))
                                            break;

                                    }
                                    Fs.Close();

                                    Console.WriteLine("New file downloaded");
                                    down = 0;


                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine(ex.ToString());
                                }

                            }

                        }
                        else
                        {

                            file_to_send = Convert.ToInt32(s);


                            byte[] SendingBuffer = null;
                            string M = "";
                            M += filePaths[file_to_send - 1];
                            try
                            {

                                Console.WriteLine("Starting to send file to Client " + clno.ToString());
                                FileStream Fs = new FileStream(M, FileMode.Open, FileAccess.Read);
                                int NoOfPackets = Convert.ToInt32(Math.Ceiling(Convert.ToDouble(Fs.Length) / Convert.ToDouble(BufferSize)));

                                int TotalLength = (int)Fs.Length, CurrentPacketLength;
                                for (int i = 0; i < NoOfPackets; i++)
                                {
                                    if (TotalLength > BufferSize)
                                    {
                                        CurrentPacketLength = BufferSize;
                                        TotalLength = TotalLength - CurrentPacketLength;
                                    }
                                    else
                                        CurrentPacketLength = TotalLength;
                                    SendingBuffer = new byte[CurrentPacketLength];
                                    Fs.Read(SendingBuffer, 0, CurrentPacketLength);
                                    stream.Write(SendingBuffer, 0, (int)SendingBuffer.Length);


                                }

                                Byte[] end = new Byte[1024];
                                end = Encoding.ASCII.GetBytes("-end-");
                                stream.Write(end, 0, end.Length);

                                Fs.Close();
                                Console.WriteLine("File transfer completed for Client " + clno.ToString());
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(ex.Message);
                            }
                        }

                    }

                }


        }

        public static void update()
        {

            Array.Clear(filePaths, 0, filePaths.Length);
            filePaths = Directory.GetFiles(@"C:\Users\sid\Desktop\SERVER");
            file_info = "";
            Array.Clear(fileNames, 0, fileNames.Length);
            int index = 0;
            string tmpmirror = "";
            foreach (string ind in filePaths)
            {
                tmpmirror = "";
                for (int i = ind.Length - 1; i >= 0; i--)
                {
                    if (ind[i] == '\\')
                        break;
                    tmpmirror += ind[i];
                }
                fileNames[index] += Reverse(tmpmirror);
                file_info += fileNames[index++] + "*";
            }

            Thread.Sleep(1000);

        }

        
    }
}