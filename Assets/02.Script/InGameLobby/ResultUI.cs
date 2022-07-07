using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Photon.Pun;

public class ResultUI : MonoBehaviour
{
    static public ResultUI Inst;
    [Header("ResultPanel")]
    public GameObject ResultPanel;          //���â
    public TextMeshProUGUI myNickTxt;       //�� �г��� 
    public TextMeshProUGUI otherNickTxt;    //��� �г���
    public TextMeshProUGUI myScoreTxt;      // �� ����
    public TextMeshProUGUI otherScoreTxt;   //��� ����
    public TextMeshProUGUI winOrLose;       //�¸�����
    public GameObject ok_Btn;                   //Ȯ�� ��ư

    public void SetResult()//���â ����
    {
        ResultPanel.SetActive(true);
        SoundMgr.Inst.PlayEffect("ResultOpenSound");
        SoundMgr.Inst.PlayBGM("InGame");

        myNickTxt.text = "<color=" + InGame.Inst.myNickNameColor.ToString() + ">" + PhotonNetwork.LocalPlayer.NickName + "</color>";
        otherNickTxt.text = "<color=" + InGame.Inst.otherNickNameColor.ToString() + ">" + PhotonNetwork.PlayerListOthers[0].NickName + "</color>";
        //�� ���� �������� 
        int myscore = PhotonNetwork.LocalPlayer.CustomProperties.ContainsKey("score") ? (int)PhotonNetwork.LocalPlayer.CustomProperties["score"] : 0;
        int otherscore = PhotonNetwork.PlayerListOthers[0].CustomProperties.ContainsKey("score") ? (int)PhotonNetwork.PlayerListOthers[0].CustomProperties["score"] : 0;
        //���� ǥ��
        myScoreTxt.text = myscore.ToString();
        otherScoreTxt.text = otherscore.ToString();
        //�¸� �����ϱ�
        if (myscore == otherscore)
        {
            winOrLose.text = "���º�";
            winOrLose.color = Color.green;
        }
        else if (myscore > otherscore)
        {
            winOrLose.text = "�¸�";
            winOrLose.color = Color.blue;
            InGame.Inst.WinGame();  //�� ���Ӹ޴������� �¸� ī��Ʈ ���ֱ�
        }
        else if (myscore < otherscore)
        {
            winOrLose.text = "�й�";
            winOrLose.color = Color.red;
        }
        ok_Btn.SetActive(true);
    }
    public void OnOkBtn()   //���� ������ Ȯ�ι�ư ������
    {
        //�̴ϰ��� ����
        ok_Btn.SetActive(false);
        ResultPanel.SetActive(false);
        //ȭ�� �������ֱ�
        InGame.Inst.SetLobby();
    }



}
