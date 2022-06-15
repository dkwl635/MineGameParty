using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;


public class RememberGame : MonoBehaviourPunCallbacks, IPunObservable
{
    //화살표 이미지   0 : left , 1 : right
    public Sprite[] arrowSprite;

    //레벨
    int level = 0;

    //레벨별 이미지 갯수
    int[] levelCount = { 3, 4, 5, 6 };
    float[] leveltimer = { 1.0f, 1.3f, 1.5f };
    int count = 0;

    //레벨별 그룹
    public GameObject[] levelGroup;
    public List<Image[]> imgs = new List<Image[]>();

    //내가 입력한 순서를 저장할    //레벨에 따라 최대 6개 까지입력
    int mySelNum = 0; //내가 입력한 순서;
    public Image[] mySetImg;

    //정답을 저장할
    int[] answer = new int[6];
    public TextMeshProUGUI OX;
    //맞힌갯수 UI
    public TextMeshProUGUI answerCountTxt;
    //맞힌갯수 저장변수
    int answerCount = 0;

    //체크하는 큐
    Queue<int> check = new Queue<int>();

    public GameObject timerObj;
    float timer = 15.0f;
    public Image timerbar;
    bool wait = false;
    
    //화살표를 숨기게 하는 함수를 담은 코루틴
    Coroutine currCo;

    public TextMeshProUGUI infoText;

    public GameObject GamePanel;          //게임 패널
    [Header("ResultPanel")]
    public GameObject ResultPanel;          //결과창

    public TextMeshProUGUI myNickTxt;       //내 닉네임 
    public TextMeshProUGUI otherNickTxt;    //상대 닉네임
    public TextMeshProUGUI myScoreTxt;      // 내 점수
    public TextMeshProUGUI otherScoreTxt;   //상대 점수
    public TextMeshProUGUI winOrLose;       //승리판정

    public GameObject ok_Btn;                   //확인 버튼

    //동기화를 위한 변수 선언
    ExitGames.Client.Photon.Hashtable playerHash;

    bool allEnd = false;

    private void Awake()
    {
        for (int i = 0; i < levelGroup.Length; i++)
        {
            imgs.Add(levelGroup[i].GetComponentsInChildren<Image>());
        }
    }

