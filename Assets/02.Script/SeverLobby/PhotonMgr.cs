using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;



public class PhotonMgr : MonoBehaviourPunCallbacks
{
    //플레이어 닉네임
    public InputField userNick;
    //Room Name;
    public InputField roomName;

    //랜덤방입장하기
    public Button joinRandomRoomBtn;
    //방생성하기
    public Button createRoomBtn;

    //--------룸 목록 갱신을 위한 변수들
    //RoomItemdl 차일드로 생성될 Parent 객체
    public GameObject scrollContents;
    //룸 목록만큼 생성될 RoomItem 프리팹
    public GameObject roomItem;
    List<RoomInfo> myList = new List<RoomInfo>();

    //사운드 이름
    const string buttonSound = "Button";


    private void Awake()
    {    
        PhotonNetwork.SendRate = 60;            
        PhotonNetwork.SerializationRate = 30;

        //if (!PhotonNetwork.IsConnected)
        //{
        //    //1번, 포톤 클라우드에 접속
        //    PhotonNetwork.ConnectUsingSettings();
        //    //포톤 서버에 접속시도(지역 서버 접속) -> AppID 사용자 인증 
        //    //-> 로비 입장 진행
        //}

        //userNick.text = GetUserNick();
    }

    private void Start()
    {
        if (!PhotonNetwork.IsConnected)
        {
            //1번, 포톤 클라우드에 접속
            PhotonNetwork.ConnectUsingSettings();
            //포톤 서버에 접속시도(지역 서버 접속) -> AppID 사용자 인증 
            //-> 로비 입장 진행
        }
        else if (!PhotonNetwork.InLobby)
            OnJoinedLobby();

        userNick.text = GetUserNick();
    }

    //저장된 닉네임 가져오기
    string GetUserNick()
    {
        string userNick = PlayerPrefs.GetString("USER_Nick");

        //저장된 닉네임이 없다면
        if (string.IsNullOrEmpty(userNick))  
            userNick = "";
        
        return userNick;
    }

    bool CheckNickName()//닉네임이 비어있는지 확인
    {
        string nick = userNick.text;

        //저장된 닉네임이 없다면
        if (string.IsNullOrEmpty(nick))
            return false;

        return true;
    }

#if UNITY_EDITOR && !(UNITY_IPHONE || UNITY_ANDROID)
    void OnGUI()    //현재 접속상태를 보기 위하여
    {
        string a_str = PhotonNetwork.NetworkClientState.ToString();
        //현재 포톤의 상태를 string으로 리턴해 주는 함수
        GUI.Label(new Rect(10, 1, 1500, 60),
                    "<color=#ff0000><size=50>" + a_str + "</size></color>");
    }
    #endif

    public override void OnConnectedToMaster()
    {
        Debug.Log("서버 접속 완료");
        //단순 포톤 서버 접속만 된 상태 (ConnectedToMaster)   
        PhotonNetwork.JoinLobby();
    }

    //PhotonNetwork.JoinLobby() 성공시 호출되는 로비 접속 콜백함수
    public override void OnJoinedLobby()
    {   
        Debug.Log("로비접속완료");
        //방에서 로비로 나올 때도 유저 ID를 하나 셋팅해 주어야 한다.
    }

    //랜덤 방 버튼 클릭 시 호출되는 함수
    public void ClickJoinRandomRoom()         //3번 방 입장 요청 버튼 누름
    {
        //버튼 사운드 효과
        SoundMgr.Inst.PlayEffect(buttonSound);

        if (!CheckNickName()) //닉네임 확인
            return;

       
        //로컬 플레이어의 이름을 설정
        PhotonNetwork.LocalPlayer.NickName = userNick.text;   
        //플레이어 이름을 저장
        PlayerPrefs.SetString("USER_ID", userNick.text);

        //무작위로 추출된 방으로 입장
        PhotonNetwork.JoinRandomRoom();
    }

    //랜덤 방 입장이 실패한 경우 호출되는 콜백 함수
    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.Log("랜덤 방 참가 실패 (참가할 방이 존재하지 않습니다.)");

