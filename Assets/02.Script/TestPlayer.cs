using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestPlayer : MonoBehaviour
{

    Rigidbody2D rigidbody;
    Animator animator;
    SpriteRenderer spriteRenderer;

    [SerializeField] int h = 0;
    [SerializeField] Vector2 velocity = Vector2.zero;


    private void Awake()
    {
        rigidbody = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }


    private void Update()
    {
        Move_Update();
    }


    void Move_Update()
    {
   
            velocity.x = h * 2;
            velocity.y = rigidbody.velocity.y;
            rigidbody.velocity = velocity;
      
    }

    public void MoveStart(int h)
    {
        this.h += h;

        if (this.h.Equals(-1))
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

}
