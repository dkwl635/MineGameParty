using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;


public class RememberGame : Game, IPunObservable
{
    //ȭ��ǥ �̹���   0 : left , 1 : right
    public Sprite[] arrowSprite;

    //����
    int level = 0;

    //������ �̹��� ����
    int[] levelCount = { 3, 4, 5, 6 };
    float[] leveltimer = { 1.0f, 1.2f, 1.2f,1.4f };
    int count = 0;

    //������ �׷�
    public GameObject[] levelGroup;
    public List<Image[]> imgs = new List<Image[]>();

    //���� �Է��� ������ ������    //������ ���� �ִ� 6�� �����Է�
    int mySelNum = 0; //���� �Է��� ����;
    public Image[] mySetImg;

    //������ ������
    int[] answer = new int[6];
    public TextMeshProUGUI OX;
    //�������� UI
    public TextMeshProUGUI answerCountTxt;
    //�������� ���庯��
    int answerCount = 0;

    //üũ�ϴ� ť//�Է��� 
    Queue<int> check = new Queue<int>();

    public GameObject timerObj;
    float timer = 15.0f;
    public Image timerbar;
    bool wait = false;
    
    //ȭ��ǥ�� ����� �ϴ� �Լ��� ���� �ڷ�ƾ
    Coroutine currCo;

    public TextMeshProUGUI infoText;
    public GameObject GamePanel;          //���� �г�
    //ĳ����
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
        //�¿� �̵��� ���´�
        myPlayer = InGame.Inst.playerCharacters[0];
        myPlayer.isMove = false;
     
        //��ư ����
        Button LBT = GameObject.Find("LeftButton").GetComponent<Button>();
        LBT.onClick.RemoveAllListeners();
        LBT.onClick.AddListener(() => { SelMyArrow(0); });

        Button RBT = GameObject.Find("RightButton").GetComponent<Button>();
        RBT.onClick.RemoveAllListeners();
        RBT.onClick.AddListener(() => { SelMyArrow(1); });

        //���� UI On
        GamePanel.SetActive(true);
  
        //���� ���� ����
        StartCoroutine(Game_Co());
    }



    IEnumerator Game_Co()
    {
        //�ʱ�ȭ
        level = 0;
        answerCount = 0;
      
        timerObj.SetActive(false);
        infoText.gameObject.SetActive(true);
        infoText.text = "������ ȭ��ǥ ���⿡ �°�\n������� �Է����ּ���\n�ð��� ������ ������ϴ�.";
        yield return new WaitForSeconds(1.5f);
        infoText.gameObject.SetActive(false);

        //ȭ��ǥ ����
        ArrowSetting();
    
        answerCountTxt.gameObject.SetActive(true);
        answerCountTxt.text = "���� ���� : 0";

  
        timerObj.SetActive(true);
        timer = 15.0f;
        timerbar.fillAmount = timer / 60.0f;
        //Ÿ�̸� 
        StartCoroutine(Time_Update());


        //���� ����
        while (timer > 0)
        {      
            yield return null;

            //üũ�ϱ� //�Է¹��� ť������ִ� ���� Ȯ���ϸ鼭
            if (check.Count > 0)
            {
                int RL = check.Dequeue(); //�տ� �Է��Ѱ� �����ͼ� 
                mySetImg[mySelNum].gameObject.SetActive(true); //���� ������ ȭ��ǥ �����ֱ�
                mySetImg[mySelNum].sprite = arrowSprite[RL];    //�̹��� ����

                
                if (RL != answer[mySelNum]) //�����̸�
                {
                    OX.gameObject.SetActive(true);
                    OX.text = "X";
                    OX.color = Color.red;
                    //������ ��� ���
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
                        //������ ��� ���
                        wait = true;
                        yield return new WaitForSeconds(0.5f);
                        wait = false;
                        OX.gameObject.SetActive(false);

                        SuccessRemember();
                    }
                }
            }
        }

        //Ÿ�� ������
        levelGroup[level].SetActive(false);
        MySetClear();

        answerCountTxt.text = "Ÿ�ӿ���!!";
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


    private void Update() //�׽�Ʈ�� ����
    {
        if (Input.GetKeyDown(KeyCode.LeftArrow))
            SelMyArrow(0);

        if (Input.GetKeyDown(KeyCode.RightArrow))
            SelMyArrow(1);
    }


    void ArrowSetting() //������ �°� ������ ȭ��ǥ ����
    {
        check.Clear();
        wait = false;

        levelGroup[level].SetActive(true);
        //������ ������ ����
        int rand = 0;
        //������ ���� ȭ��ǥ ��ġ
        for (int i = 0; i < imgs[level].Length; i++)
        {
            rand = Random.Range(0, 2);
            imgs[level][i].sprite = arrowSprite[rand];
            answer[i] = rand;
        }


        if (currCo != null) //���� �Լ��� �ߵ��Ǳ����� �̹� ���ư��� ������ ���� �Լ����� 
            StopCoroutine(currCo);

        // ȭ��ǥ �����ð� �ڿ� ������� �ϴ� �ڷ�ƾ �Լ�
        currCo = StartCoroutine(OffArrow(leveltimer[level]));
    }

    IEnumerator OffArrow(float time)//�����ð� �ڿ� ȭ��ǥ �Ⱥ��̰�
    {
        yield return new WaitForSeconds(time);
        levelGroup[level].SetActive(false);

        currCo = null;
    }


    void SelMyArrow(int RL)
    {
        //���� ������ Ƚ���� 
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

        //����
        ArrowSetting();
        MySetClear();

        //�� �ɸ��� Ʋ���� //���� ������ ó��
        InGame.Inst.playerCharacters[0].SetHit();
        mySelNum = 0;
    }

    void SuccessRemember()
    {
        SoundMgr.Inst.PlayEffect("Success");

        levelGroup[level].SetActive(false);

        //���� �ܰ�� ���� ������ ī��Ʈup
        if (level < levelCount.Length - 1)
            count++;

        if (count == 5)//5���� ���߸�
        {
            if (level < levelCount.Length)
                level++;
       
            count = 0;
        }

        answerCount++;
        answerCountTxt.text = "���� ���� : " + answerCount;


        //���� ����ȭ�� ���� CustomProperties�� ����
        playerHash = PhotonNetwork.LocalPlayer.CustomProperties;

        //����
        if (playerHash.ContainsKey("score"))
            playerHash["score"] = answerCount;
        else
            playerHash.Add("score", answerCount);

        PhotonNetwork.LocalPlayer.SetCustomProperties(playerHash);

        //����
        ArrowSetting();
        MySetClear();

        mySelNum = 0;
    }

   

    //Ÿ�̸� ����ȭ�� ����
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
