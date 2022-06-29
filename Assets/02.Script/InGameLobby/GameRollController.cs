using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameRollController : MonoBehaviourPunCallbacks 
{
    public  PhotonView pv;

    public int GameCount = 0;

    [Header("Roll")]
    public GameObject roller;
    Transform rollTr;
    [SerializeField]float power = 0;
    bool roll = false; //���� �ִ���
    bool endroll = false;   //
    //���� ��ȣ ���� ����� �� ���� 
    //float[] angles = { 0.0f, 45.0f, 90.0f, 135.0f, 180.0f, 225.0f, 270.0f , 315.0f, 360.0f};
    float[] angles = { 0.0f, 90.0f, 180.0f, 270.0f};
    float goalAngle = 0;// ��ǥ ���� ��



    private void Awake()
    {
        pv = GetComponent<PhotonView>();

        rollTr = roller.GetComponent<Transform>();
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


    }
    public int  Roll()
    {
        //���� �� ������
        int rand = Random.Range(0, GameCount);            
        pv.RPC("RollStart", RpcTarget.All, rand); //��� �÷��̾�� ���� ���� ��ȣ �˷��ְ� 
        //�ǵ�����
        //���� ���������� �̹� ����������
        //���ư��� ������ �����ش�.

        return rand;    
    }

    IEnumerator StartRoll(int num)
    {
        roller.SetActive(true);
           goalAngle = angles[num];
        roll = true;
        power = 100;
    
        while(true)
        {
            yield return null;

            if (roll)
            {
                rollTr.Rotate(0, power, 0);
                power *= 0.97f;

                if (power < 1.0f)   //���� �ӵ� ������ �������� ������ ������ �������� �غ�
                {                  
                    roll = false;
                    endroll = true;                  
                }                      

            }
           else if (endroll)    //������ ��ǥ ȸ������ ���� ���ٰ� �����.
            {
                rollTr.Rotate(0, power, 0);             
                if (Mathf.Abs(goalAngle - rollTr.rotation.eulerAngles.y) <= 1.0f)
                {
                    rollTr.rotation = Quaternion.Euler(new Vector3(0, angles[num], 0));

                 
                    yield return new WaitForSeconds(1.5f);
                    roller.SetActive(false);

                    endroll = false;
                    break;

                }

            }
        }
       

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
        StartCoroutine(StartRoll(i));
    }
   

}
