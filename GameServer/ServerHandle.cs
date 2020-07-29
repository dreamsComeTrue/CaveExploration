using System;
using System.Numerics;

namespace GameServer
{
    class ServerHandle
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public static void WelcomeReceived(int clientID, Packet packet)
        {
            int clientIdCheck = packet.ReadInt();
            string userName = packet.ReadString();

            logger.Info($"{Server.clients[clientID].tcp.socket.Client.RemoteEndPoint} connected successfully and is now player: {clientID}:{userName}");

            if (clientID != clientIdCheck)
            {
                logger.Error($"Player \"{userName}\" (ID: {clientID}) has assumed the wrong client ID ({clientIdCheck})!");
            }

            Server.clients[clientID].SendIntoGame(userName);
        }

        public static void PlayerMovement(int clientID, Packet packet)
        {
            bool[] inputs = new bool[packet.ReadInt()];

            for (int i = 0; i < inputs.Length; ++i)
            {
                inputs[i] = packet.ReadBool();
            }

            Quaternion rotation = packet.ReadQuaternion();

            Server.clients[clientID].player.SetInput(inputs, rotation);
        }

        public static void Ping(int clientID, Packet packet)
        {
            int clientIdCheck = packet.ReadInt();
            long pingTimeBinary = packet.ReadLong();
            DateTime pingTime = DateTime.FromBinary(pingTimeBinary);

            ServerSend.Pong(clientID, pingTime);
        }
    }
}
