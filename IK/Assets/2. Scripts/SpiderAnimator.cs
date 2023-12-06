using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using static UnityEditor.PlayerSettings;

public class SpiderAnimator : MonoBehaviour
{
    [Header("Related to Game play")]
    public float maxTargetRange;
    private PlayerCtrl player;  //플레이어 정보
    private Vector3 targetPos;  //현재 추적 위치
    private Coroutine chaseCor;
    private Coroutine skillCor = null;
    [Header("Related to Spider Animation")]
    private const int legLength = 4;
    public float speed;
    public float maxLegDist;
    [Range(0.05f, 5f)]
    public float bodyOffset;
    [Range(0.05f, 0.5f)]
    public float bodyShakingOffset;
    public Transform body;
    public Transform head;
    //현재 다리가 가리킬 타겟 트랜스폼
    public Transform[] legTarget = new Transform[legLength];

    //레이캐스팅 트랜스폼
    public Transform[] legRayTr = new Transform[legLength];

    //마지막 다리 위치
    private Vector3[] lastLegPos = new Vector3[legLength];

    //이동할 다리 위치
    private Vector3[] moveToLegPos = new Vector3[legLength];

    //현재 코루틴 실행중인가
    private Coroutine[] legCor = new Coroutine[legLength];
    public float smoothness = 5f;
    private int[] rPair = { 0, 1};
    private int[] lPair = { 3, 2};

    private Rigidbody rb;
    private float rotY;
    private Vector3 lastBodyUp;

    // Start is called before the first frame update
    void Start()
    {
        rb = gameObject.GetComponent<Rigidbody>();
        player = FindObjectOfType<PlayerCtrl>();
        targetPos = player.transform.position;

        for (int i = 0; i < legLength; i++)
        {
            lastLegPos[i] = legTarget[i].position;
        }
        lastBodyUp = transform.up;
        rotY = Mathf.Atan2(-(targetPos.z - transform.position.z), -(targetPos.x - transform.position.x)) * Mathf.Rad2Deg + 90;
        body.localRotation = Quaternion.Euler(body.localEulerAngles.x, rotY, body.localEulerAngles.z);
    }

    private void Update()
    {


    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (Vector3.Distance(transform.position, player.transform.position) > 10 && skillCor == null)
        {
            skillCor = StartCoroutine(Dash(player.transform.position + Vector3.up * GetBodyOffsetY(), 1.5f));
        }
        else if (Vector3.Distance(transform.position, player.transform.position) > maxTargetRange && skillCor == null)
        {
            if (chaseCor != null)
                StopCoroutine(chaseCor);
            targetPos = player.transform.position;
            chaseCor = StartCoroutine(MoveToPosition(targetPos, speed, 75f));
        }

        if (skillCor == null)
            SpiderAnim();
        head.LookAt(targetPos);
    }

    public IEnumerator MoveToPosition(Vector3 pos, float moveSpeed, float rotSpeed)
    {
        //rotation
        if (Physics.Raycast(pos, Vector3.down, out RaycastHit hit))
        {
            pos = hit.point;
        }
        else
            yield break;

        float originY = rotY;
        float lookAtY = Mathf.Atan2(-(pos.z - transform.position.z), -(pos.x - transform.position.x)) * Mathf.Rad2Deg + 90;
        float rotTime = Mathf.Repeat(Mathf.Max(originY, lookAtY) - Mathf.Min(originY, lookAtY), 360f) / rotSpeed;
        float dT = 0;
        while (dT < rotTime)
        {
            dT += Time.deltaTime;
            yield return null;
            rotY = Mathf.Lerp(originY, lookAtY, dT / rotTime);
        }
        //

        //move
        dT = 0;
        Vector3 originPos = transform.position;
        float moveTime = Vector3.Distance(originPos, pos) / moveSpeed;
        while (dT < moveTime)
        {
            dT += Time.deltaTime;
            yield return null;

            Vector3 moveToPos = Vector3.Lerp(originPos, pos, dT / moveTime);

            transform.position = new Vector3(moveToPos.x, GetBodyOffsetY(), moveToPos.z);
        }
        //
        chaseCor = null;
    }

