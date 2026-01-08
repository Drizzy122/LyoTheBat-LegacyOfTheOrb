using UnityEngine;

namespace Platformer
{
    public class TextInteractable : MonoBehaviour
    {
        public new GameObject gameObject;

        private void Awake()
        {
            gameObject.SetActive(false);
        }
        

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                gameObject.SetActive(true);
                
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                gameObject.SetActive(false);
            }
        }
    }
}