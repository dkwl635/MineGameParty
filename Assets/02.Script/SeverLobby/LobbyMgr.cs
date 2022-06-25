using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LobbyMgr : MonoBehaviour
{
    const string LobbyBGM = "Lobby";
    const string ClickEffect = "Click";

    public Button soundBtn;

    // Start is called before the first frame update
    void Start()
    {   
        //�κ� BGM On
        SoundMgr.Inst.PlayBGM(LobbyBGM);
        //������ư ����
        soundBtn.onClick.AddListener(SoundMgr.Inst.OnSoundCtrlBox);
    }


}