    private void SpiderAnim()
    {
        for (int i = 0; i < legLength; i++)
        {
            legTarget[i].position = lastLegPos[i];
        }

        for (int i = 0; i < legLength; i++)
        {
            if (Physics.Raycast(legRayTr[i].position, legRayTr[i].transform.up, out RaycastHit hit, float.PositiveInfinity, 1 << LayerMask.NameToLayer("Ground")))
            {
                moveToLegPos[i] = hit.point;
            }
        }

        //Zigzag pattern
        for (int i = 0; i < legLength; i++)
        {
            //보폭 이상으로 이동했을 때 다리 이동
            if (Vector3.Distance(lastLegPos[i], moveToLegPos[i]) > maxLegDist
                && legCor[i] == null
                && IsOpperatingOppositeLegCor(i) == false)
            {
                if (Array.IndexOf(rPair, i) != -1)
                {
                    foreach (var legIdx in rPair)
                    {
                        legCor[legIdx] = StartCoroutine(LegIK(legIdx, moveToLegPos[legIdx]));
                    }
                    break;
                }
                else if (Array.IndexOf(lPair, i) != -1)
                {
                    foreach (var legIdx in lPair)
                    {
                        legCor[legIdx] = StartCoroutine(LegIK(legIdx, moveToLegPos[legIdx]));
                    }
                    break;
                }
            }
        }
        
        //기울기
        Vector3 v1 = legTarget[0].position - legTarget[1].position;
        Vector3 v2 = legTarget[2].position - legTarget[3].position;
        Vector3 normal = Vector3.Cross(v1, v2).normalized;
        Vector3 up = Vector3.Lerp(lastBodyUp, normal, 1f / (float)(smoothness + 1));

        transform.up = up;
        body.localRotation = Quaternion.Euler(body.localEulerAngles.x, rotY, body.localEulerAngles.z);
        lastBodyUp = up;
        //

        

        transform.position = new Vector3(transform.position.x, GetBodyOffsetY(), transform.position.z);
        //

    }

    //Spider Anim
    private IEnumerator LegIK(int idx, Vector3 moveTo)
    {
        Vector3 origin = lastLegPos[idx];

        for (int i = 1; i <= smoothness; ++i)
        {
            lastLegPos[idx] = Vector3.Lerp(origin, moveTo, i / smoothness);
            lastLegPos[idx].y += Mathf.Sin(Mathf.Lerp(0, 180, i / smoothness) * Mathf.Deg2Rad) * maxLegDist * 3f;
            yield return new WaitForFixedUpdate();
        }

        StartCoroutine(Util.CameraShakeCor(0.1f, 0.05f, false));
        lastLegPos[idx] = moveTo;
        legCor[idx] = null;
    }

    private bool IsOpperatingOppositeLegCor(int idx)
    {
        if(Array.IndexOf(rPair, idx) != -1)
        {
            //오른쪽 다리일 때(왼쪽 쌍 체크)
            foreach (var legIdx in lPair)
            {
                if (legCor[legIdx] != null)
                    return true;
            }
            return false;
        }
        else
        {
            foreach (var legIdx in rPair)
            {
                if (legCor[legIdx] != null)
                    return true;
            }
            return false;
        }
    }

    private float GetBodyOffsetY()
    {

        float averageY = 0f;
        foreach (var pos in moveToLegPos)
        {
            averageY += pos.y;
        }
        averageY = averageY / legLength;

        float shakeOffset = 0f;
        foreach (var pos in legTarget)
        {
            shakeOffset += pos.position.y;
        }
        shakeOffset /= legLength;
        shakeOffset *= bodyShakingOffset;

        //averageY *= bodyShakingOffset;
        //float shakeOffset = Mathf.Sin((Mathf.Repeat(Time.time * 3f, 2f) - 1f) * 180f * Mathf.Deg2Rad) * bodyShakingOffset;
        return averageY + bodyOffset + shakeOffset;
    }

    private void OnDrawGizmos()
    {
        for (int i = 0; i < legLength; i++)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(legTarget[i].position, 0.05f);

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(moveToLegPos[i], 0.05f);

            Gizmos.color = Color.blue;
            Gizmos.DrawLine(legTarget[i].position, moveToLegPos[i]);
        }
    }
    //

    private IEnumerator Dash(Vector3 targetPos, float time)
    {
        Vector3 originPos = transform.position;
        targetPos = Vector3.Lerp(originPos, targetPos, 0.9f);
        Vector3 high = Vector3.Lerp(originPos, targetPos, 0.2f);
        high.y = (originPos.y + targetPos.y) / 2 + 7.5f;

        foreach (var leg in legTarget)
        {
            leg.position = body.position;
        }

        float dT = 0;
        while(dT < time * 0.667f)
        {
            dT += Time.deltaTime;
            yield return null;
            transform.position = Vector3.Lerp(originPos, high, dT / (time * 0.667f));
        }
        dT = 0;

        while (dT < time * 0.333f)
        {
            dT += Time.deltaTime;
            yield return null;
            transform.position = Vector3.Lerp(high, targetPos, dT / (time * 0.333f));
        }
        StartCoroutine(Util.CameraShakeCor(0.5f, 0.2f, false));
        transform.position = targetPos;
        skillCor = null;
    }
}
