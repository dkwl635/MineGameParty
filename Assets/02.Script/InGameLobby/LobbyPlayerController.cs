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

    //���� ������ ����
    public Vector3 currPos = Vector3.zero; //��ġ
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

    // Start is called before the first frame update
    void Start()
    {
        InGameLobbyMgr gameMgr = FindObjectOfType<InGameLobbyMgr>();

        if (pv.IsMine)//�� ĳ���� ��� ��Ű��
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
        if (pv.IsMine) //���� �����Ҷ�
        {
            velocity.x = h * 2;
            velocity.y = rigidbody.velocity.y;
            rigidbody.velocity = velocity;
        }
        else //���� �÷��̾��� �� ����
        {       
            if (isOnece > 0.0f)
            { //���� ������ �� �� �̹� �����ϴ� OtherPC���� ��ġ�� ����ȭ ����
                isOnece -= Time.deltaTime;
                if (isOnece <= 0.0f)
                {
                    transform.position = currPos;
                }
                
                return;
            }


            //���� ��ġ�� ����ȭ �Ǵ� ��ġ�� �� ��� �ٷ� �̵�
            if (1.0f < (transform.position - currPos).magnitude)
            {
                transform.position = currPos;            
            }
            else
            {
                //���� �÷��̾��� �÷��̾ ���Ź��� ��ġ���� �ε巴�� �̵���Ŵ
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

        //���� �÷��̾��� ��ġ ���� �۽�
        if (stream.IsWriting)
        {
            stream.SendNext(transform.position);    //��ġ
            stream.SendNext(h);    //��ġ
            stream.SendNext(spriteRenderer.flipX ? 1 : 0);     
        }
        else //���� �÷��̾��� ��ġ ���� ����
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
