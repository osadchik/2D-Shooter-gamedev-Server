using UnityEngine;

namespace Assets.Scripts
{
    public class ServerHandle
    {
        public static void WelcomeReceived(int fromClient, Packet packet)
        {
            int _clientIdCheck = packet.ReadInt();
            string _username = packet.ReadString();

            Debug.Log($"{Server.clients[fromClient].Udp.endPoint} connected successfully and is now player {fromClient}.");
            if (fromClient != _clientIdCheck)
            {
                Debug.Log($"Player \"{_username}\" (ID: {fromClient}) has assumed the wrong client ID ({_clientIdCheck})!");
            }
            Server.clients[fromClient].SendIntoGame(_username);
        }

        public static void PlayerMovement(int fromClient, Packet packet)
        {
            bool[] _inputs = new bool[packet.ReadInt()];
            for (int i = 0; i < _inputs.Length; i++)
            {
                _inputs[i] = packet.ReadBool();
            }
            Quaternion _rotation = packet.ReadQuaternion();

            Server.clients[fromClient].Player.SetInput(_inputs, _rotation);
        }
    }
}
