using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TestPlayer : MonoBehaviourPunCallbacks
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
    public Vector2 currPos = Vector2.zero; //위치
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

    private void Update()
    {
       // Move_Update();
    }

    void Move_Update()
    {
        velocity.x = h * 2;
        velocity.y = rigidbody.velocity.y;
        rigidbody.velocity = velocity;     
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

    
    public void SetHit()
    {
        animator.SetTrigger("hit");
    }
}
