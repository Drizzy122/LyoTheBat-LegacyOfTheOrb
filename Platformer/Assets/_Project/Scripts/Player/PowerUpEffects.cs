using System;
using Platformer;
using UnityEngine;
using UnityEngine.VFX;

public class PowerUpEffects : MonoBehaviour
{
   public VisualEffect lightningVfx;
   private LuminOrbs luminOrbs;
   bool isCollected = false;

   public void Awake()
   {
      lightningVfx.Stop();
   }

   public void Update()
   {
       EnableLightning();
   }

   public void EnableLightning()
    {
        if (!isCollected)
        {
            isCollected = true;
            GameEventsManager.instance.miscEvents.LuminCollected();
            lightningVfx.Play();
        }
        else
        {
            DisableLightning();
        }
    }

    public void DisableLightning()
    {
        lightningVfx.Stop();
    }
   
}