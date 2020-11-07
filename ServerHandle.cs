using System;
using System.Collections.Generic;
using System.Text;

namespace GameServer
{
    class ServerHandle
    {
        public static void WelcomeReceived(int _fromClient, Packet _packet)
        {
            int _clientIdCheck = _packet.ReadInt();
            string _packetData = _packet.ReadString();
            Server._clientsConnected += 1;
            Console.WriteLine($"[{DateTime.Now.TimeOfDay}] {Server.clients[_fromClient].tcp.socket.Client.RemoteEndPoint} connected successfully as player ID {_clientIdCheck}.");
            Console.WriteLine($"[{DateTime.Now.TimeOfDay}] {_packetData}");
            ServerSend.PlayerCountUpdate(Server._clientsConnected.ToString());
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
    }
}