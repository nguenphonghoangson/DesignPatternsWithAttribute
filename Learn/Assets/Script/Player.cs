using UnityEngine;

public class Player : MonoBehaviour
{
    private void Start()
    {
            SingletonManager.GetInstance<EventManager>().Trigger(nameof(Bot.BotTakeDamge), (object)10);
    }
}