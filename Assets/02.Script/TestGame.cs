using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TestGame : MonoBehaviour
{
    //내 플레이어
    public TestPlayer player;
    //계단 프리팹
    public GameObject stairPrefab;
    //계단 처음 스폰 위치
    public GameObject spawnPos;
    //계단들을 담아둘 링크드 리스트
    LinkedList<Stairs> stairs = new LinkedList<Stairs>();
    //소환된 계단을 담아둘 게임으오브젝트
    public GameObject stairsGroup;
    //카메라
    public GameObject camera;

    public Image gageBar;
    float nextTimer = 2.0f;
    float timer = 2.0f;


    //게임 진행 중
    bool game = true;


    public void RightMove()
    {
        if (!game)
            return;

        player.transform.position += Vector3.up + Vector3.right;

        camera.transform.position += Vector3.up;

        CheckStair();
    }
    public void LeftMove()
    {
        if (!game)
            return;

        player.transform.position += Vector3.up + Vector3.left;
        
        camera.transform.position += Vector3.up;

        CheckStair();
    }

    private void Start()
    {
  
        camera = Camera.main.gameObject;

        Stairs newStairs = GameObject.Instantiate(stairPrefab).GetComponent<Stairs>();
        newStairs.transform.position = spawnPos.transform.position;
        newStairs.num = 0;

        stairs.AddFirst(newStairs);
        newStairs.transform.SetParent(stairsGroup.transform);

        for (int i = 0; i < 20; i++)
        {
            SpawnStair();
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftArrow))
            LeftMove();

        if (Input.GetKeyDown(KeyCode.RightArrow))
            RightMove();

        if (stairs.First.Value.transform.position.y + 1 < player.transform.position.y)
        {
            GameObject obj = stairs.First.Value.gameObject;
            stairs.RemoveFirst();
            Destroy(obj);

            SpawnStair();
        }

        if(timer > 0)
        {
            timer -= Time.deltaTime;
            gageBar.fillAmount = timer / nextTimer;
            if (timer <= 0)
            {
                game = false;
            }
        }


    }

    void SpawnStair()
    {
        //새로운 다음 계단 만들기
        Stairs newStairs = GameObject.Instantiate(stairPrefab).GetComponent<Stairs>();
        int nextX = 0;

        Stairs Last = stairs.Last.Value;    
        if (Last.num == 3)
        {
            nextX = -1;
        }
        else if (Last.num == -3)
        {
            nextX = 1;
        }
        else
        {
            nextX = Random.Range(0, 2) == 0 ? -1 : 1;        
        }

        newStairs.num = Last.num + nextX;
        Vector3 nextPos = new Vector3(nextX , 1);
        newStairs.transform.position = Last.transform.position + nextPos;

        stairs.AddLast(newStairs);
    }

    void CheckStair()
    {
        RaycastHit2D hit = Physics2D.Raycast(player.transform.position, Vector3.down, 1.0f);
        if(hit)
        {
            Debug.Log("계단");
            nextTimer -= 0.1f;
            if (nextTimer <= 0.5f)
                nextTimer = 0.5f;

            timer = nextTimer;
            gageBar.fillAmount = timer / nextTimer;

        }
        else
        {
            Debug.Log("없음");
            NoStair();
        }
    }

    void NoStair()
    {
        game = false;
        player.SetHit();

        GameOver();
    }

    void GameOver()
    {

    }
}
