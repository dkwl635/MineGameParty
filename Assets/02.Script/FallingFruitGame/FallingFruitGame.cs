using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Realtime;

public class FallingFruitGame : Game
{
   
    enum FruitsType //���� ������Ʈ�� �ҷ����� ���� 
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

    public static FallingFruitGame Inst;    //�̱��� ������ ����
    public PlayerCharacter[] playerObj;   //���ϵ��� �浹ĳ���͵��� �Ÿ� ����� ���� �÷��̾�� ĳ���ͺ���

    public GameObject fruitsSpanwPos; //������ �߽ɽ�����ġ
    


    //���� �԰� ���� ����Ʈ
    public GameObject collectObj;   //������ �԰� ���� ����Ʈ ������
    Transform collectObjTr;
    Queue<GameObject> collectQu = new Queue<GameObject>();

    [Header("UI")]
    public GameObject GamePanel;
    public TextMeshProUGUI scoreTxt;    //���� 
    public TextMeshProUGUI CountTxt;   //�����ð�
                      //���� 

    
    float timer = 0.0f;                 //���ӽð�   
    float nextSpawnTime = 1.0f; //���� ���� �ֱ�
    bool gameStart = false;  //���� ���� bool ����

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

        GamePanel.SetActive(true);
        //���� �ִ� �÷��̾� ������Ʈ �ҷ�����
        playerObj = InGame.Inst.playerCharacters;
        //���� ����
        score = 0;
        scoreTxt.text = "0";

      
        StartCoroutine(Game_Co());

        if (PhotonNetwork.IsMasterClient)
            StartCoroutine(SpawnFruits_Update());


    }

    IEnumerator Game_Co()
    {
        gameStart = true;
        int count = 15; //15��

        CountTxt.gameObject.SetActive(true);
        while (count >= 0)
        {
            CountTxt.text = count.ToString();
            count--;
            yield return new WaitForSeconds(1.0f);
        }
        
        //���� ����
        gameStart = false;


        //���Ͽ�����Ʈ ����
        if (PhotonNetwork.LocalPlayer.IsMasterClient)
        {
            Fruits[] fruits = FindObjectsOfType<Fruits>();
            for (int i = 0; i < fruits.Length; i++)
            {
                PhotonNetwork.Destroy(fruits[i].gameObject);
            }
        }

        CountTxt.text = "����!!";
        yield return new WaitForSeconds(1.5f);
        CountTxt.text = "�����ǥ";
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

            //���� ����
            timer += Time.deltaTime;
            if (timer >= nextSpawnTime)
            {
                int randCount = Random.Range(2, 5);
                for (int i = 0; i < randCount; i++)
                {
                    SpawnFruits(); //���� ����
                }

                timer = 0.0f;
                nextSpawnTime = Random.Range(0.7f, 1.5f);
            }

        }
    }

   
    public void SpawnFruits()   //���� �����ϱ�
    {
        //�������� �� ������ġ ���
        int rand = Random.Range(0, (int)FruitsType.Max);
        float randx = Random.Range(-3.5f, 3.5f);
        float randy = Random.Range(-0.3f, 0.3f);

        //���� ���ҽ��������� ������ �����ϱ�.. �������� ��ȯ�Ͽ� ��� Ŭ�󿡰� ����
        string name = ((FruitsType)Random.Range(0, (int)FruitsType.Max)).ToString();
        PhotonNetwork.InstantiateRoomObject("Fruits/" + name, fruitsSpanwPos.transform.position + Vector3.right * randx + Vector3.up * randy, Quaternion.identity);  
    }
    
    public void GetFruit(PlayerCharacter player , Vector3 pos)//���� �浹�� ������ ����Ʈ ��ȯ //ȣ��Ǵ� ���� ������ Ŭ���̾�Ʈ������ ȣ��ȴ�.
    {
        //PunRPC ���� ���� �Լ� ���� �������� ��� ������
        pv.RPC("GetFruit", player.pv.Owner, pos);          
    }

    [PunRPC]
    void GetFruit(Vector3 pos)//�������� �� ���� ��ġ�� ȿ�������ֽ�    
    {
        playerHash = PhotonNetwork.LocalPlayer.CustomProperties;
        //�÷��̾� ���� ���� ��������ֱ� 
        //���� �÷��̾� SetCustomProperties�� �̿��Ͽ� ����ȭ
        if (playerHash.ContainsKey("score"))
        {
            playerHash["score"] = (int)playerHash["score"] + 100;
            PhotonNetwork.LocalPlayer.SetCustomProperties(playerHash);     
        }   
     
        SpawnCollect(pos);  //ŉ�� ����Ʈ �����ֱ�
    }

    //PlayerProperties���� ������Ʈ �ȴٸ� ���� ����
    public override void OnPlayerPropertiesUpdate(Player targetPlayer
                     , ExitGames.Client.Photon.Hashtable changedProps)
    {
        //�ڱ��ڽ��϶�
        if (targetPlayer.Equals(PhotonNetwork.LocalPlayer))
        {
            if (changedProps.ContainsKey("score"))
            {
                score = (int)changedProps["score"];
                scoreTxt.text = score.ToString();
            }
        }
    }

    void SpawnCollect(Vector3 pos)//���� ������ ����Ʈ ������ �ϱ�  //���� ������Ʈ Ǯ�� �ٲٱ�
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
