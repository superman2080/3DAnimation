using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpiderAnimator : MonoBehaviour
{
    
    public float maxLegDist;
    public Transform body;
    public Transform[] legTarget;
    public Transform[] legRayTr;

    private Vector3[] lastLegPos = new Vector3[4];
    private Vector3[] moveToLegPos = new Vector3[4];
    private Coroutine[] legCor = new Coroutine[4];


    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < legTarget.Length; i++)
        {
            lastLegPos[i] = legTarget[i].position;
        }

    }

    private void Update()
    {
        transform.Translate(new Vector3(0, 0, Input.GetAxis("Vertical") * 0.3f * Time.deltaTime));   
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

        for (int i = 0; i < legTarget.Length; i++)
        {
            if (Vector3.Distance(lastLegPos[i], moveToLegPos[i]) > maxLegDist && legCor[i] == null)
            {
                Debug.LogWarning(i);
                legCor[i] = StartCoroutine(LegIK(i, moveToLegPos[i]));
            }
        }
        Vector3 v1 = legTarget[0].position - legTarget[1].position;
        Vector3 v2 = legTarget[2].position - legTarget[3].position;
        Vector3 normal = Vector3.Cross(v1, v2).normalized;
        Vector3 up = Vector3.Lerp(body.position, normal, 1f);
        transform.up = up;
    }

    private IEnumerator LegIK(int idx, Vector3 moveTo)
    {
        Vector3 origin = lastLegPos[idx];
        float dT = 0;
        
        while (dT < 0.3f)
        {
            lastLegPos[idx] = Vector3.Lerp(origin, moveTo, dT / 0.3f);
            dT += Time.deltaTime;
            yield return null;
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
