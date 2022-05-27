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

    //���ÿ� �׽�Ʈ
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

    public GameObject ok_Btn;

    private int score = 0;
    //�÷��̾� ���ھ� ������ ���� ���̺�
    //ExitGames.Client.Photon.Hashtable playerHash = new ExitGames.Client.Photon.Hashtable();
    

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
    }

    private void Update()
    {
        if (!gameStart)
            return;

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
        int rand = Random.Range(0, (int)FruitsType.Max);
        float randx = Random.Range(-3.0f, 3.0f);
        float randy = Random.Range(-0.3f, 0.3f);

        string name = ((FruitsType)Random.Range(0, (int)FruitsType.Max)).ToString();
        PhotonNetwork.InstantiateRoomObject("Fruits/" + name, fruitsSpanwPos.transform.position + Vector3.right * randx + Vector3.up * randy, Quaternion.identity);
       
    }

    public void AddScore(LobbyPlayerController player)
    {
        //i == �浹�� �÷��̾�ѹ�
        ExitGames.Client.Photon.Hashtable playerHash = player.pv.Owner.CustomProperties;
        if(playerHash.ContainsKey("score"))
        {
            playerHash["score"] = (int)playerHash["score"] + 100;
            player.pv.Owner.SetCustomProperties(playerHash);
            Debug.Log(player.pv.Owner.NickName);
        }
           
    }

    public void SetScoreTxt()
    {
        if (PhotonNetwork.LocalPlayer.CustomProperties["score"] == null)
            return;

        score = (int)PhotonNetwork.LocalPlayer.CustomProperties["score"];
        scoreTxt.text = score.ToString(); 
    }

    IEnumerator GameStart()
    {
        //������ ���� ������ �����ϱ�
        if (!InGameLobbyMgr.Inst.PlayerHash.ContainsKey("score"))
            InGameLobbyMgr.Inst.PlayerHash.Add("score", 0);
        else
            InGameLobbyMgr.Inst.PlayerHash["score"] = 0;

        score = 0;
        scoreTxt.text = score.ToString();

        PhotonNetwork.LocalPlayer.SetCustomProperties(InGameLobbyMgr.Inst.PlayerHash);

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

        //���Ͽ�����Ʈ ����
        if (PhotonNetwork.LocalPlayer.IsMasterClient)
        {    
            Fruits[] fruits = FindObjectsOfType<Fruits>();
            for (int i = 0; i < fruits.Length; i++)
            {
                PhotonNetwork.Destroy(fruits[i].gameObject);
            }
        }

        CountTxt.text = "����!!";
        yield return new WaitForSeconds(1.5f);
        CountTxt.text = "�����ǥ";
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
            winOrLose.text = "���º�";
            winOrLose.color = Color.green;
        }
        else if (myscore > otherscore)
        {
            winOrLose.text = "�¸�";
            winOrLose.color = Color.blue;
            //D
            InGameLobbyMgr.Inst.WinGame();
        }
        else if (myscore < otherscore)
        {
            winOrLose.text = "�й�";
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
            winOrLose.text = "���º�";
            winOrLose.color = Color.green;
        }
        else if (myscore > otherscore)
        {
            winOrLose.text = "�¸�";
            winOrLose.color = Color.blue;

            InGameLobbyMgr.Inst.WinGame();
        }
        else if (myscore < otherscore)
        {
            winOrLose.text = "�й�";
            winOrLose.color = Color.red;
        }

        
    }

  

    public void OnOkBtn()   //���� ������ Ȯ�ι�ư ������
    {
        ok_Btn.SetActive(false);
        ResultPanel.SetActive(false);
        this.gameObject.SetActive(false);

        InGameLobbyMgr.Inst.SetLobby();
    }


}
