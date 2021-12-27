using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

namespace Assets.Scripts
{
    public class Server
    {
        public static int MaxPlayers { get; private set; }
        public static int Port { get; private set; }
        public static Dictionary<int, Client> clients = new Dictionary<int, Client>();
        public delegate void PacketHandler(int fromClient, Packet packet);
        public static Dictionary<int, PacketHandler> packetHandlers;

        private static UdpClient udpListener;

        public static void Start(int maxPlayers, int port)
        {
            MaxPlayers = maxPlayers;
            Port = port;

            Debug.Log("Starting server...");
            InitializeServerData();

            udpListener = new UdpClient(Port);
            udpListener.BeginReceive(UDPReceiveCallback, null);

            Debug.Log($"Server started on port {Port}.");
        }

        /// <summary>Sends a packet to the specified endpoint via UDP.</summary>
        /// <param name="_clientEndPoint">The endpoint to send the packet to.</param>
        /// <param name="_packet">The packet to send.</param>
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
                Debug.Log($"Error sending data to {_clientEndPoint} via UDP: {_ex}");
            }
        }

        private static void UDPReceiveCallback(IAsyncResult _result)
        {
            try
            {
                IPEndPoint clientEndPoint = new IPEndPoint(IPAddress.Any, 0);
                byte[] _data = udpListener.EndReceive(_result, ref clientEndPoint);
                udpListener.BeginReceive(UDPReceiveCallback, null);

                if (_data.Length < 4)
                {
                    Debug.Log("Incorrect package size!");
                    return;
                }

                using (Packet _packet = new Packet(_data))
                {
                    int clientId = _packet.ReadInt();

                    if (clientId == 0)
                    {
                        for (int i = 1; i <= MaxPlayers; i++)
                        {
                            if (clients[i].Udp.endPoint == null)
                            {
                                clients[i].Udp.Connect(clientEndPoint);
                                return;
                            }
                        }

                        Debug.Log($"{clientEndPoint} failed to connect: Server full!");
                    }

                    if (clients[clientId].Udp.endPoint.ToString() == clientEndPoint.ToString())
                    {
                        // Ensures that the client is not being impersonated by another by sending a false clientID
                        clients[clientId].Udp.HandleData(_packet);
                    }
                }
            }
            catch (Exception _ex)
            {
                Debug.Log($"Error receiving UDP data: {_ex}");
            }
        }


        /// <summary>Initializes all necessary server data.</summary>
        private static void InitializeServerData()
        {
            for (int i = 1; i <= MaxPlayers; i++)
            {
                clients.Add(i, new Client(i));
            }

            packetHandlers = new Dictionary<int, PacketHandler>()
            {
                { (int)ClientPackets.welcomeReceived, ServerHandle.WelcomeReceived },
                { (int)ClientPackets.playerMovement, ServerHandle.PlayerMovement },
            };
            Debug.Log("Initialized packets.");
        }
    }
}
