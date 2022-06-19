using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;




public class Game : MonoBehaviourPunCallbacks
{
    //Photon
    protected PhotonView pv;
    //동기화를 위한 변수 선언
   protected  ExitGames.Client.Photon.Hashtable playerHash;

   public virtual void StartGame()
    {
       
    }


}
