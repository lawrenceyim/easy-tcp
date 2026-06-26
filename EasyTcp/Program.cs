// using EasyTcp;
//
// string ip = "10.0.0.180";
// int port = 8080;
// JsonListener listener = new JsonListener(ip, port);
// listener.ClientConnected += s => { Console.WriteLine($"Client connected: {s}"); };
// listener.JsonReceived += s => { Console.WriteLine($"Received: {s}"); };
// listener.ClientDisconnected += s => { Console.WriteLine($"Client disconnected: {s}"); };
// listener.Start();