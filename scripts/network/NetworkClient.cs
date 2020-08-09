using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using Godot;

public class NetworkClient : Node
{
    public static int defaultBufferSize = 4096;

    public string hostName;

    public int port = 20050;
    public int id = 0;
    public TCP tcp;
    public UDP udp;

    private bool isConnected = false;

    private delegate void PacketHandler(Packet packet);
    private static Dictionary<int, PacketHandler> packetHandlers;

    public void ConnectToServer(string hostName)
    {
        this.hostName = hostName;

        tcp = new TCP(this);
        udp = new UDP(this);

        InitializeClientData();

        isConnected = true;
        tcp.Connect();
    }

    public override void _ExitTree()
    {
        Disconnect();
    }
    public class TCP
    {
        public delegate void ClientConnected();
        public event ClientConnected OnClientConnected;

        public TcpClient socket;

        private NetworkStream stream;
        private Packet receivedData;
        private byte[] receiveBuffer;

        private NetworkClient client;

        public TCP(NetworkClient client)
        {
            this.client = client;
        }

        public void Connect()
        {
            socket = new TcpClient
            {
                SendBufferSize = defaultBufferSize,
                ReceiveBufferSize = defaultBufferSize
            };

            receiveBuffer = new byte[defaultBufferSize];
            socket.BeginConnect(client.hostName, client.port, ConnectCallback, socket);
        }

        private void ConnectCallback(IAsyncResult result)
        {
            socket.EndConnect(result);

            if (!socket.Connected)
            {
                return;
            }

            OnClientConnected?.Invoke();

            stream = socket.GetStream();
            receivedData = new Packet();
            stream.BeginRead(receiveBuffer, 0, defaultBufferSize, ReceiveCallback, null);
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
                GD.Print($"Error sending data to server via TCP: {ex}");
            }
        }

        private void ReceiveCallback(IAsyncResult result)
        {
            try
            {
                int byteLength = stream.EndRead(result);

                if (byteLength <= 0)
                {
                    client.Disconnect();

                    return;
                }

                byte[] data = new byte[byteLength];
                Array.Copy(receiveBuffer, data, byteLength);

                receivedData.Reset(HandleData(data));
                stream.BeginRead(receiveBuffer, 0, defaultBufferSize, ReceiveCallback, null);
            }
            catch (Exception ex)
            {
                GD.Print($"Error receiving TCP data: {ex}");
                Disconnect();
            }
        }

        private void Disconnect()
        {
            client.Disconnect();

            stream = null;
            receivedData = null;
            receiveBuffer = null;
            socket = null;
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
                        packetHandlers[packetID](packet);
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
    }

    public class UDP
    {
        public UdpClient socket;
        public IPEndPoint endPoint;

        private NetworkClient client;

        public UDP(NetworkClient client)
        {
            this.client = client;
            endPoint = new IPEndPoint(IPAddress.Parse(client.hostName), client.port);
        }

        public void Connect(int localPort)
        {
            socket = new UdpClient(localPort);
            socket.Connect(endPoint);
            socket.BeginReceive(ReceiveCallback, null);

            using (Packet packet = new Packet())
            {
                SendData(packet);
            }
        }

        public void SendData(Packet packet)
        {
            try
            {
                packet.InsertInt(client.id);

                if (socket != null)
                {
                    socket.BeginSend(packet.ToArray(), packet.Length(), null, null);
                }
            }
            catch (Exception ex)
            {
                GD.Print($"Error sending data to server via UDP: {ex}");
            }
        }

        public void ReceiveCallback(IAsyncResult result)
        {
            try
            {
                byte[] data = socket.EndReceive(result, ref endPoint);
                socket.BeginReceive(ReceiveCallback, null);

                if (data.Length < 4)
                {
                    client.Disconnect();

                    return;
                }

                HandleData(data);
            }
            catch
            {
                Disconnect();
            }
        }

        private void Disconnect()
        {
            client.Disconnect();

            endPoint = null;
            socket = null;
        }

        private void HandleData(byte[] data)
        {
            using (Packet packet = new Packet(data))
            {
                int packetLength = packet.ReadInt();
                data = packet.ReadBytes(packetLength);
            }

            ThreadManager.ExecuteOnMainThread(() =>
            {
                using (Packet packet = new Packet(data))
                {
                    int packetID = packet.ReadInt();
                    packetHandlers[packetID](packet);
                }
            });
        }
    }
    private void InitializeClientData()
    {
        packetHandlers = new Dictionary<int, PacketHandler>()
        {
            { (int)ServerPackets.Welcome, ClientHandle.Welcome },
            { (int)ServerPackets.Pong, ClientHandle.Pong },
            { (int)ServerPackets.SpawnPlayer, ClientHandle.SpawnPlayer },
            { (int)ServerPackets.PlayerDisconnected, ClientHandle.PlayerDisconnected },
            { (int)ServerPackets.PlayerPosition, ClientHandle.PlayerPosition },
            { (int)ServerPackets.PlayerRotation, ClientHandle.PlayerRotation }
        };
    }

    private void Disconnect()
    {
        if (isConnected)
        {
            isConnected = false;

            tcp?.socket?.Close();
            udp?.socket?.Close();

            GD.Print("Disconnected from server.");
        }
    }
}
