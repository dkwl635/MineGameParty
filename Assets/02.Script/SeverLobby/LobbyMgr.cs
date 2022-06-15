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
        SoundMgr.Inst.PlayBGM(LobbyBGM);

        soundBtn.onClick.AddListener(SoundMgr.Inst.OnSoundCtrlBox);
    }

    private void Update()
    {
        if(Input.GetMouseButtonDown(0))
        {
            SoundMgr.Inst.PlayEffect(ClickEffect);
        }


    }
}
