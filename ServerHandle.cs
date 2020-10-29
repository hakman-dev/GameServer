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
            string _username = _packet.ReadString();

            Console.WriteLine($"{Server.clients[_fromClient].tcp.socket.Client.RemoteEndPoint} connected successfully. Total players {_clientIdCheck}.");
            ServerSend.PlayerCountUpdate("500");
            // Server.CurrentPlayers = _fromClient;
            if (_fromClient != _clientIdCheck)
            {
                Console.WriteLine($"Player \"{_username}\" (ID: {_fromClient}) has assumed the wrong client ID ({_clientIdCheck})!");
            }
            // TODO: send player into game 213.202.238.136
        }
    }
}