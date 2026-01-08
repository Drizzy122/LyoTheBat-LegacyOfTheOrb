using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Platformer
{
    public class HealthCollectible : MonoBehaviour
    {
        [SerializeField] private float healthValue;

        private void OnTriggerEnter(Collider collision)
        {
            if (collision.tag == "Player")
            {
                collision.GetComponent<PlayerHealth>().AddHealth(healthValue);
                gameObject.SetActive(false);
            }
        }
    }
}