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
    public GameObject starImg;
    public GameObject ready;

    //방향 값과 이동속도 값    
    [SerializeField] int h = 0; 
    [SerializeField]  Vector2 velocity = Vector2.zero;

    //원격 조종용 변수 (동기화를 위한)
    public Vector3 currPos = Vector3.zero; //위치
    public bool isMove = true;

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
            animator.SetInteger("Char", (int)UserData.CharName);

            FindObjectOfType<CharController>().player = this;              
            //내캐릭이 먼저 보이게하게
            transform.position -= Vector3.forward;
            this.tag = "Player";
            //닉네임 색깔
            nickNameTxt.text = "<color=" + InGame.Inst.myNickNameColor.ToString() + ">" + pv.Owner.NickName + "</color>";      
            InGame.Inst.playerCharacters[0] = this;

        }
        else    //원격 동기화 캐릭터일 경우
        {  
            rigidbody.gravityScale = 0.0f;     //중력 끄기
            this.tag = "OtherPlayer";   //태그 설정
            //닉네임 색깔
            nickNameTxt.text = "<color=" + InGame.Inst.otherNickNameColor.ToString() + ">" + pv.Owner.NickName + "</color>";
            InGame.Inst.playerCharacters[1] = this; 
        }


        //방장이 아니면 방장표시 끄기
        if (!pv.Owner.IsMasterClient)
            starImg.SetActive(false);
    }

    private void Update()
    {
        Move_Update();
    }

    void Move_Update()
    {
        //내가 조종할때
        if (pv.IsMine) {       
            if (!isMove)
                return;
            velocity.x = h * 2;
            velocity.y = rigidbody.velocity.y;
            rigidbody.velocity = velocity;
        }
        else { 
            currPos = transform.position;
            currPos.z = 0;
            transform.position = currPos;
        }
    }
    //버튼에 연결되는 함수     //움직이기 시작할때
    public void MoveStart(int h) { this.h += h; SetAnim(); }
    //버튼에 연결되는 함수   //움직이는 버튼에서 손을때면
    public void MoveEnd(int h) { this.h -= h; SetAnim(); }
    
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
    //닉네임 컬러가 바꾸면
    public void ChangeNickName(MyColor color) { nickNameTxt.text = "<color=" + color.ToString() + ">" + pv.Owner.NickName + "</color>"; }

    //원격동기화
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        //로컬 플레이어의 위치 정보 송신
        if (stream.IsWriting)
        {                    
            stream.SendNext(spriteRenderer.flipX ? 1 : 0);
            stream.SendNext(animator.GetInteger("Char"));
            stream.SendNext(nickNameTxt.text);
        }
        else //원격 플레이어의 위치 정보 수신
        {                     
            spriteRenderer.flipX = ((int)stream.ReceiveNext()) == 1 ? true : false;
            animator.SetInteger("Char", (int)stream.ReceiveNext());
            nickNameTxt.text = (string)stream.ReceiveNext();
        }
    }

    public void SetHit()
    {
        pv.RPC("RPCHit", RpcTarget.Others);
        animator.SetTrigger("hit");
    }

    [PunRPC] //트리거 동기화 다른 플레이어도 보여지게
    void RPCHit() { animator.SetTrigger("hit"); }

    public void Ready(bool bReady) //머리위 레디 표시 
    {
        ready.SetActive(bReady);    
        pv.RPC("RPCReady", RpcTarget.Others, bReady);
    }

    [PunRPC]// 동기화를 위한
    void RPCReady(bool bReady) { ready.SetActive(bReady); }

}
