using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BatMovement : MonoBehaviour {

    public GameObject player;

    Animator anim;

    [Range(1, 10)]
    public float Speed;

    [Range(1, 10)]
    public float TriggerDistance;

    // Use this for initialization
    void Start () {
        anim = GetComponent<Animator>();
    }
	
	// Update is called once per frame
	void Update () {
        // Ist der Spieler in der nähe?
        float playerDistanceX = player.transform.position.x - transform.position.x;
        float playerDistanceY = player.transform.position.y - transform.position.y;
        float playerDistanceZ = player.transform.position.z - transform.position.z;

        Vector3 directionVector = new Vector3(playerDistanceX, playerDistanceY, playerDistanceZ);

        bool isWalking = false;
        if (directionVector.magnitude <= TriggerDistance)
        {
            isWalking = true;
        }

        anim.SetBool("isWalking", isWalking);
        if (isWalking)
        {
            float input_x = Mathf.Min(Mathf.Abs(directionVector.x), 1) * Mathf.Sign(directionVector.x);
            float input_y = Mathf.Min(Mathf.Abs(directionVector.y), 1) * Mathf.Sign(directionVector.y);

            anim.SetFloat("x", input_x);
            anim.SetFloat("y", input_y);

            transform.position += new Vector3(input_x, input_y, 0).normalized * Time.deltaTime * Speed;
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        Debug.Log("Gotcha!");
        //Destroy(gameObject);
    }
}
