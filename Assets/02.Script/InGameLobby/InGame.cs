using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public enum GameType    //미니 게임 종류
{
    FallingFruitGame,
    OXGame,
    StairGame,
    RememberGame
}

public class InGame : MonoBehaviourPunCallbacks 
{
    public static InGame Inst; //싱글턴을 위한

    //캐릭터 스폰 위치
    public GameObject spawnPos; //게임입장시 중심 스폰위치
    PhotonView pv;  //포톤 동기화를 위한 포톤뷰

    //동기화를 위한 변수 선언
    ExitGames.Client.Photon.Hashtable playerHash; 
  
   
    [Header("UI")]
    public GameObject roomCanavas;  //게임 방 정보 창
    public Button readyBtn;                 //레디 버튼    ... 반장은 없는 버튼
    public Text readyTxt;                     //레디 버튼 (준비완료, 준비) 를 나타낼 텍스트
    public Button StartBtn;                  //시작 버튼 .... 반장만 나올 버튼
    public Text myNickName;               // 내닉네임 
    public Text ohterNickName;             //상대 닉네임

    public TextMeshProUGUI myWinCountTxt;   //내 승점
    public TextMeshProUGUI otherWinCountTxt;    //상대 승점


    [Header("Game")]
    public GameRollController GameRoll; //게임 선택을 위한 룰러 
    public GameObject[] MiniGame;       //미니게임이 담겨있는

    //입장한 플레이어 목록
   public PlayerCharacter[] playerCharacters = new PlayerCharacter[2];


    bool isReady = false;   //레디 상태

    private void Awake()
    {
        Inst = this;

        //전체적인 프레임을 맞춰주기 위한
        Application.targetFrameRate = 60;

        //PhotonView 컴포넌트 할당
        pv = GetComponent<PhotonView>();
        //자기자신캐릭터 생성하는 함수 호출

        if (playerHash == null)
            playerHash = new ExitGames.Client.Photon.Hashtable();
    }

    // Start is called before the first frame update
    void Start()
    {
//#if (UNITY_ANDROID)
        //해상도 잡기
        SetResolution();
//#endif


        //방에 입장에 성공적이면 통신 시작
        PhotonNetwork.IsMessageQueueRunning = true;

        //첫 셋팅
        ohterNickName.text = "플레이어 기달리는 중...";


        //플레이어들 만들기
        CreatePlayer();
 
        //버튼 UI 셋팅하기
        StartBtn.gameObject.SetActive(false);
        
        if (PhotonNetwork.LocalPlayer.IsMasterClient)
            readyBtn.gameObject.SetActive(false);
        else
            readyBtn.gameObject.SetActive(true);


        //플레이어 정보 SetCustomProperties 시키기
        playerHash.Add("winCount", 0);
        playerHash.Add("ready", false);
        PhotonNetwork.LocalPlayer.SetCustomProperties(playerHash);  

    }
    

    private void Update()
    {
        //실시간 레디 체크
        CheckReady();
        //SetWinCount();

        //test 
        PlayerCharacter[] plays = FindObjectsOfType<PlayerCharacter>();
        for (int i = 0; i < plays.Length; i++)
        {
            if (plays[i].pv.IsMine)
                playerCharacters[0] = plays[i];
            else
                playerCharacters[1] = plays[i];
        }

    }

//    #if (UNITY_ANDROID)
    /* 해상도 설정하는 함수 */
    public void SetResolution()
    {
        int setWidth = 720; // 사용자 설정 너비
        int setHeight = 1280; // 사용자 설정 높이

        int deviceWidth = Screen.width; // 기기 너비 저장
        int deviceHeight = Screen.height; // 기기 높이 저장

        Screen.SetResolution(setWidth, setHeight, false); // SetResolution 함수 제대로 사용하기


    //    if ((float)setWidth / setHeight < (float)deviceWidth / deviceHeight) // 기기의 해상도 비가 더 큰 경우
    //    {
    //        float newWidth = ((float)setWidth / setHeight) / ((float)deviceWidth / deviceHeight); // 새로운 너비
    //        Camera.main.rect = new Rect((1f - newWidth) / 2f, 0f, newWidth, 1f); // 새로운 Rect 적용
    //    }
    //    else // 게임의 해상도 비가 더 큰 경우
    //    {
    //        float newHeight = ((float)deviceWidth / deviceHeight) / ((float)setWidth / setHeight); // 새로운 높이
    //        Camera.main.rect = new Rect(0f, (1f - newHeight) / 2f, 1f, newHeight); // 새로운 Rect 적용
    //    }
    }
//#endif

void CreatePlayer() //캐릭터 만들기
    {
        //랜덤한 위치에 만들어주기
        Vector3 a_HPos = Vector3.zero;
        Vector3 a_AddPos = Vector3.zero;
        GameObject a_HPosObj = GameObject.Find("SpawnPos");
        
        if (a_HPosObj != null)
        {
            a_AddPos.x = Random.Range(-2.0f, 2.0f);      
            a_HPos = spawnPos.transform.position + a_AddPos;
        }

       

        //포톤으로 만들어져 다른 클라에도 방에 들어오면 똑같이 만들어진다.
        //하지만 캐릭터 쪽에서 위치 동기화가 진행되어 만들어지자마자 다른 위치로 이동된다.
        //내 케릭터만 떨어지는 모습을 볼수 있다.
        GameObject playerObj = PhotonNetwork.Instantiate("PlayerChacter/"+ UserData.charName, a_HPos, Quaternion.identity, 0);    
    }


#region RoomController   //서버 관련 함수
    public void LeftRoom() //방을나갈려는 함수
    {
        //저장된 CustomProperties 초기화 시켜주기
        PhotonNetwork.LocalPlayer.CustomProperties.Clear();

        //만약 방장이면   룸에 대한CustomProperties 초기화 시켜주기
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.CurrentRoom.CustomProperties.Clear();       
        }

