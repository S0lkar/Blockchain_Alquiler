using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net.Sockets;
using System.Text;
using System;



public class ML_Supervised : MonoBehaviour
{
    [Header("Connectiojn Settings")]
    //[SerializeField] private bool _yieldData = true;
    public int portSend = 50000;
    string TextToSend = "";
    TcpClient socketConnection;
    bool _isReady2Send = false;
    NetworkStream stream;

    [Header("Characteristics Info")] //El tipico ejemplo de la pelota porque es lo mas directo ahora mismo
    public Transform _goalTransform;
    private List<Action> FuncList;
    float current;

    void Start(){
        socketConnection = new TcpClient("localhost", portSend);
        TextToSend = "";
        FuncList = new()
        {
            () => Up(),
            () => Down(),
            () => Left(),
            () => Right(),
            () => RUp(),
            () => LUp(),
            () => RDown(),
            () => LDown()
        };
        StartCoroutine(Episode(0.1f));
    }

    IEnumerator Episode(float time){
        for(int i = 0; i < 1000; i++){
            yield return new WaitForSeconds(time);
            TakeAction();
        }
    }

    private void TakeAction(){
        Up();
        float Best = current;
        int action_taken = 0;
        for(int i = 1; i < FuncList.Count; i++){
            FuncList[i]();
            if(current < Best){
                Best = current;
                action_taken = i;
            }
        }

        TextToSend = transform.position.ToString() + "," + _goalTransform.transform.position.ToString() + "," + action_taken;
        _isReady2Send = true;
        //Pone la siguiente pose de la meta en otro lugar random
        _goalTransform.transform.position = new Vector3(UnityEngine.Random.Range(-10, 11), 0, UnityEngine.Random.Range(-10, 11));
    }

    void Update(){
        if(_isReady2Send){
            if (socketConnection == null) return;
            try{		
                stream = socketConnection.GetStream();
                if (stream.CanWrite){
                    if (!TextToSend.Equals("")){         
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


    // -------- Data oriented ----------
    public void Up(){
        current =  Vector3.Distance(_goalTransform.position, transform.position + new Vector3(1, 0, 0));
    }
    void Down(){
        current = Vector3.Distance(_goalTransform.position, transform.position + new Vector3(-1, 0, 0));
    }
    void Right(){
        current = Vector3.Distance(_goalTransform.position, transform.position + new Vector3(0, 0, 1));
    }
    void Left(){
        current = Vector3.Distance(_goalTransform.position, transform.position + new Vector3(0, 0, -1));
    }
    void RUp(){
        current = Vector3.Distance(_goalTransform.position, transform.position + new Vector3(1, 0, 1));
    }
    void LUp(){
        current = Vector3.Distance(_goalTransform.position, transform.position + new Vector3(1, 0, -1));
    }   
    void RDown(){
        current = Vector3.Distance(_goalTransform.position, transform.position + new Vector3(-1, 0, 1));
    }
    void LDown(){
        current = Vector3.Distance(_goalTransform.position, transform.position + new Vector3(-1, 0, -1));
    }

}
