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

    //�÷��̾� �г��� 
    public TextMeshPro nickNameTxt;

    //���� ���� �̵��ӵ� ��    
    [SerializeField] int h = 0; 
    [SerializeField]  Vector2 velocity = Vector2.zero;

    //���� ������ ���� (����ȭ�� ����)
    public Vector3 currPos = Vector3.zero; //��ġ
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

        if (pv.IsMine)//�� ĳ���� ��� ��Ű��
        {
            FindObjectOfType<CharController>().player = this;
            gameMgr.myNickName.text = pv.Owner.NickName;
            nickNameTxt.color = Color.green;
            //��ĳ���� ���� ���̰��ϰ�
            transform.position -= Vector3.forward;
        }
        else
        {
            //����  ����ȭ�� �������
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
        if (pv.IsMine) //���� �����Ҷ�
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

    //���ݵ���ȭ
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        //���� �÷��̾��� ��ġ ���� �۽�
        if (stream.IsWriting)
        {                    
            stream.SendNext(spriteRenderer.flipX ? 1 : 0);              
        }
        else //���� �÷��̾��� ��ġ ���� ����
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
    void RPCHit()//Ʈ���� ����ȭ
    {
        animator.SetTrigger("hit");
    }
}
