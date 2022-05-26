using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PlayerController : MonoBehaviour
{
    TestPlayer player;

   
    private void Awake()
    {
        player = GameObject.Find("TestPlayer").GetComponent<TestPlayer>();
    }

    
    public void MoveBtnDown(int h)
    {
        player.MoveStart(h);
    }

    public void MoveBtnUp(int h)
    {
        player.MoveEnd(h);
    }


   
}
