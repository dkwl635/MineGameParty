using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fruits : MonoBehaviourPunCallbacks 
{
    PhotonView pv;
    float dis = 0.5f;
    float temp = 0;
    public override void OnEnable()
    {
        pv = GetComponent<PhotonView>();
    }

    // Update is called once per frame
    void Update()
    {
        transform.Translate(Vector2.down * Time.deltaTime);

        //동시 충돌 알아보기
        if (PhotonNetwork.LocalPlayer.IsMasterClient)
        {
            for (int i = 0; i < FallingFruitGame.Inst.playerObj.Length; i++)
            {
             
                if (Vector2.Distance(FallingFruitGame.Inst.playerObj[i].transform.position , transform.position) <= 0.5f)
                {
                    FallingFruitGame.Inst.AddScore(FallingFruitGame.Inst.playerObj[i]);
                    PhotonNetwork.Destroy(this.pv);
                }

            }
        }
    }

}
