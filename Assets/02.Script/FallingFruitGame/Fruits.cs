using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fruits : MonoBehaviourPunCallbacks 
{
    PhotonView pv;
    float dis = 0.5f;
    float temp = 0;
    private void OnEnable()
    {
        pv = GetComponent<PhotonView>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!PhotonNetwork.LocalPlayer.IsMasterClient)
            return;

        transform.Translate(Vector2.down * Time.deltaTime);

    }

    private void OnTriggerEnter2D(Collider2D coll)
    {
        if (!PhotonNetwork.LocalPlayer.IsMasterClient)
            return;

        FallingFruitGame.Inst.AddScore(coll.GetComponent<LobbyPlayerController>());  
        PhotonNetwork.Destroy(this.gameObject);
    }

}
