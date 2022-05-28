using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Photon.Pun;

public class OXGame : MonoBehaviourPunCallbacks
{
    PhotonView pv;

    enum OX
    {
        O,
        X,
        None,
    }


    //문제가 나오는 Text
    public TextMeshProUGUI ProblemTxt;
    //문제 테이블
    List<KeyValuePair<string, OX>> questionList = new List<KeyValuePair<string, OX>>();
    //문제 번호가 저장되어있는
    List<int> questionNum = new List<int>();

    //플레이어가 선택한것
    OX userChoose = OX.None;

    public GameObject O_Btn;
    public GameObject X_Btn;


    private void Awake()
    {
        pv = GetComponent<PhotonView>();
    }
    private void Start()
    {
        questionList.Add(new KeyValuePair<string, OX>("문어다리는 10개이다", OX.O));
        questionList.Add(new KeyValuePair<string, OX>("달팽이도 이빨이 있다.", OX.O));
        questionList.Add(new KeyValuePair<string, OX>("고래는 5M 이하의 물속에서 잠을 잔다.", OX.X));
        questionList.Add(new KeyValuePair<string, OX>("원숭이에게도 지문이 있다", OX.O));
        questionList.Add(new KeyValuePair<string, OX>("남극에도 우편번호가 있다", OX.X));
        questionList.Add(new KeyValuePair<string, OX>("BUS라는 단어는 미국에서 처음 사용하였다", OX.X));
        questionList.Add(new KeyValuePair<string, OX>("닭도 왼발잡이 , 오른발잡이가 있다.", OX.O));
        questionList.Add(new KeyValuePair<string, OX>("새는 뒤로도 날 수 있다.", OX.O));


        ProblemTxt.text = questionList[0].Key;
        StartCoroutine(GameStart());
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


    public void Choose_OBtn ()
    {
        userChoose = OX.O;
    }

    public void Choose_XBtn()
    {
        userChoose = OX.X;
    }

}
