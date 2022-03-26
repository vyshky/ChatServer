using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Newtonsoft.Json;

namespace ChatServer
{
    internal class Program
    {
        static TcpListener listener;
        static List<TcpClient> clients;
        static List<string> Names;
        static void Main(string[] args)
        {
            clients = new List<TcpClient>();
            Names = new List<string>();
            WaitClient();
        }

        static void ClientListener(object client)
        {
            TcpClient _client = (TcpClient)client;

            while (true)
            {
                try
                {
                    NetworkStream listener = _client.GetStream();
                    byte[] buffer = new byte[255];
                    listener.Read(buffer, 0, 255);

                    string message = Encoding.Default.GetString(buffer);
                    message = message.Remove(message.IndexOf("\0"));
                    Console.WriteLine(message);
                    if (message.IndexOf("<NAME>") == 0)
                    {
                        int index = clients.FindIndex((x) => x == client);
                        Names[index] = message.Remove(0, 6);
                    }
                    else
                    {
                        foreach (var cl in clients)
                        {
                            NetworkStream stream = cl.GetStream();
                            stream.Write(buffer, 0, 255);
                            stream.Flush();
                        }
                    }
                }
                catch
                {
                    clients.Remove(_client);
                    break;
                }
            }
        }

        static void SendList()
        {
            string message = JsonConvert.SerializeObject(Names);
            byte[] buffer = Encoding.Default.GetBytes("<LIST>" + message);

            foreach (var cl in clients)
            {
                NetworkStream stream = cl.GetStream();
                stream.Write(buffer, 0, message.Length + 6);
                stream.Flush();
            }
        }

        static void WaitClient()
        {
            TcpClient client;
            listener = new TcpListener(IPAddress.Any, 8888);
            listener.Start();
            while (true)
            {
                client = listener.AcceptTcpClient();  // работает как транзикация
                clients.Add(client);
                Names.Add("NoName");
                Console.WriteLine("У нас новый посетитель!");
                Thread thread = new Thread(new ParameterizedThreadStart(ClientListener));
                thread.Start(client);
            }
        }
    }
}
