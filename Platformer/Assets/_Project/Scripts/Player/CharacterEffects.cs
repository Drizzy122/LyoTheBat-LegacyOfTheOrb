using UnityEngine;
using UnityEngine.VFX;
public class CharacterEffects : MonoBehaviour
{
    public VisualEffect bodyEffects;

    void Start()
    {
        if (bodyEffects == null) bodyEffects = GetComponentInChildren<VisualEffect>();
        
        // Stop sends the 'OnStop' event
        bodyEffects.Stop(); 
        
        // This physically pauses the simulation to be 100% sure
        bodyEffects.pause = true; 
    }
    
    public void PlayEffect()
    {
        bodyEffects.pause = false;
        bodyEffects.Play();
    }
}