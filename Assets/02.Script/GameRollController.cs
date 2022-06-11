using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameRollController : MonoBehaviourPunCallbacks 
{
   public  PhotonView pv;

    [SerializeField]float power = 0;
    bool roll = false; //돌고 있는지
    bool endroll = false;   //

    //각각 번호 마다 멈춰야 할 각도 
    float[] angles = { 0.0f, 45.0f, 90.0f, 135.0f, 180.0f, 225.0f, 270.0f , 315.0f, 360.0f};
    float goalAngle = 0;// 목표 각도 값

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
            if (power < 1.0f)   //일정 속도 밑으로 내려가면 마지막 바퀴를 돌기위한 준비
            {
                power = 0.0f;
                roll = false;
                endroll = true;
                return;
            }

            transform.Rotate(0, power, 0);
            power *= 0.97f;

        }

        if (endroll)    //마지막 목표 회전각도 까지 돌다가 멈춘다.
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
        //게임 판 돌리기
        int rand = Random.Range(0, 2);            
        pv.RPC("RollStart", RpcTarget.All, rand); //모든 플레이어에게 다음 게임 번호 알려주고 
        //판돌리기

        //판을 돌리기전에 이미 정해졌지만
        //돌아가는 연출을 보여준다.

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
