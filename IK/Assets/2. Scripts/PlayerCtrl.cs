using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCtrl : MonoBehaviour
{
    public Animator animator;


    [Header("Related to movement")]
    [Range(0.5f, 2.5f)]
    public float mouseSensation;
    public float speed;
    private bool canMove = true;
    private Rigidbody rb;
    private Vector3 dir;
    private float rotX;
    private float rotY;
    // Start is called before the first frame update
    void Start()
    {
        rb = gameObject.GetComponent<Rigidbody>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // Update is called once per frame
    void Update()
    {

        PlayerMove();
        PlayerAnim();
    }

    private void PlayerMove()
    {
        if(canMove)
        {
            dir.x = Input.GetAxis("Horizontal");
            dir.z = Input.GetAxis("Vertical");
            transform.Translate(dir * Time.deltaTime * speed);
        }
        //rb.velocity += dir * Time.deltaTime * speed;
        rotX = Mathf.Repeat(rotX + Input.GetAxis("Mouse X") * mouseSensation, 360f);
        rotY = Mathf.Clamp(rotY + Input.GetAxis("Mouse Y") * mouseSensation, -90, 90);

        Camera.main.transform.localEulerAngles = new Vector3(-rotY, 0, 0);
        transform.eulerAngles = (new Vector3(0, rotX, 0));
        if (Input.GetKeyDown(KeyCode.Space))
            StartCoroutine(DelayedJump(0.5f));
    }

    private IEnumerator DelayedJump(float delayTime)
    {
        canMove = false;
        yield return new WaitForSeconds(delayTime);
        rb.AddForce(Vector3.up * 5f, ForceMode.Impulse);
        canMove = true;
    }

    private void PlayerAnim()
    {
        animator.SetBool("Walk", dir != Vector3.zero);
        if (Input.GetKeyDown(KeyCode.Space))
            animator.SetTrigger("Jump");
    }
}
