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
    public GameObject starImg;
    public GameObject ready;

    //���� ���� �̵��ӵ� ��    
    [SerializeField] int h = 0; 
    [SerializeField]  Vector2 velocity = Vector2.zero;

    //���� ������ ���� (����ȭ�� ����)
    public Vector3 currPos = Vector3.zero; //��ġ
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

        if (pv.IsMine)//�� ĳ���� ��� ��Ű��
        {
            animator.SetInteger("Char", (int)UserData.CharName);

            FindObjectOfType<CharController>().player = this;              
            //��ĳ���� ���� ���̰��ϰ�
            transform.position -= Vector3.forward;
            this.tag = "Player";
            //�г��� ����
            nickNameTxt.text = "<color=" + InGame.Inst.myNickNameColor.ToString() + ">" + pv.Owner.NickName + "</color>";      
            InGame.Inst.playerCharacters[0] = this;

        }
        else    //���� ����ȭ ĳ������ ���
        {  
            rigidbody.gravityScale = 0.0f;     //�߷� ����
            this.tag = "OtherPlayer";   //�±� ����
            //�г��� ����
            nickNameTxt.text = "<color=" + InGame.Inst.otherNickNameColor.ToString() + ">" + pv.Owner.NickName + "</color>";
            InGame.Inst.playerCharacters[1] = this; 
        }


        //������ �ƴϸ� ����ǥ�� ����
        if (!pv.Owner.IsMasterClient)
            starImg.SetActive(false);
    }

    private void Update()
    {
        Move_Update();
    }

    void Move_Update()
    {
        //���� �����Ҷ�
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
    //��ư�� ����Ǵ� �Լ�     //�����̱� �����Ҷ�
    public void MoveStart(int h) { this.h += h; SetAnim(); }
    //��ư�� ����Ǵ� �Լ�   //�����̴� ��ư���� ��������
    public void MoveEnd(int h) { this.h -= h; SetAnim(); }
    
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
    //�г��� �÷��� �ٲٸ�
    public void ChangeNickName(MyColor color) { nickNameTxt.text = "<color=" + color.ToString() + ">" + pv.Owner.NickName + "</color>"; }

    //���ݵ���ȭ
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        //���� �÷��̾��� ��ġ ���� �۽�
        if (stream.IsWriting)
        {                    
            stream.SendNext(spriteRenderer.flipX ? 1 : 0);
            stream.SendNext(animator.GetInteger("Char"));
            stream.SendNext(nickNameTxt.text);
        }
        else //���� �÷��̾��� ��ġ ���� ����
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

    [PunRPC] //Ʈ���� ����ȭ �ٸ� �÷��̾ ��������
    void RPCHit() { animator.SetTrigger("hit"); }

    public void Ready(bool bReady) //�Ӹ��� ���� ǥ�� 
    {
        ready.SetActive(bReady);    
        pv.RPC("RPCReady", RpcTarget.Others, bReady);
    }

    [PunRPC]// ����ȭ�� ����
    void RPCReady(bool bReady) { ready.SetActive(bReady); }

}
