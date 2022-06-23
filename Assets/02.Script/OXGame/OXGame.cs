using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
using Photon.Realtime;

public class OXGame : Game
{

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
    int currQuestion = 0;
    int step = 0;
    bool bIng = false;

    //�÷��̾ �����Ѱ�
    OX myChoose = OX.None;
    OX otherChoose = OX.None;
    
    bool choose = false;
    //OX ���� ��ư
    public GameObject O_Btn;
    public GameObject X_Btn;

    [Header("UI")]
    public GameObject GamePanel;
    public TextMeshProUGUI myNickName;
    public TextMeshProUGUI otherNickName;
    public TextMeshProUGUI myscore;

    //�÷��̾���� ������ ��
    public GameObject myChoose_Img;
    public TextMeshProUGUI myChoose_Txt;
    public GameObject otherChoose_Img;
    public TextMeshProUGUI ohterChoose_Txt;

    //��������Ʈ
    public TextMeshProUGUI myAnswerEffect;
    public TextMeshProUGUI ohterAnswerEffect;


    //Ÿ�̸�
    [Header("Timer")]
    public Image gageBar;
    float timer = 0.0f;
    float curTimer = 0.0f;


    protected override void Init()
    {
        base.Init();

        //���� ���̺� 
        QuestionTableSet();
    }

    public override void StartGame()
    {
        //���� UI On
        GamePanel.SetActive(true);

        //���常    
        if (PhotonNetwork.IsMasterClient)
        {
            //������ �����ϱ� ..���������� ����
            QuestionSet();
        }


        //���� ���� ����
        StartCoroutine(Game_Co());
    }


    void QuestionTableSet() //���� ���̺� �����ϱ�
    {
        questionList.Add(new KeyValuePair<string, OX>("����ٸ��� 10���̴�", OX.X));
        questionList.Add(new KeyValuePair<string, OX>("�����̵� �̻��� �ִ�.", OX.O));
        questionList.Add(new KeyValuePair<string, OX>("���� 5M ������ ���ӿ��� ���� �ܴ�.", OX.X));
        questionList.Add(new KeyValuePair<string, OX>("�����̿��Ե� ������ �ִ�", OX.O));
        questionList.Add(new KeyValuePair<string, OX>("���ؿ��� �����ȣ�� �ִ�", OX.X));
        questionList.Add(new KeyValuePair<string, OX>("BUS��� �ܾ�� �̱����� ó�� ����Ͽ���", OX.X));
        questionList.Add(new KeyValuePair<string, OX>("�ߵ� �޹����� , ���������̰� �ִ�.", OX.O));
        questionList.Add(new KeyValuePair<string, OX>("���� �ڷε� �� �� �ִ�.", OX.O));
    }

    void QuestionSet()//���常 �������� �������� 
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

    IEnumerator Game_Co()
    {
        questionTxt.text = "OX �����Դϴ�.";
        myscore.text = "�� ���� : 0";
        yield return new WaitForSeconds(1.0f);

        while (step < 5)
        {        
          
            yield return new WaitForSeconds(1.0f);      
            //���� ���� ���常
            if (PhotonNetwork.IsMasterClient)
            {
                pv.RPC("SetTextQuestion", RpcTarget.AllViaServer, (int)questionNum[step]);            
            }

         
            yield return new WaitForSeconds(0.5f);
           

            //��� �÷��̾ �����Ҷ����� ��� //�Ǵ� �ð��� ��������
            //�÷��̾ �����Ѱ� �����ֱ�
            choose = false;
            bIng = true;
            while (bIng) //Ÿ�̸� �������� ����
            {
                yield return null;

                if (choose) //���� �� ������������ ��� �����Ѱ͵� üũ�ϱ�
                {
                    OtherChooseCheck();
                }
            }

            Choose_TimeOver();

            questionTxt.gameObject.SetActive(true);
            questionTxt.text = "Ÿ�ӿ���";                         
            yield return new WaitForSeconds(1.0f);
            questionTxt.text = "������";
            //�ð� ������
            yield return new WaitForSeconds(1.0f);
            //��� �����ֱ�
            OnCheckOX(); //OX ����� ī����       
            yield return new WaitForSeconds(2.0f);

            if (step.Equals(4))          
               questionTxt.text = "���� ����";            
            else if (step.Equals(3))
                questionTxt.text = "������ ����";
            else
                questionTxt.text = "���� ����";

            //UI �� �� �ʱ�ȭ
            yield return new WaitForSeconds(1.5f);
            ResetUI();
            step++; //���� ����
        }

        //��� ���� �Ϸ�� ��� �����ְ�
        //������� â
        InGame.Inst.ShowResult();
        //���� UI Off
        GamePanel.SetActive(false);

    }

