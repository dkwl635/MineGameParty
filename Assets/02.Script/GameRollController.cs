using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameRollController : MonoBehaviourPunCallbacks 
{
   public  PhotonView pv;

    [SerializeField]float power = 0;
    bool roll = false;
    bool endroll = false;

    float[] angles = { 0.0f, 45.0f, 90.0f, 135.0f, 180.0f, 225.0f, 270.0f , 315.0f, 360.0f};
    float goalAngle = 0;

    private void Awake()
    {
        pv = GetComponent<PhotonView>();
     
    }


    // Update is called once per frame
    void Update()
    {
        if (PhotonNetwork.LocalPlayer.IsMasterClient)
        {
            if (Input.GetKeyDown(KeyCode.R))
            {
                Roll();
            }
        }


            if (roll)
            {
                if (power < 1.0f)
                {                 
                    power = 0.0f;
                    roll = false;
                    endroll = true;
                    return;
                }

                transform.Rotate(0, power, 0);
                power *= 0.97f;

            }

        if (endroll)
        {
            transform.Rotate(0, 1, 0);
            if (Mathf.Abs(goalAngle - transform.rotation.eulerAngles.y) <= 1.0f)
            {
                endroll = false;
            }
        }

        
    }
    public int  Roll()
    {
        int rand = Random.Range(0, 9);            
        pv.RPC("RollStart", RpcTarget.All, rand);

        return rand;    
    }

    public bool EndRoll()
    {
        if (!roll && !endroll)
            return true;

        return false;
    }

    [PunRPC]
    void RollStart(int i)
    {
        goalAngle = angles[i]; 
        roll = true;
        power = 100;     
    }
   

}
