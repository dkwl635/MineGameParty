using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FallingFruitGame : MonoBehaviourPunCallbacks
{
    PhotonView pv;

    enum FruitsType //���� ������Ʈ�� �ҷ����� ���� 
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

    public static FallingFruitGame Inst;    //�̱��� ������ ����
    public LobbyPlayerController[] playerObj;   //���ϵ��� �浹ĳ���͵��� �Ÿ� ����� ���� �÷��̾�� ĳ���ͺ���

    //����ȭ�� ���� ���� ����
    ExitGames.Client.Photon.Hashtable playerHash;

    public GameObject fruitsSpanwPos; //������ �߽ɽ�����ġ

    //���� �԰� ���� ����Ʈ
    public GameObject collectObj;   //������ �԰� ���� ����Ʈ ������

    [Header("UI")]
    public TextMeshProUGUI scoreTxt;    //���� 
    public TextMeshProUGUI CountTxt;   //�����ð�

    public GameObject ResultPanel;          //���â
    public TextMeshProUGUI myNickTxt;       //�� �г��� 
    public TextMeshProUGUI otherNickTxt;    //��� �г���
    public TextMeshProUGUI myScoreTxt;      // �� ����
    public TextMeshProUGUI otherScoreTxt;   //��� ����
    public TextMeshProUGUI winOrLose;       //�¸�����

    public GameObject ok_Btn;                   //Ȯ�� ��ư

    private int score = 0;                          //���� 

    
    float timer = 0.0f;                 //���ӽð�
    float nextSpawnTime = 1.0f; //���� ���� �ֱ�

    bool gameStart = true;  //���� ���� bool ����

    private void Awake()
    {
        Inst = this;
        pv = GetComponent<PhotonView>();
    }

    public override void OnEnable() //���� ������ �ٽ� �����Ұ�� ����
    {    
        //���� �ִ� �÷��̾� ������Ʈ �ҷ�����
        playerObj = GameObject.FindObjectsOfType<LobbyPlayerController>();

        //���� ����
        StartCoroutine(GameStart());

        //���� ����
        SetScoreTxt();
    }

    private void Update()
    {
        if (!gameStart)
            return;

        //���� ������ ������ Ŭ���̾𸶽��͸�
        if (!PhotonNetwork.LocalPlayer.IsMasterClient)
            return;
   
        //���� ����
        timer += Time.deltaTime;
        if(timer >= nextSpawnTime)
        {
            int randCount = Random.Range(2, 5);
            for (int i = 0; i < randCount; i++)
            {
                SpawnFruits(); //���� ����
            }

            timer = 0.0f;
            nextSpawnTime = Random.Range(0.7f, 1.5f);
        }
    }

   
    public void SpawnFruits()   //���� �����ϱ�
    {
        //�������� �� ������ġ ���
        int rand = Random.Range(0, (int)FruitsType.Max);
        float randx = Random.Range(-3.0f, 3.0f);
        float randy = Random.Range(-0.3f, 0.3f);

        //���� ���ҽ��������� ������ �����ϱ�.. �������� ��ȯ�Ͽ� ��� Ŭ�󿡰� ����
        string name = ((FruitsType)Random.Range(0, (int)FruitsType.Max)).ToString();
        PhotonNetwork.InstantiateRoomObject("Fruits/" + name, fruitsSpanwPos.transform.position + Vector3.right * randx + Vector3.up * randy, Quaternion.identity);  
    }

    
    public void AddScore(LobbyPlayerController player , Vector3 pos)//���� �浹�� ������ ����Ʈ ��ȯ
    {
        ExitGames.Client.Photon.Hashtable playerHash = player.pv.Owner.CustomProperties;
        //�÷��̾� ���� ���� ��������ֱ� 
        //���� �÷��̾� SetCustomProperties�� �̿��Ͽ� ����ȭ
        if (playerHash.ContainsKey("score"))
        {
            playerHash["score"] = (int)playerHash["score"] + 100;
            player.pv.Owner.SetCustomProperties(playerHash);

            //PRC �Լ��� �̿��Ͽ� ���� �÷��̾��� Ŭ�󿡰� �Լ�ȣ��
            pv.RPC("SetScoreTxt", player.pv.Owner);             //���� �÷����� �����ؽ�Ʈ ��������
            pv.RPC("SpawnCollect", player.pv.Owner, pos);     //���� ���� ��ġ�� ����Ʈ ����
        }          
    }

  
   [PunRPC]
    public void SetScoreTxt()   //������ ����
    {
        //������ LocalPlayer.CustomProperties["score"]�� ����Ǵ� ���� ó��
        if (PhotonNetwork.LocalPlayer.CustomProperties["score"] == null)
        {
            scoreTxt.text = "0";
            return;
        }
        
        score = (int)PhotonNetwork.LocalPlayer.CustomProperties["score"];
        scoreTxt.text = score.ToString(); 
    }

    [PunRPC]
    void SpawnCollect(Vector3 pos)//���� ������ ����Ʈ ������ �ϱ�
    {       
        GameObject collect = Instantiate(collectObj, pos, Quaternion.identity);
        Destroy(collect, 0.5f);
    }

    //���� ����
    IEnumerator GameStart()
    {
        playerHash = pv.Owner.CustomProperties;

        //������ ���� ������ �����ϱ� �� �ʱ�ȭ
        if (!playerHash.ContainsKey("score"))
            playerHash.Add("score", 0);
        else
            playerHash["score"] = 0;

        score = 0;
        scoreTxt.text = score.ToString();

        //�� ���� �ʱ�ȭ ����
        PhotonNetwork.LocalPlayer.SetCustomProperties(playerHash);

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

        //���â ����
        ResultPanel.SetActive(true);
        myNickTxt.text = PhotonNetwork.LocalPlayer.NickName;
        otherNickTxt.text = PhotonNetwork.PlayerListOthers[0].NickName;

        int myscore = score;
        int otherscore = (int)PhotonNetwork.PlayerListOthers[0].CustomProperties["score"];


        myScoreTxt.text = myscore.ToString();
        otherScoreTxt.text = otherscore.ToString();

        //�¸� �����ϱ�
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
            InGame.Inst.WinGame();  //�� ���Ӹ޴������� �¸� ī��Ʈ ���ֱ�
        }
        else if (myscore < otherscore)
        {
            winOrLose.text = "�й�";
            winOrLose.color = Color.red;
        }

        yield return new WaitForSeconds(2.0f);

        ok_Btn.SetActive(true);
    }

    public void OnOkBtn()   //���� ������ Ȯ�ι�ư ������
    {
        //�̴ϰ��� (���ϸԱ�) ����
        ok_Btn.SetActive(false);
        ResultPanel.SetActive(false);
        this.gameObject.SetActive(false);

        //ȭ�� �������ֱ�
        InGame.Inst.SetLobby();
    }


}
