using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace GameServer
{
    class ServerHandle
    {
        private static string _packetData;
        
        public static void WelcomeReceived(int _fromClient, Packet _packet)
        {
            int _clientIdCheck = _packet.ReadInt();
            _packetData = _packet.ReadString();
            // Server._clientsConnected += 1;
            Console.WriteLine($"[{DateTime.Now.TimeOfDay}] {Server.clients[_fromClient].tcp.socket.Client.RemoteEndPoint} connected successfully as player ID {_clientIdCheck}.");
            Console.WriteLine($"[{DateTime.Now.TimeOfDay}] {_packetData}");
            ServerSend.PlayerCountUpdate(Server._clientsConnected.ToString());

            // using the method 
            char[] seperator = {':'};
            String[] packetData = _packetData.Split(seperator);
            if (packetData[0] == "userid") {
                Console.WriteLine($"[{DateTime.Now.TimeOfDay}] Recieving userid:" + packetData[1]);
                Server.clients[_fromClient].userid = packetData[1];
            }
            
            // ServerSend.PlayerCountUpdate("500");
            // Server.CurrentPlayers = _fromClient;
            // if (_fromClient != _clientIdCheck)
            // {
            //     Console.WriteLine($"[{DateTime.Now.TimeOfDay}] Player \"{_packetData}\" (ID: {_fromClient}) has assumed the wrong client ID ({_clientIdCheck})!");
            // }
            // TODO: send player into game 213.202.238.136
        }

        public static void Pong(int _fromClient, Packet _packet)
        {
            try
            {
                _packetData = _packet.ReadString();
                char[] seperator = {':'};
                String[] packetData = _packetData.Split(seperator);

                if (packetData[0] == "pong")
                {
                    Server.clients[_fromClient].lastPingIDRecieved = Int32.Parse(packetData[1]);

                    if (Server.clients[_fromClient].lastPingIDRecieved <= (ServerSend.pingID - 3)
                    ) // missed 3 pings.. disconnect!
                    {
                        Console.WriteLine(
                            $"[{DateTime.Now.TimeOfDay}] {Server.clients[_fromClient].tcp.socket.Client.RemoteEndPoint} was removed from game.");
                        if (Server.clients[_fromClient].isConnected)
                        {
                            Server.clients[_fromClient].isConnected = false;
                            Server.clients[_fromClient].tcp.Disconnect();
                            return;
                        }
                    }
                }

                Server.clients[_fromClient].isConnected = true;
                // Console.WriteLine($"[{DateTime.Now.TimeOfDay}] {Server.clients[_fromClient].tcp.socket.Client.RemoteEndPoint} Pong!");
            }
            catch (Exception e)
            {
                Console.WriteLine($"[{DateTime.Now.TimeOfDay}] Someone tried sending data while disconnected from our side");
            }
            
        }
    }
}