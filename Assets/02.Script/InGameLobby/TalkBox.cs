using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;

public class TalkBox : MonoBehaviour
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
        string str = inputField.text;
        if (string.IsNullOrEmpty(str))
            return;

        pv.RPC("SendMsg", RpcTarget.All, str, PhotonNetwork.LocalPlayer.NickName);

        inputField.text = "";
    }



    [PunRPC]
    void SendMsg(string str, string nick)
    {
        if (nick.Equals(PhotonNetwork.LocalPlayer.NickName))
            textInfo += "<color=blue>" + nick + "</color>" + " : ";
        else
            textInfo += "<color=red>" + nick + "</color>" + " : ";

        textInfo += str;
        textInfo += "\n";

        text.text = textInfo;
    }


}
