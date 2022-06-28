using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Photon.Pun;

public class ResultUI : MonoBehaviour
{
    static public ResultUI Inst;

    [Header("ResultPanel")]
    public GameObject ResultPanel;          //결과창
    public TextMeshProUGUI myNickTxt;       //내 닉네임 
    public TextMeshProUGUI otherNickTxt;    //상대 닉네임
    public TextMeshProUGUI myScoreTxt;      // 내 점수
    public TextMeshProUGUI otherScoreTxt;   //상대 점수
    public TextMeshProUGUI winOrLose;       //승리판정
    public GameObject ok_Btn;                   //확인 버튼

    public void SetResult()
    {
        ResultPanel.SetActive(true);

        SoundMgr.Inst.PlayEffect("ResultOpenSound");


        myNickTxt.text = PhotonNetwork.LocalPlayer.NickName;
        otherNickTxt.text = PhotonNetwork.PlayerListOthers[0].NickName;


        int myscore = PhotonNetwork.LocalPlayer.CustomProperties.ContainsKey("score") ? (int)PhotonNetwork.LocalPlayer.CustomProperties["score"] : 0;
        int otherscore = PhotonNetwork.PlayerListOthers[0].CustomProperties.ContainsKey("score") ? (int)PhotonNetwork.PlayerListOthers[0].CustomProperties["score"] : 0;

        myScoreTxt.text = myscore.ToString();
        otherScoreTxt.text = otherscore.ToString();

        //승리 판정하기
        if (myscore == otherscore)
        {
            winOrLose.text = "무승부";
            winOrLose.color = Color.green;
        }
        else if (myscore > otherscore)
        {
            winOrLose.text = "승리";
            winOrLose.color = Color.blue;
            //이김
            InGame.Inst.WinGame();  //본 게임메니저에서 승리 카운트 해주기
        }
        else if (myscore < otherscore)
        {
            winOrLose.text = "패배";
            winOrLose.color = Color.red;
        }


        ok_Btn.SetActive(true);
    }


    public void OnOkBtn()   //게임 종료후 확인버튼 누르면
    {
        //미니게임 종료
        ok_Btn.SetActive(false);
        ResultPanel.SetActive(false);

        //화면 갱신해주기
        InGame.Inst.SetLobby();
    }



}
