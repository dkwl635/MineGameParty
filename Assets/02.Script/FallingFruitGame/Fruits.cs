using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fruits : MonoBehaviourPunCallbacks 
{
    PhotonView pv;

    LobbyPlayerController player;
    float dis = 10000.0f;
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
            bool get = false;
            for (int i = 0; i < FallingFruitGame.Inst.playerObj.Length; i++)
            {
                temp = Vector2.Distance(FallingFruitGame.Inst.playerObj[i].transform.position, transform.position);
                
                if (temp <= 0.5f)
                {
                    get = true;                                  
                    
                    if(temp < dis)  //가장 가까운 유저
                    {
                        player = FallingFruitGame.Inst.playerObj[i];
                        dis = temp;
                    }             
                }
            
            }
            
            if(get)
            {
                FallingFruitGame.Inst.AddScore(player, this.transform.position);
                PhotonNetwork.Destroy(this.pv);
            }

        }
    }


}