    [PunRPC]
    void SetTextQuestion(int num)   //�����ʿ��� ������ ���� ����
    {
        questionTxt.text = (step + 1) +"�� ����\n"+ questionList[num].Key;
        currQuestion = num;
        O_Btn.SetActive(true);
        X_Btn.SetActive(true);
    
       StartCoroutine(Timer_Update());

    }
    IEnumerator Timer_Update()
    {
        timer = 5.0f; //�ᱹ ���常 �����
        while (timer >= 0)
        {
            yield return null;
            timer -= Time.deltaTime;
            gageBar.fillAmount = timer / 10.0f;
        }

        bIng = false;

    }


    public void Choose_OBtn ()  //���� ����  O 
    {
        myChoose = OX.O;
        choose = true;

        OnUserChoose();
    }

    public void Choose_XBtn()//���� ����  X
    {
        myChoose = OX.X;
        choose = true;

        OnUserChoose();
    }

    void Choose_TimeOver()//�ð� ������
    {
        OnUserChoose();
    }

    //������ �Ϸ��ϸ�
    void OnUserChoose()
    {
        myNickName.text = PhotonNetwork.LocalPlayer.NickName;
        myNickName.gameObject.SetActive(true);
        otherNickName.text = PhotonNetwork.PlayerListOthers[0].NickName;
        otherNickName.gameObject.SetActive(true);

        //���� ������ OX �����ֱ�
        myChoose_Img.SetActive(true);
        questionTxt.gameObject.SetActive(false);

        //��ư ��Ȱ��ȭ
        O_Btn.SetActive(false);
        X_Btn.SetActive(false);

        //���ÿ� ���� ���� �����ֱ�
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
            //���ݵ���ȭ�� ���� CustomProperties�� �����Ѱ� �ø���
            playerHash = PhotonNetwork.LocalPlayer.CustomProperties;

            if (playerHash.ContainsKey("ChooseOX"))
                playerHash["ChooseOX"] = myChoose;
            else
                playerHash.Add("ChooseOX", myChoose);

            PhotonNetwork.LocalPlayer.SetCustomProperties(playerHash);
        }
      
        //�ѹ� ���沨 ȣ�����ֱ�
        OtherChooseCheck();
    }

    float temp = 0;
    void OtherChooseCheck()//�ٸ� ���� ���ÿϷ�Ǹ�
    {
        otherChoose_Img.SetActive(true);
     
        //���ÿ� ���� ���� �����ֱ�
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
        
        if(timer <= 0 && otherChoose.Equals(OX.None))
        {
            ohterChoose_Txt.text = "";
        }

    
    }

    void OnCheckOX()//���� Ȯ��
    {
        myAnswerEffect.gameObject.SetActive(true);
        questionTxt.text = "������ : " + questionList[currQuestion].Value;

        if (myChoose == questionList[currQuestion].Value)
        {  
            myAnswerEffect.text = "O";
            myAnswerEffect.color = Color.blue;

            //���� ������ ����� ����Ŭ���̾�Ʈ���� ������Ʈ ���ش�.
            playerHash = PhotonNetwork.LocalPlayer.CustomProperties;

            if (playerHash.ContainsKey("score"))
                playerHash["score"] = (int)playerHash["score"] + 1;
            else
                playerHash.Add("score", 1);

            PhotonNetwork.LocalPlayer.SetCustomProperties(playerHash);
            score++;
            myscore.text = "�� ���� : " + score.ToString();

        }
        else
        {
            myAnswerEffect.text = "X";
            myAnswerEffect.color = Color.red;
        }

        ohterAnswerEffect.gameObject.SetActive(true);
        //ȿ�� ��
        if (otherChoose == questionList[currQuestion].Value)
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

    void ResetUI()//�ѹ��� ������ �ٽ� ���󺹱�
    {   
        //UI ����
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



    //PlayerProperties���� ������Ʈ �ȴٸ� //���⼭ ���� ���� üũ
    public override void OnPlayerPropertiesUpdate(Player targetPlayer
                     , ExitGames.Client.Photon.Hashtable changedProps)
    {
        if (targetPlayer != PhotonNetwork.LocalPlayer)
        {
            
            if (changedProps.ContainsKey("ChooseOX"))
            {
                Debug.Log("����");
                otherChoose = (OX)changedProps["ChooseOX"];

            }
        }
    }



}
