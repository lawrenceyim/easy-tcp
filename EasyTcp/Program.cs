using System.Text;
using EasyTcp;

string ip = "10.0.0.180";
int port = 8080;
JsonListener listener = new JsonListener(ip, port);
listener.JsonReceived += s => { Console.WriteLine($"Received: {s}"); };
listener.Start();