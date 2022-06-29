using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;




public class Game : MonoBehaviourPunCallbacks
{
    //Photon
    protected PhotonView pv;
    //동기화를 위한 변수 선언
    protected  ExitGames.Client.Photon.Hashtable playerHash;
    //점수 
    protected int score = 0;

    public delegate void Sound(string name);

    private void Awake()
    {
        Init(); 
    }

    protected virtual void Init()
    {
        pv = GetComponent<PhotonView>();
    }

    public virtual void StartGame()
    {
        //등록된 점수 초기화
        playerHash = PhotonNetwork.LocalPlayer.CustomProperties;
        if (playerHash.ContainsKey("score"))
            playerHash["score"] = 0;
        else
            playerHash.Add("score", 0);

        PhotonNetwork.LocalPlayer.SetCustomProperties(playerHash);

        score = 0;

    }

   


}
