using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
using Photon.Realtime;

public class OXGame : MonoBehaviourPunCallbacks, IPunObservable
{
    PhotonView pv;

    enum OX
    {
        O,
        X,
        None,
    }


    //문제가 나오는 Text
    public TextMeshProUGUI questionTxt;
    //문제 테이블
    List<KeyValuePair<string, OX>> questionList = new List<KeyValuePair<string, OX>>();
    //문제 번호가 저장되어있는
    List<int> questionNum = new List<int>();
    int curQuestionNum = 0;

   

    //플레이어가 선택한것
    OX myChoose = OX.None;
    OX otherChoose = OX.None;
    bool choose = false;

    //OX 선택 버튼
    public GameObject O_Btn;
    public GameObject X_Btn;

    [Header("UI")]
    public TextMeshProUGUI myNickName;
    public TextMeshProUGUI otherNickName;

    //플레이어들이 선택한 것
    public GameObject myChoose_Img;
    public TextMeshProUGUI myChoose_Txt;
    public GameObject otherChoose_Img;
    public TextMeshProUGUI ohterChoose_Txt;

    //정답이펙트
    public TextMeshProUGUI myAnswerEffect;
    public TextMeshProUGUI ohterAnswerEffect;
 

    //타이머
    public Image gageBar;
    float timer = 0.0f;

    //동기화를 위한 변수 선언
    ExitGames.Client.Photon.Hashtable playerHash;

    private void Awake()
    {
        pv = GetComponent<PhotonView>();
    }
    private void Start()
    {
        //문제 테이블 적용
        QuestionTableSet();

        //방장만    
        if (PhotonNetwork.IsMasterClient)
        {        
            //문제들 셋팅하기 ..무슨문제를 낼지
            QuestionSet();
        }


        //게임 로직 시작
        StartCoroutine(GameStart());
    }

