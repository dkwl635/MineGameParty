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


    //������ ������ Text
    public TextMeshProUGUI questionTxt;
    //���� ���̺�
    List<KeyValuePair<string, OX>> questionList = new List<KeyValuePair<string, OX>>();
    //���� ��ȣ�� ����Ǿ��ִ�
    List<int> questionNum = new List<int>();
    int curQuestionNum = 0;


    //�÷��̾ �����Ѱ�
    OX myChoose = OX.None;

    //OX ���� ��ư
    public GameObject O_Btn;
    public GameObject X_Btn;

    [Header("UI")]
    public TextMeshProUGUI myNickName;
    public TextMeshProUGUI OhterNickName;

    //�÷��̾���� ������ ��
    public GameObject myChoose_Img;
    public TextMeshProUGUI myChoose_Txt;
    public GameObject ohterChoose_Img;
    public TextMeshProUGUI ohterChoose_Txt;

    //��������Ʈ
    public GameObject myAnswerEffect;
    public GameObject OhterAnswerEffect;

    //Ÿ�̸�
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
        myChoose = OX.O;

        OnUserChoose();
    }

    public void Choose_XBtn()//���� ����  X
    {
        myChoose = OX.X;

        OnUserChoose();
    }

    void OnUserChoose()
    {
        myChoose_Img.SetActive(true);
        questionTxt.gameObject.SetActive(true);
       
        //��ư ��Ȱ��ȭ
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
