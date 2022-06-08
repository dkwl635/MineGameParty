using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
using Photon.Realtime;


public class StairGame : MonoBehaviourPunCallbacks
{

    PhotonView pv;


    //�� �÷��̾�
    public PlayerCharacter myPlayer;

    //ĳ���͵�
    PlayerCharacter[] players;

    public GameObject GamePanel;          //���Ӱ��� UI
    //��� ������
    public GameObject stairPrefab;
    //��� ó�� ���� ��ġ
    public GameObject spawnPos;
    //��ȯ�� ��ܵ��� ��Ƶ� ��ũ�� ����Ʈ
    Queue<Stairs> stairs = new Queue<Stairs>();
    //������ ����� Ȯ���ϱ� ����
    Stairs lastStair;

    //��ȯ�� ����� ��Ƶ� ������������Ʈ
    public GameObject stairsGroup;
    //ī�޶�
    GameObject camera;
    //���
    GameObject BG;

    //���� �ؽ�Ʈ
    int score = 0;
    public TextMeshProUGUI scroe_Txt;

    //���ѽð�
    public Image gageBar;
    float nextTimer = 2.0f;
    float timer = 2.0f;

    //���� ���� ��
    bool game = false;

    //��� Sprite ������ �ֱ�����
    SpriteRenderer otherSprite;

    //���� ī��Ʈ
    public TextMeshProUGUI countText;

    //����ȭ�� ���� ���� ����
    ExitGames.Client.Photon.Hashtable playerHash;


    [Header("ResultPanel")]
    public GameObject ResultPanel;          //���â
   
    public TextMeshProUGUI myNickTxt;       //�� �г��� 
    public TextMeshProUGUI otherNickTxt;    //��� �г���
    public TextMeshProUGUI myScoreTxt;      // �� ����
    public TextMeshProUGUI otherScoreTxt;   //��� ����
    public TextMeshProUGUI winOrLose;       //�¸�����

    public GameObject ok_Btn;                   //Ȯ�� ��ư


    //private void Start()
    private  void OnEnable()
    {
        pv = GetComponent<PhotonView>();

        players = InGame.Inst.playerCharacters;
        myPlayer = players[0];

        otherSprite = players[1].GetComponent<SpriteRenderer>();
        otherSprite.color = new Color(1, 1, 1, 0.5f);

        camera = Camera.main.gameObject;
        BG = GameObject.Find("BG");

        GameObject.Find("LeftButton").GetComponent<Button>().onClick.AddListener(LeftMove);
        GameObject.Find("RightButton").GetComponent<Button>().onClick.AddListener(RightMove);

        if(PhotonNetwork.IsMasterClient)
        {
            FirstSpawnStair();

            for (int i = 0; i < 20; i++)
            {
                SpawnStair();
            }     
        }


        myPlayer.isMove = false;
        myPlayer.transform.position = spawnPos.transform.position+Vector3.up;
        score = 0;

        GamePanel.SetActive(true);

        playerHash = PhotonNetwork.LocalPlayer.CustomProperties;
        if (playerHash.ContainsKey("StairGameScore"))
            playerHash["StairGameScore"] = 0;
        else
            playerHash.Add("StairGameScore", 0);

        PhotonNetwork.LocalPlayer.SetCustomProperties(playerHash);

        StartCoroutine(GameStart());
    }

    public void RightMove()
    {
        if (!game)
            return;

        myPlayer.transform.position += Vector3.up + Vector3.right;

        camera.transform.position += Vector3.up;
        BG.transform.position += Vector3.up;

        CheckStair();
    }
    public void LeftMove()
    {
        if (!game)
            return;

        myPlayer.transform.position += Vector3.up + Vector3.left;
        
        camera.transform.position += Vector3.up;
        BG.transform.position += Vector3.up;

        CheckStair();
    }