    private void Update()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            if (timer >= 0)
                timer -= Time.deltaTime;
        }
        gageBar.fillAmount = timer / 10.0f;

    }

    void QuestionTableSet() //문제 테이블 셋팅하기
    {
        questionList.Add(new KeyValuePair<string, OX>("문어다리는 10개이다", OX.O));
        questionList.Add(new KeyValuePair<string, OX>("달팽이도 이빨이 있다.", OX.O));
        questionList.Add(new KeyValuePair<string, OX>("고래는 5M 이하의 물속에서 잠을 잔다.", OX.X));
        questionList.Add(new KeyValuePair<string, OX>("원숭이에게도 지문이 있다", OX.O));
        questionList.Add(new KeyValuePair<string, OX>("남극에도 우편번호가 있다", OX.X));
        questionList.Add(new KeyValuePair<string, OX>("BUS라는 단어는 미국에서 처음 사용하였다", OX.X));
        questionList.Add(new KeyValuePair<string, OX>("닭도 왼발잡이 , 오른발잡이가 있다.", OX.O));
        questionList.Add(new KeyValuePair<string, OX>("새는 뒤로도 날 수 있다.", OX.O));
    }

    public void QuestionSet()//방장만 무슨문제 출제할지 
    {
        List<int> temp = new List<int>();
        while (questionNum.Count < 5)
        {
            int rand = Random.Range(0, questionList.Count);

            if (!questionNum.Contains(rand))
            {
                questionNum.Add(rand);
            }
        }


    }

    IEnumerator GameStart()
    {
        questionTxt.text = "OX 문제입니다.";
        yield return new WaitForSeconds(1.0f);

        while (curQuestionNum <= 5)
        {
            questionTxt.text = (curQuestionNum+1) + "번 문제";
            yield return new WaitForSeconds(1.0f);      
            //문제 내기 방장만
            if (PhotonNetwork.IsMasterClient)
            {
                pv.RPC("SetTextQuestion", RpcTarget.AllViaServer, (int)questionNum[curQuestionNum]);            
            }

         
            yield return new WaitForSeconds(0.5f);
            timer = 10.0f;

            //모든 플레이어가 결정할때까지 대기 //또는 시간이 다지나면
            //플레이어가 선택한것 보여주기
            choose = false;
            while (timer >= 0) //타이머 돌때까지 루프
            {
                yield return null;

                if (choose) //만약 내 선택을했으면 상대 선택한것도 체크하기
                {
                    OtherChooseCheck();
                }
            }

            questionTxt.text = "타임오버";                
            Choose_TimeOver();
            yield return new WaitForSeconds(0.5f);
            questionTxt.text = "정답은";

            //시간 종료후
            yield return new WaitForSeconds(1.0f);
            //결과 보여주기
            OnCheckOX(); //OX 맞춘거 카운터       
            yield return new WaitForSeconds(2.0f);
            questionTxt.text = "다음문제";

            //UI 및 값 초기화
            yield return new WaitForSeconds(2.0f);
            ResetUI();
        }

        //모든 문제 완료시 결과 보여주고
        //다시 메인 로비로


        yield return null;
    }

    [PunRPC]
    void SetTextQuestion(int num)   //방장쪽에서 정해진 문제 재출
    {
        questionTxt.text = (curQuestionNum+1) +"번 문제\n"+ questionList[num].Key;
        curQuestionNum++;

        O_Btn.SetActive(true);
        X_Btn.SetActive(true);
    }


    public void Choose_OBtn ()  //유저 선택  O 
    {
        myChoose = OX.O;
        choose = true;

        OnUserChoose();
    }

    public void Choose_XBtn()//유저 선택  X
    {
        myChoose = OX.X;
        choose = true;

        OnUserChoose();
    }

    void Choose_TimeOver()//시간 오버시
    {         
        OnUserChoose();
    }

    //선택을 완료하면
    void OnUserChoose()
    {
        myNickName.text = PhotonNetwork.LocalPlayer.NickName;
        myNickName.gameObject.SetActive(true);
        otherNickName.text = PhotonNetwork.PlayerListOthers[0].NickName;
        otherNickName.gameObject.SetActive(true);

        //내가 선택한 OX 보여주기
        myChoose_Img.SetActive(true);
        questionTxt.gameObject.SetActive(false);
       
        //버튼 비활성화
        O_Btn.SetActive(false);
        X_Btn.SetActive(false);

        //선택에 따른 문양 보여주기
        if (myChoose.Equals(OX.O))
        {
            myChoose_Txt.text = "O";
            myChoose_Txt.color = Color.blue;
        }
        else if(myChoose.Equals(OX.X))
        {
            myChoose_Txt.text = "X";
            myChoose_Txt.color = Color.red;
        }
        else
        {
            myChoose_Txt.text = "";
        }


        if(!myChoose.Equals(OX.None))
        {
            //원격동기화를 위헤 CustomProperties에 선택한것 올리기
            playerHash = PhotonNetwork.LocalPlayer.CustomProperties;

            if (playerHash.ContainsKey("ChooseOX"))
                playerHash["ChooseOX"] = myChoose;
            else
                playerHash.Add("ChooseOX", myChoose);

            PhotonNetwork.LocalPlayer.SetCustomProperties(playerHash);
        }
      
        //한번 상대방꺼 호출해주기
        OtherChooseCheck();
    }

    float temp = 0;
    void OtherChooseCheck()//다른 유저 선택완료되면
    {
        otherChoose_Img.SetActive(true);
     
        //선택에 따른 문양 보여주기
        if (otherChoose.Equals(OX.O))
        {
            ohterChoose_Txt.text = "O";
            ohterChoose_Txt.color = Color.blue;
        }
        else if (otherChoose.Equals(OX.X))
        {
            ohterChoose_Txt.text = "X";
            ohterChoose_Txt.color = Color.red;
        }
        else
        {
            temp += Time.deltaTime;

            ohterChoose_Txt.color = Color.black;
            ohterChoose_Txt.text = "";

            if (temp > 0 && temp <= 1.0f)
                ohterChoose_Txt.text = ".";
            else if (temp > 1.0f && temp <= 2.0f)
                ohterChoose_Txt.text = "..";
            else if (temp > 2.0f && temp <= 3.0f)
                ohterChoose_Txt.text = "...";
                     
            if (temp > 3.0f)
                temp = 0.0f;
        }
    }

    void OnCheckOX()//정답 확인
    {
        myAnswerEffect.gameObject.SetActive(true);
        if (myChoose == questionList[curQuestionNum].Value)
        {  
            myAnswerEffect.text = "O";
            myAnswerEffect.color = Color.blue;

            //각자 정답인 사람이 각자클라이언트에서 업데이트 해준다.
            playerHash = PhotonNetwork.LocalPlayer.CustomProperties;

            if (playerHash.ContainsKey("OXWinCount"))
                playerHash["OXWinCount"] = (int)playerHash["OXWinCount"] + 1;
            else
                playerHash.Add("OXWinCount", 1);

            PhotonNetwork.LocalPlayer.SetCustomProperties(playerHash);
        }
        else
        {
            myAnswerEffect.text = "X";
            myAnswerEffect.color = Color.red;
        }

        ohterAnswerEffect.gameObject.SetActive(true);
        //효과 만
        if (otherChoose == questionList[curQuestionNum].Value)
        {
            ohterAnswerEffect.text = "O";
            ohterAnswerEffect.color = Color.blue;
        }
        else
        {
            ohterAnswerEffect.text = "X";
            ohterAnswerEffect.color = Color.red;
        }

    }

    void ResetUI()//한문제 끝나면 다시 원상복구
    {
        O_Btn.SetActive(true);
        X_Btn.SetActive(true);

        ohterAnswerEffect.gameObject.SetActive(false);
        myAnswerEffect.gameObject.SetActive(false); 

        myNickName.gameObject.SetActive(false);
        otherNickName.gameObject.SetActive(false);

        myChoose = OX.None;
        otherChoose = OX.None;

        myChoose_Img.SetActive(false);
        otherChoose_Img.SetActive(false);
        questionTxt.gameObject.SetActive(true);
    }


    //PlayerProperties들이 업데이트 된다면
    public override void OnPlayerPropertiesUpdate(Player targetPlayer
                     , ExitGames.Client.Photon.Hashtable changedProps)
    {
        if (targetPlayer != PhotonNetwork.LocalPlayer)
        {
            
            if (changedProps.ContainsKey("ChooseOX"))
            {
                Debug.Log("선택");
                otherChoose = (OX)changedProps["ChooseOX"];
            }
        }
    }


    //타이머 동기화를 위한
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if(stream.IsWriting)
        {
            stream.SendNext(timer);
        }
        else
        {
            timer = (float)stream.ReceiveNext();
        }
     
    }


}
