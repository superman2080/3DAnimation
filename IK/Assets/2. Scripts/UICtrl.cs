using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UICtrl : MonoBehaviour
{
    public Slider hpBar;

    private PlayerCtrl player;
    // Start is called before the first frame update
    void Start()
    {
        player = FindObjectOfType<PlayerCtrl>();
    }

    // Update is called once per frame
    void Update()
    {
        hpBar.value = player.hp / player.maxHP;
    }
}
