using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.Progress;

public class PlayerCtrl : MonoBehaviour
{
    public Animator animator;


    [Header("Related to movement")]
    [Range(0.5f, 2.5f)]
    public float mouseSensation;
    public float speed;
    public Transform mouseTr;
    public Transform lookAt;
    public Transform headTr;
    public Transform weaponTr;
    [Range(2f, 5f)]
    public float senseDist;

    
    private Coroutine attackCor;

    public GameObject nowWeapon;
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
        Cursor.visible = false;    }

    // Update is called once per frame
    void Update()
    {
        PlayerBehavior();
        PlayerAnim();
    }

    private void PlayerBehavior()
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

        if(Input.GetMouseButtonDown(0) && nowWeapon == null && rightHandObj != null)
        {
            rightHandObj.SetParent(weaponTr);
            nowWeapon = rightHandObj.gameObject;
            nowWeapon.transform.position = weaponTr.position;
            nowWeapon.transform.eulerAngles = weaponTr.eulerAngles;
            nowWeapon.GetComponent<WeaponCtrl>().trailMesh.SetActive(false);
        }
    }

    private IEnumerator DelayedJump(float delayTime)
    {
        canMove = false;
        yield return new WaitForSeconds(delayTime);
        rb.AddForce(Vector3.up * 5f, ForceMode.Impulse);
        canMove = true;
    }

    //    private IEnumerator PunchCor()
    //    {
    //        nowWeapon.GetComponent<WeaponCtrl>().trailMesh.SetActive(true);
    //        float punchAnimMag = 1;
    //        animator.SetLayerWeight(1, 1);
    //        if (nowWeapon != null)
    //            animator.SetTrigger("Attack0");
    //        while (true)
    //        {
    //            yield return null;
    //            if (animator.GetCurrentAnimatorStateInfo(1).normalizedTime > 0.5f)
    //            {
    //                punchAnimMag -= Time.deltaTime;
    ////                animator.SetLayerWeight(1, punchAnimMag);
    //                if (punchAnimMag <= 0)
    //                    break;
    //            }
    //        }
    //        animator.SetLayerWeight(1, 0);
    //        nowWeapon.GetComponent<WeaponCtrl>().trailMesh.SetActive(false);
    //        attackCor = null;
    //    }

    private IEnumerator PunchCor(int idx)
    {

        bool nextCombo = false;

        nowWeapon.GetComponent<WeaponCtrl>().trailMesh.SetActive(true);
        animator.SetLayerWeight(1, 1);
        animator.SetTrigger("Attack" + idx.ToString());

        while (true)
        {
            yield return null;
            if (Input.GetMouseButtonDown(0) && idx < 1 && animator.GetCurrentAnimatorStateInfo(1).normalizedTime >= 0.5f)
            {
                nextCombo = true;
            }
            if (animator.GetCurrentAnimatorStateInfo(1).normalizedTime >= 1f)
                break;
        }
        if (nextCombo)
        {
            attackCor = StartCoroutine(PunchCor(idx + 1));
        }
        else
        {
            nowWeapon.GetComponent<WeaponCtrl>().trailMesh.SetActive(false);
            animator.SetLayerWeight(1, 0);
            animator.SetTrigger("AttackIdle");
            attackCor = null;
        }
    }

    private void PlayerAnim()
    {
        //블렌드 애니메이션 연습
        animator.SetFloat("Vertical", Input.GetAxis("Vertical"));
        animator.SetFloat("Horizontal", Input.GetAxis("Horizontal"));
        //

        //애니메이션 레이어 연습

        if (Input.GetMouseButtonDown(0) && attackCor == null && nowWeapon != null)
        {
            attackCor = StartCoroutine(PunchCor(0));
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
            //float dist = float.PositiveInfinity;
            int idx = -1;
            for (int i = 0; i < touchables.Length; i++)
            {
                if (touchables[i].gameObject == nowWeapon)
                    continue;

                if (IsTargetInSight(touchables[i].transform, headTr, 30f))
                {
                    if(idx == -1)
                    {
                        idx = i;
                    }
                    else if(Vector3.Distance(transform.position, touchables[i].transform.position) < Vector3.Distance(transform.position, touchables[idx].transform.position))
                    {
                        idx = i;
                    }
                }
            }
            if(idx == -1)
            {
                rightHandObj = null;
            }
            else
            {
                rightHandObj = touchables[idx].transform;
            }
            //foreach (var item in touchables)
            //{
            //    if (rightHandObj != null)
            //    {
            //        if (Vector3.Distance(transform.position, item.transform.position) < dist && IsTargetInSight(item.transform, headTr, 60f))
            //        {
            //            rightHandObj = item.transform;
            //            dist = Vector3.Distance(transform.position, item.transform.position);
            //        }
            //    }
            //    else
            //    {
            //        if (IsTargetInSight(item.transform, lookAt, 60f))
            //        {
            //            rightHandObj = item.transform;
            //            dist = Vector3.Distance(transform.position, item.transform.position);
            //        }
            //    }

            //}
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
            cam.transform.localPosition = originPos + new Vector3(UnityEngine.Random.Range(-nowIntense, nowIntense), UnityEngine. Random.Range(-nowIntense, nowIntense), 0);
            nowIntense = Mathf.Lerp(intense, 0, dT / time);
            dT += Time.deltaTime;
            yield return null;
        }
        cam.transform.localPosition = originPos;
    }

    private bool IsTargetInSight(Transform target, Transform origin, float degree)
    {
        Vector3 dir = (target.position - origin.position).normalized;

        float dot = Vector3.Dot(origin.forward, dir);
        float theta = MathF.Acos(dot) * Mathf.Rad2Deg;

        return theta <= degree;
    }

    private void UnequipWeapon()
    {
        if (nowWeapon == null)
            return;

        if(Physics.Raycast(weaponTr.position, Vector3.down, out RaycastHit hit, 5f))
        {
            nowWeapon.transform.SetParent(null);
        }
    }

    public void Attack()
    {
        if (nowWeapon == null)
            return;

        StartCoroutine(CameraShakeCor(0.1f, 0.1f));

        WeaponCtrl weapon = nowWeapon.GetComponent<WeaponCtrl>();

        Collider[] colliders = Physics.OverlapSphere(weapon.transform.position, weapon.attackDist);
        if(colliders.Length > 0)
        {
            foreach (var entity in colliders)
            {
                Debug.Log(entity.name);
            }
        }
    }
}