    public override void OnEnable()
    {
        level = 0;
        check.Clear();
        timer = 15.0f;


        Button LeftButton = GameObject.Find("LeftButton").GetComponent<Button>();
        LeftButton.onClick.AddListener(() => { SelMyArrow(0); });

        GameObject.Find("RightButton").GetComponent<Button>().onClick.AddListener(() => { SelMyArrow(1); });

        StartCoroutine(GameStart());
    }


   


    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftArrow))
            SelMyArrow(0);

        if (Input.GetKeyDown(KeyCode.RightArrow))
            SelMyArrow(1);

        if (PhotonNetwork.IsMasterClient)
        {
            if (timer >= 0)
                timer -= Time.deltaTime;
        }

        timerbar.fillAmount = timer / 60.0f;

       
    }


    void ArrowSetting()
    {
        levelGroup[level].SetActive(true);
        //무작위 선택을 위해
        int rand = 0;
        for (int i = 0; i < imgs[level].Length; i++)
        {
            rand = Random.Range(0, 2);
            imgs[level][i].sprite = arrowSprite[rand];
            answer[i] = rand;
        }


        if (currCo != null)
            StopCoroutine(currCo);

        currCo = StartCoroutine(OffArrow(leveltimer[level]));

    }

    IEnumerator OffArrow(float time)
    {
        yield return new WaitForSeconds(time);
        levelGroup[level].SetActive(false);

        currCo = null;
    }


    IEnumerator GameStart()
    {
        GamePanel.SetActive(true);
        timerObj.SetActive(false);
        infoText.gameObject.SetActive(true);
        infoText.text = "나오는 화살표 방향에 맞게\n순서대로 입력해주세요\n시간이 지나면 사라집니다.";
        yield return new WaitForSeconds(1.5f);
        infoText.gameObject.SetActive(false);

        ArrowSetting();
        answerCountTxt.gameObject.SetActive(true);
        answerCountTxt.text = "정답 갯수 : 0";

        //임시 15초 원래는 60초
        timerObj.SetActive(true);
        timer = 15.0f;
        timerbar.fillAmount = timer / 60.0f;


        //게임 로직
        while (true)
        {
            //타임 오버
            if (timer <= 0)
                break;

            yield return null;

            //체크하기
            if (check.Count > 0)
            {
                int RL = check.Dequeue();
                mySetImg[mySelNum].gameObject.SetActive(true);
                mySetImg[mySelNum].sprite = arrowSprite[RL];

                if (RL != answer[mySelNum])
                {
                    OX.gameObject.SetActive(true);
                    OX.text = "X";
                    OX.color = Color.red;
                    wait = true;

                    yield return new WaitForSeconds(0.5f);
                    wait = false;
                    OX.gameObject.SetActive(false);

                    FailRemember();
                }
                else
                {
                    mySelNum++;

                    if (mySelNum > levelCount[level] - 1)
                    {
                        OX.gameObject.SetActive(true);
                        OX.text = "O";
                        OX.color = Color.blue;
                        wait = true;
                        yield return new WaitForSeconds(0.5f);
                        wait = false;
                        OX.gameObject.SetActive(false);

                        SuccessRemember();
                    }
                }
            }
        }

        //타임 종료후
        levelGroup[level].SetActive(false);
        MySetClear();

        answerCountTxt.text = "타임오버!!";


        yield return new WaitForSeconds(1.0f);



        GamePanel.SetActive(false);
        ResultPanel.SetActive(true);
        GameEnd();

    }

    void SelMyArrow(int RL)
    {
        if (mySelNum == levelCount[level])
            return;

        if (wait)
            return;

        check.Enqueue(RL);
    }

    void MySetClear()
    {
        for (int i = 0; i < mySetImg.Length; i++)
        {
            mySetImg[i].gameObject.SetActive(false);
        }
    }

    void FailRemember()
    {
        ArrowSetting();
        MySetClear();

        //내 케릭터 틀린거 
        InGame.Inst.playerCharacters[0].SetHit();


        mySelNum = 0;
    }

    void SuccessRemember()
    {

        levelGroup[level].SetActive(false);
        count++;
        if (count == 5)
        {
            level++;
            if (level > 2)
                level = 2;

            count = 0;
        }
        answerCount++;
        answerCountTxt.text = "정답 갯수 : " + answerCount;


        //점수 동기화를 위해 CustomProperties에 저장
        playerHash = PhotonNetwork.LocalPlayer.CustomProperties;

        //점수
        if (playerHash.ContainsKey("RememberCount"))
            playerHash["RememberCount"] = answerCount;
        else
            playerHash.Add("RememberCount", answerCount);

        PhotonNetwork.LocalPlayer.SetCustomProperties(playerHash);


        ArrowSetting();
        MySetClear();

        mySelNum = 0;
    }

    void GameEnd()//모든 문제 끝나고 결과창 업데이트
    {
        ResultPanel.SetActive(true);
        GamePanel.SetActive(false);

        answerCountTxt.gameObject.SetActive(false);
        myNickTxt.text = PhotonNetwork.LocalPlayer.NickName;
        otherNickTxt.text = PhotonNetwork.PlayerListOthers[0].NickName;

        int myscore = answerCount;
        int otherscore = PhotonNetwork.PlayerListOthers[0].CustomProperties.ContainsKey("RememberCount") ? (int)PhotonNetwork.PlayerListOthers[0].CustomProperties["RememberCount"] : 0;


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

    //타이머 동기화를 위한
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(timer);
        }
        else
        {
            timer = (float)stream.ReceiveNext();
        }

    }

}
