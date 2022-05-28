using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public enum GameState
{
    GS_Ready = 0,
    GS_Playing, 
    GS_GameEnd,
}

public enum GameType
{
    FallingFruitGame,
}

public class InGame : MonoBehaviourPunCallbacks 
{
    public static InGame Inst;

    //캐릭터 스폰 위치
    public GameObject spawnPos;
    PhotonView pv;

    ExitGames.Client.Photon.Hashtable playerHash;
    public ExitGames.Client.Photon.Hashtable PlayerHash { get { return playerHash; } }

    [Header("UI")]
    public GameObject roomCanavas;
    public Button readyBtn;
    public Text readyTxt;
    public Button StartBtn;
    public Text myNickName;
    public Text ohterNickName;

    public TextMeshProUGUI myWinCountTxt;
    public TextMeshProUGUI otherWinCountTxt;


    [Header("Game")]
    public GameRollController GameRoll;
    public GameObject[] MiniGame;


    //내가 조종하는 캐릭터
    public delegate void MoveBtnEvent(int h);
    public MoveBtnEvent MoveStart;
    public MoveBtnEvent MoveEnd;

    [HideInInspector] public  LobbyPlayerController player;
   
    bool isReady = false;   //레디 상태

    private void Awake()
    {
        Inst = this;

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
        PhotonNetwork.IsMessageQueueRunning = true;

        ohterNickName.text = "플레이어 기달리는 중...";

        CreatePlayer();
 
        StartBtn.gameObject.SetActive(false);
        
        if (PhotonNetwork.LocalPlayer.IsMasterClient)
            readyBtn.gameObject.SetActive(false);
        else
            readyBtn.gameObject.SetActive(true);


        playerHash.Add("winCount", 0);
        playerHash.Add("ready", false);
        PhotonNetwork.LocalPlayer.SetCustomProperties(playerHash);  
    }

    private void Update()
    {
        CheckReady();
        SetWinCount();
    }

    void CreatePlayer()
    {
        Vector3 a_HPos = Vector3.zero;
        Vector3 a_AddPos = Vector3.zero;
        GameObject a_HPosObj = GameObject.Find("SpawnPos");
        
        if (a_HPosObj != null)
        {
            a_AddPos.x = Random.Range(-2.0f, 2.0f);      
            a_HPos = spawnPos.transform.position + a_AddPos;
        }

       GameObject character =  PhotonNetwork.Instantiate("PlayerChacter/Player", a_HPos, Quaternion.identity, 0); 
    }

    #region PlayerController
    public void MoveBtnDown(int h)
    {
        MoveStart?.Invoke(h); 
    }
    public void  MoveBtnUp(int h)
    {
        MoveEnd?.Invoke(h);
    }
    #endregion

    #region RoomController  
    public void LeftRoom()
    {
        PhotonNetwork.LocalPlayer.CustomProperties.Clear();
       
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.CurrentRoom.CustomProperties.Clear();       
        }

        PhotonNetwork.LeaveRoom();
        PhotonNetwork.JoinLobby();

        SceneManager.LoadScene("ServerLobby");
       
    }

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

    #region GameController
    public void ReadyBtn()
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

        playerHash["ready"] = isReady;
        PhotonNetwork.LocalPlayer.SetCustomProperties(playerHash);
    }
    public void GameStartBtn()
    {
        pv.RPC("GameSelStart", RpcTarget.AllBufferedViaServer);
    }
    public void CheckReady()
    {
        if (!PhotonNetwork.LocalPlayer.IsMasterClient)
        {
            return;
        }

        if (PhotonNetwork.PlayerListOthers.Length <= 0)
            return;

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
        int curGame = GameRoll.Roll();
        yield return null;

        while (!GameRoll.EndRoll())
        {
            yield return null;
        }

        yield return new WaitForSeconds(2.0f);


        pv.RPC("StartMiniGame", RpcTarget.AllBufferedViaServer, 0);
    }

    [PunRPC]
    void StartMiniGame(int idx)
    {
        GameRoll.gameObject.SetActive(false);
        MiniGame[idx].gameObject.SetActive(true);
    }

    public void WinGame() //승리카운트
    {
        playerHash = PhotonNetwork.LocalPlayer.CustomProperties;
        if (playerHash.ContainsKey("winCount"))
        {
            playerHash["winCount"] = (int)playerHash["winCount"] + 1;
            PhotonNetwork.LocalPlayer.SetCustomProperties(playerHash);
        }

    }
    void SetWinCount()
    {
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
    public void SetLobby()
    {
        roomCanavas.SetActive(true);
        GameRoll.gameObject.SetActive(false);

        isReady = false;

        StartBtn.gameObject.SetActive(false);

        if (PhotonNetwork.LocalPlayer.IsMasterClient)
            readyBtn.gameObject.SetActive(false);
        else
            readyBtn.gameObject.SetActive(true);

    }
    #endregion


}
