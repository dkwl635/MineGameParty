using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestGame : MonoBehaviour
{
    public TestPlayer player;
    public GameObject stairPrefab;
    public GameObject spawnPos;
    LinkedList<Stairs> stairs = new LinkedList<Stairs>();
    public GameObject stairsGroup;
    public GameObject camera;

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

    }

    void SpawnStair()
    {
        Stairs newStairs = GameObject.Instantiate(stairPrefab).GetComponent<Stairs>();
        int nextX = 0;

        var Last = stairs.Last.Value;    
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
    }

}
