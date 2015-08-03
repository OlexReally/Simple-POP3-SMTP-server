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

namespace POP3
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

                Int32 port = 110;//POP3 port
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

            POP3 pop3 = new POP3();

            int i;

            while ((i = stream.Read(bytes, 0, bytes.Length)) != 0)
            {
                data = System.Text.Encoding.ASCII.GetString(bytes, 0, i);

                split = data.Split(new Char[] { ' ' });

                switch (split[0])
                {
                    case "HELLO":                        
                        msg = System.Text.Encoding.ASCII.GetBytes(pop3.GenerateAnswerHello());
                        break;

                    case "QUIT":
                        db.QuitDelete(pop3.User);
                        msg = System.Text.Encoding.ASCII.GetBytes(pop3.GenerateAnswerQuit());
                        stream.Write(msg, 0, msg.Length);
                        client.Close();
                        return;

                    case "USER":
                        if (db.User(split[1]))
                        {
                            pop3.User = split[1];
                            msg = System.Text.Encoding.ASCII.GetBytes(pop3.GenerateAnswerOK("User accepted"));
                        }
                        else
                            msg = System.Text.Encoding.ASCII.GetBytes(pop3.GenerateAnswerERR("User missing"));
                        break;

                    case "PASS":
                        if (pop3.User != "")
                        {
                            if(db.Pass(pop3.User,split[1]))
                            {
                                pop3.SetPass(split[1]);
                                msg = System.Text.Encoding.ASCII.GetBytes(pop3.GenerateAnswerOK("PASS accepted"));
                            }
                            else
                                msg = System.Text.Encoding.ASCII.GetBytes(pop3.GenerateAnswerERR("Incorrect password"));
                        }
                        else
                            msg = System.Text.Encoding.ASCII.GetBytes(pop3.GenerateAnswerERR("Incorrect user. Use command USER first"));
                        break;

                    case "STAT":
                        if(pop3.User != "")
                        {
                            msg = System.Text.Encoding.ASCII.GetBytes(pop3.GenerateAnswerOK(db.Select(pop3.User)));
                        }
                        else
                            msg = System.Text.Encoding.ASCII.GetBytes(pop3.GenerateAnswerERR("Incorrect user. Use command USER first"));
                        break;

                    case "LIST":
                        if (pop3.User != "" && split.GetLength(0) == 1)//LIST without arg
                        {
                            msg = System.Text.Encoding.ASCII.GetBytes(pop3.GenerateAnswerOK(db.SelectList(pop3.User)));
                        }
                        else if (pop3.User != "" && split.GetLength(0) == 2)//LIST *arg*
                        {
                            msg = System.Text.Encoding.ASCII.GetBytes(pop3.GenerateAnswerOK(db.SelectList(pop3.User, split[1])));
                        }
                        else if (pop3.User != "" && split.GetLength(0) > 2)//incorrect numbers of arg
                        {
                            msg = System.Text.Encoding.ASCII.GetBytes(pop3.GenerateAnswerERR("Incorrect argument of command LIST"));
                        }
                        else
                            msg = System.Text.Encoding.ASCII.GetBytes(pop3.GenerateAnswerERR("Incorrect user. Use command USER first"));
                        break;

                    case "RETR":
                        if (pop3.User != "" && split.GetLength(0) == 1)//RETR without arg
                        {
                            msg = System.Text.Encoding.ASCII.GetBytes(pop3.GenerateAnswerERR("Need arguments"));
                        }
                        else if (pop3.User != "" && split.GetLength(0) == 2)//RETR *arg*
                        {
                            msg = System.Text.Encoding.ASCII.GetBytes(pop3.GenerateAnswerOK(db.SelectRetr(pop3.User, split[1])));
                        }
                        else if (pop3.User != "" && split.GetLength(0) > 2)//incorrect numbers of arg
                        {
                            msg = System.Text.Encoding.ASCII.GetBytes(pop3.GenerateAnswerERR("So much arguments"));
                        }
                        else
                            msg = System.Text.Encoding.ASCII.GetBytes(pop3.GenerateAnswerERR("Incorrect user. Use command USER first"));
                        break;

                    case "RSET":
                        if (pop3.User != "")//RSET
                        {
                            msg = System.Text.Encoding.ASCII.GetBytes(pop3.GenerateAnswerOK(db.UpdateRset(pop3.User)));
                        }
                        else
                            msg = System.Text.Encoding.ASCII.GetBytes(pop3.GenerateAnswerERR("Incorrect user. Use command USER first"));
                        break;

                    case "DELE":
                        if (pop3.User != "" && split.GetLength(0) == 1)//DELE without arg -> arror
                        {
                            msg = System.Text.Encoding.ASCII.GetBytes(pop3.GenerateAnswerERR("Need arguments"));
                        }
                        else if (pop3.User != "" && split.GetLength(0) == 2)//DELE *arg*
                        {
                            int k = 0;
                            if (int.TryParse(split[1], out k))
                            {
                                msg = System.Text.Encoding.ASCII.GetBytes(pop3.GenerateAnswerOK(db.UpdateDele(pop3.User, split[1])));
                            }
                            else
                                msg = System.Text.Encoding.ASCII.GetBytes(pop3.GenerateAnswerERR("Incorrect argument"));
                        }
                        else if (pop3.User != "" && split.GetLength(0) > 2)//incorrect numbers of arg
                        {
                            msg = System.Text.Encoding.ASCII.GetBytes(pop3.GenerateAnswerERR("So much arguments"));
                        }
                        else
                            msg = System.Text.Encoding.ASCII.GetBytes(pop3.GenerateAnswerERR("Incorrect user. Use command USER first"));
                        break;

                    default:
                        msg = System.Text.Encoding.ASCII.GetBytes(pop3.GenerateAnswerERR("Incorrect command"));
                        break;
                }
                stream.Write(msg, 0, msg.Length);
            }
            client.Close();
        }        
    }

    class POP3
    {
        private bool isLogin = false; //user login check
        private bool isPass = false;  //user pass check
        private string userName = "";
        private string userPass = "";
        private string email = "";
        private const string ok = "+OK";
        private const string error = "-ERR";
        private const string hello = "Hello from POP3-server";
        private const string quit = "POP3 server signing off";
        private const string user = "User accepted";

        public POP3()//constructor
        {

        }

        public bool Login()
        {
            return true;
        }

        public bool Password()
        {
            return true;
        }

        public bool Stat()
        {
            return true;
        }

        public string GenerateAnswerOK(string s1)
        {
            return ok + " " + s1;
        }
        public string GenerateAnswerERR(string s1)
        {
            return error + " " + s1;
        }
        public string GenerateAnswerHello()
        {
            return ok + " " + hello;
        }
        public string GenerateAnswerQuit()
        {
            return ok + " " + quit;
        }

        public void SetPass(string pass)
        {
            if (this.userName != "")
                this.userPass = pass;
        }

        public string User//set or get User Name
        {
            get
            {
                return this.userName;
            }
            set
            {
                this.userName = value;
            }
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

        private int GetNumbers(string user)//повертає к-сть повідомлень в скриньці user'a
        {
            string query = "SELECT * FROM email WHERE login = '"
                + user + "' AND `delete` = 0";
            int iter = 0;

            if (this.OpenConnection() == true)
            {

                MySqlCommand cmd = new MySqlCommand(query, connection);
                MySqlDataReader dataReader = cmd.ExecuteReader();
                while (dataReader.Read())
                {
                    iter++;
                }
                this.CloseConnection();
                return iter;
            }
            else
                return -1;
        }

        public string SelectRetr(string user, string s)//RETR
        {
            string query = "SELECT text FROM email WHERE login = '"
                + user + "' AND `delete` = 0";
            string str = "";
            int i = 1;

            if (this.OpenConnection() == true)
            {

                MySqlCommand cmd = new MySqlCommand(query, connection);
                MySqlDataReader dataReader = cmd.ExecuteReader();
                while (dataReader.Read())
                {
                    if (i == Convert.ToInt32(s))
                    {
                        str += dataReader["text"];
                        break;
                    }
                    i++;
                }

                str = Encoding.ASCII.GetBytes(str).Length.ToString() + " octets " + "\n\t" + str + "\n\t.";
                this.CloseConnection();
                return str;
            }
            else
                return "Err";
        }

        public string UpdateDele(string user, string s)//DELE
        {
            string query = "SELECT * FROM email WHERE login = '"
                + user + "' AND `delete` = 0";
            string str_time = "";
            string str_text = "";
            int count = GetNumbers(user);

            if (this.OpenConnection() == true)
            {
                if (Convert.ToInt32(s) > count)//connection already open -- crash
                    return "Cannot DELE this msg";
                count = 1;
                MySqlCommand cmd = new MySqlCommand(query, connection);
                MySqlDataReader dataReader = cmd.ExecuteReader();
                while (dataReader.Read())
                {
                    if (count == Convert.ToInt32(s))
                    {
                        str_time += dataReader["date"];
                        str_text += dataReader["text"];
                        break;
                    }
                    count++;
                }
                this.CloseConnection();
            }
            else
                return "Err";

            if (this.OpenConnection() == true)
            {
                string update = "UPDATE email SET `delete` = 1 WHERE text = '"
                    + str_text + "' AND login = '"
                    + user + "' AND date = '"
                    + HackMethod(str_time) + "'";

                MySqlCommand cmd = new MySqlCommand(update, connection);
                cmd.ExecuteNonQuery();

                this.CloseConnection();

                return "message " + s + " deleted";
            }
            else
                return "Err";
        }

        public void QuitDelete(string user)//DELETE before QUIT
        {
            if (user == "")
                return;

            string query = "DELETE FROM email WHERE login = '"
                + user + "' AND `delete` = 1";
            if (this.OpenConnection() == true)
            {
                MySqlCommand cmd = new MySqlCommand(query, connection);
                cmd.ExecuteNonQuery();

                this.CloseConnection();
            }
        }
        public string UpdateRset(string user)//RSET
        {
            string query = "UPDATE email SET `delete` = 0 WHERE `delete` = 1 AND login = '"
                + user + "';";
            if (this.OpenConnection() == true)
            {
                MySqlCommand cmd = new MySqlCommand(query, connection);
                cmd.ExecuteNonQuery();

                this.CloseConnection();

                return Select(user);
            }
            else
                return "Err";
        }

        public string SelectList(string user, string s)//LIST з аргументом
        {
            string query = "SELECT text FROM email WHERE login = '"
                + user + "' AND `delete` = 0";
            string str = "";
            int i = 1;

            if (this.OpenConnection() == true)
            {

                MySqlCommand cmd = new MySqlCommand(query, connection);
                MySqlDataReader dataReader = cmd.ExecuteReader();
                while (dataReader.Read())
                {
                    if (i == Convert.ToInt32(s))
                    {
                        str += dataReader["text"];
                        break;
                    }
                    i++;
                }

                str = Convert.ToInt32(s) + " " + Encoding.ASCII.GetBytes(str).Length.ToString();
                this.CloseConnection();
                return str;
            }
            else
                return "Err";
        }

        public string SelectList(string user)//LIST без аргументів
        {
            string query = "SELECT text FROM email WHERE login = '"
                + user + "' AND `delete` = 0";
            string str = "";

            int iter = GetNumbers(user);
            string[] str1 = new string[iter];
            for (int i = 1; i <= iter; i++)
            {
                str1[i - 1] = "";
            }

            if (this.OpenConnection() == true)
            {

                MySqlCommand cmd = new MySqlCommand(query, connection);
                MySqlDataReader dataReader = cmd.ExecuteReader();
                int ii = 1;
                while (dataReader.Read())
                {
                    str1[ii - 1] += dataReader["text"];
                    str += str1[ii - 1];
                    ii++;
                }

                str = iter.ToString() + " msg in box "
                    + "(" + Encoding.ASCII.GetBytes(str).Length.ToString() + " octets" + ")";
                for (int i = 1; i <= iter; i++)
                {
                    str += "\n\t" + i.ToString() + " " + Encoding.ASCII.GetBytes(str1[i - 1]).Length.ToString();
                }
                str += "\n\t.";
                this.CloseConnection();
                return str;
            }
            else
                return "Err";
        }
        public string Select(string user)//STAT
        {
            string query = "SELECT text FROM email WHERE login = '" + user + "' AND `delete` = 0";
            string str = "";
            int i = 0;//counter

            if (this.OpenConnection() == true)
            {

                MySqlCommand cmd = new MySqlCommand(query, connection);
                MySqlDataReader dataReader = cmd.ExecuteReader();
                while (dataReader.Read())
                {
                    str += dataReader["text"];
                    i++;
                }
                str = i.ToString() + " " + Encoding.ASCII.GetBytes(str).Length.ToString();
                this.CloseConnection();
                return str;
            }
            else
                return "Err";
        }
        
        public bool User(string user)//check for user
        {
            string query = "SELECT * FROM users WHERE login ='" + user + "'";
            int i = 0;
            if (this.OpenConnection() == true)
            {

                MySqlCommand cmd = new MySqlCommand(query, connection);
                MySqlDataReader dataReader = cmd.ExecuteReader();
                while (dataReader.Read())
                {
                    i++;
                }
                this.CloseConnection();
                if (i == 1)
                {
                    return true;
                }
                else
                    return false;
            }
            return false;
        }

        public bool Pass(string user, string pass)//check for user/pass
        {
            string query = "SELECT * FROM users WHERE login ='" 
                + user + "' and password = '"
                + pass + "'";
            int i = 0;
            if (this.OpenConnection() == true)
            {

                MySqlCommand cmd = new MySqlCommand(query, connection);
                MySqlDataReader dataReader = cmd.ExecuteReader();
                while (dataReader.Read())
                {
                    i++;
                }
                this.CloseConnection();
                if (i == 1)
                {
                    return true;
                }
                else
                    return false;
            }
            return false;
        }

        private string HackMethod(string str)//хакова конвертація DATETIME
        {
            string[] split = str.Split(new Char[] { '.', ' ' });
            return split[2] + "-" + split[1] + "-" + split[0] + " " + split[3];
        }
    }
}