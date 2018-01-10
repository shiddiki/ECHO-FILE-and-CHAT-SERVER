using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace clientForm
{
    public partial class Form1 : Form
    {

        public static int port = 12346, match = 0, sent = 0, updated = 0, connected = 0;
        Int32 portt = 13000;
        public static string a = "", b = "", mysms = "", temp = "";
        public static string received = "", prev = "", cur_user = "", prev_user = "", Show = "";
        public static TcpClient client = null;
        public static Thread t;
        public static string[] clients = new string[100];
        public static Form1 fm;

        public Form1()
        {

            InitializeComponent();
            send.Enabled = false;


        }

        private void connect_Click(object sender, EventArgs e)
        {
            if (match == 0)
            {

                a = "127.0.0.1";// Console.ReadLine();

                b = "1234";// Console.ReadLine();
                portt = Convert.ToInt32(b);

                IPAddress localAddr = IPAddress.Parse(a);

                try
                {
                    client = new TcpClient(localAddr.ToString(), portt);
                    match = 1;
                    mysms = "";
                    MessageBox.Show("Connection established successfully!");
                    connected = 1;// connected
                    mysms += "000";
                    ThreadPool.QueueUserWorkItem(ThreadProc, client);
                    ThreadPool.QueueUserWorkItem(ThreadProc2, client);
                    //Thread lnk = new Thread(() => link(this.richTextBox1));
                    //lnk.Start();
                    send.Enabled = true;

                    connect.Text = "Disconnect";

                }
                catch
                {

                    match = 0;
                    client = null;
                    MessageBox.Show("Unable to establish connection!");
                    connected = 0;
                }
            }
            else if (match == 1)
            {
                mysms = "";
                mysms += "111 end-of-session";
                sent = 0;
                Thread.Sleep(1000);
                connect.Text = "Connect";
                match = 0;
                richTextBox1.Clear();
                richTextBox3.Clear();
                richTextBox2.Clear();
                connected = 0;
            }
        }

        public void ThreadProc(object obj)      //chatbox
        {
            var client1 = (TcpClient)obj;
            NetworkStream stream1 = client1.GetStream();
            StreamReader reader1 = new StreamReader(stream1);
            StreamWriter writer1 = new StreamWriter(stream1) { AutoFlush = true };
            int j, i;

            while (true && match == 1)
            {


                received = "";
                if (stream1.DataAvailable)
                {
                    Byte[] bytes = new Byte[1024];
                    stream1.Read(bytes, 0, 1024);
                    received += Encoding.UTF8.GetString(bytes).TrimEnd('\0');
                    updated = 0;

                    temp = "";
                    j = 0;
                    i = -1;
                    if (received[0] == '*') //mark of holding info
                    {
                        for (i = 1; i < received.Length; i++)
                        {
                            clients[j] = "";
                            if (received[i] == '#')
                                break;
                            if (received[i] != '-')
                                temp += received[i];
                            else
                            {
                                clients[j] += temp;
                                cur_user += clients[j++];
                                temp = "";
                            }


                        }
                    }
                    Show = "";
                    for (int k = i + 1; k < received.Length; k++)
                        Show += received[k];

                    if (cur_user != prev_user)
                    {
                        this.Invoke((MethodInvoker)delegate()
                        {
                            richTextBox3.Clear();
                        });
                        for (int ii = 0; ii < j; ii++)
                        {
                            this.Invoke((MethodInvoker)delegate()
                            {
                                richTextBox3.AppendText(Environment.NewLine + clients[ii]);
                            });

                        }
                        prev_user = "";
                        prev_user += cur_user;
                    }


                    this.Invoke((MethodInvoker)delegate()
                    {
                        richTextBox1.AppendText(Environment.NewLine + Show);
                    });

                    //while (updated == 0)
                    {
                        Thread.Sleep(500);
                    }

                }
                //AppendRichTextBox(received);


            }


        }

        public void AppendRichTextBox(string value)
        {
            if (InvokeRequired)
            {
                this.Invoke(new Action<string>(AppendRichTextBox), new object[] { value });
                return;
            }
            richTextBox1.Text += value;
            this.Refresh();

        }
       

        private static void ThreadProc2(object obj)// sms
        {
            var client2 = (TcpClient)obj;
            NetworkStream stream2 = client2.GetStream();
            StreamReader reader2 = new StreamReader(stream2);
            StreamWriter writer2 = new StreamWriter(stream2) { AutoFlush = true };
            sent = 0;
            while (true)
            {
                if (sent == 0 && mysms.Length > 0)
                {
                    string lineToSend = "";
                    lineToSend += mysms;
                    byte[] bytes = new byte[1024];
                    bytes = Encoding.ASCII.GetBytes(lineToSend);
                    try
                    {
                        stream2.Write(bytes, 0, bytes.Length);
                        sent = 1;
                    }
                    catch
                    {
                        MessageBox.Show("sorry.. please resend ur message");

                    }
                }
                if (connected == 0)
                    return;


            }


        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void send_Click(object sender, EventArgs e)
        {
            NetworkStream stream = client.GetStream();
            StreamReader reader = new StreamReader(stream);
            StreamWriter writer = new StreamWriter(stream) { AutoFlush = true };

            if (mysms != richTextBox2.Text)
            {
                mysms = "";
                mysms += richTextBox2.Text;
                sent = 0;
                richTextBox2.Clear();
            }


        }


    }
}
