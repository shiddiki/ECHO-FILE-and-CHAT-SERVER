using System;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Text;
using System.Threading;
using System.Collections.Generic;
namespace TcpEchoServer
{
    public class TcpEchoServer
    {
        #region Variables
        private Socket socket;
        public static int cno = 1,tot=1,c=0,smsposition=1,update_done=0;
        public static string[] sms = new string[1000];
        public static TcpClient[] clients = new TcpClient[20];
        public static int[,] smsindex = new int[100, 100];
        public static int[] msgno = new int[100];       //who gets how many messages
        public static int[] current_users = new int[100];
        public static string user_info = "", prev_info="";
        public static  Dictionary<string,int> client_id=new Dictionary<string,int>();
        #endregion

        public static void Main()
        {
            Console.Title = "Server";
            Console.WriteLine("Starting CHAT server...");

            int port = 1234;
            
            TcpListener listener = new TcpListener(IPAddress.Loopback, port);
            listener.Start();
            Console.WriteLine("Address: "+ IPAddress.Loopback.ToString()+"      Port: "+port);

            

            while(true)
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

                ThreadPool.QueueUserWorkItem(ThreadProc, clt);
                cno++;
            
            }
            
            
        }
        private static void ThreadProc(object obj)
        {
            int initial_connection = 0; 
            var client = (TcpClient)obj;
            
            Console.WriteLine(client.ToString());
            NetworkStream networkStream = client.GetStream();
            StreamWriter writer2 = new StreamWriter(networkStream, Encoding.ASCII) { AutoFlush = true };
            
           
            c++;
            int clno = c;
            int j; //number of messages i have
            tot++;


            while (true)
            {
                string inputLine = "";
                try
                {
                    while (inputLine != null)
                    {

                        byte[] bytes = new byte[1024];
                        string data = "";

                        if (networkStream.DataAvailable)     //only when client writes something
                        {

                            msgno[clno]++;

                            networkStream.Read(bytes, 0, 1024);
                            data = Encoding.UTF8.GetString(bytes).TrimEnd('\0');

                            Console.WriteLine(data);

                            if (data.Contains("end-of-session"))
                            {
                                client.Close();

                                client_id.Remove("client " + clno.ToString());
                                Thread upd = new Thread(update);
                                upd.Start();

                                Console.WriteLine("Ending session for client " + clno);
                                tot--;
                                if (tot == 1)
                                {
                                    Console.WriteLine("Waiting for connections....");

                                }

                            }
                            int num = 0;
                            for (int i = 0; i <= 2; i++)
                            {

                                num = num * 10 + (Convert.ToInt16(data[i]) - 48);


                            }
                            if (num == 111 || num == 0)     //all
                            {
                                for (int i = 1; i < cno; i++)
                                {
                                    if (clno != i)
                                        smsindex[clno, i] = smsposition;   //clno send sms to i

                                    smsindex[clno, clno] = smsposition;
                                    if (num == 0)
                                        sms[smsposition] += " >>just joined";
                                    else
                                        sms[smsposition] += " >>just left";
                                    smsposition++;

                                    if (i != clno)
                                        msgno[i]++;

                                }

                            }
                            else
                            {
                                if (clno != num)
                                    smsindex[clno, num] = smsposition;   //clno send sms to num
                                int l = data.Length - 1;
                                smsindex[clno, clno] = smsposition;

                                //  string ye = data.Substring(3, l);
                                Console.WriteLine("length " + l.ToString() + " " + data);
                                sms[smsposition] += data.Substring(3);
                                smsposition++;
                                if (clno != num)
                                    msgno[num]++;

                            }

                        }

                        if (msgno[clno] > 0)
                        {
                            for (int i = 0; i < cno; i++)
                            {
                                byte[] bytes1 = new byte[1024];
                                if (smsindex[i, clno] != 0)         //which i send clno something
                                {
                                    j = smsindex[i, clno];
                                    data = "";

                                    if (update_done > 0)
                                    {


                                        data += user_info;
                                        update_done--;

                                    }
                                    if (update_done == 0 && prev_info != user_info)
                                    {
                                        prev_info = "";
                                        prev_info += user_info;
                                    }
                                    if (initial_connection == 0)
                                    {
                                        data += "Client " + clno.ToString();
                                        initial_connection = 1;
                                    }
                                    else
                                    {
                                        data += "Client " + i.ToString() + ": ";
                                        data += sms[j];
                                    }
                                    bytes1 = Encoding.ASCII.GetBytes(data);
                                    smsindex[i, clno] = 0;

                                    Console.WriteLine("Server sent message: {0}", data);
                                    networkStream.Write(bytes1, 0, bytes1.Length);
                                    msgno[clno]--;
                                    if (msgno[clno] < 0)
                                        msgno[clno] = 0;
                                }



                            }
                        }


                    }
                }
                catch (Exception ex)
                {
                    client.Close();
                    client_id.Remove("client " + clno.ToString());

                    Thread upd = new Thread(update);
                    upd.Start();
                    Console.WriteLine("Ending session for client " + clno + " error " + ex.ToString());

                    return;
                }

            }

        }

        public static void update()
        {

            user_info = "*";

            foreach (KeyValuePair<string, int> item in client_id)
            {
                user_info += item.Key + "-";
                update_done++;
            }
            user_info += "#";

            //Thread.Sleep(1000);

        }

        
    }
}