using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net.Sockets;
using System.Text;
using System;



public class ML_SendStatus : MonoBehaviour
{
    [Header("Connectiojn Settings")]
    //[SerializeField] private bool _yieldData = true;
    public int portSend = 50000;
    string TextToSend = "";
    TcpClient socketConnection;
    bool _isReady2Send = false;
    NetworkStream stream;

    [Header("Characteristics Info")] // Informacion relevante
    public string _wallet;
    public string coords = "(1,0)";

    void Start(){
        socketConnection = new TcpClient("localhost", portSend);
        LOAD();
    }

    private void LOAD(){
        TextToSend = "LOAD";
        _isReady2Send = true;
    }

    private void BUY(){
        TextToSend = "BUY " + this._wallet + " " + this.coords;
        _isReady2Send = true;
    }
    private void RETURN(){
        TextToSend = "RETURN " + this._wallet + " " + this.coords;
        _isReady2Send = true;
    }

    private void CHECK(){
        TextToSend = "CHECK " + this._wallet;
        _isReady2Send = true;
    }

    private void SAVE(){
        TextToSend = "SAVE";
        _isReady2Send = true;
    }

    void Update(){
        if(Input.GetKeyDown(KeyCode.B)) BUY();
        if(Input.GetKeyDown(KeyCode.R)) RETURN();
        if(Input.GetKeyDown(KeyCode.C)) CHECK();
        if(Input.GetKeyDown(KeyCode.S)) SAVE();
        if(Input.GetKeyDown(KeyCode.L)) LOAD();
        

        if(_isReady2Send){
            if (socketConnection == null) return;
            try{
                stream = socketConnection.GetStream();
                if (stream.CanWrite){
                    if (!TextToSend.Equals("")){    
                        Debug.Log(TextToSend);     
                        byte[] clientMessageAsByteArray = Encoding.ASCII.GetBytes(TextToSend);
                        stream.Write(clientMessageAsByteArray, 0, clientMessageAsByteArray.Length);
                        _isReady2Send = false;
                    }
                }
            }catch (SocketException socketException){Debug.Log("Socket exception: " + socketException);}
        }
    }
    protected void OnApplicationQuit(){
        socketConnection.Close();
    }


}
