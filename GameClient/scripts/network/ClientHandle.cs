using System;
using System.Net;
using Godot;

public class ClientHandle : Node
{
    private static NetworkClient networkClient;
    private static ClientSend clientSend;

    public override void _Ready()
    {
        networkClient = (NetworkClient)GetNode("/root/NetworkClient");
        clientSend = (ClientSend)GetNode("/root/ClientSend");
    }

    public static void Welcome(Packet packet)
    {
        string message = packet.ReadString();
        int clientID = packet.ReadInt();

        GD.Print($"Message from server: {message}");
        networkClient.id = clientID;
        clientSend.WelcomeReceived();

        networkClient.udp.Connect(((IPEndPoint)networkClient.tcp.socket.Client.LocalEndPoint).Port);
    }

    public static void Pong(Packet packet)
    {
        int playerID = packet.ReadInt();
        DateTime start = DateTime.FromBinary(packet.ReadLong());
        DateTime end = DateTime.FromBinary(packet.ReadLong());

        //PingManager.LastPing = (end - start);
    }

    public static void SpawnPlayer(Packet packet)
    {
        int playerID = packet.ReadInt();
        string userName = packet.ReadString();
        Vector3 position = packet.ReadVector3();
        Quat rotation = packet.ReadQuaternion();

        // GameManager.SpawnPlayer(playerID, userName, position, rotation);
    }

    public static void PlayerPosition(Packet packet)
    {
        int playerID = packet.ReadInt();
        Vector3 position = packet.ReadVector3();

        if (GameManager.players.ContainsKey(playerID))
        {
            // GameManager.players[playerID].MoveCharacter(position);
        }
    }

    public static void PlayerRotation(Packet packet)
    {
        int playerID = packet.ReadInt();
        Quat rotation = packet.ReadQuaternion();

        if (GameManager.players.ContainsKey(playerID))
        {
            //        GameManager.players[playerID].transform.rotation = rotation;
        }
    }

    public static void PlayerDisconnected(Packet packet)
    {
        int playerID = packet.ReadInt();

        if (GameManager.players.ContainsKey(playerID))
        {
            // Destroy(GameManager.players[playerID].playerObject);
            GameManager.players.Remove(playerID);
        }
    }
}
