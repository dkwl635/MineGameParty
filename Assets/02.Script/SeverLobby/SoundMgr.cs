using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class SoundMgr : MonoBehaviour
{
    //싱글턴 패턴
    static public SoundMgr Inst;
    //오디오클립
    public AudioClip[] audioClips;
    //bgm를 실행 시키는 
    AudioSource bgmSource;
    //Effect sound 를 실행시켜 주는
    Queue<AudioSource> effectSource = new Queue<AudioSource>();
    //사운드 클립을 담아 놓는 곳
    Dictionary<string, AudioClip> sounds = new Dictionary<string, AudioClip>();
    //이펙트 사운드 
    float effectVolum = 1;  
    public float EffectVolum { get { return effectVolum; } set { effectVolum = value; } }
    public float BGMVolum { get { return bgmSource.volume; } set { bgmSource.volume = value; } }
    public GameObject SoundCtrlBox; //사운드 설정 하는 팝업창
    private void Awake()
    {
        if (Inst == null)
        {
            Inst = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else  
            Destroy(this.gameObject);     

        //BGM 전용 셋팅
        bgmSource = this.gameObject.AddComponent<AudioSource>();
        bgmSource.loop = true;

        //effect 셋팅  5개정도 생성
        for (int i = 0; i < 5; i++)
        {
            AudioSource newAudioSource = this.gameObject.AddComponent<AudioSource>();
            effectSource.Enqueue(newAudioSource);
        }
        SoundInit();      
    }

    void SoundInit()
    {
        for (int i = 0; i < audioClips.Length;i++)      
            sounds.Add(audioClips[i].name, audioClips[i]);
    }

    public void PlayBGM(string BgmName, float speed = 1)   //BGM 실핼
    {
        //사운드가 있는지 체크
        if(sounds.ContainsKey(BgmName)) 
            bgmSource.clip = sounds[BgmName];               
        else //없으면
        {   
            return;
        }

        bgmSource.pitch = speed;
        bgmSource.Play();
    }

    public void PlayEffect(string EffectName)
    {
        AudioSource nowAudio = effectSource.Dequeue();

        //사운드가 있는지 체크
        if (sounds.ContainsKey(EffectName))
            nowAudio.clip = sounds[EffectName];
        else //없으면
        {
            return;
        }

        nowAudio.volume = effectVolum;
       // nowAudio.PlayOneShot(nowAudio.clip);
        nowAudio.Play();

        effectSource.Enqueue(nowAudio);
    }

    public void OnSoundCtrlBox()
    {
        Instantiate(SoundCtrlBox);
    }

}
