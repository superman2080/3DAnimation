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
    public Transform mouseTr;
    public Transform lookAt;
    [Range(2f, 5f)]
    public float senseDist;

    
    private Coroutine punchCor;

    private Transform rightHandObj;

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

        mouseTr.localEulerAngles = new Vector3(-rotY, 0, 0);
        transform.eulerAngles = (new Vector3(0, rotX, 0));
        if (Input.GetKeyDown(KeyCode.Space) && rb.velocity.y == 0)
            StartCoroutine(DelayedJump(0.5f));

        
    }

    private IEnumerator DelayedJump(float delayTime)
    {
        canMove = false;
        yield return new WaitForSeconds(delayTime);
        rb.AddForce(Vector3.up * 5f, ForceMode.Impulse);
        canMove = true;
    }

    private IEnumerator PunchCor()
    {
        float punchAnimMag = 1;
        animator.SetLayerWeight(1, 1);
        animator.SetTrigger("Punch");
        while (true)
        {
            yield return null;
            if (animator.GetCurrentAnimatorStateInfo(1).normalizedTime > 0.5f)
            {
                punchAnimMag -= Time.deltaTime;
                animator.SetLayerWeight(1, punchAnimMag);
                if (punchAnimMag <= 0)
                    break;
            }
        }
        punchCor = null;
    }

    private void PlayerAnim()
    {
        //블렌드 애니메이션 연습
        animator.SetFloat("Vertical", Input.GetAxis("Vertical"));
        animator.SetFloat("Horizontal", Input.GetAxis("Horizontal"));
        //

        //애니메이션 레이어 연습

        if (Input.GetMouseButtonDown(0) && punchCor == null)
        {
            punchCor = StartCoroutine(PunchCor());
        }
        if (Input.GetKeyDown(KeyCode.Space) && rb.velocity.y == 0)
            animator.SetTrigger("Jump");


        //IK 연습
        Collider[] touchables = Physics.OverlapSphere(transform.position, senseDist, 1 << LayerMask.NameToLayer("Touchable"));
        if (touchables.Length <= 0)
        {
            rightHandObj = null;
        }
        else
        {
            float dist = Vector3.Distance(transform.position, touchables[0].transform.position);
            foreach (var item in touchables)
            {
                if (rightHandObj != null)
                {
                    if (Vector3.Distance(transform.position, item.transform.position) < dist && IsTargetInSight(item.transform, lookAt, 60f))
                        rightHandObj = item.transform;
                }
                else
                {
                    if (IsTargetInSight(item.transform, lookAt, 60f))
                    {
                        rightHandObj = item.transform;
                    }
                }

            }
        }
        //
    }

    void OnAnimatorIK()
    {
        if (animator)
        {
            // Set the look target position, if one has been assigned
            if (lookAt != null)
            {
                animator.SetLookAtWeight(1);
                animator.SetLookAtPosition(lookAt.position);
            }
            if(rightHandObj != null) { 
                animator.SetIKPosition(AvatarIKGoal.RightHand, rightHandObj.position);
                animator.SetIKPositionWeight(AvatarIKGoal.RightHand, (senseDist - Vector3.Distance(transform.position, rightHandObj.position)) / senseDist);
            }
        }
    }

    public static IEnumerator CameraShakeCor(float intense, float time)
    {
        float dT = 0;
        float nowIntense = intense;
        Camera cam = Camera.main;
        Vector3 originPos = cam.transform.localPosition;
        while(dT < time)
        {
            cam.transform.localPosition = originPos + new Vector3(Random.Range(-nowIntense, nowIntense), Random.Range(-nowIntense, nowIntense), 0);
            nowIntense = Mathf.Lerp(intense, 0, dT / time);
            dT += Time.deltaTime;
            yield return null;
        }
        cam.transform.localPosition = originPos;
    }

    private bool IsTargetInSight(Transform target, Transform origin, float angle)
    {
        Vector3 dir = (target.position - origin.position).normalized;

        float dot = Vector3.Dot(origin.up, dir);
        float theta = dot * Mathf.Deg2Rad;

        return theta < angle;
    }
}
