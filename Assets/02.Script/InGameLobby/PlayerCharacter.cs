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
    public Vector2 currPos = Vector2.zero; //��ġ
    int currH = 0; //���� ��ġ ���� �� �ӵ� 
    float isOnece = 0.02f;  //ù ����ȭ�� ����
    public  bool isMove = true;


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
            if (!isMove)
                return;

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
            if (0.4f < ((Vector2)transform.position - currPos).magnitude)
            {  
                transform.position = currPos;
            }
            else
            {
                //���� �÷��̾��� �÷��̾ ���Ź��� ��ġ���� �ε巴�� �̵���Ŵ
                transform.position = Vector2.Lerp(transform.position, currPos, Time.deltaTime * 10.0f);
            }

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
            stream.SendNext((Vector2)transform.position);    //��ġ
            stream.SendNext(h);    //��ġ
            stream.SendNext(spriteRenderer.flipX ? 1 : 0);     
        }
        else //���� �÷��̾��� ��ġ ���� ����
        {
            currPos = (Vector2)stream.ReceiveNext();        
            currH = (int)stream.ReceiveNext();
            spriteRenderer.flipX = ((int)stream.ReceiveNext()) == 1 ? true : false;



            animator.SetBool("move", currH == 0 ? false : true);         
        }
    }

    [PunRPC]
    void StartPosSet(Vector2 pos)
    {
        transform.position = pos;
    }

    public void SetHit()
    {
        animator.SetTrigger("hit");
    }
}
