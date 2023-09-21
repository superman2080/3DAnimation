using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpiderAnimator : MonoBehaviour
{
    public Transform[] legTarget;
    private Vector3[] legPos = new Vector3[4];
    private Ray[] legRay = new Ray[4];

    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < legTarget.Length; i++)
        {
            legPos[i] = legTarget[i].position;
        }
    }

    // Update is called once per frame
    void Update()
    {
        for (int i = 0; i < legTarget.Length; i++)
        {
            legTarget[i].position = legPos[i];
        }
    }
}
