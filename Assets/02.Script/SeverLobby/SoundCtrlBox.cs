using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SoundCtrlBox : MonoBehaviour
{
    public Image BGM;
    public Image Effect;
        
    public Sprite[] imgs;   //BGM  : ON OFF/Effect : ON OFF/ 그림 순서 

    public Slider bgmSlider;
    public Slider EffectSlider;

    public Button backBtn;

    private void Start()
    {
        SoundMgr.Inst.PlayEffect("OpenSonudBox");

        bgmSlider.value = SoundMgr.Inst.BGMVolum;
        ChangeBGMSoundVolume(SoundMgr.Inst.BGMVolum);
        EffectSlider.value = SoundMgr.Inst.EffectVolum;
        ChangeEffectSoundVolume(SoundMgr.Inst.EffectVolum);

        bgmSlider.onValueChanged.AddListener(ChangeBGMSoundVolume);
        EffectSlider.onValueChanged.AddListener(ChangeEffectSoundVolume);
        backBtn.onClick.AddListener(BackBtn);

    }


     void ChangeBGMSoundVolume(float v)
    {
        SoundMgr.Inst.BGMVolum = v;

        if (v == 0)
            BGM.sprite = imgs[1];
        else
            BGM.sprite = imgs[0];

    }


     void ChangeEffectSoundVolume(float v)
    {
        SoundMgr.Inst.EffectVolum = v;

        if (v == 0)
            Effect.sprite = imgs[3];
        else
            Effect.sprite = imgs[2];

    }

    void BackBtn()
    {
        SoundMgr.Inst.PlayEffect("Button");
        Destroy(this.gameObject);
    }

}
