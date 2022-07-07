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

    public void SetResult()//결과창 셋팅
    {
        ResultPanel.SetActive(true);
        SoundMgr.Inst.PlayEffect("ResultOpenSound");
        SoundMgr.Inst.PlayBGM("InGame");

        myNickTxt.text = "<color=" + InGame.Inst.myNickNameColor.ToString() + ">" + PhotonNetwork.LocalPlayer.NickName + "</color>";
        otherNickTxt.text = "<color=" + InGame.Inst.otherNickNameColor.ToString() + ">" + PhotonNetwork.PlayerListOthers[0].NickName + "</color>";
        //각 점수 가져오기 
        int myscore = PhotonNetwork.LocalPlayer.CustomProperties.ContainsKey("score") ? (int)PhotonNetwork.LocalPlayer.CustomProperties["score"] : 0;
        int otherscore = PhotonNetwork.PlayerListOthers[0].CustomProperties.ContainsKey("score") ? (int)PhotonNetwork.PlayerListOthers[0].CustomProperties["score"] : 0;
        //점수 표시
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
