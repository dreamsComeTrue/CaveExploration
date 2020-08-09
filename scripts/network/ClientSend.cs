using System;
using Godot;

public class ClientSend : Node
{
    private NetworkClient networkClient;
    
    public override void _Ready()
    {
        networkClient = (NetworkClient)GetNode("/root/NetworkClient");
    }
    
    private void SendTCPData(Packet packet)
    {
        packet.WriteLength();
        networkClient.tcp.SendData(packet);
    }

    private void SendUDPData(Packet packet)
    {
        packet.WriteLength();
        networkClient.udp.SendData(packet);
    }

    public void WelcomeReceived()
    {
        using (Packet packet = new Packet((int)ClientPackets.WelcomeReceived))
        {
            packet.Write(networkClient.id);
            // packet.Write(UIManager.usernameField.text);

            SendTCPData(packet);
        }
    }

    public void PlayerMovement(bool[] inputs)
    {
        using (Packet packet = new Packet((int)ClientPackets.PlayerMovement))
        {
            packet.Write(inputs.Length);

            foreach (bool input in inputs)
            {
                packet.Write(input);
            }

            packet.Write(GameManager.players[networkClient.id].Rotation);

            SendUDPData(packet);
        }
    }

    public void Ping()
    {
        using (Packet packet = new Packet((int)ClientPackets.Ping))
        {
            packet.Write(networkClient.id);
            packet.Write(DateTime.Now.ToBinary());

            SendTCPData(packet);
        }
    }
}
