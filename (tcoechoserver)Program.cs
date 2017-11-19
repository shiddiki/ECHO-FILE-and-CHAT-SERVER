using System;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Text;
using System.Threading;

namespace TcpEchoServer
{
    public class TcpEchoServer
    {
        private Socket socket;
        public static int cno = 1,tot=1,c=0;
        public static string[] sms = new string[1000];
        public static TcpClient[] clients = new TcpClient[20];
        public static void Main()
        {
            Console.Title = "Server";
            Console.WriteLine("Starting echo server...");

            int port = 1234;
            
            TcpListener listener = new TcpListener(IPAddress.Loopback, port);
            listener.Start();
            Console.WriteLine("Address: "+ IPAddress.Loopback.ToString()+"      Port: "+port);
            int opt = 0;

            Console.WriteLine("Select one of the following options...\n 1. Iterative Server\n 2. Concurrent Server");

            string c="";
            c = Console.ReadLine();
            opt = Convert.ToInt16(c);
            

            while(true)
            {
                
                c=c;
                
                 clients[cno] = listener.AcceptTcpClient();
                TcpClient clt=clients[cno];
            Console.WriteLine("Starting session for client "+cno+" from address " +
                                         IPAddress.Parse(((IPEndPoint)listener.LocalEndpoint).Address.ToString()) +
                                            " on port number " + ((IPEndPoint)listener.LocalEndpoint).Port.ToString());

                if(opt==2)
                ThreadPool.QueueUserWorkItem(ThreadProc, clt);       //for iterative remove thread(this) and do all threadproc work here
                else
            { 
                    NetworkStream stream = clt.GetStream();
        StreamWriter writer = new StreamWriter(stream, Encoding.ASCII) { AutoFlush = true };
        StreamReader reader = new StreamReader(stream, Encoding.ASCII);

        while (true)
        {
            string inputLine = "";
            while (inputLine != null)
            {
                inputLine = reader.ReadLine();
                writer.WriteLine("Echoing string: " + inputLine);
                Console.WriteLine("Echoing string: " + inputLine);
                    
            }
            Console.WriteLine("Server saw disconnect from client.");
                
        }
                 
                }
             

            cno++;
            
            }
            
            
        }
        private static void ThreadProc(object obj)
        {
            
            var client = (TcpClient)obj;
            
            Console.WriteLine(client.ToString());
            NetworkStream stream = client.GetStream();
            StreamWriter writer = new StreamWriter(stream, Encoding.ASCII) { AutoFlush = true };
            StreamReader reader = new StreamReader(stream, Encoding.ASCII);
            c++;
            int clno = c;
            tot++;

            // Do your work here
            while (true)
            {
                string inputLine = "";
                try
                {
                    while (inputLine != null)
                    {
                        inputLine = reader.ReadLine();
                        writer.WriteLine( inputLine);
                        Console.WriteLine("Received from Client " + clno + ": " + inputLine);
                        TcpClient clt = clients[1];
                        //ThreadPool.QueueUserWorkItem(ThreadProc1, clt); 
                        

                    }
                }
                catch
                {
                    client.Close();
                    Console.WriteLine("Ending session for client "+clno);
                    tot--;
                    if (tot == 1)
                        Console.WriteLine("Waiting for connections....");
                    return;
                }

            }
             



        }

        
    }
}