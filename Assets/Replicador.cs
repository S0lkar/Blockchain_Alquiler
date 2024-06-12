using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Replicador : MonoBehaviour{
    [SerializeField] GameObject casilla;
    public static Dictionary<string, GameObject> Tablero;


    public void Update(){
        if(Input.GetKeyDown(KeyCode.B)) Replicar(10, 10);
        if(Input.GetKeyDown(KeyCode.V)) Marcar("(3,3)", Color.red);
    }

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
        

        for (int i = 0; i < largo; i++) {
            for (int j = 0; j < ancho; j++) {
                GameObject copy = Instantiate(casilla);
                copy.transform.position = new Vector3(i, 0, j);
                Tablero.Add($"({i},{j})", copy);
            }
        }
    }
}
