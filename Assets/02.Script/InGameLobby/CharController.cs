using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharController : MonoBehaviour
{
    public PlayerCharacter player;

    public void MoveBtnDown(int h)
    {
        player.MoveStart(h);
    }
    public void MoveBtnUp(int h)
    {
        player.MoveEnd(h);
    }
}
