using Platformer;
using UnityEngine;

public class MusicChangeTrigger : MonoBehaviour
{
    [Header("Area")]
    [SerializeField] private MusicArea area;

    private void OnTriggerEnter(Collider collider)
    {
        if(collider.tag.Equals("Player"))
        {
            AudioManager.instance.SetMusicArea(area);
        }
    }
}