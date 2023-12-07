using Mono.Cecil;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectManager : MonoBehaviour
{
    public static GameObject EffectOneshot(string effectName, Vector3 effectPos, Quaternion rot)
    {
        GameObject effect = Resources.Load<GameObject>("Effects/" + effectName);
        GameObject effectTemp = Instantiate(effect, effectPos, rot);
        Destroy(effectTemp, effectTemp.GetComponent<ParticleSystem>().main.duration);
        return effectTemp;
    }

    public static GameObject InstantiateEffect(string effectName, float t, Vector3 effectPos, Quaternion rot)
    {
        GameObject effect = Resources.Load<GameObject>("Effects/" + effectName);
        GameObject effectTemp = Instantiate(effect, effectPos, rot);
        Destroy(effectTemp, t);
        return effectTemp;
    }
}