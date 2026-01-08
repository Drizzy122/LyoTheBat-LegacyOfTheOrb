using System;
using UnityEngine;
using UnityEngine.UI;


namespace Platformer
{
    public class TutorialTextDisplay : MonoBehaviour
    {
        public bool collided = false;
        public GameObject canvasImage;

        private void Awake()
        {
            canvasImage.SetActive(false);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                if (!collided)
                {
                    collided = true;
                    canvasImage.SetActive(true);
                    
                }
            }
        }
        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                Destroy(gameObject, 5f);
                canvasImage.SetActive(false);

            }
        }
    }
}