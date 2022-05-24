using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fruits : MonoBehaviourPunCallbacks , IPunObservable
{
    PhotonView pv;


    private void OnEnable()
    {
        pv = GetComponent<PhotonView>();
    }

    // Update is called once per frame
    void Update()
    {
        transform.Translate(Vector2.down * Time.deltaTime);

        for (int i = 0; i < FallingFruitGame.Inst.playerObj.Length; i++)
        {
            Debug.Log(Vector2.Distance(transform.position, FallingFruitGame.Inst.playerObj[i].transform.position));
            if(Vector2.Distance(transform.position, FallingFruitGame.Inst.playerObj[i].transform.position) <= 0.5f)
            {
                PhotonNetwork.Destroy(this.gameObject);
                return;
            }

        }


    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        
    }


    

}
