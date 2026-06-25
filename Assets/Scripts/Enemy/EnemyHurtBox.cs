using UnityEngine;
using UnityEngine.SceneManagement;

public class EnemyHurtBox : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            EventBus.Publish(new OnPlayerDeath());
            Debug.Log("Player hit!");
        }
    }
}