        //지정한 조건에 맞는 룸 생성 함수
        //생성할 룸의 조건 설정
        string _roomName = "ROOM_" + Random.Range(0, 999).ToString("000");
        RoomOptions roomOptions = new RoomOptions();
        roomOptions.IsOpen = true;     //입장 가능 여부
        roomOptions.IsVisible = true;  //로비에서 룸의 노출 여부
        roomOptions.MaxPlayers = 2;   //룸에 입장할 수 있는 최대 접속자 수

        //지정한 조건에 맞는 룸 생성 함수
        PhotonNetwork.CreateRoom(_roomName, roomOptions, TypedLobby.Default);
        // 방이 없을 때는 내가 방을 만들고 입장해 버린다.
        //(서버 역할의 Client는 이쪽으로 접속하게 될 것이다.)
    }


    public void ClickCreateRoom()
    {
        //버튼 사운드 효과
        SoundMgr.Inst.PlayEffect(buttonSound);

        if (!CheckNickName()) //닉네임 확인
            return;

        string _roomName = roomName.text;
        //룸 이름이 없거나 Null일 경우 룸 이름 지정
        if (string.IsNullOrEmpty(roomName.text))
        {
            _roomName = "ROOM_" + Random.Range(0, 999).ToString("000");
        }

        //로컬 플레이어의 이름을 설정
        PhotonNetwork.LocalPlayer.NickName = userNick.text;
        //플레이어 이름을 저장
        PlayerPrefs.SetString("USER_ID", userNick.text);

        //생성할 룸의 조건 설정
        RoomOptions roomOptions = new RoomOptions();
        roomOptions.IsOpen = true;     //입장 가능 여부
        roomOptions.IsVisible = true;  //로비에서 룸의 노출 여부
        roomOptions.MaxPlayers = 2;   //룸에 입장할 수 있는 최대 접속자 수

        //지정한 조건에 맞는 룸 생성 함수
        PhotonNetwork.CreateRoom(_roomName, roomOptions, TypedLobby.Default);
    }

    //PhotonNetwork.CreateRoom() 이 함수가 실패 하면 호출되는 함수
    //(같은 이름의 방이 있을 때 실패함)
    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        Debug.Log("방 만들기 실패");
        //주로 같은 이름의 방이 존재할 때 룸생성 에러가 발생된다.
        Debug.Log(returnCode.ToString()); //오류 코드(ErrorCode 클래스)
        Debug.Log(message); //오류 메시지
    }

    //RoomItem 눌르면 호출된는
    public void OnClickRoomItem(string roomName)
    {
        SoundMgr.Inst.PlayEffect(buttonSound);
        //로컬 플레이어의 이름을 설정
        PhotonNetwork.LocalPlayer.NickName = userNick.text;
        //플레이어 이름을 저장
        PlayerPrefs.SetString("USER_Nick", userNick.text);

        //인자로 전달된 이름에 해당하는 룸으로 입장
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

        //룸 목록을 다시 받았을 때 갱신하기 위해 기존에 생성된 RoomItem을 삭제
        foreach (var obj in GameObject.FindObjectsOfType<RoomItem>())
        {
            Destroy(obj.gameObject);
        }

        //스크롤 영역 초기화
        scrollContents.GetComponent<RectTransform>().sizeDelta = Vector2.zero;

        for (int i = 0; i < myList.Count; i++)
        {
            GameObject room = Instantiate(roomItem) as GameObject;
            room.transform.SetParent(scrollContents.transform, false);

            //생성한 RoomItem에 표시하기 위한 텍스트 정보 전달
            RoomItem roomData = room.GetComponent<RoomItem>();
            roomData.roomName = myList[i].Name;
            roomData.connectPlayer = myList[i].PlayerCount;
            roomData.maxPlayer = myList[i].MaxPlayers;

            //텍스트 정보를 표시
            roomData.DispRoomData(myList[i].IsOpen);
            }
    }

    //방입장 성공시
    public override void OnJoinedRoom()
    {
        Debug.Log("방 참가 완료");
        //룸 씬으로 이동하는 코루틴 실행
        LoadMgr.Inst.LoadScene("InGame");
    }


    




} 
