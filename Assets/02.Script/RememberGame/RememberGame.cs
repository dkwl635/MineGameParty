using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;


public class RememberGame : Game, IPunObservable
{
    //화살표 이미지   0 : left , 1 : right
    public Sprite[] arrowSprite;

    //레벨
    int level = 0;

    //레벨별 이미지 갯수
    int[] levelCount = { 3, 4, 5, 6 };
    float[] leveltimer = { 1.0f, 1.2f, 1.2f,1.4f };
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

    //체크하는 큐//입력한 
    Queue<int> check = new Queue<int>();

    public GameObject timerObj;
    float timer = 15.0f;
    public Image timerbar;
    bool wait = false;
    
    //화살표를 숨기게 하는 함수를 담은 코루틴
    Coroutine currCo;

    public TextMeshProUGUI infoText;
    public GameObject GamePanel;          //게임 패널
    //캐릭터
    PlayerCharacter myPlayer;

    protected override void Init()
    {
        base.Init();

        for (int i = 0; i < levelGroup.Length; i++)
        {
            imgs.Add(levelGroup[i].GetComponentsInChildren<Image>());
        }

    }


    

    public override void StartGame()
    {
        base.StartGame();

        SoundMgr.Inst.PlayBGM("RememberGame");
        //좌우 이동을 막는다
        myPlayer = InGame.Inst.playerCharacters[0];
        myPlayer.isMove = false;
     
        //버튼 적용
        Button LBT = GameObject.Find("LeftButton").GetComponent<Button>();
        LBT.onClick.RemoveAllListeners();
        LBT.onClick.AddListener(() => { SelMyArrow(0); });

        Button RBT = GameObject.Find("RightButton").GetComponent<Button>();
        RBT.onClick.RemoveAllListeners();
        RBT.onClick.AddListener(() => { SelMyArrow(1); });

        //게임 UI On
        GamePanel.SetActive(true);
  
        //게임 로직 시작
        StartCoroutine(Game_Co());
    }



    IEnumerator Game_Co()
    {
        //초기화
        level = 0;
        answerCount = 0;
      
        timerObj.SetActive(false);
        infoText.gameObject.SetActive(true);
        infoText.text = "나오는 화살표 방향에 맞게\n순서대로 입력해주세요\n시간이 지나면 사라집니다.";
        yield return new WaitForSeconds(1.5f);
        infoText.gameObject.SetActive(false);

        //화살표 셋팅
        ArrowSetting();
    
        answerCountTxt.gameObject.SetActive(true);
        answerCountTxt.text = "정답 갯수 : 0";

  
        timerObj.SetActive(true);
        timer = 15.0f;
        timerbar.fillAmount = timer / 60.0f;
        //타이머 
        StartCoroutine(Time_Update());


        //게임 로직
        while (timer > 0)
        {      
            yield return null;

            //체크하기 //입력받은 큐에들어있는 것을 확인하면서
            if (check.Count > 0)
            {
                int RL = check.Dequeue(); //앞에 입력한거 가져와서 
                mySetImg[mySelNum].gameObject.SetActive(true); //내가 선택한 화살표 보여주기
                mySetImg[mySelNum].sprite = arrowSprite[RL];    //이미지 적용

                
                if (RL != answer[mySelNum]) //정답이면
                {
                    OX.gameObject.SetActive(true);
                    OX.text = "X";
                    OX.color = Color.red;
                    //판정시 잠시 대기
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
                        //판정시 잠시 대기
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

        answerCountTxt.gameObject.SetActive(false);
        GamePanel.SetActive(false);
        
        InGame.Inst.ShowResult();
        myPlayer.isMove = true;
    }


    IEnumerator Time_Update()
    {
        while (timer >= 0)
        {
            yield return null;
            timer -= Time.deltaTime;
            timerbar.fillAmount = timer / 60.0f;
        }
    }


    private void Update() //테스트를 위한
    {
        if (Input.GetKeyDown(KeyCode.LeftArrow))
            SelMyArrow(0);

        if (Input.GetKeyDown(KeyCode.RightArrow))
            SelMyArrow(1);
    }


    void ArrowSetting() //레벨에 맞게 무작위 화살표 셋팅
    {
        check.Clear();
        wait = false;

        levelGroup[level].SetActive(true);
        //무작위 선택을 위해
        int rand = 0;
        //래벨에 따라 화살표 배치
        for (int i = 0; i < imgs[level].Length; i++)
        {
            rand = Random.Range(0, 2);
            imgs[level][i].sprite = arrowSprite[rand];
            answer[i] = rand;
        }


        if (currCo != null) //만약 함수가 발동되기전에 이미 돌아가고 있으면 기존 함수멈춤 
            StopCoroutine(currCo);

        // 화살표 일정시간 뒤에 사라지게 하는 코루틴 함수
        currCo = StartCoroutine(OffArrow(leveltimer[level]));
    }

    IEnumerator OffArrow(float time)//일정시간 뒤에 화살표 안보이게
    {
        yield return new WaitForSeconds(time);
        levelGroup[level].SetActive(false);

        currCo = null;
    }


    void SelMyArrow(int RL)
    {
        //만약 선택한 횟수가 
        if (mySelNum == levelCount[level])
            return;

        if (wait)
            return;

        if (RL.Equals(0))
            SoundMgr.Inst.PlayEffect("BtnL");
        else
            SoundMgr.Inst.PlayEffect("BtnR");

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
        SoundMgr.Inst.PlayEffect("Fail");

        //다음
        ArrowSetting();
        MySetClear();

        //내 케릭터 틀린거 //뭐에 맞은것 처럼
        InGame.Inst.playerCharacters[0].SetHit();
        mySelNum = 0;
    }

    void SuccessRemember()
    {
        SoundMgr.Inst.PlayEffect("Success");

        levelGroup[level].SetActive(false);

        //다음 단계로 갈수 있으면 카운트up
        if (level < levelCount.Length - 1)
            count++;

        if (count == 5)//5개씩 맞추면
        {
            if (level < levelCount.Length)
                level++;
       
            count = 0;
        }

        answerCount++;
        answerCountTxt.text = "정답 갯수 : " + answerCount;


        //점수 동기화를 위해 CustomProperties에 저장
        playerHash = PhotonNetwork.LocalPlayer.CustomProperties;

        //점수
        if (playerHash.ContainsKey("score"))
            playerHash["score"] = answerCount;
        else
            playerHash.Add("score", answerCount);

        PhotonNetwork.LocalPlayer.SetCustomProperties(playerHash);

        //다음
        ArrowSetting();
        MySetClear();

        mySelNum = 0;
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
