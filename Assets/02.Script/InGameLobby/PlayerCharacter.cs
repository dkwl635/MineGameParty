using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerCharacter : MonoBehaviourPunCallbacks, IPunObservable
{
    Rigidbody2D rigidbody; 
    Animator animator;
    SpriteRenderer spriteRenderer;
    [HideInInspector]public PhotonView pv;

    //플레이어 닉네임 
    public TextMeshPro nickNameTxt;

    //방향 값과 이동속도 값    
    [SerializeField] int h = 0; 
    [SerializeField]  Vector2 velocity = Vector2.zero;

    //원격 조종용 변수 (동기화를 위한)
    public Vector3 currPos = Vector3.zero; //위치
    public  bool isMove = true;


    private void Awake()
    {
        rigidbody = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        pv = GetComponent<PhotonView>();
       
    }

    // Start is called before the first frame update
    void Start()
    {
        InGame gameMgr = FindObjectOfType<InGame>();

        if (pv.IsMine)//내 캐릭터 등록 시키기
        {
            FindObjectOfType<CharController>().player = this;
            gameMgr.myNickName.text = pv.Owner.NickName;
            nickNameTxt.color = Color.green;
            //내캐릭이 먼저 보이게하게
            transform.position -= Vector3.forward;
        }
        else
        {
            //원격  동기화는 적용안함
            rigidbody.gravityScale = 0.0f;
            gameMgr.ohterNickName.text = pv.Owner.NickName;
            nickNameTxt.color = Color.blue;
        }

        nickNameTxt.text = pv.Owner.NickName;
    }

    private void Update()
    {
        Move_Update();
    }

    void Move_Update()
    {
        if (pv.IsMine) //내가 조종할때
        {
            currPos = transform.position;
            currPos.z = -1;
            transform.position = currPos;

            if (!isMove)
                return;

            velocity.x = h * 2;
            velocity.y = rigidbody.velocity.y;
            rigidbody.velocity = velocity;

            if (Input.GetKeyDown(KeyCode.A))
                SetHit();

        }
        else
        {
            currPos = transform.position;
            currPos.z = 0;
            transform.position = currPos;
        }
    }

    //움직이기 시작할때
    public void MoveStart(int h)
    {
        this.h += h;

        SetAnim();
    }

    //움직이는 버튼에서 손을때면
    public void MoveEnd(int h)
    {
        this.h -= h;
      
        SetAnim();
    }

    void SetAnim()//이미지 , 애니메이션 변경
    {
        if (this.h.Equals(-1))
            spriteRenderer.flipX = true;
        else if (this.h.Equals(1))
            spriteRenderer.flipX = false;

        if (this.h.Equals(0))
            animator.SetBool("move", false);
        else
            animator.SetBool("move", true);
    }

    //원격동기화
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        //로컬 플레이어의 위치 정보 송신
        if (stream.IsWriting)
        {                    
            stream.SendNext(spriteRenderer.flipX ? 1 : 0);              
        }
        else //원격 플레이어의 위치 정보 수신
        {                     
            spriteRenderer.flipX = ((int)stream.ReceiveNext()) == 1 ? true : false;         
        }
    }

    public void SetHit()
    {
        pv.RPC("RPCHit", RpcTarget.AllViaServer);
        //animator.SetTrigger("hit");

    }
    [PunRPC]
    void RPCHit()//트리거 동기화
    {
        animator.SetTrigger("hit");
    }
}
