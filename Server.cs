using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading;

namespace GameServer
{
    class Server
    {
        public static int MaxPlayers { get; private set; }
        public static int _clientsConnected { get; set; }
        public static int Port { get; private set; }
        public static Dictionary<int, Client> clients = new Dictionary<int, Client>();
        public delegate void PacketHandler(int _fromClient, Packet _packet);
        public static Dictionary<int, PacketHandler> packetHandlers;

        private static TcpListener tcpListener;
        private static UdpClient udpListener;
        private static Int32 startTime;
        private static Int32 currentTime;
        private static bool shouldPing = true;
        private static bool shouldStopPinging = false;
        public static void Start(int _maxPlayers, int _port)
        {
            startTime = (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1, 0, 0, 0))).TotalSeconds;

            MaxPlayers = _maxPlayers;
            _clientsConnected = 0;
            Port = _port;

            Console.WriteLine("Starting server...");
            InitializeServerData();

            tcpListener = new TcpListener(IPAddress.Any, Port);
            tcpListener.Start();
            tcpListener.BeginAcceptTcpClient(TCPConnectCallback, null);

            udpListener = new UdpClient(Port);
            udpListener.BeginReceive(UDPReceiveCallback, null);

            Console.WriteLine($"[{DateTime.Now.TimeOfDay}] Server started on port {Port}.");
            

            Thread pingThread = new Thread(Ping);
            pingThread.Start();
            
            Thread checkConnectionThread = new Thread(CheckConnections);
            checkConnectionThread.Start();
        }


        public static void CheckConnections()
        {
            Thread.Sleep(15000);
            clients = null;
            for (int i = 0; i < clients.Count; i++) {
                if (clients[i].lastPingIDRecieved <= (ServerSend.pingID - 3))
                {
                    clients[i].Disconnect();
                }
            }
            CheckConnections();
        }

        public static void Ping() {
            currentTime = (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1, 0, 0, 0))).TotalSeconds;
            if (currentTime - startTime != 0)
            {
                if (shouldPing)
                {
                    if (((currentTime - startTime)) % 300 == 0)
                    {
                        shouldPing = false;
                    }
                }
                else
                {
                    if (((currentTime - startTime) - 60) % 300 == 0)
                    {
                        shouldPing = true;
                    }
                }
            }
            
            if ((currentTime - startTime) % 5 == 0 && shouldPing)
            {
                ServerSend.Ping();
            }
            
            Thread.Sleep(990);
            Ping();
        }

        private static void TCPConnectCallback(IAsyncResult _result)
        {
            TcpClient _client = tcpListener.EndAcceptTcpClient(_result);
            tcpListener.BeginAcceptTcpClient(TCPConnectCallback, null);
            Console.WriteLine($"[{DateTime.Now.TimeOfDay}] Incoming connection from {_client.Client.RemoteEndPoint}...");
            _clientsConnected += 1;

            for (int i = 1; i <= MaxPlayers; i++)
            {
                if (clients[i].tcp.socket == null)
                {
                    clients[i].tcp.Connect(_client);
                    return;
                }
            }
            //TODO:: Disable new clients when game started
            Console.WriteLine($"[{DateTime.Now.TimeOfDay}] {_client.Client.RemoteEndPoint} failed to connect: Server full!");
        }

        private static void UDPReceiveCallback(IAsyncResult _result)
        {
            try
            {
                IPEndPoint _clientEndPoint = new IPEndPoint(IPAddress.Any, 0);
                byte[] _data = udpListener.EndReceive(_result, ref _clientEndPoint);
                udpListener.BeginReceive(UDPReceiveCallback, null);

                if (_data.Length < 4)
                {
                    return;
                }

                using (Packet _packet = new Packet(_data))
                {
                    int _clientId = _packet.ReadInt();

                    if (_clientId == 0)
                    {
                        return;
                    }

                    if (clients[_clientId].udp.endPoint == null)
                    {
                        clients[_clientId].udp.Connect(_clientEndPoint);
                        return;
                    }

                    if (clients[_clientId].udp.endPoint.ToString() == _clientEndPoint.ToString())
                    {
                        clients[_clientId].udp.HandleData(_packet);
                    }
                }
            }
            catch (Exception _ex)
            {
                Console.WriteLine($"[{DateTime.Now.TimeOfDay}] Error receiving UDP data: {_ex}");
            }
        }

        public static void SendUDPData(IPEndPoint _clientEndPoint, Packet _packet)
        {
            try
            {
                if (_clientEndPoint != null)
                {
                    udpListener.BeginSend(_packet.ToArray(), _packet.Length(), _clientEndPoint, null, null);
                }
            }
            catch (Exception _ex)
            {
                Console.WriteLine($"[{DateTime.Now.TimeOfDay}] Error sending data to {_clientEndPoint} via UDP: {_ex}");
            }
        }

        private static void InitializeServerData()
        {
            for (int i = 1; i <= MaxPlayers; i++)
            {
                clients.Add(i, new Client(i));
            }

            packetHandlers = new Dictionary<int, PacketHandler>()
            {
                { (int)ClientPackets.welcomeReceived, ServerHandle.WelcomeReceived },
                { (int)ClientPackets.pong, ServerHandle.Pong },
                // { (int)ClientPackets.playerMovement, ServerHandle.PlayerMovement },
            };
            Console.WriteLine("Initialized packets.");
        }
    }
}