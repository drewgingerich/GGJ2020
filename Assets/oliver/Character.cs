﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// this is the visuals/etc of the character sprite/physics.  it is used by Player which is
// in charge of getting user input and making this character do something.
public class Character : MonoBehaviour
{
    Animator animator;
    CapsuleCollider2D capsule;
    Rigidbody2D rb;
    SpriteRenderer spriteRenderer;
    // if we're overlapping a ladder, it will be here (regardless of whether or not we're climbing it)
    public Ladder ladder { get; private set; }
    public Transform feet;
    public bool touchingGround { get { return animator.GetBool("touchingGround"); } }

    public bool isClimbingLadder {
        get {
            return animator.GetBool("isClimbing");
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
        capsule = GetComponent<CapsuleCollider2D>();
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    void OnTriggerEnter2D(Collider2D coll) {
        var l = coll.GetComponent<Ladder>();
        if (l == null)
            return;
        ladder = l;
    }

    void OnTriggerExit2D(Collider2D coll) {
        if (coll.gameObject == ladder.gameObject) {
            if (isClimbingLadder)
                StopClimbing();
            ladder = null;
        }
    }

    // this walks to a position over the next physics update - meant to be called every FixedUpdate that
    // the player is walking
    public void WalkTo(Vector2 pos) {
        if (isClimbingLadder)
            Debug.LogError("Player can't walk when climbing");
        spriteRenderer.flipX = pos.x < transform.position.x;
        rb.MovePosition(pos);
        animator.SetBool("isClimbing", false);
        animator.SetFloat("speed", 1);
    }

    // this falls to a position over the next physics update - meant to be called every FixedUpdate that
    // the player is falling (i.e. not climbing and delta y < 0)
    // public void FallTo(Vector2 pos) {
    //     if (isClimbingLadder)
    //         Debug.LogError("Player can't fall when climbing");
    //     rb.MovePosition(pos);
    //     animator.SetBool("isClimbing", false);
    //     // todo - set falling
    //     animator.SetFloat("speed", 1);
    // }

    public void StartClimbing() {
        if (ladder == null)
            throw new System.Exception("Can't start climbing when not overlapping a ladder");
        // make it so the player doesn't collide with the ladder's transition floor
        Physics2D.IgnoreCollision(capsule, ladder.transitionFloor);
        animator.SetBool("isClimbing", true);
    }

    public void StopClimbing() {
        if (!isClimbingLadder)
            throw new System.Exception("Can't stop climbing when not climbing");
        // make the player collide with the transition floor again
        Physics2D.IgnoreCollision(capsule, ladder.transitionFloor, false);
        animator.SetBool("isClimbing", false);
    }

    // this climbs to a position over the next physics update - meant to be called every FixedUpdate that
    // the player is climbing.  call StartClimbing() first.
    public void ClimbTo(Vector2 pos) {
        if (!isClimbingLadder)
            throw new System.Exception("Player can't walk when climbing");
        spriteRenderer.flipX = pos.x < transform.position.x;
        rb.MovePosition(pos);
        animator.SetFloat("speed", 1);
    }

    // should be called in a fixed update when the user has no directional input (but the character might
    // be falling due to gravity)
    public void IdleTo(Vector2 pos) {
        rb.MovePosition(pos);
        animator.SetFloat("speed", 0);
    }

    // Update is called once per frame
    void Update()
    {
        var touchingGround = Physics2D.Raycast(feet.position, Vector2.down, 0.1f, LayerMask.GetMask("Ground"));
        animator.SetBool("touchingGround", touchingGround);
    }
}
