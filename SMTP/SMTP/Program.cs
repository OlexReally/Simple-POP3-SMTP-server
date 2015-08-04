/*
SMTP server

Author Kutaev O. V.
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using MySql.Data.MySqlClient;
using System.IO;
using System.Net;
using System.Net.Sockets;

namespace SMTP
{
    class Program
    {
        static void Main(string[] args)
        {
            TcpListener server = null;
            try
            {
                int MaxThreadsCount = Environment.ProcessorCount * 4;
                ThreadPool.SetMaxThreads(MaxThreadsCount, MaxThreadsCount);
                ThreadPool.SetMinThreads(2, 2);

                Int32 port = 25;//SMTP port
                IPAddress localAddr = IPAddress.Parse("127.0.0.1");
                int counter = 0;
                server = new TcpListener(localAddr, port);

                server.Start();

                while (true)
                {
                    Console.Write("\n\tWaiting for a connection... ");

                    ThreadPool.QueueUserWorkItem(ConnectFunc, server.AcceptTcpClient());
                    counter++;
                    Console.Write("\nConnection №" + counter.ToString() + "!");
                }
            }
            catch (SocketException e)
            {
                Console.WriteLine("SocketException: {0}", e);
            }
            finally
            {
                server.Stop();
            }

            Console.WriteLine("\nHit enter to continue...");
            Console.Read();
        }

        static void ConnectFunc(object client_obj)
        {
            string[] split;
            byte[] msg;
            DBMySQL db = new DBMySQL();
            Byte[] bytes = new Byte[256];
            String data = null;

            TcpClient client = client_obj as TcpClient;

            data = null;

            NetworkStream stream = client.GetStream();

            SMTP smtp = new SMTP();

            int i;

            while ((i = stream.Read(bytes, 0, bytes.Length)) != 0)
            {
                data = System.Text.Encoding.ASCII.GetString(bytes, 0, i);

                //parse input data on words
                split = data.Split(new Char[] { ' ' });

                switch (split[0])//first word - command, second - argument
                {
                    case "QUIT":
                        msg = System.Text.Encoding.ASCII.GetBytes(smtp.GenerateAnswerQuit());
                        stream.Write(msg, 0, msg.Length);
                        client.Close();
                        return;

                    case "HELO":
                        msg = System.Text.Encoding.ASCII.GetBytes("250 domain name should be qualified");
                        break;

                    case "DATA":
                        if (smtp.User != "")
                        {
                            msg = System.Text.Encoding.ASCII.GetBytes("354 Enter mail, end with \".\" on a line by itself");
                            stream.Write(msg, 0, msg.Length);

                            i = stream.Read(bytes, 0, bytes.Length);
                            data = System.Text.Encoding.ASCII.GetString(bytes, 0, i);

                            db.InsertSendMail(smtp.User, data);

                            msg = System.Text.Encoding.ASCII.GetBytes("250 message accepted for delivery");
                        }
                        else
                        {
                            msg = System.Text.Encoding.ASCII.GetBytes("No deliver. Need \"RCPT TO:\" first");
                        }
                        break;

                    case "RCPT":
                        if(split[1] == "TO:")
                        {
                            if(split[2] != "")
                            {
                                string CHECK_RIGHT_USER_NAME = "";
                                string CHECK_RIGHT_EMAIL_ADRR = "";
                                CHECK_RIGHT_EMAIL_ADRR = split[2];
                                CHECK_RIGHT_USER_NAME = db.User(CHECK_RIGHT_EMAIL_ADRR);
                                if (CHECK_RIGHT_USER_NAME != "Err" && CHECK_RIGHT_USER_NAME != "")
                                {
                                    smtp.Receiver = CHECK_RIGHT_EMAIL_ADRR;
                                    smtp.User = CHECK_RIGHT_USER_NAME;
                                    msg = System.Text.Encoding.ASCII.GetBytes(smtp.GenerateAnswerOK(smtp.Receiver));
                                }
                                else if (smtp.User == "")
                                {
                                    msg = System.Text.Encoding.ASCII.GetBytes("550 " + split[2] + " unknown user account");
                                }
                                else
                                {
                                    smtp.User = "";
                                    msg = System.Text.Encoding.ASCII.GetBytes("Command not implemented");
                                }
                            }
                            else
                                msg = System.Text.Encoding.ASCII.GetBytes(smtp.GenerateAnswerErrArgument());
                        }
                        else
                            msg = System.Text.Encoding.ASCII.GetBytes(smtp.GenerateAnswerErrCommand());
                        break;

                    case "HELLO"://handshake
                        msg = System.Text.Encoding.ASCII.GetBytes(smtp.GenerateAnswerHello());
                        break;

                    default:
                        msg = System.Text.Encoding.ASCII.GetBytes(smtp.GenerateAnswerErrCommand());
                        break;
                }
                stream.Write(msg, 0, msg.Length);
            }
            client.Close();
        }
    }

    class SMTP
    {
        private string emailReceiver = "";//qwerty@qwe.rty
        private string userName = "";
        public bool isData = false;

        public string Receiver
        {
            get
            {
                return this.emailReceiver;
            }
            set
            {
                this.emailReceiver = value;
            }
        }
        public string User
        {
            get
            {
                return this.userName;
            }
            set
            {
                if (this.userName == "")
                    this.userName += value;
                else
                    this.userName += " " + value;
            }
        }
        public SMTP()
        {

        }
        public string GenerateAnswerOK(string s1)
        {
            return "250 " + s1 + " ok";
        }
        public string GenerateAnswerHello()
        {
            return "220 SMTP is glad to see you!";
        }
        public string GenerateAnswerQuit()
        {
            return "221 SMTP closing connection";
        }
        public string GenerateAnswerErrCommand()
        {
            return "500 Syntax error, command unrecognized";
        }
        public string GenerateAnswerErrArgument()
        {
            return "501 Syntax error in parameters or arguments";
        }
        public string GenerateAnswerErrUnkUser(string s1)
        {
            return "550 " + s1 + " unknown user account";
        }
    }

    class DBMySQL
    {
        public MySqlConnection connection;
        public string server;
        public string database;
        public string uid;
        public string password;

        public DBMySQL()
        {
            Initialize();
        }
        public void Initialize()
        {
            server = "localhost";
            database = "mail";
            uid = "root";
            password = "121212";
            string connectionString;
            connectionString = "SERVER=" + server + ";" + "DATABASE=" +
            database + ";" + "UID=" + uid + ";" + "PASSWORD=" + password + ";";

            connection = new MySqlConnection(connectionString);
        }
        public bool OpenConnection()
        {
            try
            {
                connection.Open();
                return true;
            }
            catch (MySqlException ex)
            {
                switch (ex.Number)
                {
                    case 0:
                        Console.Write("Cannot connect to server.  Contact administrator;\n");
                        break;

                    case 1045:
                        Console.Write("Invalid username/password, please try again;\n");
                        break;
                }
                return false;
            }
        }
        public bool CloseConnection()
        {
            try
            {
                connection.Close();
                return true;
            }
            catch (MySqlException ex)
            {
                Console.Write("Cannot close connection;\n", ex.Message);
                return false;
            }
        }

        public string User(string email)//check for user
        {
            string query = "SELECT login FROM users WHERE email ='" + email + "'";
            string user = "";
            if (this.OpenConnection() == true)
            {

                MySqlCommand cmd = new MySqlCommand(query, connection);
                MySqlDataReader dataReader = cmd.ExecuteReader();
                while (dataReader.Read())
                {
                    user += dataReader["login"];
                }
                this.CloseConnection();
                return user;
            }
            return "Err";
        }

        public void InsertSendMail(string user, string message)//send message, write in DB emil message
        {
            string[] userMessage = user.Split(new Char[] { ' ' });
            for (int i = 0; i < userMessage.GetLength(0); i++)
            {
                DateTime date1 = DateTime.Now;
                //INSERT INTO email(login, date, `delete`, `read`, text) VALUES ('olex', '2015-05-31 21:01:01', 0, 0, 'new text');
                string query = "INSERT INTO email (login, date, `delete`, `read`, text) VALUES ('"
                    + userMessage[i] + "', '"
                    + HackMethod(date1.ToString()) + "', 0, 0, '"
                    + message + "')";

                if (this.OpenConnection() == true)
                {

                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    MySqlDataReader dataReader = cmd.ExecuteReader();

                    this.CloseConnection();
                }
            }
        }

        //HACK
        private string HackMethod(string str)//хакова конвертація DATETIME
        {
            string[] split = str.Split(new Char[] { '.', ' ' });
            return split[2] + "-" + split[1] + "-" + split[0] + " " + split[3];
        }
    }
}