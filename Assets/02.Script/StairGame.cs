using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
using Photon.Realtime;


public class StairGame : MonoBehaviourPunCallbacks
{
    //내 플레이어
    public PlayerCharacter myPlayer;

    //캐릭터들
    PlayerCharacter[] players;

    //계단 프리팹
    public GameObject stairPrefab;
    //계단 처음 스폰 위치
    public GameObject spawnPos;
    //소환된 계단들을 담아둘 링크드 리스트
    Queue<Stairs> stairs = new Queue<Stairs>();
    //마지막 계단을 확인하기 위해
    Stairs lastStair;

    //소환된 계단을 담아둘 게임으오브젝트
    public GameObject stairsGroup;
    //카메라
    GameObject camera;
    //배경
    GameObject BG;

    //점수 텍스트
    int score = 0;
    public TextMeshProUGUI scroe_Txt;


    public Image gageBar;
    float nextTimer = 2.0f;
    float timer = 2.0f;


    //게임 진행 중
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
      
        //맨 마지막 블럭 체크
        for (int i = 0; i < players.Length; i++)
        {
            Debug.Log(i + " 번째 체크");

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

        //맨 위 블럭에서 일정 밑에 있는 유저를 보고 새로운 계단 만들기
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
        //새로운 다음 계단 만들기
       
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
            Debug.Log("계단");
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
            Debug.Log("없음");
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
