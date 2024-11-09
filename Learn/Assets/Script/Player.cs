using UnityEngine;

public class Player : MonoBehaviour
{
     private void Start()
     {
             EventListenerManager.RegisterListener(this);
             EventListenerManager.Trigger((typeof(Bot),EventName.BotTakeDamge), 10);
             var x= EventListenerManager.Trigger<string>((typeof(Bot),EventName.BotTakeDamgeAction), (float)10);
             Debug.LogError(x);
     }
}