    IEnumerator GameStart()
    {
        countText.text = "3";
        yield return new WaitForSeconds(1.0f);

        countText.text = "2";
        yield return new WaitForSeconds(1.0f);
        countText.text = "1";
        yield return new WaitForSeconds(1.0f);
        countText.text = "Start!!";
        game = true;
        gageBar.fillAmount = 1.0f;
        timer = 2.0f;

        yield return new WaitForSeconds(1.0f);
        countText.text = "";
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftArrow))
            LeftMove();

        if (Input.GetKeyDown(KeyCode.RightArrow))
            RightMove();

        if (PhotonNetwork.IsMasterClient && game)
            SpawnCheckStair();


        if (!game)
            return;

        //if (timer > 0)
        //{
        //    timer -= Time.deltaTime;
        //    gageBar.fillAmount = timer / nextTimer;
        //    if (timer <= 0)
        //    {
        //        GameOver();
        //    }
        //}
    }


    void SpawnCheckStair()
    {
        bool destory = true;
      
        //�� ������ �� üũ
        for (int i = 0; i < players.Length; i++)
        {         
            if (stairs.Peek().transform.position.y + 1.2f > players[i].transform.position.y)
                destory = false;
            // - 1.5, - 2.4

            Debug.Log(stairs.Peek().transform.position.y + " : " + players[i].transform.position.y);
        }

        if (destory)
        {
            var d = stairs.Dequeue();
            PhotonNetwork.Destroy(d.gameObject);
        }

        //�� �� ������ ���� �ؿ� �ִ� ������ ���� ���ο� ��� �����
        bool spawn = false;
        for (int i = 0; i < players.Length; i++)
        {
            if (lastStair.transform.position.y - 10 < players[i].transform.position.y)
                spawn = true;     
        }

        if (spawn)
            SpawnStair();


    }


    void FirstSpawnStair()
    {
        Stairs newStairs = PhotonNetwork.InstantiateRoomObject("Stairs", spawnPos.transform.position, Quaternion.identity).GetComponent<Stairs>();
            //GameObject.Instantiate(stairPrefab).GetComponent<Stairs>();     
        newStairs.num = 0;
        stairs.Enqueue(newStairs);
        lastStair = newStairs;
        newStairs.transform.SetParent(stairsGroup.transform);
    }

    void SpawnStair()
    {
        //���ο� ���� ��� �����
       
        int nextX = 0;   
        if (lastStair.num == 3)
        {
            nextX = -1;
        }
        else if (lastStair.num == -3)
        {
            nextX = 1;
        }
        else
        {
            nextX = Random.Range(0, 2) == 0 ? -1 : 1;        
        }

        //Stairs newStairs = GameObject.Instantiate(stairPrefab).GetComponent<Stairs>()   ;
        Vector3 nextPos = new Vector3(nextX , 1);
        nextPos = lastStair.transform.position + nextPos;

        GameObject newStairObj = PhotonNetwork.InstantiateRoomObject("Stairs", nextPos, Quaternion.identity);
        Stairs newStair = newStairObj.GetComponent<Stairs>();
        newStair.transform.SetParent(stairsGroup.transform);      
        newStair.num = lastStair.num + nextX;
        stairs.Enqueue(newStair);
        lastStair = newStair;
    }

    void CheckStair()
    {
        RaycastHit2D hit = Physics2D.Raycast(myPlayer.transform.position, Vector3.down, 1.0f);
        if(hit)
        {
            Debug.Log("���");
            nextTimer -= 0.1f;
            if (nextTimer <= 0.5f)
                nextTimer = 0.5f;

            timer = nextTimer;
            gageBar.fillAmount = timer / nextTimer;

            score += 1;
            scroe_Txt.text = score.ToString();

            playerHash = PhotonNetwork.LocalPlayer.CustomProperties;
            if (playerHash.ContainsKey("StairGameScore"))
                playerHash["StairGameScore"] = (int)playerHash["StairGameScore"] + 1;
            else
                playerHash.Add("StairGameScore", 1);

            PhotonNetwork.LocalPlayer.SetCustomProperties(playerHash);

        }
        else
        {
            Debug.Log("����");
            NoStair();
        }
    }

    void NoStair()
    {   
        GameOver();
    }

    void GameOver()
    {
    
        myPlayer.SetHit();
        pv.RPC("GameEnd", RpcTarget.AllViaServer);   
    }

    [PunRPC]
    void GameEnd()
    {
        game = false; 
        StartCoroutine(GameEnd_Co());
    }

    IEnumerator GameEnd_Co()
    {

        ResultPanel.SetActive(true);
        GamePanel.SetActive(false);

        yield return new WaitForSeconds(0.5f);
        otherSprite.color = Color.white;
        players[0].transform.position = Vector3.zero;
        players[1].transform.position = Vector3.zero;

        camera.transform.position = new Vector3(0, 0, camera.transform.position.z);
        BG.transform.position = new Vector3(0, 0, BG.transform.position.z);

        while (stairs.Count > 0)
        {
            PhotonNetwork.Destroy(stairs.Dequeue().gameObject);
        }
        stairs.Clear();


        winOrLose.text = "�����..";
        winOrLose.color = Color.green;

        myScoreTxt.text = "0";
        otherScoreTxt.text = "0";
        myNickTxt.text = PhotonNetwork.LocalPlayer.NickName;
        otherNickTxt.text = PhotonNetwork.PlayerListOthers[0].NickName;

        yield return new WaitForSeconds(1.5f);
    
        int myscore = score;
        int otherscore = PhotonNetwork.PlayerListOthers[0].CustomProperties.ContainsKey("StairGameScore") ? (int)PhotonNetwork.PlayerListOthers[0].CustomProperties["StairGameScore"] : 0;


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

        myPlayer.isMove = true;

        ok_Btn.SetActive(true);
    }

   

    public void OnOkBtn()   //���� ������ Ȯ�ι�ư ������
    {
        //�̴ϰ��� ����
        ok_Btn.SetActive(false);
        ResultPanel.SetActive(false);
        this.gameObject.SetActive(false);

       

        //ȭ�� �������ֱ�
        InGame.Inst.SetLobby();
    }


}
