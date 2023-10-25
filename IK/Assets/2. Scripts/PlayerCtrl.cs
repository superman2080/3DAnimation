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
    [Range(1, 3)]
    public float dashSpeedMag;
    public Transform mouseTr;
    public Transform lookAt;
    public Transform headTr;
    public Transform weaponTr;      //���� ��ġ Ʈ������(���� Ʈ������)
    [Range(2f, 5f)]
    public float touchableObjSenseDist;         //���� �� �ִ� ������Ʈ �ν� �Ÿ�
    public Transform jumpTr;        //���� �ν� Ʈ������(�� ��)

    
    private Coroutine attackCor;
    private Coroutine jumpCor;

    [HideInInspector]
    public GameObject nowWeapon;

    private readonly int maxAttackComboNum = 2;
    private readonly int attackLayer = 1;   //���� �ִϸ��̼� ���̾�
    private Transform rightHandObj;     //���� ������Ʈ

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
        PlayerBehavior();
        PlayerAnim();
    }

    private void PlayerBehavior()
    {
        if(canMove)
        {
            dir.x = Input.GetAxis("Horizontal");
            dir.z = Input.GetAxis("Vertical");
            if (Input.GetKey(KeyCode.LeftShift) && canMove == true)
                dir *= dashSpeedMag;
            transform.Translate(dir * Time.deltaTime * speed);
        }
        //rb.velocity += dir * Time.deltaTime * speed;
        rotX = Mathf.Repeat(rotX + Input.GetAxis("Mouse X") * mouseSensation, 360f);
        rotY = Mathf.Clamp(rotY + Input.GetAxis("Mouse Y") * mouseSensation, -90, 90);

        mouseTr.localEulerAngles = new Vector3(-rotY, 0, 0);
        transform.eulerAngles = (new Vector3(0, rotX, 0));

        //����
        if (Input.GetKeyDown(KeyCode.Space) && rb.velocity.y <= 0)
        {
            if (Physics.OverlapSphere(jumpTr.position, 0.1f, 1 << LayerMask.NameToLayer("Ground")).Length > 0 && jumpCor == null) 
            {
                jumpCor = StartCoroutine(DelayedJump(0.5f));
            }
        }

        //if (Input.GetKeyDown(KeyCode.Space) && rb.velocity.y == 0)
        //    StartCoroutine(DelayedJump(0.5f));

        if (Input.GetMouseButtonDown(0) && nowWeapon == null && rightHandObj != null)
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
        animator.SetTrigger("Jump");
        yield return new WaitForSeconds(delayTime);
        rb.AddForce(Vector3.up * 5f, ForceMode.Impulse);
        canMove = true;
        jumpCor = null;
    }


    private void PlayerAnim()
    {
        //���� �ִϸ��̼� ����
        animator.SetFloat("Vertical", Input.GetAxis("Vertical"));
        animator.SetFloat("Horizontal", Input.GetAxis("Horizontal"));
        animator.SetBool("IsRunning", Input.GetKey(KeyCode.LeftShift));
        //

        //�ִϸ��̼� ���̾� ����

        if (Input.GetMouseButtonDown(0) && attackCor == null && nowWeapon != null)
        {
            attackCor = StartCoroutine(AttackCor(0, maxAttackComboNum));
        }


        //IK ����
        Collider[] touchables = Physics.OverlapSphere(transform.position, touchableObjSenseDist, 1 << LayerMask.NameToLayer("Touchable"));
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

                if (IsTargetInSight(touchables[i].transform, headTr, 60f))
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

    private IEnumerator AttackCor(int idx, int maxComboCnt)
    {
        bool nextCombo = false;

        nowWeapon.GetComponent<WeaponCtrl>().trailMesh.SetActive(true);
        animator.SetLayerWeight(attackLayer, 1);
        animator.SetTrigger("Attack" + idx.ToString());
        yield return new WaitUntil(() => IsAnimationClipPlaying("Attack" + idx.ToString(), attackLayer) == true);
        while (true)
        {
            yield return null;
            if (Input.GetMouseButton(0) && idx < maxComboCnt)
            {
                nextCombo = true;
            }
            if (animator.GetCurrentAnimatorStateInfo(attackLayer).normalizedTime >= 0.95f)
                break;
        }
        if (nextCombo)
        {
            attackCor = StartCoroutine(AttackCor(idx + 1, maxComboCnt));
            yield break;
        }
        else
        {
            nowWeapon.GetComponent<WeaponCtrl>().trailMesh.SetActive(false);
            yield return StartCoroutine(LerpAnimationLayer(1f, 0, attackLayer, 0.25f));
            animator.SetTrigger("AttackIdle");
            attackCor = null;
        }
    }

    //�ִϸ��̼� ��������(���̾� ����)
    private IEnumerator LerpAnimationLayer(float start, float end, int layerIdx, float duration)
    {
        float dT = 0;
        animator.SetLayerWeight(layerIdx, start);
        while (dT < duration)
        {
            dT += Time.deltaTime;
            yield return null;
            animator.SetLayerWeight(layerIdx, Mathf.Lerp(start, end, dT / duration));
        }
        animator.SetLayerWeight(layerIdx, end);
    }

    //�ִϸ��̼� ���������� ����
    private bool IsAnimationClipPlaying(string name, int layerIdx)
    {
        return animator.GetCurrentAnimatorStateInfo(layerIdx).IsName(name) && animator.GetCurrentAnimatorStateInfo(layerIdx).normalizedTime < 1f;
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
                animator.SetIKPositionWeight(AvatarIKGoal.RightHand, (touchableObjSenseDist - Vector3.Distance(transform.position, rightHandObj.position)) / touchableObjSenseDist);
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
                if(IsTargetInSight(entity.transform, transform, weapon.attackDegree) 
                    && entity.transform.IsChildOf(weaponTr) == false
                    && entity.GetComponent<PlayerCtrl>() is null)
                {
                    Debug.Log(entity.name);
                }
            }
        }
    }
}
