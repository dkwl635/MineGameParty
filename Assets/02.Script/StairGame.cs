using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
using Photon.Realtime;


public class StairGame : MonoBehaviourPunCallbacks
{
    //�� �÷��̾�
    public PlayerCharacter myPlayer;

    //ĳ���͵�
    PlayerCharacter[] players;

    //��� ������
    public GameObject stairPrefab;
    //��� ó�� ���� ��ġ
    public GameObject spawnPos;
    //��ȯ�� ��ܵ��� ��Ƶ� ��ũ�� ����Ʈ
    Queue<Stairs> stairs = new Queue<Stairs>();
    //������ ����� Ȯ���ϱ� ����
    Stairs lastStair;

    //��ȯ�� ����� ��Ƶ� ������������Ʈ
    public GameObject stairsGroup;
    //ī�޶�
    GameObject camera;
    //���
    GameObject BG;

    //���� �ؽ�Ʈ
    int score = 0;
    public TextMeshProUGUI scroe_Txt;


    public Image gageBar;
    float nextTimer = 2.0f;
    float timer = 2.0f;


    //���� ���� ��
    bool game = true;
    private void Start()
    {
        players = InGame.Inst.playerCharacters;
        myPlayer = players[0];
            
        camera = Camera.main.gameObject;
        BG = GameObject.Find("BG");

        GameObject.Find("LeftButton").GetComponent<Button>().onClick.AddListener(LeftMove);
        GameObject.Find("RightButton").GetComponent<Button>().onClick.AddListener(RightMove);

        if(PhotonNetwork.IsMasterClient)
        {
            FirstSpawnStair();

            for (int i = 0; i < 20; i++)
            {
                SpawnStair();
            }     
        }

        myPlayer.isMove = false;
        myPlayer.transform.position = spawnPos.transform.position + Vector3.up * 1.0f;
        score = 0;

        
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

 
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftArrow))
            LeftMove();

        if (Input.GetKeyDown(KeyCode.RightArrow))
            RightMove();


        if (PhotonNetwork.IsMasterClient)
            SpawnCheckStair();

        //if(timer > 0)
        //{
        //    timer -= Time.deltaTime;
        //    gageBar.fillAmount = timer / nextTimer;
        //    if (timer <= 0)
        //    {                   
        //        GameOver();
        //    }
        //}
        


    }


    void SpawnCheckStair()
    {
        bool destory = false;
      
        //�� ������ �� üũ
        for (int i = 0; i < players.Length; i++)
        {
            Debug.Log(i + " ��° üũ");

            if (stairs.Peek().transform.position.y + 1 < players[i].transform.position.y)
                destory = true;        
            else
                destory = false;

        }

        if(destory)
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


    void FirstSpawnStair()
    {
        Stairs newStairs = PhotonNetwork.InstantiateRoomObject("Stairs", spawnPos.transform.position, Quaternion.identity).GetComponent<Stairs>();
            //GameObject.Instantiate(stairPrefab).GetComponent<Stairs>();     
        newStairs.num = 0;
        stairs.Enqueue(newStairs);
        lastStair = newStairs;
        newStairs.transform.SetParent(stairsGroup.transform);
    }

    void SpawnStair()
    {
        //���ο� ���� ��� �����
       
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

        //Stairs newStairs = GameObject.Instantiate(stairPrefab).GetComponent<Stairs>()   ;
        Vector3 nextPos = new Vector3(nextX , 1);
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
            nextTimer -= 0.1f;
            if (nextTimer <= 0.5f)
                nextTimer = 0.5f;

            timer = nextTimer;
            gageBar.fillAmount = timer / nextTimer;

            score += 1;
            scroe_Txt.text = score.ToString();

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
        game = false;
        myPlayer.SetHit();
    }
}
