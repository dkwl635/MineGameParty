using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
using Photon.Realtime;


public class StairGame : Game
{
    //�� �÷��̾�
    public PlayerCharacter myPlayer;
    //ĳ���͵�
    PlayerCharacter[] players;

    public GameObject GamePanel;          //���Ӱ��� UI
    //��� ������
    public GameObject stairPrefab;
    //��� ó�� ���� ��ġ
    public GameObject spawnPosObj;
    Vector3 spawnPos = Vector3.zero; 
    //��ȯ�� ��ܵ��� ��Ƶ�  ť 
    Queue<Stairs> stairs = new Queue<Stairs>();
    //������ ����� Ȯ���ϱ� ����
    Stairs lastStair;

    //��ȯ�� ����� ��Ƶ� ���ӿ�����Ʈ
    public GameObject stairsGroup;
    //ī�޶� //ĳ���Ͱ� �ö󰡱� ������ ī�޶�� ���ȭ�鵵 ���� �ø��� ���ؼ�
    GameObject camera;
    //���
    GameObject BG;

    //���� �ؽ�Ʈ
    public TextMeshProUGUI scroe_Txt;

    //���ѽð�
    public Image gageBar;
    float nextTimer = 2.0f;
    float timer = 2.0f;

    //���� ���� ��
    bool game = false;

    //��� Sprite ������ �ֱ�����
    SpriteRenderer otherSprite;

    //���� ī��Ʈ
    public TextMeshProUGUI countText;


    public override void StartGame()
    {
        base.StartGame();

        //ĳ���� ����
        players = InGame.Inst.playerCharacters;
        myPlayer = players[0];

        //���ĳ���ʹ� ����������
        otherSprite = players[1].GetComponent<SpriteRenderer>();
        otherSprite.color = new Color(1, 1, 1, 0.5f);

        camera = Camera.main.gameObject;
        BG = GameObject.Find("BG");

        //�ö󰡴� ��ư ����
        Button LBT = GameObject.Find("LeftButton").GetComponent<Button>();
        LBT.onClick.RemoveAllListeners();
        LBT.onClick.AddListener(LeftMove);

        Button RBT = GameObject.Find("RightButton").GetComponent<Button>();
        RBT.onClick.RemoveAllListeners();
        RBT.onClick.AddListener(RightMove);

        //��� �����ϱ�
        if (PhotonNetwork.IsMasterClient)
            for (int i = 0; i < 20; i++)
                SpawnStair();
            
       
        //�¿� �̵��� ���´�
        myPlayer.isMove = false;

        //ĳ���� �̵�
        spawnPos = spawnPosObj.transform.position + Vector3.up;
        spawnPos.z = myPlayer.transform.position.z;
        myPlayer.transform.position = spawnPos;

        gageBar.fillAmount = 1.0f;


        GamePanel.SetActive(true);
        StartCoroutine(Game_Co());
    }

  
    public void RightMove()
    {
        if (!game)
            return;

        myPlayer.transform.position += Vector3.up + Vector3.right;

        camera.transform.position += Vector3.up;
        BG.transform.position += Vector3.up;

        CheckStair();
    }
    public void LeftMove()
    {
        if (!game)
            return;

        myPlayer.transform.position += Vector3.up + Vector3.left;
        
        camera.transform.position += Vector3.up;
        BG.transform.position += Vector3.up;

        CheckStair();
    }

    IEnumerator Game_Co()
    {
        countText.text = "3";
        yield return new WaitForSeconds(1.0f);
        countText.text = "2";
        yield return new WaitForSeconds(1.0f);
        countText.text = "1";
        yield return new WaitForSeconds(1.0f);
        countText.text = "Start!!";

        game = true;
        gageBar.fillAmount = 1.0f;
        timer = 2.0f;

        yield return new WaitForSeconds(1.0f);
        countText.text = "";


        while (true)
        {
            yield return null;

            if (PhotonNetwork.IsMasterClient && game)
                SpawnCheckStair();


            if (!game)
                continue;

            if (timer > 0)
            {
                timer -= Time.deltaTime;
                gageBar.fillAmount = timer / nextTimer;
                if (timer <= 0)
                {
                    GameOver();
                }
            }
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftArrow))
            LeftMove();

