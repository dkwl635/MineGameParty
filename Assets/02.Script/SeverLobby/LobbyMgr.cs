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
        //전체적인 프레임을 맞춰주기 위한
        Application.targetFrameRate = 60;
       
        SetResolution();
    }


    void Start()
    {
      
        //로비 BGM On
        SoundMgr.Inst.PlayBGM(LobbyBGM);
        //설정버튼 셋팅
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

    public void SetResolution() //로비 메니저로 갈예정
    {
        int setWidth = 720; // 사용자 설정 너비
        int setHeight = 1280; // 사용자 설정 높이
        Screen.SetResolution(setWidth, setHeight, false); // SetResolution 함수 제대로 사용하기

    }
}
