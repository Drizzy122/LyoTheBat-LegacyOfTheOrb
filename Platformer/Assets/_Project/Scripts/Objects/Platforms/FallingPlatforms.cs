using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Platformer
{
    public class FallingPlatforms : MonoBehaviour
    {
        [SerializeField] private float _timeToWaitBeforeFall;

        private Rigidbody _rigidbody;

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.GetComponent<PlayerController>() == false)
            {
                return;
            }

            if (collision.GetContact(0).normal.y >= 0)
            {
                return;
            }

            StopAllCoroutines();
            StartCoroutine(PlatformFallCoroutine());
        }

        private IEnumerator PlatformFallCoroutine()
        {
            yield return new WaitForSeconds(_timeToWaitBeforeFall);
            _rigidbody.isKinematic = false;
        }
    }
}