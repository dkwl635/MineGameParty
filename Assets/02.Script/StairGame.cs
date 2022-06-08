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


    //내 플레이어
    public PlayerCharacter myPlayer;

    //캐릭터들
    PlayerCharacter[] players;

    public GameObject GamePanel;          //게임관련 UI
    //계단 프리팹
    public GameObject stairPrefab;
    //계단 처음 스폰 위치
    public GameObject spawnPos;
    //소환된 계단들을 담아둘 링크드 리스트
    Queue<Stairs> stairs = new Queue<Stairs>();
    //마지막 계단을 확인하기 위해
    Stairs lastStair;

    //소환된 계단을 담아둘 게임으오브젝트
    public GameObject stairsGroup;
    //카메라
    GameObject camera;
    //배경
    GameObject BG;

    //점수 텍스트
    int score = 0;
    public TextMeshProUGUI scroe_Txt;

    //제한시간
    public Image gageBar;
    float nextTimer = 2.0f;
    float timer = 2.0f;

    //게임 진행 중
    bool game = false;

    //상대 Sprite 투명도를 주기위해
    SpriteRenderer otherSprite;

    //시작 카운트
    public TextMeshProUGUI countText;

    //동기화를 위한 변수 선언
    ExitGames.Client.Photon.Hashtable playerHash;


    [Header("ResultPanel")]
    public GameObject ResultPanel;          //결과창
   
    public TextMeshProUGUI myNickTxt;       //내 닉네임 
    public TextMeshProUGUI otherNickTxt;    //상대 닉네임
    public TextMeshProUGUI myScoreTxt;      // 내 점수
    public TextMeshProUGUI otherScoreTxt;   //상대 점수
    public TextMeshProUGUI winOrLose;       //승리판정

    public GameObject ok_Btn;                   //확인 버튼


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
      
        //맨 마지막 블럭 체크
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

        //맨 위 블럭에서 일정 밑에 있는 유저를 보고 새로운 계단 만들기
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
        //새로운 다음 계단 만들기
       
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
            Debug.Log("계단");
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
            Debug.Log("없음");
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


        winOrLose.text = "결과는..";
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

        //승리 판정하기
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
            InGame.Inst.WinGame();  //본 게임메니저에서 승리 카운트 해주기
        }
        else if (myscore < otherscore)
        {
            winOrLose.text = "패배";
            winOrLose.color = Color.red;
        }

        myPlayer.isMove = true;

        ok_Btn.SetActive(true);
    }

   

    public void OnOkBtn()   //게임 종료후 확인버튼 누르면
    {
        //미니게임 종료
        ok_Btn.SetActive(false);
        ResultPanel.SetActive(false);
        this.gameObject.SetActive(false);

       

        //화면 갱신해주기
        InGame.Inst.SetLobby();
    }


}
