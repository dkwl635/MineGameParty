using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
using Photon.Realtime;

public class TalkBox : MonoBehaviourPunCallbacks
{
    PhotonView pv;

    public TMP_InputField inputField;
    public TextMeshProUGUI text;
    public Button btn;

    string textInfo;

    private void Awake()
    {
        pv = GetComponent<PhotonView>();
    }

    private void Start()
    {
        btn.onClick.AddListener(SendMsg);
    }

    public void ClearText()
    {
        text.text = "";
        textInfo = "";

    }

    void SendMsg()
    {
        SoundMgr.Inst.PlayEffect("Button");

        string str = inputField.text;
        if (string.IsNullOrEmpty(str))
            return;

        pv.RPC("SendMsg", RpcTarget.All, str, PhotonNetwork.LocalPlayer.NickName);

        inputField.text = "";
    }

     void SendMsg(string msg)
    {
        string str = inputField.text;
        if (string.IsNullOrEmpty(str))
            return;

        pv.RPC("SendMsg", RpcTarget.All, str, PhotonNetwork.LocalPlayer.NickName);

    }


    [PunRPC]
    void SendMsg(string str, string nick)
    {
        if (nick.Equals(PhotonNetwork.LocalPlayer.NickName))
            textInfo += "<color=" + InGame.Inst.myNickNameColor.ToString() + ">" + nick + "</color>" + " : ";
        else
            textInfo += "<color=" + InGame.Inst.otherNickNameColor.ToString() + ">" + nick + "</color>" + " : ";

        textInfo += str;
        textInfo += "\n";

        text.text = textInfo;
    }

    public override void OnPlayerEnteredRoom(Player newPlayer) //누군가 들어오면
    {
        string str = "<color=blue>" + newPlayer.NickName + "님이 입장하였습니다." + "</color>";
        textInfo += str;
        textInfo += "\n";

        text.text = textInfo;
    }

    public override void OnPlayerLeftRoom(Player otherPlayer) //만약 다른 플레이어가 나간다면 
    {
        string str = "<color=red>" + otherPlayer.NickName + "님이 퇴장하였습니다." + "</color>";
        textInfo += str;
        textInfo += "\n";

        text.text = textInfo;
    }
}
