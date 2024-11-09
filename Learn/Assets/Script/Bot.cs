using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bot : MonoBehaviour
{
    public void Awake()
    {
        EventListenerManager.RegisterListener(this);
    }

    [EventListener(typeof(Bot),EventName.BotTakeDamge)]
    public string BotTakeDamge(int damage)
    {
        int actualDamage = (int)damage;
        Debug.Log($"Bot took {actualDamage} damage!");
        return actualDamage.ToString();
    }
    [EventListener(typeof(Bot),EventName.BotTakeDamgeAction)]
    public void BotTakeDamgeAction(float damage)
    {
        int actualDamage = (int)damage;
        Debug.Log($"Bot took aaa{actualDamage} damage!");
    }
    [EventListener(typeof(Bot),EventName.BotTakeDamgeAction)]
    public void BotTakeDamgeAction(int damage,int dame)
    {
        int actualDamage = (int)damage;
        Debug.Log($"Bot took {actualDamage} damage!");
    }

}
