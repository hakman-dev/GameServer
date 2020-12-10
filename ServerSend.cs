using System;
using System.Collections.Generic;
using System.Text;

namespace GameServer
{
    class ServerSend
    {
        public static int pingID = 0;
        private static int playerCount = 0;
        
        private static void SendTCPData(int _toClient, Packet _packet)
        {
            _packet.WriteLength();
            Server.clients[_toClient].tcp.SendData(_packet);
        }

        private static void SendTCPDataToAll(Packet _packet)
        {
            _packet.WriteLength();
            for (int i = 1; i <= Server.MaxPlayers; i++)
            {
                Server.clients[i].tcp.SendData(_packet);
            }
        }
        private static void SendTCPDataToAll(int _exceptClient, Packet _packet)
        {
            _packet.WriteLength();
            for (int i = 1; i <= Server.MaxPlayers; i++)
            {
                Console.WriteLine("Sending to :" + i);
                if (i != _exceptClient)
                {
                    Server.clients[i].tcp.SendData(_packet);
                }
            }
        }

        public static void Welcome(int _toClient, string _msg)
        {
            using (Packet _packet = new Packet((int)ServerPackets.welcome))
            {
                _packet.Write(_msg);
                _packet.Write(_toClient);
                SendTCPData(_toClient, _packet);
            }
        }
        
        public static void Ping()
        {
            Console.WriteLine($"[{DateTime.Now.TimeOfDay}] Ping ID" + pingID);
            
            using (Packet _packet = new Packet((int)ServerPackets.ping))
            {
                _packet.Write("ping" + pingID);
                SendTCPDataToAll(_packet);
            }

            pingID++;
        }
        
        public static void PlayerCountUpdate(string _msg)
        {
            Console.WriteLine($"[{DateTime.Now.TimeOfDay}] Players:" + Server._clientsConnected);
            using (Packet _packet = new Packet((int)ServerPackets.PlayerCountUpdate))
            {
                _packet.Write("" + Server._clientsConnected);
                SendTCPDataToAll(_packet);
            }
        }
        
        public static void PriceUpdate(int _toClient, string _msg)
        {
            using (Packet _packet = new Packet((int)ServerPackets.PriceUpdate))
            {
                _packet.Write(_msg);
                _packet.Write(_toClient);
                SendTCPData(_toClient, _packet);
            }
        }
    }
}