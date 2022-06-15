using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;


public class RememberGame : MonoBehaviourPunCallbacks, IPunObservable
{
    //ȭ��ǥ �̹���   0 : left , 1 : right
    public Sprite[] arrowSprite;

    //����
    int level = 0;

    //������ �̹��� ����
    int[] levelCount = { 3, 4, 5, 6 };
    float[] leveltimer = { 1.0f, 1.3f, 1.5f };
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

    //üũ�ϴ� ť
    Queue<int> check = new Queue<int>();

    public GameObject timerObj;
    float timer = 15.0f;
    public Image timerbar;
    bool wait = false;
    
    //ȭ��ǥ�� ����� �ϴ� �Լ��� ���� �ڷ�ƾ
    Coroutine currCo;

    public TextMeshProUGUI infoText;

    public GameObject GamePanel;          //���� �г�
    [Header("ResultPanel")]
    public GameObject ResultPanel;          //���â

    public TextMeshProUGUI myNickTxt;       //�� �г��� 
    public TextMeshProUGUI otherNickTxt;    //��� �г���
    public TextMeshProUGUI myScoreTxt;      // �� ����
    public TextMeshProUGUI otherScoreTxt;   //��� ����
    public TextMeshProUGUI winOrLose;       //�¸�����

    public GameObject ok_Btn;                   //Ȯ�� ��ư

    //����ȭ�� ���� ���� ����
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
        //������ ������ ����
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
        infoText.text = "������ ȭ��ǥ ���⿡ �°�\n������� �Է����ּ���\n�ð��� ������ ������ϴ�.";
        yield return new WaitForSeconds(1.5f);
        infoText.gameObject.SetActive(false);

        ArrowSetting();
        answerCountTxt.gameObject.SetActive(true);
        answerCountTxt.text = "���� ���� : 0";

        //�ӽ� 15�� ������ 60��
        timerObj.SetActive(true);
        timer = 15.0f;
        timerbar.fillAmount = timer / 60.0f;


        //���� ����
        while (true)
        {
            //Ÿ�� ����
            if (timer <= 0)
                break;

            yield return null;

            //üũ�ϱ�
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

        //Ÿ�� ������
        levelGroup[level].SetActive(false);
        MySetClear();

        answerCountTxt.text = "Ÿ�ӿ���!!";


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

        //�� �ɸ��� Ʋ���� 
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
        answerCountTxt.text = "���� ���� : " + answerCount;


        //���� ����ȭ�� ���� CustomProperties�� ����
        playerHash = PhotonNetwork.LocalPlayer.CustomProperties;

        //����
        if (playerHash.ContainsKey("RememberCount"))
            playerHash["RememberCount"] = answerCount;
        else
            playerHash.Add("RememberCount", answerCount);

        PhotonNetwork.LocalPlayer.SetCustomProperties(playerHash);


        ArrowSetting();
        MySetClear();

        mySelNum = 0;
    }

    void GameEnd()//��� ���� ������ ���â ������Ʈ
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

        //�¸� �����ϱ�
        if (myscore == otherscore)
        {
            winOrLose.text = "���º�";
            winOrLose.color = Color.green;
        }
        else if (myscore > otherscore)
        {
            winOrLose.text = "�¸�";
            winOrLose.color = Color.blue;
            //D
            InGame.Inst.WinGame();  //�� ���Ӹ޴������� �¸� ī��Ʈ ���ֱ�
        }
        else if (myscore < otherscore)
        {
            winOrLose.text = "�й�";
            winOrLose.color = Color.red;
        }

        ok_Btn.SetActive(true);
    }

    public void OnOkBtn()   //���� ������ Ȯ�ι�ư ������
    {
        //�̴ϰ��� ����
        ok_Btn.SetActive(false);
        ResultPanel.SetActive(false);
        this.gameObject.SetActive(false);

        //ȭ�� �������ֱ�
        InGame.Inst.SetLobby();
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
