using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LobbyMgr : MonoBehaviour
{
    const string LobbyBGM = "Lobby";
    
    public Button soundBtn;

    [Header("GameExitBox")]
    public GameObject gameExitBox;
    public Button gameExitBoxBtn;
    public Button gameExitBtn;
    public Button gameExitBackBtn;

    private void Awake()
    {
        //��ü���� �������� �����ֱ� ����
        Application.targetFrameRate = 30;
    }


    void Start()
    {
       

        //�κ� BGM On
        SoundMgr.Inst.PlayBGM(LobbyBGM);
        //������ư ����
        soundBtn.onClick.AddListener(SoundMgr.Inst.OnSoundCtrlBox);


        gameExitBoxBtn.onClick.AddListener(GameExitBoxBtn);
        gameExitBtn.onClick.AddListener(GameExitOkBtn);
        gameExitBackBtn.onClick.AddListener(GameExitBackBtn);
    }

    void GameExitBoxBtn()
    {
        SoundMgr.Inst.PlayEffect("Button");
        gameExitBox.SetActive(true);
    }

    void GameExitBackBtn()
    {
        SoundMgr.Inst.PlayEffect("Button");
        gameExitBox.SetActive(false);
    }
    
    void GameExitOkBtn()
    {
        SoundMgr.Inst.PlayEffect("Button");
        Application.Quit();
    }

}
