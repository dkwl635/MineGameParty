using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public enum GameType    //�̴� ���� ����
{
    FallingFruitGame,
    OXGame,
    StairGame,
    RememberGame,
    Last
}

public class InGame : MonoBehaviourPunCallbacks 
{
    public static InGame Inst; //�̱����� ����

    PhotonView pv;  //���� ����ȭ�� ���� �����
    

    //ĳ���� ���� ��ġ
    public GameObject spawnPos; //��������� �߽� ������ġ



    //����ȭ�� ���� ���� ����
    ExitGames.Client.Photon.Hashtable playerHash; 
   
    [Header("UI")]
    public GameObject roomCanavas;  //���� �� ���� â   
    public GameObject pallet;       //�г��� ���� â
    public GameObject configAndLobbyBtn;  //���� ��ư �������ư
    public Button readyBtn;                 //���� ��ư    ... ������ ���� ��ư
    public Text readyTxt;                     //���� ��ư (�غ�Ϸ�, �غ�) �� ��Ÿ�� �ؽ�Ʈ
    public Button StartBtn;                  //���� ��ư .... ���常 ���� ��ư
   
  
    public GameObject roomMark; //���帶ũ

    public TextMeshProUGUI myNickName;               // ���г��� 
    public TextMeshProUGUI otherNickName;             //��� �г���
    public TextMeshProUGUI myWinCountTxt;   //�� ����
    public TextMeshProUGUI otherWinCountTxt;    //��� ����
    public Button soundBtn; //���� ������ư

    [Header("ResultUI")]
    public ResultUI resultUI;

    [Header("Game")]
    public GameRollController GameRoll; //���� ������ ���� �귯 
    public Game[] MiniGame;       //�̴ϰ����� ����ִ�

    TalkBox talkBox;

    
    public  MyColor myNickNameColor = MyColor.black;
    public  MyColor otherNickNameColor = MyColor.black;

    //ĳ����
    public PlayerCharacter[] playerCharacters = new PlayerCharacter[2];
    bool isReady = false;   //���� ����

    private void Awake()
    {
        Inst = this;



        //PhotonView ������Ʈ �Ҵ�
        pv = GetComponent<PhotonView>();
        //�ڱ��ڽ�ĳ���� �����ϴ� �Լ� ȣ��

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
        //�濡 ���忡 �������̸� ��� ����
        PhotonNetwork.IsMessageQueueRunning = true;
        //�г� ����
        InitPanel();
        
        //�÷��̾�� �����
        CreatePlayer();

        //�÷��̾� ���� SetCustomProperties ��Ű��
        playerHash.Add("winCount", 0);
        playerHash.Add("ready", false);

        PhotonNetwork.LocalPlayer.SetCustomProperties(playerHash);

        PhotonNetwork.CurrentRoom.IsVisible = true;
    }




