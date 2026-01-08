using System;
using UnityEngine;

namespace Platformer
{
    public class WorldCanvas : MonoBehaviour
    {
        public Transform cam;

        private void LateUpdate()
        {
            transform.LookAt(transform.position + cam.forward);
        }
    }
}
