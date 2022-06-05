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

    //�÷��̾� �г��� 
    public TextMeshPro nickNameTxt;

    //���� ���� �̵��ӵ� ��    
    [SerializeField] int h = 0; 
    [SerializeField]  Vector2 velocity = Vector2.zero;

    //���� ������ ���� (����ȭ�� ����)
    public Vector2 currPos = Vector2.zero; //��ġ
    int currH = 0; //���� ��ġ ���� �� �ӵ� 
    float isOnece = 0.02f;  //ù ����ȭ�� ����

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

    //�����̱� �����Ҷ�
    public void MoveStart(int h)
    {
        this.h += h;

        SetAnim();
    }

    //�����̴� ��ư���� ��������
    public void MoveEnd(int h)
    {
        this.h -= h;
      
        SetAnim();
    }

    void SetAnim()//�̹��� , �ִϸ��̼� ����
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
