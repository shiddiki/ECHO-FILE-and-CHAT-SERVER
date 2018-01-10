using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace TcpEchoClient
{
    class TcpEchoClient
    {
        static void Main(string[] args)
        {
            Console.Title = "Client";
            Console.WriteLine("Starting echo client...");

            int port=12346,match=0;
            Int32 portt = 13000;
            string a = "",b="";
            TcpClient client = null;
            while (match == 0)
            {
                Console.WriteLine("Please enter the server address");
                a = Console.ReadLine();
                //a = "127.0.0.1";
                Console.WriteLine("Please enter the port number");
                b = Console.ReadLine();
                portt = Convert.ToInt32(b);

                IPAddress localAddr = IPAddress.Parse(a);

                try
                {
                    client = new TcpClient(localAddr.ToString(), portt);
                    match = 1;
                    Console.WriteLine("Connection established successfully!");
                }
                catch {

                    match = 0;
                    client = null;
                    Console.WriteLine("Unable to establish connection!");
                }
            }
            
        
            NetworkStream stream = client.GetStream();
            StreamReader reader = new StreamReader(stream);
            StreamWriter writer = new StreamWriter(stream) { AutoFlush = true };

            while (true)
            {
               Console.Write("Enter text to send: ");
                string lineToSend = ""; 
                lineToSend +=Console.ReadLine();
               
                    
                writer.WriteLine(lineToSend);
                if (lineToSend != "end-of-session")
                {
                    Console.WriteLine("Sending to server: " + lineToSend);
                    string lineReceived = reader.ReadLine();
                    Console.WriteLine("Server response: " + lineReceived);
                }
                else
                {
                    Console.WriteLine("Good Bye");
                    Thread.Sleep(1000);
                    return;
                
                }
            }
            client.Close();
        }
    }
}