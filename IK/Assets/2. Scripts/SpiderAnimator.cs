using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpiderAnimator : MonoBehaviour
{
    private const int legLength = 4;
    public float speed;
    public float maxLegDist;
    [Range(0.05f, 0.5f)]
    public float bodyOffset;
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
    private float smoothness = 10f;
    private int[] rPair = { 0, 1};
    private int[] lPair = { 3, 2};

    private float rotY;
    private Vector3 lastBodyUp;

    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < legLength; i++)
        {
            lastLegPos[i] = legTarget[i].position;
        }
        lastBodyUp = transform.up;
    }

    private void Update()
    {
        float v = Input.GetAxis("Vertical");
        float h = Input.GetAxis("Horizontal") * 0.15f;
        if (v == 0 && h == 0)
        {
            //isFirstStep = true;
            //for (int i = 0; i < legTarget.Length; i++)
            //{
            //    if ((legTarget[i].position - moveToLegPos[i]).magnitude >= 0.01f && legCor[i] == null)
            //        legCor[i] = StartCoroutine(LegIK(i, moveToLegPos[i]));
            //}
        }
        transform.Translate(0, 0, v * speed * Time.deltaTime);
        rotY = Mathf.Repeat(rotY + h, 360);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        SpiderAnim();
    }

    private void SpiderAnim()
    {
        for (int i = 0; i < legLength; i++)
        {
            legTarget[i].position = lastLegPos[i];
        }

        for (int i = 0; i < legLength; i++)
        {
            if (Physics.Raycast(legRayTr[i].position, legRayTr[i].transform.up, out RaycastHit hit, float.PositiveInfinity))
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
                //만약 맞은편 다리가 이동 중이라면
                //if()
                //{
                //    if(Array.IndexOf(rPair, i) != -1)
                //    {

                //    }
                //    else
                //    {

                //    }
                //}
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
        //body.up = Vector3.Cross(v1, v2).normalized;
        Vector3 normal = Vector3.Cross(v1, v2).normalized;
        Vector3 up = Vector3.Lerp(lastBodyUp, normal, 1f / (float)(smoothness + 1));
        //up = Quaternion.AngleAxis(rotY, Vector3.up) * up;
        transform.up = up;
        //

        //일정 높이로 걷기
        float averageY = 0f;
        foreach (var leg in legRayTr)
        {
            if (Physics.Raycast(leg.position, leg.transform.up, out RaycastHit hit, float.PositiveInfinity))
            {
                averageY += hit.point.y;
            }
        }
        averageY /= legLength;

        transform.position = new Vector3(transform.position.x, averageY + bodyOffset, transform.position.z);
        //

        //transform.localEulerAngles = new Vector3(transform.eulerAngles.x, rotY, transform.eulerAngles.z);

        //body.localEulerAngles = new Vector3(body.localEulerAngles.x, rotY, body.localEulerAngles.z);
        //transform.rotation = Quaternion.LookRotation(transform.forward, up);
        lastBodyUp = up;
    }

    private IEnumerator LegIK(int idx, Vector3 moveTo)
    {
        Vector3 origin = lastLegPos[idx];

        for (int i = 1; i <= smoothness; ++i)
        {
            lastLegPos[idx] = Vector3.Lerp(origin, moveTo, i / smoothness);
            lastLegPos[idx].y += Mathf.Sin(Mathf.Lerp(0, 180, i / smoothness) * Mathf.Deg2Rad) * maxLegDist;
            yield return new WaitForFixedUpdate();
        }
        lastLegPos[idx] = moveTo;
        legCor[idx] = null;
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
}
