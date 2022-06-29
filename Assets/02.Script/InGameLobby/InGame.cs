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
    RememberGame,
    Last
}

public class InGame : MonoBehaviourPunCallbacks 
{
    public static InGame Inst; //싱글턴을 위한

    PhotonView pv;  //포톤 동기화를 위한 포톤뷰
    

    //캐릭터 스폰 위치
    public GameObject spawnPos; //게임입장시 중심 스폰위치



    //동기화를 위한 변수 선언
    ExitGames.Client.Photon.Hashtable playerHash; 
   
    [Header("UI")]
    public GameObject roomCanavas;  //게임 방 정보 창   
    public GameObject pallet;       //닉네임 색깔 창
    public GameObject configAndLobbyBtn;  //설정 버튼 나가기버튼
    public Button readyBtn;                 //레디 버튼    ... 반장은 없는 버튼
    public Text readyTxt;                     //레디 버튼 (준비완료, 준비) 를 나타낼 텍스트
    public Button StartBtn;                  //시작 버튼 .... 반장만 나올 버튼
   
  
    public GameObject roomMark; //방장마크

    public TextMeshProUGUI myNickName;               // 내닉네임 
    public TextMeshProUGUI otherNickName;             //상대 닉네임
    public TextMeshProUGUI myWinCountTxt;   //내 승점
    public TextMeshProUGUI otherWinCountTxt;    //상대 승점
    public Button soundBtn; //사운드 설정버튼

    [Header("ResultUI")]
    public ResultUI resultUI;

    [Header("Game")]
    public GameRollController GameRoll; //게임 선택을 위한 룰러 
    public Game[] MiniGame;       //미니게임이 담겨있는

    TalkBox talkBox;

    
    public  MyColor myNickNameColor = MyColor.black;
    public  MyColor otherNickNameColor = MyColor.black;

    //캐릭터
    public PlayerCharacter[] playerCharacters = new PlayerCharacter[2];
    bool isReady = false;   //레디 상태

    private void Awake()
    {
        Inst = this;



        //PhotonView 컴포넌트 할당
        pv = GetComponent<PhotonView>();
        //자기자신캐릭터 생성하는 함수 호출

        if (playerHash == null)
            playerHash = new ExitGames.Client.Photon.Hashtable();

        talkBox = GetComponent<TalkBox>();

        if (PhotonNetwork.PlayerListOthers.Length > 0)
            otherNickNameColor = PhotonNetwork.PlayerListOthers[0].CustomProperties.ContainsKey("NickColor") ? (MyColor)((int)PhotonNetwork.PlayerListOthers[0].CustomProperties["NickColor"]) : MyColor.black;

        GameRoll.GameCount = (int)GameType.Last;
    }

    // Start is called before the first frame update
    void Start()
    {
        SoundMgr.Inst.PlayBGM("InGame");
               
        SetResolution();
        //방에 입장에 성공적이면 통신 시작
        PhotonNetwork.IsMessageQueueRunning = true;
        //패널 셋팅
        InitPanel();
        
        //플레이어들 만들기
        CreatePlayer();

        //플레이어 정보 SetCustomProperties 시키기
        playerHash.Add("winCount", 0);
        playerHash.Add("ready", false);

        PhotonNetwork.LocalPlayer.SetCustomProperties(playerHash);

        PhotonNetwork.CurrentRoom.IsVisible = true;
    }




    void InitPanel() //입장시 패널 설정
    {
        //사운드 설정하는 버튼 함수 연결
        soundBtn.onClick.AddListener(SoundMgr.Inst.OnSoundCtrlBox);
        
        //패널 셋팅,버튼 UI 셋팅하기       
        StartBtn.gameObject.SetActive(false);

        //내닉네임 적용
        myNickName.text = "<color=" + myNickNameColor.ToString() + ">" + PhotonNetwork.LocalPlayer.NickName + "</color>"; 


        if (PhotonNetwork.IsMasterClient) //방장일경우
        {
            readyBtn.gameObject.SetActive(false);
            roomMark.transform.SetParent(myNickName.transform, false);
        }
        else
        {
            readyBtn.gameObject.SetActive(true);
            roomMark.transform.SetParent(otherNickName.transform, false);
        }

        
        SetWinCount();
    }

  
    public void SetResolution() //로비 메니저로 갈예정
    {
        int setWidth = 720; // 사용자 설정 너비
        int setHeight = 1280; // 사용자 설정 높이
        Screen.SetResolution(setWidth, setHeight, false); // SetResolution 함수 제대로 사용하기

    }
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
     
        PhotonNetwork.Instantiate("PlayerChacter/Player", a_HPos, Quaternion.identity, 0);
 
    }


#region RoomController   //서버 관련 함수
    public void LeftRoom() //방을나갈려는 함수 //나가기 버튼에 연결
    {
        SoundMgr.Inst.PlayEffect("Button");
        //저장된 CustomProperties 초기화 시켜주기
        PhotonNetwork.LocalPlayer.CustomProperties.Clear();

        //만약 방장이면   룸에 대한CustomProperties 초기화 시켜주기
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.CurrentRoom.CustomProperties.Clear();       
        }

        //캐릭터 삭제
        PhotonNetwork.Destroy(playerCharacters[0].gameObject);

        //방을 떠나고 다시 로비로 접속하기
        PhotonNetwork.LeaveRoom();
        PhotonNetwork.JoinLobby();

        //씬 전환해주기
        //SceneManager.LoadScene("ServerLobby");

        LoadMgr.Inst.LoadScene("ServerLobby");

    }
    //public override void OnPlayerEnteredRoom(Player newPlayer) //누군가 들어오면
    //{
    //    otherNickName.text = "<color="+ otherNickNameColor.ToString() +">" + newPlayer.NickName + "</color>";
    //    otherWinCountTxt.text = "";
      
    //}

    public override void OnPlayerLeftRoom(Player otherPlayer) //만약 다른 플레이어가 나간다면 
    {
        otherNickName.text = "플레이어 기달리는 중...";
        otherWinCountTxt.text = "";
       

        StartBtn.gameObject.SetActive(false);
        //레디마크 off
        playerCharacters[0].Ready(false);
        readyBtn.gameObject.SetActive(false);

        //방장 마크 옮기기
        playerCharacters[0].starImg.SetActive(true);
        roomMark.transform.SetParent(myNickName.transform, false);

    }


    #endregion

