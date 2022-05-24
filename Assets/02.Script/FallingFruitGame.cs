using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FallingFruitGame : MonoBehaviourPunCallbacks
{
    public static FallingFruitGame Inst;
    public GameObject[] playerObj;

    enum Fruits
    {
        Apple,
        Bananas,
        Cherries,
        Kiwi,
        Melon,
        Orange,
        Pineapple,
        Strawberry,
        Max,
    }

    private void Awake()
    {
        Inst = this;
    }

    private void Start()
    {
        string name = ((Fruits)Random.Range(0, (int)Fruits.Max)).ToString();

        Debug.Log(name);

        PhotonNetwork.InstantiateRoomObject("Fruits/"+ name , Vector3.zero + Vector3.right, Quaternion.identity);    
    }



    public void SpawnFruits()
    {
        playerObj = GameObject.FindGameObjectsWithTag("Player");

        string name = ((Fruits)Random.Range(0, (int)Fruits.Max)).ToString();
        PhotonNetwork.InstantiateRoomObject("Fruits/" + name, Vector3.zero, Quaternion.identity);
    }

}
