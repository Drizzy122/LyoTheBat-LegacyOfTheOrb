using System;
using UnityEngine;
using UnityEngine.Events;

namespace Platformer
{
    public abstract class EventListener<T> : MonoBehaviour
    {
        [field: SerializeField] EventChannel<T> eventChannel;
        [field: SerializeField] UnityEvent<T> unityEvent;

        protected void Awake()
        {
            eventChannel.Register(this);
        }

        protected void OnDestroy()
        {
            eventChannel.Deregister(this);
        }
        
        public void Raise(T value)
        {
            unityEvent?.Invoke(value);
        }
    }
    
    public class EventListener : EventListener<Empty> { }
}