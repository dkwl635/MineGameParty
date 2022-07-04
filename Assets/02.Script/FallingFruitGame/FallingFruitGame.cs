using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Realtime;

public class FallingFruitGame : Game
{
    enum FruitsType //과일 오브젝트를 불러오기 위한 
    {
        Apple,
        Bananas,
        Cherries,
        Kiwi,
        Melon,
        Orange,
        Pineapple,
        Strawberry,
        Max,
    }

    public static FallingFruitGame Inst;    //싱글턴 패턴을 위한
    public PlayerCharacter[] playerObj;   //과일들의 충돌캐릭터들의 거리 계산을 위한 플레이어들 캐릭터변수

    public GameObject fruitsSpanwPos; //과일의 중심스폰위치
    


    //과일 먹고 나올 이펙트
    public GameObject collectObj;   //과일을 먹고 나올 이펙트 프리팹
    Transform collectObjTr;
    Queue<GameObject> collectQu = new Queue<GameObject>();

    [Header("UI")]
    public GameObject GamePanel;
    public TextMeshProUGUI scoreTxt;    //점수 
    public TextMeshProUGUI CountTxt;   //남은시간
                                       //점수 

    int count = 30;
    
    float spawnTimer = 0.0f;                 //게임시간   
    float nextSpawnTime = 1.0f; //과일 스폰 주기
    bool gameStart = false;  //게임 상태 bool 변수

    protected override void Init()
    {
        base.Init();

        Inst = this;

        collectObjTr = new GameObject("CollectObjPool").transform;
        collectObjTr.SetParent(transform);

        for (int i = 0; i < 10; i++)
        {
            GameObject obj = Instantiate(collectObj, collectObjTr);
            obj.SetActive(false);
            collectQu.Enqueue(obj);
        }

    }


    public override void StartGame()
    {
        base.StartGame();

        SoundMgr.Inst.PlayBGM("FallingFruitGame");

        GamePanel.SetActive(true);
        //씬에 있는 플레이어 오브젝트 불러오기
        playerObj = InGame.Inst.playerCharacters;
        //점수 셋팅
        score = 0;
        scoreTxt.text = "0";

      
        StartCoroutine(Game_Co());

        if (PhotonNetwork.IsMasterClient)
            StartCoroutine(SpawnFruits_Update());


    }

    IEnumerator Game_Co()
    {
        gameStart = true;
        count = 30; //30초

        CountTxt.gameObject.SetActive(true);
        CountTxt.text = count.ToString();
        while (count >= 0)
        {     
            yield return new WaitForSeconds(1.0f);
            count--;
            CountTxt.text = count.ToString();
        }
        
        //게임 종료
        gameStart = false;
        //과일오브젝트 삭제
        if (PhotonNetwork.LocalPlayer.IsMasterClient)
        {
            Fruits[] fruits = FindObjectsOfType<Fruits>();
            for (int i = 0; i < fruits.Length; i++)
            {
                PhotonNetwork.Destroy(fruits[i].gameObject);
            }
        }

        CountTxt.text = "종료!!";
        yield return new WaitForSeconds(1.5f);
        CountTxt.text = "결과발표";
        yield return new WaitForSeconds(1.0f);
        CountTxt.text = "";

        

        InGame.Inst.ShowResult();
        GamePanel.SetActive(false);

        StopAllCoroutines();
    }

    IEnumerator SpawnFruits_Update()
    {
       while(gameStart)
        {
            yield return null;

            //과일 스폰
            spawnTimer += Time.deltaTime;
            if (spawnTimer >= nextSpawnTime)
            {
                int randCount = Random.Range(3, 8);
                for (int i = 0; i < randCount; i++)
                {
                    SpawnFruits(); //과일 스폰
                }

                spawnTimer = 0.0f;
                nextSpawnTime = Random.Range(0.7f, 1.5f);
            }

        }
    }

   [PunRPC]
   void TimeTxt(int count)
    {
        CountTxt.text = count.ToString();
        this.count = count;
    }

    public void SpawnFruits()   //과일 스폰하기
    {
        Debug.Log("a");
        //랜덤과일 및 스폰위치 잡기
        int rand = Random.Range(0, (int)FruitsType.Max);
        float randx = Random.Range(-4.0f, 4.0f);
        float randy = Random.Range(-0.5f, 0.5f);

        //과일 리소스폴더에서 가져와 스폰하기.. 포톤으로 소환하여 모든 클라에게 전달
        GameObject fruitObj = PhotonNetwork.InstantiateRoomObject("Fruits", fruitsSpanwPos.transform.position + Vector3.right * randx + Vector3.up * randy, Quaternion.identity);
        fruitObj.GetComponent<Fruits>().type = rand;
    }
    
    public void GetFruit(PlayerCharacter player , Vector3 pos)//과일 충돌시 점수와 이펙트 소환 //호출되는 곳은 마스터 클라이언트에서만 호출된다.
    {
        //PunRPC 점수 증가 함수 먹은 유저에게 결과 보내기
        pv.RPC("GetFruit", player.pv.Owner, pos);          
    }

    [PunRPC]
    void GetFruit(Vector3 pos)//점수증가 와 먹은 위치에 효과보여주시    
    {
        playerHash = PhotonNetwork.LocalPlayer.CustomProperties;
        //플레이어 에게 점수 적용시켜주기 
        //포톤 플레이어 SetCustomProperties을 이용하여 동기화
        if (playerHash.ContainsKey("score"))
        {
            playerHash["score"] = (int)playerHash["score"] + 100;
            PhotonNetwork.LocalPlayer.SetCustomProperties(playerHash);     
        }   
     
        SpawnCollect(pos);  //흭득 이펙트 보여주기
        SoundMgr.Inst.PlayEffect("GetFruits");
    }

    //PlayerProperties들이 업데이트 된다면 점수 갱신
    public override void OnPlayerPropertiesUpdate(Player targetPlayer
                     , ExitGames.Client.Photon.Hashtable changedProps)
    {
        //자기자신일때
        if (targetPlayer.Equals(PhotonNetwork.LocalPlayer))
        {
            if (changedProps.ContainsKey("score"))
            {
                score = (int)changedProps["score"];
                scoreTxt.text = score.ToString();
            }
        }
    }

    void SpawnCollect(Vector3 pos)//과일 먹을시 이펙트 나오게 하기  //추후 오브젝트 풀로 바꾸기
    {
        GameObject collect;
        if (collectQu.Count <= 0)
            collect = Instantiate(collectObj, collectObjTr);
        else
            collect = collectQu.Dequeue();

        collect.SetActive(true);
        collect.transform.position = pos;

        StartCoroutine(CollectActiveOff(collect));
    }

    WaitForSeconds collectOffTime = new WaitForSeconds(0.3f);
    IEnumerator CollectActiveOff(GameObject obj)
    {
        yield return collectOffTime;

        obj.SetActive(false);
        collectQu.Enqueue(obj);
    }

 
}