    void InitPanel() //����� �г� ����
    {
        //���� �����ϴ� ��ư �Լ� ����
        soundBtn.onClick.AddListener(SoundMgr.Inst.OnSoundCtrlBox);
        
        //�г� ����,��ư UI �����ϱ�       
        StartBtn.gameObject.SetActive(false);

        //���г��� ����
        myNickName.text = "<color=" + myNickNameColor.ToString() + ">" + PhotonNetwork.LocalPlayer.NickName + "</color>"; 


        if (PhotonNetwork.IsMasterClient) //�����ϰ��
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

  
    public void SetResolution() //�κ� �޴����� ������
    {
        int setWidth = 720; // ����� ���� �ʺ�
        int setHeight = 1280; // ����� ���� ����
        Screen.SetResolution(setWidth, setHeight, false); // SetResolution �Լ� ����� ����ϱ�

    }
void CreatePlayer() //ĳ���� �����
    {
        //������ ��ġ�� ������ֱ�
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


#region RoomController   //���� ���� �Լ�
    public void LeftRoom() //������������ �Լ� //������ ��ư�� ����
    {
        SoundMgr.Inst.PlayEffect("Button");
        //����� CustomProperties �ʱ�ȭ �����ֱ�
        PhotonNetwork.LocalPlayer.CustomProperties.Clear();

        //���� �����̸�   �뿡 ����CustomProperties �ʱ�ȭ �����ֱ�
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.CurrentRoom.CustomProperties.Clear();       
        }

        //ĳ���� ����
        PhotonNetwork.Destroy(playerCharacters[0].gameObject);

        //���� ������ �ٽ� �κ�� �����ϱ�
        PhotonNetwork.LeaveRoom();
        PhotonNetwork.JoinLobby();

        //�� ��ȯ���ֱ�
        //SceneManager.LoadScene("ServerLobby");

        LoadMgr.Inst.LoadScene("ServerLobby");

    }
    //public override void OnPlayerEnteredRoom(Player newPlayer) //������ ������
    //{
    //    otherNickName.text = "<color="+ otherNickNameColor.ToString() +">" + newPlayer.NickName + "</color>";
    //    otherWinCountTxt.text = "";
      
    //}

    public override void OnPlayerLeftRoom(Player otherPlayer) //���� �ٸ� �÷��̾ �����ٸ� 
    {
        otherNickName.text = "�÷��̾� ��޸��� ��...";
        otherWinCountTxt.text = "";
       

        StartBtn.gameObject.SetActive(false);
        //����ũ off
        playerCharacters[0].Ready(false);
        readyBtn.gameObject.SetActive(false);

        //���� ��ũ �ű��
        playerCharacters[0].starImg.SetActive(true);
        roomMark.transform.SetParent(myNickName.transform, false);

    }


    #endregion

#region GameController ���� ���� ���� �Լ�
    public void ReadyBtn()  //���� ��ư 
    {
        SoundMgr.Inst.PlayEffect("Button");

        isReady = !isReady;

        if (isReady)
        {
            readyTxt.text = "�غ�Ϸ�";
        }
        else
        {
            readyTxt.text = "�غ��ϱ�";
        }

        //���� ������ �����ؼ� �����ʿ��� Ȯ���Ҽ� �ְ� �Ѵ�.
        playerHash = PhotonNetwork.LocalPlayer.CustomProperties;
        playerHash["ready"] = isReady;
        PhotonNetwork.LocalPlayer.SetCustomProperties(playerHash);
   

        //ĳ���� ���� ǥ��
        playerCharacters[0].Ready(isReady);
    }

    //OnPlayerProperties ��ȭ�� �����ؼ� ���� Ȯ���ؼ� ���� �����Ҽ� �ֵ���
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

    public void GameStartBtn() //���常 ������ �ִ¹�ư
    {
        SoundMgr.Inst.PlayEffect("Button");
        //��� �÷��̾�� ������ �����������Ѵ�.
        pv.RPC("GameSelStart", RpcTarget.AllViaServer);
    }
  
    [PunRPC]
    public void GameSelStart()
    {
        //ä��â Ŭ����
        talkBox.ClearText();

        //�غ�Ϸ� �Ѱ� �غ��ϱ�� �ʱ�ȭ
        readyTxt.text = "�غ��ϱ�";
        isReady = false;

        playerHash = PhotonNetwork.LocalPlayer.CustomProperties;
        playerHash["ready"] = isReady;
        PhotonNetwork.LocalPlayer.SetCustomProperties(playerHash);
        roomCanavas.SetActive(false);
        pallet.SetActive(false);
        configAndLobbyBtn.SetActive(false);


        GameRoll.roller.SetActive(true);
    
        //�ι�° �÷��̾� ���
        playerCharacters[1] = GameObject.FindGameObjectWithTag("OtherPlayer").GetComponent<PlayerCharacter>();

        playerCharacters[0].Ready(false);

        if (PhotonNetwork.LocalPlayer.IsMasterClient)
            StartCoroutine(StartSelGame());
    }

    IEnumerator StartSelGame()   //���� ������ ������
    {
        yield return null;
        int curGame = GameRoll.Roll(); //�������� �������� ���� ���� ��ȣ

        yield return null;

        while (!GameRoll.EndRoll()) //�������� ���߸�
        {
            yield return null;
        }

        // ������ �̴ϰ��� �����ϱ�
        pv.RPC("StartMiniGame", RpcTarget.AllViaServer, curGame);
    }

    [PunRPC]
    void StartMiniGame(int idx)//������ �̴ϰ��� Ȱ��ȭ �ϱ�
    {     
       
        MiniGame[idx].StartGame();
       
    }

    public void WinGame() //�¸�ī��Ʈ
    {
        //CustomProperties�� ������ ������ �÷��ְ� �������ֱ�
        playerHash = PhotonNetwork.LocalPlayer.CustomProperties;
        if (playerHash.ContainsKey("winCount"))
        {
            playerHash["winCount"] = (int)playerHash["winCount"] + 1;
            PhotonNetwork.LocalPlayer.SetCustomProperties(playerHash);
        }

    }
    void SetWinCount()
    {
        //�κ� â�� ���� �����ֱ�

        if (PhotonNetwork.PlayerListOthers.Length <= 0)
            return;

        if (PhotonNetwork.LocalPlayer.CustomProperties.ContainsKey("winCount"))
        {
            int winCount = (int)PhotonNetwork.LocalPlayer.CustomProperties["winCount"];
            if (winCount != 0)
                myWinCountTxt.text = winCount + "��";
            else
                myWinCountTxt.text = "";
        }
        else
            myWinCountTxt.text = "";



        if (PhotonNetwork.PlayerListOthers[0].CustomProperties.ContainsKey("winCount"))
        {          
          int  winCount = (int)PhotonNetwork.PlayerListOthers[0].CustomProperties["winCount"];
            if (winCount != 0)
                otherWinCountTxt.text = winCount + "��";
            else
                otherWinCountTxt.text = "";
        }
        else
            otherWinCountTxt.text = "";




    }

    public void ShowResult()//���â �����ֱ�
    {
        resultUI.SetResult();   
    }

    public void SetLobby()// �κ� â ���ΰ�ħ
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
