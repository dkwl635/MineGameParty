using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using Photon.Pun;

public class LoadMgr : MonoBehaviour
{
    public static LoadMgr Inst;

    public GameObject LoadCanvas;
    public TextMeshProUGUI loadText;
    float timer = 0.0f;

    AsyncOperation asyncOperation;


    private void Awake()
    {
       
        if (Inst == null)
        {
            Inst = this;
            DontDestroyOnLoad(this);
        }
        else 
            Destroy(this.gameObject);  
    }

  

    public void LoadScene(string sceneName)
    {
        PhotonNetwork.IsMessageQueueRunning = false;
        StartCoroutine(LoadCoroutine(sceneName));
    }
    IEnumerator LoadCoroutine(string sceneName)
    {
        LoadCanvas.SetActive(true);
        asyncOperation = SceneManager.LoadSceneAsync(sceneName);
        asyncOperation.allowSceneActivation = false;     

        timer = 0.0f;
        loadText.text = "Loading";

        float timer2 = 0.0f;

        while (!asyncOperation.isDone)
        {
            yield return null;
            timer += Time.deltaTime;
            timer2 += Time.deltaTime;

            if (asyncOperation.progress < 0.9f)
            {
                if (timer > 0.3f)
                {
                    loadText.text += ".";
                    if (loadText.text == "Loading.....")
                        loadText.text = "Loading";

                    timer = 0.0f;
                }
            }

            if (asyncOperation.progress < 0.99f && timer2 >= 1.0f)
            {
                loadText.text = "Loading End";
                asyncOperation.allowSceneActivation = true;
                PhotonNetwork.IsMessageQueueRunning = true;
                yield return new WaitForSeconds(0.5f);

            }

        }

        LoadCanvas.SetActive(false);
        yield return null;
    }

}
