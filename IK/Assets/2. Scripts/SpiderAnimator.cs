using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpiderAnimator : MonoBehaviour
{
    
    public float maxLegDist;
    public Transform body;
    //현재 다리가 가리킬 타겟 트랜스폼
    public Transform[] legTarget;

    //레이캐스팅 트랜스폼
    public Transform[] legRayTr;

    //마지막 다리 위치
    private Vector3[] lastLegPos = new Vector3[4];

    //이동할 다리 위치
    private Vector3[] moveToLegPos = new Vector3[4];

    //현재 코루틴 실행중인가
    private Coroutine[] legCor = new Coroutine[4];
    private float smoothness = 10f;
    private float[] step = { 0.5f, 0.5f, 1f, 1f };
    private bool isFirstStep = true;
    private float rotY;
    private Vector3 lastBodyUp;

    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < legTarget.Length; i++)
        {
            lastLegPos[i] = legTarget[i].position;
        }
        lastBodyUp = transform.up;
    }

    private void Update()
    {
        float v = Input.GetAxis("Vertical");
        float h = Input.GetAxis("Horizontal");
        if (v == 0 && h == 0)
        {
            isFirstStep = true;
            for (int i = 0; i < legTarget.Length; i++)
            {
                if ((legTarget[i].position - moveToLegPos[i]).magnitude >= 0.01f && legCor[i] == null)
                    legCor[i] = StartCoroutine(LegIK(i, moveToLegPos[i]));
            }
        }
        transform.Translate(new Vector3(h * maxLegDist * Time.deltaTime, 0, v * maxLegDist * Time.deltaTime));
        rotY = Mathf.Repeat(rotY + h, 360);
        //transform.eulerAngles = Vector3.up * rotY;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        SpiderAnim();
    }

    private void SpiderAnim()
    {
        for (int i = 0; i < legTarget.Length; i++)
        {
            legTarget[i].position = lastLegPos[i];
        }

        for (int i = 0; i < legTarget.Length; i++)
        {
            if (Physics.Raycast(legRayTr[i].position, legRayTr[i].transform.up, out RaycastHit hit, float.PositiveInfinity))
            {
                moveToLegPos[i] = hit.point;
            }
        }

        if (isFirstStep)
        {
            for (int i = 0; i < legTarget.Length; i++)
            {
                if (Vector3.Distance(lastLegPos[i], moveToLegPos[i]) > maxLegDist * step[i] && legCor[i] == null)
                {
                    legCor[i] = StartCoroutine(LegIK(i, moveToLegPos[i]));
                    isFirstStep = false;
                }
            }
        }
        else
        {
            for (int i = 0; i < legTarget.Length; i++)
            {
                if (Vector3.Distance(lastLegPos[i], moveToLegPos[i]) > maxLegDist && legCor[i] == null)
                {
                    legCor[i] = StartCoroutine(LegIK(i, moveToLegPos[i]));
                }
            }
        }


        //for (int i = 0; i < legTarget.Length; i++)
        //{
        //    if (isFirstStep)
        //    {
        //        if (Vector3.Distance(lastLegPos[i], moveToLegPos[i]) > maxLegDist * step[i] && legCor[i] == null)
        //        {
        //            legCor[i] = StartCoroutine(LegIK(i, moveToLegPos[i]));
        //            step[i] = step[i] == 1 ? 0.5f : 1f;
        //            isFirstStep = false;
        //        }
        //    }
        //    else
        //    {
        //        if (Vector3.Distance(lastLegPos[i], moveToLegPos[i]) > maxLegDist && legCor[i] == null)
        //        {
        //            legCor[i] = StartCoroutine(LegIK(i, moveToLegPos[i]));
        //            step[i] = step[i] == 1 ? 0.5f : 1f;
        //        }
        //    }
        //}

        Vector3 v1 = legTarget[0].position - legTarget[1].position;
        Vector3 v2 = legTarget[2].position - legTarget[3].position;
        //body.up = Vector3.Cross(v1, v2).normalized;
        Vector3 normal = Vector3.Cross(v1, v2).normalized;
        Vector3 up = Vector3.Lerp(lastBodyUp, normal, 1f / (float)(smoothness + 1));
        transform.up = Quaternion.AngleAxis(rotY, up) * transform.up;
        Debug.Log(transform. up);
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
        for (int i = 0; i < moveToLegPos.Length; i++)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(moveToLegPos[i], 0.05f);
        }

        for (int i = 0; i < legTarget.Length; i++)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(legTarget[i].position, 0.05f);
        }
    }
}
