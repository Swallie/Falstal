using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{

    Animator anim;

    [Range(1, 10)]
    public float Speed;

    // Use this for initialization
    void Start()
    {

        anim = GetComponent<Animator>();

    }

    // Update is called once per frame
    void Update()
    {

        float input_x = Input.GetAxis("Horizontal");
        float input_y = Input.GetAxis("Vertical");

        bool isWalking = (Mathf.Abs(input_x) + Mathf.Abs(input_y)) > 0;

        anim.SetBool("isWalking", isWalking);
        if (isWalking)
        {
            anim.SetFloat("x", input_x);
            anim.SetFloat("y", input_y);

            transform.position += new Vector3(input_x, input_y, 0).normalized * Time.deltaTime * Speed;
        }

    }
}