#region GameController 게임 진행 관련 함수
    public void ReadyBtn()  //레디 버튼 
    {
        SoundMgr.Inst.PlayEffect("Button");

        isReady = !isReady;

        if (isReady)
        {
            readyTxt.text = "준비완료";
        }
        else
        {
            readyTxt.text = "준비하기";
        }

        //레디 정보를 저장해서 방장쪽에서 확인할수 있게 한다.
        playerHash = PhotonNetwork.LocalPlayer.CustomProperties;
        playerHash["ready"] = isReady;
        PhotonNetwork.LocalPlayer.SetCustomProperties(playerHash);
   

        //캐릭터 레디 표시
        playerCharacters[0].Ready(isReady);
    }

    //OnPlayerProperties 변화를 감지해서 레디를 확인해서 게임 시작할수 있도록
    public override void OnPlayerPropertiesUpdate(Player targetPlayer
                 , ExitGames.Client.Photon.Hashtable changedProps)
    {
        if (!PhotonNetwork.LocalPlayer.IsMasterClient)
            return;

        if (targetPlayer != PhotonNetwork.LocalPlayer)
        { 
            if (changedProps.ContainsKey("ready"))
            {   
                StartBtn.gameObject.SetActive((bool)changedProps["ready"]);
            }
        }
    }

    void CheckReady()
    {
        if (!PhotonNetwork.LocalPlayer.IsMasterClient)
            return;

        if (PhotonNetwork.PlayerListOthers.Length > 0)
            StartBtn.gameObject.SetActive((bool)PhotonNetwork.PlayerListOthers[0].CustomProperties["ready"]);
    }

    public void GameStartBtn() //반장만 누를수 있는버튼
    {
        SoundMgr.Inst.PlayEffect("Button");
        //모든 플레이어에게 전달해 게임을진행한다.
        pv.RPC("GameSelStart", RpcTarget.AllViaServer);
    }
  
    [PunRPC]
    public void GameSelStart()
    {
        //채팅창 클리어
        talkBox.ClearText();

        //준비완료 한거 준비하기로 초기화
        readyTxt.text = "준비하기";
        isReady = false;

        playerHash = PhotonNetwork.LocalPlayer.CustomProperties;
        playerHash["ready"] = isReady;
        PhotonNetwork.LocalPlayer.SetCustomProperties(playerHash);
        roomCanavas.SetActive(false);
        pallet.SetActive(false);
        configAndLobbyBtn.SetActive(false);


        GameRoll.roller.SetActive(true);
    
        //두번째 플레이어 등록
        playerCharacters[1] = GameObject.FindGameObjectWithTag("OtherPlayer").GetComponent<PlayerCharacter>();

        playerCharacters[0].Ready(false);

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

        // 정해진 미니게임 시작하기
        pv.RPC("StartMiniGame", RpcTarget.AllViaServer, curGame);
    }

    [PunRPC]
    void StartMiniGame(int idx)//정해진 미니게임 활성화 하기
    {     
       
        MiniGame[idx].StartGame();
       
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

        if (PhotonNetwork.LocalPlayer.CustomProperties.ContainsKey("winCount"))
        {
            int winCount = (int)PhotonNetwork.LocalPlayer.CustomProperties["winCount"];
            if (winCount != 0)
                myWinCountTxt.text = winCount + "승";
            else
                myWinCountTxt.text = "";
        }
        else
            myWinCountTxt.text = "";



        if (PhotonNetwork.PlayerListOthers[0].CustomProperties.ContainsKey("winCount"))
        {          
          int  winCount = (int)PhotonNetwork.PlayerListOthers[0].CustomProperties["winCount"];
            if (winCount != 0)
                otherWinCountTxt.text = winCount + "승";
            else
                otherWinCountTxt.text = "";
        }
        else
            otherWinCountTxt.text = "";




    }

    public void ShowResult()//결과창 보여주기
    {
        resultUI.SetResult();   
    }

    public void SetLobby()// 로비 창 새로고침
    {
        roomCanavas.SetActive(true);
        pallet.SetActive(true);
        configAndLobbyBtn.SetActive(true);

        isReady = false;
        StartBtn.gameObject.SetActive(false);

        if (PhotonNetwork.LocalPlayer.IsMasterClient)
            readyBtn.gameObject.SetActive(false);
        else
            readyBtn.gameObject.SetActive(true);

        SetWinCount();

        CheckReady();
    }

  public  void ChangeNickColor()
    {
        myNickName.text = "<color=" + myNickNameColor.ToString() + ">" + PhotonNetwork.LocalPlayer.NickName + "</color>";
        playerCharacters[0].ChangeNickName(myNickNameColor);

        if (PhotonNetwork.PlayerListOthers.Length > 0 && !otherNickNameColor.Equals(MyColor.end))    
            otherNickName.text = "<color=" + otherNickNameColor.ToString() + ">" + PhotonNetwork.PlayerListOthers[0].NickName + "</color>";        
        

    }

#endregion

   

}
