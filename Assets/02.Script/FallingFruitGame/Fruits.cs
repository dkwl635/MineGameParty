using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fruits : MonoBehaviourPunCallbacks 
{
    PhotonView pv;
    Animator animator;
  
    PlayerCharacter player;
    float dis = 10000.0f;
    float temp = 0;

   public int type = 0;

    public override void OnEnable()
    {
        pv = GetComponent<PhotonView>();
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {              
        //동시 충돌 알아보기
        if (PhotonNetwork.LocalPlayer.IsMasterClient)
        {
                animator.SetInteger("Fruits", type);
                transform.Translate(Vector2.down * Time.deltaTime);
                
                bool get = false;
                for (int i = 0; i < FallingFruitGame.Inst.playerObj.Length; i++)
                {
                    temp = Vector2.Distance(FallingFruitGame.Inst.playerObj[i].transform.position, transform.position);
                    
                    if (temp <= 0.7f)
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
                    FallingFruitGame.Inst.GetFruit(player, this.transform.position);
                    PhotonNetwork.Destroy(this.pv);
                }

                if(transform.position.y < -7)
                    PhotonNetwork.Destroy(this.pv);
        }
    }


}
