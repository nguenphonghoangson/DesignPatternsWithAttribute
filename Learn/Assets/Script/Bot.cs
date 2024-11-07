using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bot : MonoBehaviour
{
    public void Awake()
    {
        this.RegisterEvent();
    }

    [Event(nameof(BotTakeDamge))]
    public void BotTakeDamge(object damage)
    {
        int actualDamage = (int)damage;
        Debug.Log($"Bot took {actualDamage} damage!");
    }

}
