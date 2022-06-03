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

    public void RightMove()
    {
        player.transform.position += Vector3.up + Vector3.right;
    }

    public void LeftMove()
    {
        player.transform.position += Vector3.up + Vector3.left;
    }

    private void Start()
    {
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
      if(stairs.First.Value.transform.position.y + 1 < player.transform.position.y)
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


}