        if (Input.GetKeyDown(KeyCode.RightArrow))
            RightMove();

    }


    void SpawnCheckStair()
    {
        bool destory = true;
      
        //�� ������ �� üũ
        for (int i = 0; i < players.Length; i++)
        {         
            if (stairs.Peek().transform.position.y + 1.2f > players[i].transform.position.y)
                destory = false;
            // - 1.5, - 2.4

            Debug.Log(stairs.Peek().transform.position.y + " : " + players[i].transform.position.y);
        }

        if (destory)
        {
            var d = stairs.Dequeue();
            PhotonNetwork.Destroy(d.gameObject);
        }

        //�� �� ������ ���� �ؿ� �ִ� ������ ���� ���ο� ��� �����
        bool spawn = false;
        for (int i = 0; i < players.Length; i++)
        {
            if (lastStair.transform.position.y - 10 < players[i].transform.position.y)
                spawn = true;     
        }

        if (spawn)
            SpawnStair();


    }

  
    void SpawnStair()
    {
        //���ο� ���� ��� �����   

        if(stairs.Count.Equals(0))
        {
            Stairs newStairs = PhotonNetwork.InstantiateRoomObject("Stairs", spawnPosObj.transform.position, Quaternion.identity).GetComponent<Stairs>();
            newStairs.num = 0;
            stairs.Enqueue(newStairs);
            lastStair = newStairs;
            newStairs.transform.SetParent(stairsGroup.transform);

            return;
        }

        int nextX = 0;   
        if (lastStair.num == 3)
        {
            nextX = -1;
        }
        else if (lastStair.num == -3)
        {
            nextX = 1;
        }
        else
        {
            nextX = Random.Range(0, 2) == 0 ? -1 : 1;        
        }
    
        Vector3 nextPos = new Vector3(nextX , 1);//���� ĭ ���� �¿�� ���� ���� 1ĭ ����
        nextPos = lastStair.transform.position + nextPos;

        GameObject newStairObj = PhotonNetwork.InstantiateRoomObject("Stairs", nextPos, Quaternion.identity);
        Stairs newStair = newStairObj.GetComponent<Stairs>();
        newStair.transform.SetParent(stairsGroup.transform);      
        newStair.num = lastStair.num + nextX;
        stairs.Enqueue(newStair);
        lastStair = newStair;
    }

    void CheckStair()
    {
        RaycastHit2D hit = Physics2D.Raycast(myPlayer.transform.position, Vector3.down, 1.0f);
        if(hit)
        {
            Debug.Log("���");
            nextTimer -= 0.01f;
            if (nextTimer <= 0.5f)
                nextTimer = 0.5f;

            timer = nextTimer;
            gageBar.fillAmount = timer / nextTimer;

            score += 1;
            scroe_Txt.text = score.ToString();

            playerHash = PhotonNetwork.LocalPlayer.CustomProperties;
            if (playerHash.ContainsKey("score"))
                playerHash["score"] = (int)playerHash["score"] + 1;
            else
                playerHash.Add("score", 1);

            PhotonNetwork.LocalPlayer.SetCustomProperties(playerHash);

        }
        else
        {
            Debug.Log("����");
            NoStair();
        }
    }

    void NoStair()
    {   
        GameOver();
    }

    void GameOver()
    { 
        myPlayer.SetHit();
        pv.RPC("GameEnd", RpcTarget.AllViaServer);   
    }

    [PunRPC]
    void GameEnd()
    {
        game = false; 
        StartCoroutine(GameEnd_Co());
    }

    IEnumerator GameEnd_Co()
    {    
        yield return new WaitForSeconds(0.5f);
        otherSprite.color = Color.white;
        players[0].transform.position = Vector3.zero;
        players[1].transform.position = Vector3.zero;

        camera.transform.position = new Vector3(0, 0, camera.transform.position.z);
        BG.transform.position = new Vector3(0, 0, BG.transform.position.z);

        while (stairs.Count > 0)    
            PhotonNetwork.Destroy(stairs.Dequeue().gameObject);
        
        stairs.Clear();

        countText.text = "����";
        yield return new WaitForSeconds(1.0f);
        countText.text = "�����";
        yield return new WaitForSeconds(1.0f);
        myPlayer.isMove = true;
        countText.text = "";

        GamePanel.SetActive(false);

        InGame.Inst.ShowResult();




    }

   


}
