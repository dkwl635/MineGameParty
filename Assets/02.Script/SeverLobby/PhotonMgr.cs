using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;



public class PhotonMgr : MonoBehaviourPunCallbacks
{
    //�÷��̾� �г���
    public InputField userNick;
    //Room Name;
    public InputField roomName;

    //�����������ϱ�
    public Button joinRandomRoomBtn;
    //������ϱ�
    public Button createRoomBtn;

    //--------�� ��� ������ ���� ������
    //RoomItemdl ���ϵ�� ������ Parent ��ü
    public GameObject scrollContents;
    //�� ��ϸ�ŭ ������ RoomItem ������
    public GameObject roomItem;
    List<RoomInfo> myList = new List<RoomInfo>();

    //���� �̸�
    const string buttonSound = "Button";


    private void Awake()
    {    
        PhotonNetwork.SendRate = 60;            
        PhotonNetwork.SerializationRate = 30;

        //if (!PhotonNetwork.IsConnected)
        //{
        //    //1��, ���� Ŭ���忡 ����
        //    PhotonNetwork.ConnectUsingSettings();
        //    //���� ������ ���ӽõ�(���� ���� ����) -> AppID ����� ���� 
        //    //-> �κ� ���� ����
        //}

        //userNick.text = GetUserNick();
    }

    private void Start()
    {
        if (!PhotonNetwork.IsConnected)
        {
            //1��, ���� Ŭ���忡 ����
            PhotonNetwork.ConnectUsingSettings();
            //���� ������ ���ӽõ�(���� ���� ����) -> AppID ����� ���� 
            //-> �κ� ���� ����
        }
        else if (!PhotonNetwork.InLobby)
            OnJoinedLobby();

        userNick.text = GetUserNick();
    }

    //����� �г��� ��������
    string GetUserNick()
    {
        string userNick = PlayerPrefs.GetString("USER_Nick");

        //����� �г����� ���ٸ�
        if (string.IsNullOrEmpty(userNick))  
            userNick = "";
        
        return userNick;
    }

    bool CheckNickName()//�г����� ����ִ��� Ȯ��
    {
        string nick = userNick.text;

        //����� �г����� ���ٸ�
        if (string.IsNullOrEmpty(nick))
            return false;

        return true;
    }

#if UNITY_EDITOR && !(UNITY_IPHONE || UNITY_ANDROID)
    void OnGUI()    //���� ���ӻ��¸� ���� ���Ͽ�
    {
        string a_str = PhotonNetwork.NetworkClientState.ToString();
        //���� ������ ���¸� string���� ������ �ִ� �Լ�
        GUI.Label(new Rect(10, 1, 1500, 60),
                    "<color=#ff0000><size=50>" + a_str + "</size></color>");
    }
    #endif

    public override void OnConnectedToMaster()
    {
        Debug.Log("���� ���� �Ϸ�");
        //�ܼ� ���� ���� ���Ӹ� �� ���� (ConnectedToMaster)   
        PhotonNetwork.JoinLobby();
    }

    //PhotonNetwork.JoinLobby() ������ ȣ��Ǵ� �κ� ���� �ݹ��Լ�
    public override void OnJoinedLobby()
    {   
        Debug.Log("�κ����ӿϷ�");
        //�濡�� �κ�� ���� ���� ���� ID�� �ϳ� ������ �־�� �Ѵ�.
    }

    //���� �� ��ư Ŭ�� �� ȣ��Ǵ� �Լ�
    public void ClickJoinRandomRoom()         //3�� �� ���� ��û ��ư ����
    {
        //��ư ���� ȿ��
        SoundMgr.Inst.PlayEffect(buttonSound);

        if (!CheckNickName()) //�г��� Ȯ��
            return;

       
        //���� �÷��̾��� �̸��� ����
        PhotonNetwork.LocalPlayer.NickName = userNick.text;   
        //�÷��̾� �̸��� ����
        PlayerPrefs.SetString("USER_ID", userNick.text);

        //�������� ����� ������ ����
        PhotonNetwork.JoinRandomRoom();
    }

    //���� �� ������ ������ ��� ȣ��Ǵ� �ݹ� �Լ�
    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.Log("���� �� ���� ���� (������ ���� �������� �ʽ��ϴ�.)");

        //������ ���ǿ� �´� �� ���� �Լ�
        //������ ���� ���� ����
        string _roomName = "ROOM_" + Random.Range(0, 999).ToString("000");
        RoomOptions roomOptions = new RoomOptions();
        roomOptions.IsOpen = true;     //���� ���� ����
        roomOptions.IsVisible = true;  //�κ񿡼� ���� ���� ����
        roomOptions.MaxPlayers = 2;   //�뿡 ������ �� �ִ� �ִ� ������ ��

        //������ ���ǿ� �´� �� ���� �Լ�
        PhotonNetwork.CreateRoom(_roomName, roomOptions, TypedLobby.Default);
        // ���� ���� ���� ���� ���� ����� ������ ������.
        //(���� ������ Client�� �������� �����ϰ� �� ���̴�.)
    }


    public void ClickCreateRoom()
    {
        //��ư ���� ȿ��
        SoundMgr.Inst.PlayEffect(buttonSound);

        if (!CheckNickName()) //�г��� Ȯ��
            return;

        string _roomName = roomName.text;
        //�� �̸��� ���ų� Null�� ��� �� �̸� ����
        if (string.IsNullOrEmpty(roomName.text))
        {
            _roomName = "ROOM_" + Random.Range(0, 999).ToString("000");
        }

        //���� �÷��̾��� �̸��� ����
        PhotonNetwork.LocalPlayer.NickName = userNick.text;
        //�÷��̾� �̸��� ����
        PlayerPrefs.SetString("USER_ID", userNick.text);

        //������ ���� ���� ����
        RoomOptions roomOptions = new RoomOptions();
        roomOptions.IsOpen = true;     //���� ���� ����
        roomOptions.IsVisible = true;  //�κ񿡼� ���� ���� ����
        roomOptions.MaxPlayers = 2;   //�뿡 ������ �� �ִ� �ִ� ������ ��

        //������ ���ǿ� �´� �� ���� �Լ�
        PhotonNetwork.CreateRoom(_roomName, roomOptions, TypedLobby.Default);
    }

    //PhotonNetwork.CreateRoom() �� �Լ��� ���� �ϸ� ȣ��Ǵ� �Լ�
    //(���� �̸��� ���� ���� �� ������)
    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        Debug.Log("�� ����� ����");
        //�ַ� ���� �̸��� ���� ������ �� ����� ������ �߻��ȴ�.
        Debug.Log(returnCode.ToString()); //���� �ڵ�(ErrorCode Ŭ����)
        Debug.Log(message); //���� �޽���
    }

    //RoomItem ������ ȣ��ȴ�
    public void OnClickRoomItem(string roomName)
    {
        SoundMgr.Inst.PlayEffect(buttonSound);
        //���� �÷��̾��� �̸��� ����
        PhotonNetwork.LocalPlayer.NickName = userNick.text;
        //�÷��̾� �̸��� ����
        PlayerPrefs.SetString("USER_Nick", userNick.text);

        //���ڷ� ���޵� �̸��� �ش��ϴ� ������ ����
        PhotonNetwork.JoinRoom(roomName);
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        int roomCount = roomList.Count;
        for (int i = 0; i < roomCount; i++)
        {
            if (!roomList[i].RemovedFromList)
            {
                if (!myList.Contains(roomList[i]))
                    myList.Add(roomList[i]);
                else
                    myList[myList.IndexOf(roomList[i])] = roomList[i];
            }
            else if (myList.IndexOf(roomList[i]) != -1)
                myList.RemoveAt(myList.IndexOf(roomList[i]));
        }

        //�� ����� �ٽ� �޾��� �� �����ϱ� ���� ������ ������ RoomItem�� ����
        foreach (var obj in GameObject.FindObjectsOfType<RoomItem>())
        {
            Destroy(obj.gameObject);
        }

        //��ũ�� ���� �ʱ�ȭ
        scrollContents.GetComponent<RectTransform>().sizeDelta = Vector2.zero;

        for (int i = 0; i < myList.Count; i++)
        {
            GameObject room = Instantiate(roomItem) as GameObject;
            room.transform.SetParent(scrollContents.transform, false);

            //������ RoomItem�� ǥ���ϱ� ���� �ؽ�Ʈ ���� ����
            RoomItem roomData = room.GetComponent<RoomItem>();
            roomData.roomName = myList[i].Name;
            roomData.connectPlayer = myList[i].PlayerCount;
            roomData.maxPlayer = myList[i].MaxPlayers;

            //�ؽ�Ʈ ������ ǥ��
            roomData.DispRoomData(myList[i].IsOpen);
            }
    }

    //������ ������
    public override void OnJoinedRoom()
    {
        Debug.Log("�� ���� �Ϸ�");
        //�� ������ �̵��ϴ� �ڷ�ƾ ����
        LoadMgr.Inst.LoadScene("InGame");
    }


    




} 
