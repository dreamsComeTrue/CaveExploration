using System;

namespace GameServer
{
    class ServerSend
    {
        public static void Welcome(int clientID, string message)
        {
            using (Packet packet = new Packet((int)ServerPackets.Welcome))
            {
                packet.Write(message);
                packet.Write(clientID);

                SendTCPData(clientID, packet);
            }
        }

        public static void SpawnPlayer(int clientID, Player player)
        {
            using (Packet packet = new Packet((int)ServerPackets.SpawnPlayer))
            {
                packet.Write(player.id);
                packet.Write(player.userName);
                packet.Write(player.position);
                packet.Write(player.rotation);

                SendTCPData(clientID, packet);
            }
        }

        public static void PlayerPosition(Player player)
        {
            using (Packet packet = new Packet((int)ServerPackets.PlayerPosition))
            {
                packet.Write(player.id);
                packet.Write(player.position);

                SendUDPDataToAll(packet);
            }
        }

        public static void PlayerRotation(Player player)
        {
            using (Packet packet = new Packet((int)ServerPackets.PlayerRotation))
            {
                packet.Write(player.id);
                packet.Write(player.rotation);

                SendUDPDataToAlExcept(player.id, packet);
            }
        }

        public static void PlayerDisconnected(int playerID)
        {
            using (Packet packet = new Packet((int)ServerPackets.PlayerDisconnected))
            {
                packet.Write(playerID);

                SendTCPDataToAll(packet);
            }
        }

        public static void Pong(int clientID, DateTime pingTime)
        {
            using (Packet packet = new Packet((int)ServerPackets.Pong))
            {
                packet.Write(clientID);
                packet.Write(pingTime.ToBinary());
                packet.Write(DateTime.Now.ToBinary());

                SendTCPData(clientID, packet);
            }
        }

        private static void SendUDPData(int clientID, Packet packet)
        {
            packet.WriteLength();
            Server.clients[clientID].udp.SendData(packet);
        }

        private static void SendTCPData(int clientID, Packet packet)
        {
            packet.WriteLength();
            Server.clients[clientID].tcp.SendData(packet);
        }

        private static void SendTCPDataToAll(Packet packet)
        {
            packet.WriteLength();

            for (int i = 1; i <= Server.MaxPlayers; ++i)
            {
                Server.clients[i].tcp.SendData(packet);
            }
        }

        private static void SendTCPDataToAlExcept(int excludedClientID, Packet packet)
        {
            packet.WriteLength();

            for (int i = 1; i <= Server.MaxPlayers; ++i)
            {
                if (i != excludedClientID)
                {
                    Server.clients[i].tcp.SendData(packet);
                }
            }
        }

        private static void SendUDPDataToAll(Packet packet)
        {
            packet.WriteLength();

            for (int i = 1; i <= Server.MaxPlayers; ++i)
            {
                Server.clients[i].udp.SendData(packet);
            }
        }

        private static void SendUDPDataToAlExcept(int excludedClientID, Packet packet)
        {
            packet.WriteLength();

            for (int i = 1; i <= Server.MaxPlayers; ++i)
            {
                if (i != excludedClientID)
                {
                    Server.clients[i].udp.SendData(packet);
                }
            }
        }
    }
}
