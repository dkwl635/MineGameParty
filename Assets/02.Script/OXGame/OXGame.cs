using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;

public class OXGame : MonoBehaviourPunCallbacks , IPunObservable
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

    //OX 선택 버튼
    public GameObject O_Btn;
    public GameObject X_Btn;

    [Header("UI")]
    public TextMeshProUGUI myNickName;
    public TextMeshProUGUI OhterNickName;

    //플레이어들이 선택한 것
    public GameObject myChoose_Img;
    public TextMeshProUGUI myChoose_Txt;
    public GameObject ohterChoose_Img;
    public TextMeshProUGUI ohterChoose_Txt;

    //정답이펙트
    public GameObject myAnswerEffect;
    public GameObject OhterAnswerEffect;

    //타이머
    public Image gageBar;
    public float timer = 10.0f;

     

    private void Awake()
    {
        pv = GetComponent<PhotonView>();
    }
    private void Start()
    {
        QuestionTableSet();
    
        questionTxt.text = questionList[0].Key;
        StartCoroutine(GameStart());
    }

    private void Update()
    {
        if (timer >= 0)
            timer -= Time.deltaTime;

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

    public  void GameStart1()
    {
        int roop = 0 ;

        List<int> temp = new List<int>();
        while(questionNum.Count < 5)
        {
            roop++;
            int rand = Random.Range(0, questionList.Count);
          
            if (!questionNum.Contains(rand))
            {
                questionNum.Add(rand);
            }
        }

        Debug.Log("루프 횟수 : " + roop);

        for (int i = 0; i < questionNum.Count; i++)
        {
            Debug.Log(questionList[questionNum[i]].Key);
        }      
    }

    IEnumerator GameStart()
    {
        O_Btn.SetActive(true);
        X_Btn.SetActive(true);

        yield return null;
    }


    public void Choose_OBtn ()  //유저 선택  O
    {
        myChoose = OX.O;

        OnUserChoose();
    }

    public void Choose_XBtn()//유저 선택  X
    {
        myChoose = OX.X;

        OnUserChoose();
    }

    void OnUserChoose()
    {
        myChoose_Img.SetActive(true);
        questionTxt.gameObject.SetActive(true);
       
        //버튼 비활성화
        O_Btn.SetActive(false);
        X_Btn.SetActive(false);

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

        //test
        OnCheckOX();


    }

    void OnOtherChoose()
    {

    }

    void OnCheckOX()
    {
        if(myChoose == questionList[curQuestionNum].Value)
        {
            myAnswerEffect.SetActive(true);
        }

    }

    

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
