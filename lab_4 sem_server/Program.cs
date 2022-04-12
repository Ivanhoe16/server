using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Data.SQLite;

namespace lab_4_sem_server
{
    class Program
    {
        private static int port = 8085;
        private static string host = "127.0.0.1";
        private static SQLiteConnection connection = new SQLiteConnection("Data Source=phones.db");

        static void Main(string[] args)
        {
            connection.Open();
            IPEndPoint point = new IPEndPoint(IPAddress.Parse(host), port);
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                socket.Bind(point);
                socket.Listen(10);
                Console.WriteLine("Сервер запущен на порту " + port);
                while (true)
                {
                    Socket handler = socket.Accept();
                    //получение сообщений
                    StringBuilder builder = new StringBuilder();
                    int bytes = 0;
                    byte[] data = new byte[256];
                    while(handler.Available > 0)
                    {
                        bytes = handler.Receive(data);
                        builder.Append(Encoding.Unicode.GetString(data, 0, bytes));
                    }
                    string sampler = builder.ToString();
                    if (sampler.Equals("get"))
                    {
                        data = Encoding.Unicode.GetBytes(getFromDB());
                        handler.Send(data);
                        Console.WriteLine(getFromDB());
                    }
                    else
                        addToDB(sampler.Split(';'));
                    handler.Shutdown(SocketShutdown.Both);
                    handler.Close();
                }
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
        private static void addToDB(string [] data)
        {
            SQLiteCommand command = new SQLiteCommand("insert into phones values('" + data[0] + "','" + data[1] + "')", connection);
            command.ExecuteNonQuery();
        }
        private static string getFromDB()
        {
            string data = "";
            SQLiteCommand command = new SQLiteCommand("select * from phones", connection);
            using (SQLiteDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                    data += reader["name"].ToString() + ";" + reader["phone"].ToString() + ";";
            }
            return data;
        }
    }
}
