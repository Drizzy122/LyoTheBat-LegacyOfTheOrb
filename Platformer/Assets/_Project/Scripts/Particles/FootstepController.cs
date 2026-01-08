using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Platformer
{
    public class FootstepController : MonoBehaviour
    {
        public event Action OnLeftFootDown;
        public event Action OnRightFootDown;
        public void LeftFootDown()
        {
            if (enabled)
            {
                OnLeftFootDown?.Invoke();
            }
        }
        public void RightFootDown()
        {
            if (enabled)
            {
                OnRightFootDown?.Invoke();
            }
        }
        public void BothFeetDown()
        {
            LeftFootDown();
            RightFootDown();
        }
    }
}