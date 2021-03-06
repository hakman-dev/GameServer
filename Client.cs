using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Net;
using System.Net.Sockets;

// using System.Numerics;

namespace GameServer {
    class Client {
        public static int dataBufferSize = 4096;

        public int id;

        // public Player player;
        public TCP tcp;
        public UDP udp;
        public string userid;
        public bool hasHadAConnection = false;
        public int lastPingIDRecieved = 0;
        public bool isConnected;

        public Client(int _clientId) {
            id = _clientId;
            tcp = new TCP(id);
            udp = new UDP(id);
        }

        public class TCP {
            public TcpClient socket;

            private readonly int id;
            private NetworkStream stream;
            private Packet receivedData;
            private byte[] receiveBuffer;


            public TCP(int _id) {
                id = _id;
            }

            public void Connect(TcpClient _socket)
            {
                socket = _socket;
                socket.ReceiveBufferSize = dataBufferSize;
                socket.SendBufferSize = dataBufferSize;

                stream = socket.GetStream();

                receivedData = new Packet();
                receiveBuffer = new byte[dataBufferSize];

                stream.BeginRead(receiveBuffer, 0, dataBufferSize, ReceiveCallback, null);

                ServerSend.Welcome(id, "Welcome to the server!");
                ServerSend.PriceUpdate(id, "10000"); // price pool of 100.00
            }

            public void SendData(Packet _packet) {
                try {
                    if (socket != null) {
                        stream.BeginWrite(_packet.ToArray(), 0, _packet.Length(), null, null);
                    }
                } catch (Exception _ex) {
                    Console.WriteLine($"[{DateTime.Now.TimeOfDay}] Error sending data to player {id} via TCP: {_ex}");
                }
            }

            private void ReceiveCallback(IAsyncResult _result) {
                try {
                    int _byteLength = stream.EndRead(_result);
                    if (_byteLength <= 0) {
                        Console.WriteLine("Bytelength was 0");
                        Server.clients[id].Disconnect();
                        return;
                    }

                    byte[] _data = new byte[_byteLength];
                    Array.Copy(receiveBuffer, _data, _byteLength);

                    receivedData.Reset(HandleData(_data));
                    stream.BeginRead(receiveBuffer, 0, dataBufferSize, ReceiveCallback, null);
                } catch (Exception _ex) {
                    Console.WriteLine($"[{DateTime.Now.TimeOfDay}] Error receiving TCP data: {_ex}");
                    try
                    {
                        Server.clients[id].Disconnect();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"[{DateTime.Now.TimeOfDay}] Old packet received");
                    }
                }
            }

            private bool HandleData(byte[] _data) {
                int _packetLength = 0;

                receivedData.SetBytes(_data);

                if (receivedData.UnreadLength() >= 4) {
                    _packetLength = receivedData.ReadInt();
                    if (_packetLength <= 0) {
                        return true;
                    }
                }

                while (_packetLength > 0 && _packetLength <= receivedData.UnreadLength()) {
                    byte[] _packetBytes = receivedData.ReadBytes(_packetLength);
                    ThreadManager.ExecuteOnMainThread(() => {
                        using (Packet _packet = new Packet(_packetBytes)) {
                            int _packetId = _packet.ReadInt();
                            Server.packetHandlers[_packetId](id, _packet);
                        }
                    });

                    _packetLength = 0;
                    if (receivedData.UnreadLength() >= 4) {
                        _packetLength = receivedData.ReadInt();
                        if (_packetLength <= 0) {
                            return true;
                        }
                    }
                }

                if (_packetLength <= 1) {
                    return true;
                }

                return false;
            }

            public void Disconnect() {
                try
                {
                    socket.Close();
                }
                catch (Exception e)
                {
                    Console.WriteLine($"[{DateTime.Now.TimeOfDay}] Someone tried sending data while disconnected from our side");
                }
                stream = null;
                receivedData = null;
                receiveBuffer = null;
                socket = null;
            }
        }

        public class UDP {
            public IPEndPoint endPoint;

            private int id;

            public UDP(int _id) {
                id = _id;
            }

            public void Connect(IPEndPoint _endPoint) {
                endPoint = _endPoint;
            }

            public void SendData(Packet _packet) {
                Server.SendUDPData(endPoint, _packet);
            }

            public void HandleData(Packet _packetData) {
                int _packetLength = _packetData.ReadInt();
                byte[] _packetBytes = _packetData.ReadBytes(_packetLength);

                ThreadManager.ExecuteOnMainThread(() => {
                    using (Packet _packet = new Packet(_packetBytes)) {
                        int _packetId = _packet.ReadInt();
                        Server.packetHandlers[_packetId](id, _packet);
                    }
                });
            }

            public void Disconnect() {
                endPoint = null;
            }
        }

        public void SendIntoGame(string _playerName) {
            // player = new Player(id, _playerName, new Vector3(0, 0, 0));

            // foreach (Client _client in Server.clients.Values)
            // {
            //     if (_client.player != null)
            //     {
            //         if (_client.id != id)
            //         {
            //             Console.WriteLine(" Spawn playerttt ");
            //             // ServerSend.SpawnPlayer(id, _client.player);
            //         }
            //     }
            // }
            //
            // foreach (Client _client in Server.clients.Values)
            // {
            //     if (_client.player != null) {
            //         Console.WriteLine(" Spawn player ");
            //         // ServerSend.SpawnPlayer(_client.id, player);
            //     }
            // }
        }

        public void Disconnect() {
            Console.WriteLine($"[{DateTime.Now.TimeOfDay}] {tcp.socket.Client.RemoteEndPoint} has disconnected.");
            Server._clientsConnected -= 1;
            tcp.Disconnect();
            udp.Disconnect();
            isConnected = false;
            
            ServerSend.PlayerCountUpdate(Server._clientsConnected.ToString());

            String clientIDs = "";
            if (Server._clientsConnected < 5) {
                // get all clients that are connected
                for (int i = 1; i <= Server.MaxPlayers; i++) {
                    // Console.WriteLine(Server.clients[i].tcp.socket);
                    if (Server.clients[i].tcp.socket != null) {
                        clientIDs = clientIDs + "," + Server.clients[i].userid;
                    }
                }
                //output array to console
                Console.WriteLine($"[{DateTime.Now.TimeOfDay}] Current in game USER IDs: {clientIDs}");
            }
        }
    }
}