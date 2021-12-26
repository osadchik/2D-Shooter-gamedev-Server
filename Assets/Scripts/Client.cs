using Assets.Scripts;
using System.Net;
using UnityEngine;

public class Client : MonoBehaviour
{
    public static int bufferSize = 4096;

    public int Id { get; set; }
    public Player player { get; set; }
    public UdpClient Udp { get; set; }

    public Client(int clientId)
    {
        Id = clientId;
        Udp = new UdpClient(Id);
    }

    public class UdpClient
    {
        public IPEndPoint endPoint;

        private readonly int id;

        public UdpClient(int _id)
        {
            id = _id;
        }

        /// <summary>Initializes the newly connected client's UDP-related info.</summary>
        /// <param name="_endPoint">The IPEndPoint instance of the newly connected client.</param>
        public void Connect(IPEndPoint _endPoint)
        {
            endPoint = _endPoint;
        }

        /// <summary>Sends data to the client via UDP.</summary>
        /// <param name="_packet">The packet to send.</param>
        public void SendData(Packet _packet)
        {
            Server.SendUDPData(endPoint, _packet);
        }

        /// <summary>Prepares received data to be used by the appropriate packet handler methods.</summary>
        /// <param name="_packetData">The packet containing the recieved data.</param>
        public void HandleData(Packet _packetData)
        {
            int _packetLength = _packetData.ReadInt();
            byte[] _packetBytes = _packetData.ReadBytes(_packetLength);

            ThreadManager.ExecuteOnMainThread(() =>
            {
                using (Packet _packet = new Packet(_packetBytes))
                {
                    int _packetId = _packet.ReadInt();
                    Server.packetHandlers[_packetId](id, _packet); // Call appropriate method to handle the packet
                }
            });
        }
        /// <summary>Cleans up the UDP connection.</summary>
        public void Disconnect()
        {
            endPoint = null;
        }
    }

    public void SendIntoGame(string _playerName)
    {
        player = NetworkManager.instance.InstantiatePlayer();
        player.Initialize(Id, _playerName);

        // Send all players to the new player
        foreach (Client _client in Server.clients.Values)
        {
            if (_client.player != null && _client.Id != Id)
            {
                ServerSend.SpawnPlayer(Id, _client.player);
            }
        }

        // Send the new player to all players (including himself)
        foreach (Client _client in Server.clients.Values)
        {
            if (_client.player != null)
            {
                ServerSend.SpawnPlayer(_client.Id, player);
            }
        }
    }

    /// <summary>Disconnects the client and stops all network traffic.</summary>
    private void Disconnect()
    {
        Debug.Log($"{Udp.endPoint} has disconnected.");

        UnityEngine.Object.Destroy(player.gameObject);
        player = null;

        Udp.Disconnect();
    }
}
