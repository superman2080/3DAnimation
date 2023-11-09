using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

public class SpiderAnimator : MonoBehaviour
{
    private const int legLength = 4;
    public float speed;
    public float maxLegDist;
    [Range(0.05f, 0.5f)]
    public float bodyOffset;
    public Transform body;
    //���� �ٸ��� ����ų Ÿ�� Ʈ������
    public Transform[] legTarget = new Transform[legLength];

    //����ĳ���� Ʈ������
    public Transform[] legRayTr = new Transform[legLength];

    //������ �ٸ� ��ġ
    private Vector3[] lastLegPos = new Vector3[legLength];

    //�̵��� �ٸ� ��ġ
    private Vector3[] moveToLegPos = new Vector3[legLength];

    //���� �ڷ�ƾ �������ΰ�
    private Coroutine[] legCor = new Coroutine[legLength];
    public float smoothness = 5f;
    private int[] rPair = { 0, 1};
    private int[] lPair = { 3, 2};

    private Rigidbody rb;
    private float rotY;
    private Vector3 lastBodyUp;
    private NavMeshAgent nav;

    // Start is called before the first frame update
    void Start()
    {
        rb = gameObject.GetComponent<Rigidbody>();
        for (int i = 0; i < legLength; i++)
        {
            lastLegPos[i] = legTarget[i].position;
        }
        lastBodyUp = transform.up;

        nav = gameObject.GetComponent<NavMeshAgent>();
        StartCoroutine(MoveToPosition(new Vector3(1, 0, 3), speed, 1f));
    }

    private void Update()
    {
        float v = Input.GetAxis("Vertical");
        float h = Input.GetAxis("Horizontal") * 0.15f;
        transform.Translate(Mathf.Sin(-rotY * Mathf.Deg2Rad) * v * speed * Time.deltaTime, 0, Mathf.Cos(-rotY * Mathf.Deg2Rad) * v * speed * Time.deltaTime);
        rotY = Mathf.Repeat(rotY - h, 360f);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        SpiderAnim();
    }

    public IEnumerator MoveToPosition(Vector3 pos, float moveSpeed, float rotSpeed)
    {
        NavMeshPath path = new NavMeshPath();
        nav.CalculatePath(pos, path);
        Debug.Log(path.status);
        if(path.status != NavMeshPathStatus.PathComplete)
        {
            NavMeshHit hit;
            if (NavMesh.SamplePosition(pos, out hit, 100, -1))
            {
                pos = hit.position;
            }
        }
        float originY = rotY;
        float lookAtY = Mathf.Atan2(-(pos.z - transform.position.z), -(pos.x - transform.position.x)) * Mathf.Rad2Deg + 90;
        float dT = 0;
        while (true)
        {
            if(rotY == lookAtY)
                break;
 
            dT += Time.deltaTime;
            yield return null;
            rotY = Mathf.Lerp(originY, lookAtY, dT * rotSpeed);
        }
        while (true)
        {
            transform.Translate(Mathf.Sin(-rotY * Mathf.Deg2Rad) * moveSpeed * Time.deltaTime, 0, Mathf.Cos(-rotY * Mathf.Deg2Rad) * moveSpeed * Time.deltaTime);
            yield return null;
            if((pos - transform.position).magnitude < 0.25f)
            {
                break;
            }
        }
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
            //���� �̻����� �̵����� �� �ٸ� �̵�
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
        
        //����
        Vector3 v1 = legTarget[0].position - legTarget[1].position;
        Vector3 v2 = legTarget[2].position - legTarget[3].position;
        Vector3 normal = Vector3.Cross(v1, v2).normalized;
        Vector3 up = Vector3.Lerp(lastBodyUp, normal, 1f / (float)(smoothness + 1));

        transform.up = up;
        body.localRotation = Quaternion.Euler(body.localEulerAngles.x, rotY, body.localEulerAngles.z);
        lastBodyUp = up;
        //

        //���� ���̷� �ȱ�
        float averageY = 0f;
        foreach (var pos in moveToLegPos)
        {
            averageY += pos.y;
        }
        averageY = averageY / legLength;

        transform.position = new Vector3(transform.position.x, averageY + bodyOffset, transform.position.z);
        //

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
            //������ �ٸ��� ��(���� �� üũ)
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
