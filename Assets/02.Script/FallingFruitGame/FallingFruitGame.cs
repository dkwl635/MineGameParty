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

    //과일 먹고 나올 이펙트
    public GameObject collectObj;

    [Header("UI")]
    public TextMeshProUGUI scoreTxt;
    public TextMeshProUGUI CountTxt;

    public GameObject ResultPanel;
    public TextMeshProUGUI myNickTxt;
    public TextMeshProUGUI otherNickTxt;
    public TextMeshProUGUI myScoreTxt;
    public TextMeshProUGUI otherScoreTxt;
    public TextMeshProUGUI winOrLose;

    public GameObject ok_Btn;

    private int score = 0;
  

    float timer = 0.0f;
    float nextSpawnTime = 1.0f;

    bool gameStart = true;

    private void Awake()
    {
        Inst = this;
        pv = GetComponent<PhotonView>();
    }

    public override void OnEnable()
    {
        Debug.Log(PhotonNetwork.LocalPlayer.IsMasterClient);
        playerObj = GameObject.FindObjectsOfType<LobbyPlayerController>();

        StartCoroutine(GameStart());

        SetScoreTxt();
    }

    private void Update()
    {
        if (!gameStart)
            return;

        //SetScoreTxt();

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
        int rand = Random.Range(0, (int)FruitsType.Max);
        float randx = Random.Range(-3.0f, 3.0f);
        float randy = Random.Range(-0.3f, 0.3f);

        string name = ((FruitsType)Random.Range(0, (int)FruitsType.Max)).ToString();
        PhotonNetwork.InstantiateRoomObject("Fruits/" + name, fruitsSpanwPos.transform.position + Vector3.right * randx + Vector3.up * randy, Quaternion.identity);
       
    }

    public void AddScore(LobbyPlayerController player , Vector3 pos)
    {
        //i == 충돌된 플레이어넘버
        ExitGames.Client.Photon.Hashtable playerHash = player.pv.Owner.CustomProperties;
        if(playerHash.ContainsKey("score"))
        {
            playerHash["score"] = (int)playerHash["score"] + 100;
            player.pv.Owner.SetCustomProperties(playerHash);

            pv.RPC("SetScoreTxt", player.pv.Owner);
            pv.RPC("SpawnCollect", player.pv.Owner, pos);
        }          
    }

  
   [PunRPC]
    public void SetScoreTxt()
    {
        Debug.Log("점수 얻음");

        if (PhotonNetwork.LocalPlayer.CustomProperties["score"] == null)
        {
            scoreTxt.text = "0";
            return;
        }
        

        score = (int)PhotonNetwork.LocalPlayer.CustomProperties["score"];
        scoreTxt.text = score.ToString(); 
    }

    [PunRPC]
    void SpawnCollect(Vector3 pos)
    {
        GameObject collect = Instantiate(collectObj, pos, Quaternion.identity);
        Destroy(collect, 0.5f);

    }

    public IEnumerator DestroyAfter(GameObject fruit, float delay)
    {
        fruit.SetActive(false);

        yield return new WaitForSeconds(delay);

        if(fruit)
        {
            PhotonNetwork.Destroy(fruit);
        }
    }

    IEnumerator GameStart()
    {
        //서버에 점수 데이터 저장하기
        if (!InGame.Inst.PlayerHash.ContainsKey("score"))
            InGame.Inst.PlayerHash.Add("score", 0);
        else
            InGame.Inst.PlayerHash["score"] = 0;

        score = 0;
        scoreTxt.text = score.ToString();

        PhotonNetwork.LocalPlayer.SetCustomProperties(InGame.Inst.PlayerHash);

        yield return new WaitForSeconds(1.0f);

        gameStart = true;
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
            for (int i = 0; i < fruits.Length; i++)
            {
                PhotonNetwork.Destroy(fruits[i].gameObject);
            }
        }

        CountTxt.text = "종료!!";
        yield return new WaitForSeconds(1.5f);
        CountTxt.text = "결과발표";
        yield return new WaitForSeconds(1.0f);
        CountTxt.text = "";

        ResultPanel.SetActive(true);
        myNickTxt.text = PhotonNetwork.LocalPlayer.NickName;
        otherNickTxt.text = PhotonNetwork.PlayerListOthers[0].NickName;

        int myscore = score;
        int otherscore = (int)PhotonNetwork.PlayerListOthers[0].CustomProperties["score"];


        myScoreTxt.text = myscore.ToString();
        otherScoreTxt.text = otherscore.ToString();

        if (myscore == otherscore)
        {
            winOrLose.text = "무승부";
            winOrLose.color = Color.green;
        }
        else if (myscore > otherscore)
        {
            winOrLose.text = "승리";
            winOrLose.color = Color.blue;
            //D
            InGame.Inst.WinGame();
        }
        else if (myscore < otherscore)
        {
            winOrLose.text = "패배";
            winOrLose.color = Color.red;
        }

        yield return new WaitForSeconds(2.0f);

        ok_Btn.SetActive(true);
    }

    void EndGame()
    {
        ResultPanel.SetActive(true);
        myNickTxt.text = PhotonNetwork.LocalPlayer.NickName;
        otherNickTxt.text = PhotonNetwork.PlayerListOthers[0].NickName;

        int myscore = score;
        int otherscore = (int)PhotonNetwork.PlayerListOthers[0].CustomProperties["score"];

      
        myScoreTxt.text = myscore.ToString();
        otherScoreTxt.text = otherscore.ToString();

        if (myscore == otherscore)
        {
            winOrLose.text = "무승부";
            winOrLose.color = Color.green;
        }
        else if (myscore > otherscore)
        {
            winOrLose.text = "승리";
            winOrLose.color = Color.blue;

            InGame.Inst.WinGame();
        }
        else if (myscore < otherscore)
        {
            winOrLose.text = "패배";
            winOrLose.color = Color.red;
        }

        
    }

  

    public void OnOkBtn()   //게임 종료후 확인버튼 누르면
    {
        ok_Btn.SetActive(false);
        ResultPanel.SetActive(false);
        this.gameObject.SetActive(false);

        InGame.Inst.SetLobby();
    }


}
