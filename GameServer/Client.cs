using System;
using System.Net;
using System.Net.Sockets;
using System.Numerics;

namespace GameServer
{
    class Client
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public static int defaultBufferSize = 4096;

        public int id;
        public Player player;
        public TCP tcp;
        public UDP udp;

        public Client(int clientID)
        {
            id = clientID;
            tcp = new TCP(clientID);
            udp = new UDP(clientID);
        }

        public class TCP
        {
            public TcpClient socket;
            private readonly int id;

            private NetworkStream stream;
            private Packet receivedData;
            private byte[] receiveBuffer;

            public TCP(int id)
            {
                this.id = id;
            }

            public void Connect(TcpClient client)
            {
                socket = client;
                socket.SendBufferSize = defaultBufferSize;
                socket.ReceiveBufferSize = defaultBufferSize;

                stream = socket.GetStream();
                receivedData = new Packet();
                receiveBuffer = new byte[defaultBufferSize];

                stream.BeginRead(receiveBuffer, 0, defaultBufferSize, ReceiveCallback, null);

                ServerSend.Welcome(id, "Welcome to the Server!");
            }

            public void SendData(Packet packet)
            {
                try
                {
                    if (socket != null)
                    {
                        stream.BeginWrite(packet.ToArray(), 0, packet.Length(), null, null);
                    }
                }
                catch (Exception ex)
                {
                    logger.Error($"Error sending data to player {id} via TCP: {ex}");
                }
            }

            private void ReceiveCallback(IAsyncResult result)
            {
                try
                {
                    if (stream != null)
                    {
                        int byteLength = stream.EndRead(result);

                        if (byteLength <= 0)
                        {
                            Server.clients[id].Disconnect();

                            return;
                        }

                        byte[] data = new byte[byteLength];
                        Array.Copy(receiveBuffer, data, byteLength);

                        receivedData.Reset(HandleData(data));
                        stream.BeginRead(receiveBuffer, 0, defaultBufferSize, ReceiveCallback, null);
                    }
                }
                catch (Exception ex)
                {
                    logger.Error($"Error receiving TCP data: {ex}");

                    Server.clients[id].Disconnect();
                }
            }

            private bool HandleData(byte[] data)
            {
                int packetLength = 0;

                receivedData.SetBytes(data);

                if (receivedData.UnreadLength() >= 4)
                {
                    packetLength = receivedData.ReadInt();
                    if (packetLength <= 0)
                    {
                        return true;
                    }
                }

                while (packetLength > 0 && packetLength <= receivedData.UnreadLength())
                {
                    byte[] packetBytes = receivedData.ReadBytes(packetLength);
                    ThreadManager.ExecuteOnMainThread(() =>
                    {
                        using (Packet packet = new Packet(packetBytes))
                        {
                            int packetID = packet.ReadInt();
                            Server.packetHandlers[packetID](id, packet);
                        }
                    });

                    packetLength = 0;

                    if (receivedData.UnreadLength() >= 4)
                    {
                        packetLength = receivedData.ReadInt();

                        if (packetLength <= 0)
                        {
                            return true;
                        }
                    }
                }

                if (packetLength <= 1)
                {
                    return true;
                }

                return false;
            }

            public void Disconnect()
            {
                socket.Close();

                stream = null;
                receivedData = null;
                receiveBuffer = null;
                socket = null;
            }
        }

        public class UDP
        {
            public int id;
            public IPEndPoint endPoint;

            public UDP(int clientID)
            {
                id = clientID;
            }

            public void Connect(IPEndPoint endPoint)
            {
                this.endPoint = endPoint;
            }

            public void SendData(Packet packet)
            {
                Server.SendUDPData(endPoint, packet);
            }

            public void HandleData(Packet packet)
            {
                int packetLength = packet.ReadInt();
                byte[] packetBytes = packet.ReadBytes(packetLength);

                ThreadManager.ExecuteOnMainThread(() =>
                {
                    using (Packet newPacket = new Packet(packetBytes))
                    {
                        int packetID = newPacket.ReadInt();
                        Server.packetHandlers[packetID](id, newPacket);
                    }
                });
            }

            public void Disconnect()
            {
                endPoint = null;
            }
        }

        public void SendIntoGame(string playerName)
        {
            player = new Player(id, playerName, new Vector3(0, 0, 0));

            foreach (Client client in Server.clients.Values)
            {
                if (client.player != null)
                {
                    if (client.id != id)
                    {
                        ServerSend.SpawnPlayer(id, client.player);
                    }
                }
            }

            foreach (Client client in Server.clients.Values)
            {
                if (client.player != null)
                {
                    ServerSend.SpawnPlayer(client.id, player);
                }
            }
        }

        public void Disconnect()
        {
            logger.Info($"{tcp.socket.Client.RemoteEndPoint} has disconnected.");

            ThreadManager.ExecuteOnMainThread(() =>
            {
                player = null;
            });

            tcp.Disconnect();
            udp.Disconnect();
            
            ServerSend.PlayerDisconnected(id);
        }
    }
}
