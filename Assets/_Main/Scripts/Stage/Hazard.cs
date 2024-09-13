using UnityEngine;
using WhereIsMyWife.Controllers;
using Zenject;

public class Hazard : MonoBehaviour
{
    [Inject] IRespawn _respawn;
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            _respawn.TriggerRespawn();
        }
    }
}