        //방을 떠나고 다시 로비로 접속하기
        PhotonNetwork.LeaveRoom();
        PhotonNetwork.JoinLobby();

        //씬 전환해주기
        SceneManager.LoadScene("ServerLobby");
       
    }

    //만액 다른 플레이어가 나간다면 // 이함수는 방에있는 모든 플레이어가 호출된다. 하지만 방에는 2명 제한이라
    //1명만 남겠된다.
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        ohterNickName.text = "플레이어 기달리는 중...";
        Debug.Log(otherPlayer.NickName + "나감");

        StartBtn.gameObject.SetActive(false);

        if (PhotonNetwork.LocalPlayer.IsMasterClient)
            readyBtn.gameObject.SetActive(false);
        else
            readyBtn.gameObject.SetActive(true);
    }



#endregion

#region GameController 게임 진행 관련 함수
    public void ReadyBtn()  //레디 버튼 
    {
        isReady = !isReady;

        if (isReady)
        {
            readyTxt.text = "준비완료";
        }
        else
        {
            readyTxt.text = "준비";
        }

        //레디 정보를 저장해서 방장쪽에서 확인할수 있게 한다.
        playerHash["ready"] = isReady;
        PhotonNetwork.LocalPlayer.SetCustomProperties(playerHash);
    }
    public void CheckReady() //래디를 확인하는 함수
    {
        //오로지 마스터 클라이언트만 확인 
        if (!PhotonNetwork.LocalPlayer.IsMasterClient)
            return;

        //방에 혼자면 할 필요 없음
        if (PhotonNetwork.PlayerListOthers.Length <= 0)
            return;

        //모든 유저 체크 
        
        bool allReady = true;
        var allPlayer = PhotonNetwork.PlayerListOthers;
        foreach (var player in allPlayer)
        {
            if (player.CustomProperties.ContainsKey("ready"))
            {
                if (!(bool)player.CustomProperties["ready"])
                    allReady = false;
            }
            else
                allReady = false;
        }      
        StartBtn.gameObject.SetActive(allReady);

    }

    public void GameStartBtn() //반장만 누를수 있는버튼
    {
        //모든 플레이어에게 전달해 게임을진행한다.
        pv.RPC("GameSelStart", RpcTarget.AllViaServer);
    }
  
    [PunRPC]
    public void GameSelStart()
    {
        isReady = false;
        readyTxt.text = "준비";
        playerHash["ready"] = isReady;
        PhotonNetwork.LocalPlayer.SetCustomProperties(playerHash);

        roomCanavas.SetActive(false);
        GameRoll.gameObject.SetActive(true);

        if (PhotonNetwork.LocalPlayer.IsMasterClient)
            StartCoroutine(StartSelGame());
    }

    IEnumerator StartSelGame()   //게임 돌림판 돌리기
    {
        yield return null;
        int curGame = GameRoll.Roll(); //돌림판을 돌려나온 다음 게임 번호

        yield return null;

        while (!GameRoll.EndRoll()) //돌림판이 멈추면
        {
            yield return null;
        }

        yield return new WaitForSeconds(2.0f);

        // 정해진 미니게임 시작하기
        pv.RPC("StartMiniGame", RpcTarget.AllBufferedViaServer, 2);
    }

    [PunRPC]
    void StartMiniGame(int idx)
    {
        //정해진 미니게임 활성화 하기
        GameRoll.gameObject.SetActive(false);
        MiniGame[idx].gameObject.SetActive(true);
    }

    public void WinGame() //승리카운트
    {
        //CustomProperties를 가져와 승점을 올려주고 저장해주기
        playerHash = PhotonNetwork.LocalPlayer.CustomProperties;
        if (playerHash.ContainsKey("winCount"))
        {
            playerHash["winCount"] = (int)playerHash["winCount"] + 1;
            PhotonNetwork.LocalPlayer.SetCustomProperties(playerHash);
        }

    }
    void SetWinCount()
    {
        //로비 창에 승점 보여주기

        if (PhotonNetwork.PlayerListOthers.Length <= 0)
            return;

        if (PhotonNetwork.LocalPlayer.CustomProperties.ContainsKey("winCount") && PhotonNetwork.PlayerListOthers[0].CustomProperties.ContainsKey("winCount"))
        {
            int winCount = (int)PhotonNetwork.LocalPlayer.CustomProperties["winCount"];
            if (winCount != 0)
                myWinCountTxt.text = winCount + "승";

            winCount = (int)PhotonNetwork.PlayerListOthers[0].CustomProperties["winCount"];
            if (winCount != 0)
                otherWinCountTxt.text = winCount + "승";
        }

    }
    public void SetLobby()// 로비 창 새로고침
    {
        roomCanavas.SetActive(true);
        GameRoll.gameObject.SetActive(false);

        isReady = false;

        StartBtn.gameObject.SetActive(false);

        if (PhotonNetwork.LocalPlayer.IsMasterClient)
            readyBtn.gameObject.SetActive(false);
        else
            readyBtn.gameObject.SetActive(true);

        SetWinCount();

    }
#endregion

   

}
