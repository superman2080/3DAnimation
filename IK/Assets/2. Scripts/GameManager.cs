using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{

    public PlayerCtrl player;
    public SpiderAnimator boss;
    public float battleChangeTime = 3f;
    public Light mainLight;
    private bool isStartBattle = false;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if(player.nowWeapon != null && isStartBattle == false)
        {
            StartCoroutine(StartBossBattle());
        }
    }


    private IEnumerator StartBossBattle()
    {
        float dT = 0;
        StartCoroutine(Util.CameraShakeCor(0.3f, battleChangeTime, true));
        while (dT < battleChangeTime)
        {
            dT += Time.deltaTime;
            yield return null;

            mainLight.gameObject.transform.eulerAngles = Vector3.Lerp(new Vector3(45, -30, 0), new Vector3(-15, -30, 0), dT / battleChangeTime);
        }
        StartCoroutine(Util.CameraShakeCor(0.3f, 0.3f, false));
        boss.gameObject.SetActive(true);
        isStartBattle = true;
    }
}
