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
}

public class InGame : MonoBehaviourPunCallbacks 
{
    public static InGame Inst; //�̱����� ����

    //ĳ���� ���� ��ġ
    public GameObject spawnPos; //��������� �߽� ������ġ
    PhotonView pv;  //���� ����ȭ�� ���� �����

    //����ȭ�� ���� ���� ����
    ExitGames.Client.Photon.Hashtable playerHash; 
  
   
    [Header("UI")]
    public GameObject roomCanavas;  //���� �� ���� â
    public Button readyBtn;                 //���� ��ư    ... ������ ���� ��ư
    public Text readyTxt;                     //���� ��ư (�غ�Ϸ�, �غ�) �� ��Ÿ�� �ؽ�Ʈ
    public Button StartBtn;                  //���� ��ư .... ���常 ���� ��ư
    public Text myNickName;               // ���г��� 
    public Text ohterNickName;             //��� �г���

    public TextMeshProUGUI myWinCountTxt;   //�� ����
    public TextMeshProUGUI otherWinCountTxt;    //��� ����


    [Header("Game")]
    public GameRollController GameRoll; //���� ������ ���� �귯 
    public GameObject[] MiniGame;       //�̴ϰ����� ����ִ�


    bool isReady = false;   //���� ����

    private void Awake()
    {
        Inst = this;

        //��ü���� �������� �����ֱ� ����
        Application.targetFrameRate = 60;

        //PhotonView ������Ʈ �Ҵ�
        pv = GetComponent<PhotonView>();
        //�ڱ��ڽ�ĳ���� �����ϴ� �Լ� ȣ��

        if (playerHash == null)
            playerHash = new ExitGames.Client.Photon.Hashtable();
    }

    // Start is called before the first frame update
    void Start()
    { 
        //�濡 ���忡 �������̸� ��� ����
        PhotonNetwork.IsMessageQueueRunning = true;

        //ù ����
        ohterNickName.text = "�÷��̾� ��޸��� ��...";

        //�÷��̾�� �����
        CreatePlayer();
 
        //��ư UI �����ϱ�
        StartBtn.gameObject.SetActive(false);
        
        if (PhotonNetwork.LocalPlayer.IsMasterClient)
            readyBtn.gameObject.SetActive(false);
        else
            readyBtn.gameObject.SetActive(true);


        //�÷��̾� ���� SetCustomProperties ��Ű��
        playerHash.Add("winCount", 0);
        playerHash.Add("ready", false);
        PhotonNetwork.LocalPlayer.SetCustomProperties(playerHash);  
    }

    private void Update()
    {
        //�ǽð� ���� üũ
        CheckReady(); 
        //SetWinCount();
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

        if (pv.IsMine)      
            a_HPos.z = -1;      
        else
            a_HPos.z = 0;

        //�������� ������� �ٸ� Ŭ�󿡵� �濡 ������ �Ȱ��� ���������.
        //������ ĳ���� �ʿ��� ��ġ ����ȭ�� ����Ǿ� ��������ڸ��� �ٸ� ��ġ�� �̵��ȴ�.
        //�� �ɸ��͸� �������� ����� ���� �ִ�.
        PhotonNetwork.Instantiate("PlayerChacter/Player", a_HPos, Quaternion.identity, 0); 
    }


    #region RoomController   //���� ���� �Լ�
    public void LeftRoom() //������������ �Լ�
    {
        //����� CustomProperties �ʱ�ȭ �����ֱ�
        PhotonNetwork.LocalPlayer.CustomProperties.Clear();

        //���� �����̸�   �뿡 ����CustomProperties �ʱ�ȭ �����ֱ�
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.CurrentRoom.CustomProperties.Clear();       
        }

        //���� ������ �ٽ� �κ�� �����ϱ�
        PhotonNetwork.LeaveRoom();
        PhotonNetwork.JoinLobby();

        //�� ��ȯ���ֱ�
        SceneManager.LoadScene("ServerLobby");
       
    }

    //���� �ٸ� �÷��̾ �����ٸ� // ���Լ��� �濡�ִ� ��� �÷��̾ ȣ��ȴ�. ������ �濡�� 2�� �����̶�
    //1�� ���ڵȴ�.
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        ohterNickName.text = "�÷��̾� ��޸��� ��...";
        Debug.Log(otherPlayer.NickName + "����");

        StartBtn.gameObject.SetActive(false);

        if (PhotonNetwork.LocalPlayer.IsMasterClient)
            readyBtn.gameObject.SetActive(false);
        else
            readyBtn.gameObject.SetActive(true);
    }



    #endregion

    #region GameController ���� ���� ���� �Լ�
    public void ReadyBtn()  //���� ��ư 
    {
        isReady = !isReady;

        if (isReady)
        {
            readyTxt.text = "�غ�Ϸ�";
        }
        else
        {
            readyTxt.text = "�غ�";
        }

        //���� ������ �����ؼ� �����ʿ��� Ȯ���Ҽ� �ְ� �Ѵ�.
        playerHash["ready"] = isReady;
        PhotonNetwork.LocalPlayer.SetCustomProperties(playerHash);
    }
    public void CheckReady() //���� Ȯ���ϴ� �Լ�
    {
        //������ ������ Ŭ���̾�Ʈ�� Ȯ�� 
        if (!PhotonNetwork.LocalPlayer.IsMasterClient)
            return;

        //�濡 ȥ�ڸ� �� �ʿ� ����
        if (PhotonNetwork.PlayerListOthers.Length <= 0)
            return;

        //��� ���� üũ 
        
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

    public void GameStartBtn() //���常 ������ �ִ¹�ư
    {
        //��� �÷��̾�� ������ �����������Ѵ�.
        pv.RPC("GameSelStart", RpcTarget.AllViaServer);
    }
  
    [PunRPC]
    public void GameSelStart()
    {
        isReady = false;
        readyTxt.text = "�غ�";
        playerHash["ready"] = isReady;
        PhotonNetwork.LocalPlayer.SetCustomProperties(playerHash);

        roomCanavas.SetActive(false);
        GameRoll.gameObject.SetActive(true);

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

        yield return new WaitForSeconds(2.0f);

        // ������ �̴ϰ��� �����ϱ�
        pv.RPC("StartMiniGame", RpcTarget.AllBufferedViaServer, 1);
    }

    [PunRPC]
    void StartMiniGame(int idx)
    {
        //������ �̴ϰ��� Ȱ��ȭ �ϱ�
        GameRoll.gameObject.SetActive(false);
        MiniGame[idx].gameObject.SetActive(true);
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

        if (PhotonNetwork.LocalPlayer.CustomProperties.ContainsKey("winCount") && PhotonNetwork.PlayerListOthers[0].CustomProperties.ContainsKey("winCount"))
        {
            int winCount = (int)PhotonNetwork.LocalPlayer.CustomProperties["winCount"];
            if (winCount != 0)
                myWinCountTxt.text = winCount + "��";

            winCount = (int)PhotonNetwork.PlayerListOthers[0].CustomProperties["winCount"];
            if (winCount != 0)
                otherWinCountTxt.text = winCount + "��";
        }

    }
    public void SetLobby()// �κ� â ���ΰ�ħ
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
