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
        //로비 BGM On
        SoundMgr.Inst.PlayBGM(LobbyBGM);
        //설정버튼 셋팅
        soundBtn.onClick.AddListener(SoundMgr.Inst.OnSoundCtrlBox);
    }


}
