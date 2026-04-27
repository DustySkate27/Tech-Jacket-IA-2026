using UnityEngine;
using UnityEngine.SceneManagement;

public class EnemyHurtBox : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            SceneManager.LoadScene("TestRoulette");
            Debug.Log("Player hit!");
        }
    }
}