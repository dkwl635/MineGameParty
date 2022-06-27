using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;


public enum MyColor
{
    red,
    orange,
    yellow,
    green,
    blue,
    purple,
    white,
    black,
    end,
}

public class Palette : MonoBehaviourPunCallbacks
{
    public Button[] colorBtn;
    int curColorNum = 0;   //방장이면 처음에 빨강
    int otherColorNum = 8;  //아니면 처음에 파랑

   // MyColor curColor = MyColor.red;
   // MyColor otherColor = MyColor.blue;

    ExitGames.Client.Photon.Hashtable playerHash;

    private void Start()
    {
        StartCoroutine(FirstColor());
    }

    IEnumerator FirstColor()
    {
        SetBtnFunc();
        
        //yield return null;
       yield return new WaitForEndOfFrame();

        if (PhotonNetwork.IsMasterClient)
            curColorNum = 0;
        else
        {
            if (PhotonNetwork.PlayerListOthers.Length > 0)
                otherColorNum = (int)PhotonNetwork.PlayerListOthers[0].CustomProperties["NickColor"];

            do
            {
                curColorNum = Random.Range((int)MyColor.red, (int)MyColor.end);
            }
             while (curColorNum == otherColorNum) ;    
        }


       
        ChangeColor(curColorNum);
       
    }


    void ChangeColor(int colorNum)
    {
        curColorNum = colorNum;
        InGame.Inst.myNickNameColor = (MyColor)curColorNum;

        playerHash = PhotonNetwork.LocalPlayer.CustomProperties;
        if (playerHash.ContainsKey("NickColor"))
            playerHash["NickColor"] = curColorNum;
        else
            playerHash.Add("NickColor", curColorNum);
        PhotonNetwork.LocalPlayer.SetCustomProperties(playerHash);


        CheckColorBtn();
    }

    void SetBtnFunc()
    {
        colorBtn = GetComponentsInChildren<Button>();
      
        for (int i = 0; i < colorBtn.Length; i++)
        {
            int idx = i;
            if(colorBtn != null)
            colorBtn[i].onClick.AddListener(() => { ChangeColor(idx); });
        }
    }

    void CheckColorBtn()
    {
        for (int i = 0; i < colorBtn.Length; i++)
        {
            colorBtn[i].gameObject.SetActive(true);
        }

        colorBtn[curColorNum].gameObject.SetActive(false);

        if (otherColorNum != (int)MyColor.end)
            colorBtn[otherColorNum].gameObject.SetActive(false);

        InGame.Inst.myNickNameColor = (MyColor)curColorNum;
        InGame.Inst.otherNickNameColor = (MyColor)otherColorNum;
        InGame.Inst.ChangeNickColor();
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer
                     , ExitGames.Client.Photon.Hashtable changedProps)
    { 
        if (targetPlayer != PhotonNetwork.LocalPlayer)
        {
            if (changedProps.ContainsKey("NickColor"))
                otherColorNum = (int)changedProps["NickColor"];
    
            CheckColorBtn();
        }
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        otherColorNum = 8;
        CheckColorBtn();
    }



}
