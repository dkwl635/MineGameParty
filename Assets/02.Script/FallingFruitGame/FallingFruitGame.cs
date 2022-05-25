using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FallingFruitGame : MonoBehaviourPunCallbacks
{
    PhotonView pv;

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

    public static FallingFruitGame Inst;
    public LobbyPlayerController[] playerObj;
    

    public GameObject fruitsSpanwPos;

    //로컬용 테스트
    public GameObject[] FruitsObj;

    public TextMeshProUGUI scoreTxt;
    private int score = 0;
    ExitGames.Client.Photon.Hashtable playerHash = new ExitGames.Client.Photon.Hashtable();


    float timer = 0.0f;
    float nextSpawnTime = 1.0f;



    private void Awake()
    {
        Inst = this;
        pv = GetComponent<PhotonView>();
    }

    private void OnEnable()
    {
        playerObj = GameObject.FindObjectsOfType<LobbyPlayerController>();

        //서버에 점수 데이터 저장하기
        playerHash.Add("score", 0);
        score = 0;
        scoreTxt.text = score.ToString();

        PhotonNetwork.LocalPlayer.SetCustomProperties(playerHash);
        
    }

    private void Update()
    {
        SetScoreTxt();

        if (!PhotonNetwork.LocalPlayer.IsMasterClient)
            return;
       
        timer += Time.deltaTime;
        if(timer >= nextSpawnTime)
        {
            int randCount = Random.Range(2, 5);
            for (int i = 0; i < randCount; i++)
            {
                SpawnFruits();
            }

            timer = 0.0f;
            nextSpawnTime = Random.Range(0.7f, 1.5f);
        }

    }


    public void SpawnFruits()
    {
        int rand = Random.Range(0, (int)Fruits.Max);
        float randf = Random.Range(-3.0f, 3.0f);

        string name = ((Fruits)Random.Range(0, (int)Fruits.Max)).ToString();
        PhotonNetwork.InstantiateRoomObject("Fruits/" + name, fruitsSpanwPos.transform.position + Vector3.right * randf, Quaternion.identity);

        //GameObject.Instantiate(FruitsObj[rand], fruitsSpanwPos.transform.position + Vector3.right * randf, Quaternion.identity);   
        
    }



    public void AddScore(LobbyPlayerController player)
    {
        //i == 충돌된 플레이어넘버
        playerHash["score"] = (int)player.pv.Owner.CustomProperties["score"] + 100;
        player.pv.Owner.SetCustomProperties(playerHash);
    }

    public void SetScoreTxt()
    {
        if (PhotonNetwork.LocalPlayer.CustomProperties["score"] == null)
            return;

        score = (int)PhotonNetwork.LocalPlayer.CustomProperties["score"];
        scoreTxt.text = score.ToString(); 
    }

}
