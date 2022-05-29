using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FallingFruitGame : MonoBehaviourPunCallbacks
{
    PhotonView pv;

    enum FruitsType //과일 오브젝트를 불러오기 위한 
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

    public static FallingFruitGame Inst;    //싱글턴 패턴을 위한
    public LobbyPlayerController[] playerObj;   //과일들의 충돌캐릭터들의 거리 계산을 위한 플레이어들 캐릭터변수

    //동기화를 위한 변수 선언
    ExitGames.Client.Photon.Hashtable playerHash;

    public GameObject fruitsSpanwPos; //과일의 중심스폰위치

    //과일 먹고 나올 이펙트
    public GameObject collectObj;   //과일을 먹고 나올 이펙트 프리팹

    [Header("UI")]
    public TextMeshProUGUI scoreTxt;    //점수 
    public TextMeshProUGUI CountTxt;   //남은시간

    public GameObject ResultPanel;          //결과창
    public TextMeshProUGUI myNickTxt;       //내 닉네임 
    public TextMeshProUGUI otherNickTxt;    //상대 닉네임
    public TextMeshProUGUI myScoreTxt;      // 내 점수
    public TextMeshProUGUI otherScoreTxt;   //상대 점수
    public TextMeshProUGUI winOrLose;       //승리판정

    public GameObject ok_Btn;                   //확인 버튼

    private int score = 0;                          //점수 

    
    float timer = 0.0f;                 //게임시간
    float nextSpawnTime = 1.0f; //과일 스폰 주기

    bool gameStart = true;  //게임 상태 bool 변수

    private void Awake()
    {
        Inst = this;
        pv = GetComponent<PhotonView>();
    }

    public override void OnEnable() //추후 게임을 다시 시작할경우 셋팅
    {    
        //씬에 있는 플레이어 오브젝트 불러오기
        playerObj = GameObject.FindObjectsOfType<LobbyPlayerController>();

        //게임 시작
        StartCoroutine(GameStart());

        //점수 셋팅
        SetScoreTxt();
    }

    private void Update()
    {
        if (!gameStart)
            return;

        //과일 스폰은 오로지 클라이언마스터만
        if (!PhotonNetwork.LocalPlayer.IsMasterClient)
            return;
   
        //과일 스폰
        timer += Time.deltaTime;
        if(timer >= nextSpawnTime)
        {
            int randCount = Random.Range(2, 5);
            for (int i = 0; i < randCount; i++)
            {
                SpawnFruits(); //과일 스폰
            }

            timer = 0.0f;
            nextSpawnTime = Random.Range(0.7f, 1.5f);
        }
    }

   
    public void SpawnFruits()   //과일 스폰하기
    {
        //랜덤과일 및 스폰위치 잡기
        int rand = Random.Range(0, (int)FruitsType.Max);
        float randx = Random.Range(-3.0f, 3.0f);
        float randy = Random.Range(-0.3f, 0.3f);

        //과일 리소스폴더에서 가져와 스폰하기.. 포톤으로 소환하여 모든 클라에게 전달
        string name = ((FruitsType)Random.Range(0, (int)FruitsType.Max)).ToString();
        PhotonNetwork.InstantiateRoomObject("Fruits/" + name, fruitsSpanwPos.transform.position + Vector3.right * randx + Vector3.up * randy, Quaternion.identity);  
    }

    
    public void AddScore(LobbyPlayerController player , Vector3 pos)//과일 충돌시 점수와 이펙트 소환
    {
        ExitGames.Client.Photon.Hashtable playerHash = player.pv.Owner.CustomProperties;
        //플레이어 에게 점수 적용시켜주기 
        //포톤 플레이어 SetCustomProperties을 이용하여 동기화
        if (playerHash.ContainsKey("score"))
        {
            playerHash["score"] = (int)playerHash["score"] + 100;
            player.pv.Owner.SetCustomProperties(playerHash);

            //PRC 함수를 이용하여 먹은 플레이어의 클라에게 함수호출
            pv.RPC("SetScoreTxt", player.pv.Owner);             //점수 늘렸으니 점수텍스트 갱신해줘
            pv.RPC("SpawnCollect", player.pv.Owner, pos);     //과일 먹은 위치에 이펙트 켜줘
        }          
    }

  
   [PunRPC]
    public void SetScoreTxt()   //점수판 갱신
    {
        //점수는 LocalPlayer.CustomProperties["score"]에 저장되니 예외 처리
        if (PhotonNetwork.LocalPlayer.CustomProperties["score"] == null)
        {
            scoreTxt.text = "0";
            return;
        }
        
        score = (int)PhotonNetwork.LocalPlayer.CustomProperties["score"];
        scoreTxt.text = score.ToString(); 
    }

    [PunRPC]
    void SpawnCollect(Vector3 pos)//과일 먹을시 이펙트 나오게 하기
    {       
        GameObject collect = Instantiate(collectObj, pos, Quaternion.identity);
        Destroy(collect, 0.5f);
    }

    //게임 진행
    IEnumerator GameStart()
    {
        playerHash = pv.Owner.CustomProperties;

        //서버에 점수 데이터 저장하기 및 초기화
        if (!playerHash.ContainsKey("score"))
            playerHash.Add("score", 0);
        else
            playerHash["score"] = 0;

        score = 0;
        scoreTxt.text = score.ToString();

        //각 게임 초기화 진행
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

        //결과창 오픈
        ResultPanel.SetActive(true);
        myNickTxt.text = PhotonNetwork.LocalPlayer.NickName;
        otherNickTxt.text = PhotonNetwork.PlayerListOthers[0].NickName;

        int myscore = score;
        int otherscore = (int)PhotonNetwork.PlayerListOthers[0].CustomProperties["score"];


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

        yield return new WaitForSeconds(2.0f);

        ok_Btn.SetActive(true);
    }

    public void OnOkBtn()   //게임 종료후 확인버튼 누르면
    {
        //미니게임 (과일먹기) 종료
        ok_Btn.SetActive(false);
        ResultPanel.SetActive(false);
        this.gameObject.SetActive(false);

        //화면 갱신해주기
        InGame.Inst.SetLobby();
    }


}
