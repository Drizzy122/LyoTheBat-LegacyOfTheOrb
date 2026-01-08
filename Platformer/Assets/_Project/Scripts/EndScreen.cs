using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement; // Required to use SceneManager

public class EndScreen : MonoBehaviour
{
    // Name of the scene to load
    [SerializeField] private string sceneName;

    // This method is called when another collider enters the trigger
    private void OnTriggerEnter(Collider other)
    {
        // Check if the object entering is the player (check tag or any other identifier)
        if (other.CompareTag("Player"))
        {
            // Load the specified scene
            SceneManager.LoadScene(sceneName);
        }
    }
}