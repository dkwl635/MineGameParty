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


    //������ ������ Text
    public TextMeshProUGUI ProblemTxt;
    //���� ���̺�
    List<KeyValuePair<string, OX>> questionList = new List<KeyValuePair<string, OX>>();
    //���� ��ȣ�� ����Ǿ��ִ�
    List<int> questionNum = new List<int>();

    //�÷��̾ �����Ѱ�
    OX userChoose = OX.None;

    public GameObject O_Btn;
    public GameObject X_Btn;

    public GameObject userChoose_Img;
    public TextMeshProUGUI userChoose_Txt;

    private void Awake()
    {
        pv = GetComponent<PhotonView>();
    }
    private void Start()
    {
        QuestionTableSet();
    
        ProblemTxt.text = questionList[0].Key;
        StartCoroutine(GameStart());
    }

    void QuestionTableSet() //���� ���̺� �����ϱ�
    {
        questionList.Add(new KeyValuePair<string, OX>("����ٸ��� 10���̴�", OX.O));
        questionList.Add(new KeyValuePair<string, OX>("�����̵� �̻��� �ִ�.", OX.O));
        questionList.Add(new KeyValuePair<string, OX>("���� 5M ������ ���ӿ��� ���� �ܴ�.", OX.X));
        questionList.Add(new KeyValuePair<string, OX>("�����̿��Ե� ������ �ִ�", OX.O));
        questionList.Add(new KeyValuePair<string, OX>("���ؿ��� �����ȣ�� �ִ�", OX.X));
        questionList.Add(new KeyValuePair<string, OX>("BUS��� �ܾ�� �̱����� ó�� ����Ͽ���", OX.X));
        questionList.Add(new KeyValuePair<string, OX>("�ߵ� �޹����� , ���������̰� �ִ�.", OX.O));
        questionList.Add(new KeyValuePair<string, OX>("���� �ڷε� �� �� �ִ�.", OX.O));
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

        Debug.Log("���� Ƚ�� : " + roop);

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


    public void Choose_OBtn ()  //���� ����  O
    {
        userChoose = OX.O;

        OnUserChoose();
    }

    public void Choose_XBtn()//���� ����  X
    {
        userChoose = OX.X;

        OnUserChoose();
    }

    void OnUserChoose()
    {
        userChoose_Img.SetActive(true);
       
        //��ư ��Ȱ��ȭ
        O_Btn.SetActive(false);
        X_Btn.SetActive(false);

        if (userChoose.Equals(OX.O))
        {
            userChoose_Txt.text = "O";
            userChoose_Txt.color = Color.blue;
        }
        else if(userChoose.Equals(OX.X))
        {
            userChoose_Txt.text = "X";
            userChoose_Txt.color = Color.red;
        }
        else
        {
            userChoose_Txt.text = "";
      
        }
       
    }

}
