using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum GameState
{
    GS_Ready = 0,
    GS_Playing, 
    GS_GameEnd,
}


public class InGameLobbyMgr : MonoBehaviourPunCallbacks
{
    //캐릭터 스폰 위치
    public GameObject spawnPos;
    PhotonView pv;

    [Header("UI")]
    public Button readyBtn;
    public Text readyTxt;
    public Button StartBtn;
    public Text myNickName;
    public Text ohterNickName;


    //내가 조종하는 캐릭터
    [HideInInspector] public  LobbyPlayerController player;


   
    bool isReady = false;   //레디 상태

    private void Awake()
    {
        Application.targetFrameRate = 60;

        //PhotonView 컴포넌트 할당
        pv = GetComponent<PhotonView>();
        //자기자신캐릭터 생성하는 함수 호출
        CreatePlayer();
    }

    // Start is called before the first frame update
    void Start()
    {
        PhotonNetwork.IsMessageQueueRunning = true;

        StartBtn.gameObject.SetActive(false);
        
        if (PhotonNetwork.LocalPlayer.IsMasterClient)
            readyBtn.gameObject.SetActive(false);
        else
            readyBtn.gameObject.SetActive(true);
  
    }

    

    void CreatePlayer()
    {
        Vector3 a_HPos = Vector3.zero;
        Vector3 a_AddPos = Vector3.zero;
        GameObject a_HPosObj = GameObject.Find("SpawnPos");
        if (a_HPosObj != null)
        {
            a_AddPos.x = Random.Range(-2.0f, 2.0f);      
            a_HPos = spawnPos.transform.position + a_AddPos;
        }

       GameObject character =  PhotonNetwork.Instantiate("PlayerChacter/Player", a_HPos, Quaternion.identity, 0);
     
    }

    public void MoveBtnDown(int h)
    {
        if (player)
            player.MoveStart(h);
    }

    public void  MoveBtnUp(int h)
    {
        if (player)
            player.MoveEnd(h);
    }


    public void ReadyBtn()
    {
        isReady = !isReady;

        if(isReady)
        {
            readyTxt.text = "준비완료";
        }
        else
        {
            readyTxt.text = "준비";
        }

        pv.RPC("CheckReady", RpcTarget.MasterClient, isReady);
    }

    [PunRPC]
    public void CheckReady(bool isReady)
    {
        if(!PhotonNetwork.LocalPlayer.IsMasterClient)
        {
            return;
        }

        StartBtn.gameObject.SetActive(isReady);

    }
}
