using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameRollController : MonoBehaviourPunCallbacks 
{
    public  PhotonView pv;

    public int GameCount = 0;

    [Header("Roll")]
    public GameObject roller;
    Transform rollTr;
    [SerializeField]float power = 0;
    bool roll = false; //돌고 있는지
    bool endroll = false;   //
    //각각 번호 마다 멈춰야 할 각도 
    float[] angles = { 0.0f, 90.0f, 180.0f, 270.0f};
    float goalAngle = 0;// 목표 각도 값

    public GameObject waitText;

    ExitGames.Client.Photon.Hashtable playerHash;
    AudioSource audioSource;

    

    private void Awake()
    {
        pv = GetComponent<PhotonView>();

        rollTr = roller.GetComponent<Transform>();
        audioSource = roller.GetComponent<AudioSource>();
    }


    public int  Roll()
    {
        //게임 판 돌리기
        int rand = Random.Range(0, GameCount);            
        pv.RPC("RollStart", RpcTarget.All, rand); //모든 플레이어에게 다음 게임 번호 알려주고 
        //판돌리기
        //판을 돌리기전에 이미 정해졌지만
        //돌아가는 연출을 보여준다.

        return rand;    
    }

    IEnumerator StartRoll(int num)
    {
        roller.SetActive(true);
        goalAngle = angles[num];
        roll = true;
        power = 100;

        waitText.SetActive(false);

        while (true)
        {
            yield return null;

            if (roll)
            {
                rollTr.Rotate(0, power, 0);
                power *= 0.97f;

                if (power < 1.0f)   //일정 속도 밑으로 내려가면 마지막 바퀴를 돌기위한 준비
                {                  
                    roll = false;
                    endroll = true;                  
                }                      

            }
           else if (endroll)    //마지막 목표 회전각도 까지 돌다가 멈춘다.
            {
                rollTr.Rotate(0, power, 0);             
                if (Mathf.Abs(goalAngle - rollTr.rotation.eulerAngles.y) <= 1.0f)
                {
                    rollTr.rotation = Quaternion.Euler(new Vector3(0, angles[num], 0));                
                    yield return new WaitForSeconds(1.5f);
                    endroll = false;

                    playerHash = PhotonNetwork.LocalPlayer.CustomProperties;
                    if (playerHash.ContainsKey("DiceEnd"))
                        playerHash["DiceEnd"] = true;
                    else
                        playerHash.Add("DiceEnd", true);
                    PhotonNetwork.LocalPlayer.SetCustomProperties(playerHash);

                    audioSource.Stop();
                    waitText.SetActive(true);
                    break;

                }

            }
        }
       

    }


    public bool EndRoll()
    {
        for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
        {
            if (!(bool)PhotonNetwork.PlayerList[i].CustomProperties["DiceEnd"])
            {
                return false;
            }
        }

        return true;
    }

    [PunRPC]
    void RollStart(int i)
    {
        StartCoroutine(StartRoll(i));
    }
   

}
