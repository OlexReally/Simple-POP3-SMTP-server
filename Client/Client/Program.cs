/*
Client program to work with SMTP/POP3 servers

Author Kutaev O. V.
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.IO;

namespace Client
{
    class Program
    {
        static void Main(string[] args)
        {
            string server = "127.0.0.1";
            int port;
            Connect connect;
            Console.WriteLine("Hi! Choose server with \"1\" or \"2\":\n\t1. SMTP-server\n\t2. POP3-server");
            int choose = 0;
            try
            {
                choose = Convert.ToInt32(Console.ReadLine());
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception in: \n\n{0}", ex);
            }
            switch (choose)
            {
                case 2:
                    port = 110;//POP3
                    break;
                case 1:
                    port = 25;//SMTP
                    break;
                default:
                    Console.WriteLine("Incorrect value.\n\nExiting...");
                    return;
            }

            connect = new Connect(server, port);
            connect.HandShake();
            string s = "";
            while(true)
            {
                Console.Write("\\> ");
                s = Console.ReadLine();
                connect.sendToServer(s);
                if (s == "QUIT")
                {
                    Console.WriteLine("Press enter to exit . . .");
                    Console.ReadKey();
                    Environment.Exit(0);
                }
                if (s == "DATA")
                {
                    string text = "";
                    while (true)
                    {
                        Console.Write("\\> ");
                        s = Console.ReadLine();
                        if (s == ".")
                        {
                            break;
                        }
                        text += s + "\n";
                    }
                    connect.sendToServer(text);
                }
            }
        }
    }

    class Connect
    {
        private string server;
        private int port;
        private TcpClient client;
        private Byte[] bytes = new Byte[1024];
        public Connect(string server, int port)
        {
            this.server = server;
            this.port = port;
        }
        public void Disconnect()
        {
            client.Close();
        }
        public void HandShake()
        {
            try
            {
                client = new TcpClient(server, port);
                Console.WriteLine("Connection success.\n");

                MessageHello();
            }
            catch(Exception ex)
            {
                Console.WriteLine("Error with:\n{0}", ex);
                Console.WriteLine("\n\nPress enter to exit . . .");
                Console.ReadKey();
                Environment.Exit(0);
            }
        }

        private void MessageHello()
        {
            string hello = "HELLO";

            Byte[] data = System.Text.Encoding.ASCII.GetBytes(hello);

            NetworkStream stream = client.GetStream();
            stream.Write(data, 0, data.Length);

            ServerAnswer(stream);
        }

        private void ServerAnswer(NetworkStream stream)
        {
            int i = stream.Read(bytes, 0, bytes.Length);
            string Data = System.Text.Encoding.ASCII.GetString(bytes, 0, i);
            Console.WriteLine("\t{0}", Data);
        }

        public void sendToServer(string message)
        {
            Byte[] data = System.Text.Encoding.ASCII.GetBytes(message);

            NetworkStream stream = client.GetStream();
            stream.Write(data, 0, data.Length);

            ServerAnswer(stream);
        }        
    }


}
