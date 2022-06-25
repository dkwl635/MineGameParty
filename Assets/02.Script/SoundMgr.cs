using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class SoundMgr : MonoBehaviour
{
    //�̱��� ����
    static public SoundMgr Inst;

    //�����Ŭ��
    public AudioClip[] audioClips;


    //bgm�� ���� ��Ű�� 
    AudioSource bgmSource;

    //Effect sound �� ������� �ִ�
    Queue<AudioSource> effectSource = new Queue<AudioSource>();

    //���� Ŭ���� ��� ���� ��
    Dictionary<string, AudioClip> sounds = new Dictionary<string, AudioClip>();

    //����Ʈ ���� 
    float effectVolum = 1;

    
    public float EffectVolum { get { return effectVolum; } set { effectVolum = value; } }
    public float BGMVolum { get { return bgmSource.volume; } set { bgmSource.volume = value; } }

    public GameObject SoundCtrlBox;



    private void Awake()
    {
        if (Inst == null)
        {
            Inst = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Destroy(this.gameObject);
        }

        //BGM ���� ����
        bgmSource = this.gameObject.AddComponent<AudioSource>();
        bgmSource.loop = true;

        //effect ����  5������ ����
        for (int i = 0; i < 5; i++)
        {
            AudioSource newAudioSource = this.gameObject.AddComponent<AudioSource>();
            effectSource.Enqueue(newAudioSource);
        }

        SoundInit();      

    }

    private void Update()
    {
        
    }


    void SoundInit()
    {
        for (int i = 0; i < audioClips.Length;i++)
        {
            sounds.Add(audioClips[i].name, audioClips[i]);
        }

    }

    public void PlayBGM(string BgmName)   //BGM ����
    {
        //���尡 �ִ��� üũ
        if(sounds.ContainsKey(BgmName)) 
            bgmSource.clip = sounds[BgmName];               
        else //������
        {
            Debug.Log(BgmName + "�� ���� ���尡 �����ϴ�.");
            return;
        }


        bgmSource.Play();
    }

    public void PlayEffect(string EffectName)
    {

        AudioSource nowAudio = effectSource.Dequeue();

        //���尡 �ִ��� üũ
        if (sounds.ContainsKey(EffectName))     
            nowAudio.clip = sounds[EffectName]; 
        else //������
        {
            Debug.Log(EffectName + "�� ���� ���尡 �����ϴ�.");
            return;
        }
        

        nowAudio.volume = effectVolum;
        nowAudio.Play();

        effectSource.Enqueue(nowAudio);
    }

    public void OnSoundCtrlBox()
    {
        Instantiate(SoundCtrlBox);
    }

}
