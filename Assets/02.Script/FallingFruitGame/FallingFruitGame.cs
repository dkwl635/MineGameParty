using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FallingFruitGame : MonoBehaviourPunCallbacks
{
    PhotonView pv;

    enum FruitsType
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

    [Header("UI")]
    public TextMeshProUGUI scoreTxt;
    public TextMeshProUGUI CountTxt;

    public GameObject ResultPanel;
    public TextMeshProUGUI myNickTxt;
    public TextMeshProUGUI otherNickTxt;
    public TextMeshProUGUI myScoreTxt;
    public TextMeshProUGUI otherScoreTxt;
    public TextMeshProUGUI winOrLose;

    private int score = 0;
    ExitGames.Client.Photon.Hashtable playerHash = new ExitGames.Client.Photon.Hashtable();
    float timer = 0.0f;
    float nextSpawnTime = 1.0f;

    bool gameStart = true;

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

        StartCoroutine(GameEnd());
    }

    private void Update()
    {
        SetScoreTxt();

        if (!PhotonNetwork.LocalPlayer.IsMasterClient)
            return;

        if (!gameStart)
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
        int rand = Random.Range(0, (int)FruitsType.Max);
        float randf = Random.Range(-3.0f, 3.0f);

        string name = ((FruitsType)Random.Range(0, (int)FruitsType.Max)).ToString();
        PhotonNetwork.InstantiateRoomObject("Fruits/" + name, fruitsSpanwPos.transform.position + Vector3.right * randf, Quaternion.identity);
       
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

    IEnumerator GameEnd()
    {
        yield return new WaitForSeconds(1.0f);
        int count = 15;

        CountTxt.gameObject.SetActive(true);
        while (count >= 0)
        {
            CountTxt.text = count.ToString();
            count--;
            yield return new WaitForSeconds(1.0f);
        }

        gameStart = false;

        //GameEnd

        //과일오브젝트 삭제
        if (PhotonNetwork.LocalPlayer.IsMasterClient)
        {    
            Fruits[] fruits = FindObjectsOfType<Fruits>();
            Debug.Log(fruits.Length);
            for (int i = 0; i < fruits.Length; i++)
            {
                PhotonNetwork.Destroy(fruits[i].gameObject);
            }
        }

        CountTxt.text = "종료!!";
        yield return new WaitForSeconds(1.0f);
        CountTxt.text = "";

        ResultPanel.SetActive(true);
        myNickTxt.text = PhotonNetwork.LocalPlayer.NickName;
        otherNickTxt.text = PhotonNetwork.PlayerListOthers[0].NickName;

        int myscore = (int)PhotonNetwork.LocalPlayer.CustomProperties["score"];
        int otherscore = (int)PhotonNetwork.PlayerListOthers[0].CustomProperties["score"];

        myScoreTxt.text = myscore.ToString();
        otherScoreTxt.text =otherscore.ToString();

        if (myscore == otherscore)
        {
            winOrLose.text = "무승부";
            winOrLose.color = Color.green;
        }
        else if(myscore > otherscore)
        {
            winOrLose.text = "승리";
            winOrLose.color = Color.blue;
        }
        else if (myscore < otherscore)
        {
            winOrLose.text = "패배";
            winOrLose.color = Color.red;
        }


    }

}
