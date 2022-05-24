using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LobbyPlayerController : MonoBehaviourPunCallbacks, IPunObservable
{
    Rigidbody2D rigidbody;
    Animator animator;
    SpriteRenderer spriteRenderer;
    [HideInInspector]public PhotonView pv;

    public TextMeshPro nickNameTxt;

    [SerializeField] int h = 0;
    [SerializeField]  Vector2 velocity = Vector2.zero;

    //원격 조종용 변수
    public Vector3 currPos = Vector3.zero; //위치
    int currH = 0; //현재 위치 방향 및 속도 
    float isOnece = 0.02f;  //첫 동기화를 위해

    private void Awake()
    {
        rigidbody = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        pv = GetComponent<PhotonView>();
        pv.ObservedComponents[0] = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        InGameLobbyMgr gameMgr = FindObjectOfType<InGameLobbyMgr>();

        if (pv.IsMine)//내 캐릭터 등록 시키기
        {
            gameMgr.player = this;
            gameMgr.myNickName.text = pv.Owner.NickName;
            nickNameTxt.color = Color.green;

            transform.position -= Vector3.forward;
          
        }
        else
        {
            rigidbody.gravityScale = 0.0f;
            gameMgr.ohterNickName.text = pv.Owner.NickName;
            nickNameTxt.color = Color.blue;
        }

        nickNameTxt.text = pv.Owner.NickName;

     

    }

    private void Update()
    {
        if (pv.IsMine) //내가 조종할때
        {
            velocity.x = h * 2;
            velocity.y = rigidbody.velocity.y;
            rigidbody.velocity = velocity;
        }
        else //원격 플레이어일 때 수행
        {       
            if (isOnece > 0.0f)
            { //내가 입장할 때 지 이미 존재하는 OtherPC들의 위치를 동기화 위해
                isOnece -= Time.deltaTime;
                if (isOnece <= 0.0f)
                {
                    transform.position = currPos;
                }
                
                return;
            }


            //기존 위치와 동기화 되는 위치가 멀 경우 바로 이동
            if (1.0f < (transform.position - currPos).magnitude)
            {
                transform.position = currPos;            
            }
            else
            {
                //원격 플레이어의 플레이어를 수신받은 위치까지 부드럽게 이동시킴
                transform.position = Vector3.Lerp(transform.position, currPos, Time.deltaTime * 10.0f);
            }
  
        } 

       
    }


    public void MoveStart(int h)
    {
        this.h += h;

        if(this.h.Equals(-1))       
            spriteRenderer.flipX = true;     
        else if (this.h.Equals(1))    
            spriteRenderer.flipX = false;

    

        if (this.h.Equals(0))
            animator.SetBool("move", false);
        else
            animator.SetBool("move", true);

    }

    public void MoveEnd(int h)
    {
        this.h -= h;

        if (this.h.Equals(-1))
            spriteRenderer.flipX = true;
        else if (this.h.Equals(1))
            spriteRenderer.flipX = false;

        if (this.h.Equals(0))
            animator.SetBool("move", false);
        else
            animator.SetBool("move", true);
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    { 

        //로컬 플레이어의 위치 정보 송신
        if (stream.IsWriting)
        {
            stream.SendNext(transform.position);    //위치
            stream.SendNext(h);    //위치
            stream.SendNext(spriteRenderer.flipX ? 1 : 0);     
        }
        else //원격 플레이어의 위치 정보 수신
        {
            currPos = (Vector3)stream.ReceiveNext();        
            currH = (int)stream.ReceiveNext();
            spriteRenderer.flipX = ((int)stream.ReceiveNext()) == 1 ? true : false;
           

            if (currH.Equals(0))
                animator.SetBool("move", false);
            else
                animator.SetBool("move", true);
        }
    }
}
