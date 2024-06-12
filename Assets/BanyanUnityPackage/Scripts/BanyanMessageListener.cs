using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Net.Sockets;
using System.Net;
using System.Text.RegularExpressions;

namespace Banyan
{
    //summary
    // This script listens to Banyan and decodes the message, to 
    // forward off to the MessageProcessor on an object, to do the action.
    //summary

    public class BanyanMessageListener : MonoBehaviour
    {
        public static BanyanMessageListener instance;
        public int port;
        public IPAddress serverIp;
        List<TcpConnectedClient> clientList = new List<TcpConnectedClient>();
        public static string messageFromBanyan;

        TcpListener listener;


        //[Header("Propiopio")]
        //public GameObject _prefab;
        //public Transform _Agent;
        //private GameObject _instance;
        // ------------------------------------------- Manipulación del tablero -------------------------------------------
        [SerializeField] GameObject casilla;
        public static Dictionary<string, GameObject> Tablero;
        
        int A, B;

        public void Marcar(string where, Color colorin){ // Color.red por ejemplo
            //Parent -> skin -> renderer -> color = colorin
            Tablero[where].GetComponentInChildren<Renderer>().materials[0].color = colorin;
        }

        public void Replicar(int largo, int ancho){
            if(Tablero == null){
                Tablero = new Dictionary<string, GameObject>();
            }else{
                for (int i = 0; i < largo; i++) {
                    for (int j = 0; j < ancho; j++) {
                        Destroy(Tablero[$"({i},{j})"]);
                    }
                }
                Tablero = new Dictionary<string, GameObject>();
            }
            Destructor.m_MyEvent.Invoke();

            for (int i = 0; i < largo; i++) {
                for (int j = 0; j < ancho; j++) {
                    GameObject copy = Instantiate(casilla);
                    copy.transform.position = new Vector3(i, 0, j);
                    Tablero.Add($"({i},{j})", copy);
                }
            }
        }

        // ----------------------------------------------------------------------------------------------------------------


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
                TcpConnectedClient connectedClient = new TcpConnectedClient(client);
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
            if (!string.IsNullOrEmpty(messageFromBanyan))
            {
                Debug.Log(messageFromBanyan);
                if (messageFromBanyan.Contains('!')){
                    string[] tam = messageFromBanyan.Split(',');
                    // Replicar("(alto" sin '(', "ancho)" sin ')')
                    A = Int32.Parse(Regex.Match(tam[0], @"\d+").Value);
                    B = Int32.Parse(Regex.Match(tam[1], @"\d+").Value);
                    Replicar(A, B);
                }else{
                    if (!messageFromBanyan.Contains("DONE")){  // DONE es para cosas de sistema
                        string[] words = messageFromBanyan.Replace(System.Environment.NewLine, "").Split(';');
                        Replicar(A, B);
                        foreach (var word in words)
                        {
                            //Debug.Log(word);
                            Marcar(word, Color.blue);
                        }
                    }
                    
                }
                messageFromBanyan = null;
            }
        }
        #endregion
        /*
        void PerformAction(string msg){
            if(_instance) Destroy(_instance);

            if(msg.Contains('0')){
                _instance = Instantiate(_prefab, _Agent.position+ new Vector3(1, 0, 0), Quaternion.identity);
                return;
            }
            if(msg.Contains('1')){
                _instance = Instantiate(_prefab, _Agent.position+ new Vector3(-1, 0, 0), Quaternion.identity);
                return;
            }
            if(msg.Contains('2')){
                _instance = Instantiate(_prefab, _Agent.position+ new Vector3(0, 0, 1), Quaternion.identity);
                return;
            }
            if(msg.Contains('3')){
                _instance = Instantiate(_prefab, _Agent.position+ new Vector3(0, 0, -1), Quaternion.identity);
                return;
            }
            if(msg.Contains('4')){
                _instance = Instantiate(_prefab, _Agent.position+ new Vector3(1, 0, 1), Quaternion.identity);
                return;
            }
            if(msg.Contains('5')){
                _instance = Instantiate(_prefab, _Agent.position+ new Vector3(1, 0, -1), Quaternion.identity);
                return;
            }
            if(msg.Contains('6')){
                _instance = Instantiate(_prefab, _Agent.position+ new Vector3(-1, 0, 1), Quaternion.identity);
                return;
            }
            if(msg.Contains('7')){
                _instance = Instantiate(_prefab, _Agent.position+ new Vector3(-1, 0, -1), Quaternion.identity);
                return;
            }
        }*/





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
    #region JSON Structure

    [System.Serializable]

    public class Message
    {
        public string info;
        public int value;
        public string action;
        public string target;
    }
    #endregion
}