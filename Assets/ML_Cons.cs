using System;
using System.Collections.Generic;
using UnityEngine;
using System.Net.Sockets;
using System.Net;


namespace Banyan{
    public class ML_Cons : MonoBehaviour {
        public static ML_Cons instance;
        public int port;
        public IPAddress serverIp;
        List<TcpConnectedClient> clientList = new List<TcpConnectedClient>();
        public static string messageFromBanyan;

        TcpListener listener;

        #region Unity Events
        public void Awake()
        {
            instance = this;

            if (serverIp == null)
            { // Server: start listening for connections
                listener = new TcpListener(IPAddress.Any, port);
                listener.Start();
                listener.BeginAcceptTcpClient(OnServerConnect, null);
            }
            else
            { // Client: try connecting to the server
                TcpClient client = new TcpClient("localhost", port);
                TcpConnectedClient connectedClient = new(client);
                clientList.Add(connectedClient);
                client.BeginConnect(serverIp, port, (ar) => connectedClient.EndConnect(ar), null);
            }
        }

        protected void OnApplicationQuit()
        {
            listener.Stop();
            for (int i = 0; i < clientList.Count; i++)
            {
                clientList[i].Close();
            }
        }

        protected void Update()
        {
            string singleMessage = "";

            if (!string.IsNullOrEmpty(messageFromBanyan))
            {
                Debug.Log(messageFromBanyan);
                messageFromBanyan = messageFromBanyan.Replace("'", "\"");
                for (int i = 0; i < messageFromBanyan.Length; i++)
                {
                    singleMessage += messageFromBanyan[i];
                    if (messageFromBanyan[i].Equals('}'))
                    {
                        Debug.Log(singleMessage);
                        Message message = JsonUtility.FromJson<Message>(singleMessage);
                        try
                        {
                            GameObject theObject = GameObject.Find(message.target);
                            MessageProcessor messageProcessor = theObject.GetComponent<MessageProcessor>();
                            messageProcessor.DoAction(message.action, message.info, message.value, message.target);
                        }
                        catch (NullReferenceException)
                        {
                            Debug.LogError(" Either the object: " + message.target + " you are referencing in your Banyan message does not exist or is not active in your Unity world, or there is no MessageProcessor script associated with the object: " + message.target + "!");
                        }


                        singleMessage = "";
                        }
                }

                messageFromBanyan = null;
            }
        }
        #endregion

        #region Async Events
        void OnServerConnect(IAsyncResult ar)
        {
            TcpClient TcpClient = listener.EndAcceptTcpClient(ar);
            clientList.Add(new TcpConnectedClient(TcpClient));

            listener.BeginAcceptTcpClient(OnServerConnect, null);
        }
        #endregion

        public void OnDisconnect(TcpConnectedClient client)
        {
            clientList.Remove(client);
        }

    }
}
