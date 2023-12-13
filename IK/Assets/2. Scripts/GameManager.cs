using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public Material normalSky;
    public Material nightSky;

    public Image fadeImage;
    public GameObject bossPanel;
    public Slider bossHPBar;

    public PlayerCtrl player;
    public SpiderAnimator boss;
    public float battleChangeTime = 3f;
    public Light mainLight;
    private bool isStartBattle = false;
    private bool isFinishedBattle = false;

    // Start is called before the first frame update
    void Start()
    {
        UnityEngine.RenderSettings.skybox = normalSky;
        bossPanel.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if(player.nowWeapon != null && isStartBattle == false)
        {
            StartCoroutine(StartBossBattle());
        }
        if(isStartBattle == true && boss != null)
        {
            bossHPBar.value = boss.hp / boss.maxHP;
        }
        if (boss == null && isFinishedBattle == false)
        {
            isFinishedBattle = true;
            fadeImage.gameObject.SetActive(true);
            StartCoroutine(FadeImage(fadeImage, Color.clear, Color.black, 1.5f));
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
        //
        fadeImage.gameObject.SetActive(true);
        bossPanel.SetActive(true);
        UnityEngine.RenderSettings.skybox = nightSky;
        yield return StartCoroutine(FadeImage(fadeImage, Color.black, Color.clear, 1f));
        //
        boss.gameObject.SetActive(true);
        fadeImage.gameObject.SetActive(false);
        StartCoroutine(Util.CameraShakeCor(0.3f, 0.3f, false));
        isStartBattle = true;
    }

    private IEnumerator FadeImage(Image image, Color origin, Color end, float time)
    {
        image.color = origin;
        float dT = 0;
        while(dT < time)
        {
            dT += Time.deltaTime;
            yield return null;
            image.color = Color.Lerp(origin, end, dT / time);
        }
        image.color = end;
    }
}
