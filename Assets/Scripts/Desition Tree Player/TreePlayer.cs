using UnityEngine;
using System.Collections.Generic;
using System;

public class TreePlayer : MonoBehaviour
{
    public LineOfSight _los;
    public Transform target;

    private float horizontal;
    private float vertical;
    [SerializeField] private float speed = 100;

    private QuestionNode root;
    private Rigidbody rb;
    void Start()
    {
        rb = GetComponent<Rigidbody>();

        ActionNode idle = new ActionNode(Idle);
        ActionNode walk = new ActionNode(Walk);
        ActionNode interact = new ActionNode(Interact);

        QuestionNode isWalking = new QuestionNode(IsWalking, walk, idle);
        QuestionNode isInteracting = new QuestionNode(IsInteracting, interact, isWalking);


        root = isInteracting;
    }

    // Update is called once per frame
    void Update()
    {
        horizontal = Input.GetAxis("Horizontal");
        vertical = Input.GetAxis("Vertical");

        root.Execute();
    }

    private bool IsInteracting() => _los.CheckRange(target) && _los.CheckAngle(target) && _los.CheckView(target) && Input.GetKeyDown(KeyCode.Space);
    private bool IsWalking() => horizontal != 0 || vertical != 0;

    private void Idle()
    {
        Debug.Log("idle");
    }

    private void Walk()
    {
        Vector3 direction = new Vector3(horizontal, 0, vertical);

        direction.Normalize();
        Quaternion targetRotation = Quaternion.LookRotation(direction);

        rb.velocity = direction * speed;
        rb.rotation = targetRotation;
    }
    private void Interact()
    {
        Debug.Log("Objetivo cumplido.");
    }


}

