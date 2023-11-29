using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Util : MonoBehaviour
{
    public static bool isCameraShake = false;
    public static IEnumerator CameraShakeCor(float intense, float time, bool increasing)
    {
        if (isCameraShake == true)
            yield break;

        isCameraShake = true;
        float dT = 0;
        float nowIntense = increasing ? 0 : intense;
        Camera cam = Camera.main;
        Vector3 originPos = cam.transform.localPosition;
        while (dT < time)
        {
            cam.transform.localPosition = originPos + new Vector3(UnityEngine.Random.Range(-nowIntense, nowIntense), UnityEngine.Random.Range(-nowIntense, nowIntense), 0);
            nowIntense = increasing ? Mathf.Lerp(0, intense, dT / time) : Mathf.Lerp(intense, 0, dT / time);
            dT += Time.deltaTime;
            yield return null;
        }
        cam.transform.localPosition = originPos;
        isCameraShake = false;
    }

    public static bool IsTargetInSight(Transform target, Transform origin, float degree)
    {
        Vector3 dir = (target.position - origin.position).normalized;

        float dot = Vector3.Dot(origin.forward, dir);
        float theta = Mathf.Acos(dot) * Mathf.Rad2Deg;

        return theta <= degree;
    }

}
