using UnityEngine;
using FMODUnity;

namespace Platformer
{
    public class FMODEvents : MonoBehaviour
    {
        public static FMODEvents instance { get; private set; }
        private void Awake()
        {
            if (instance != null)
            {
                Debug.LogError("Found more than one FMOD Events instance in the scene");   
            }
            instance = this;
        }
        
        [field: Header("Player SFX")]
        [field: SerializeField] public EventReference playerFootsteps { get; private set; }
        [field: SerializeField] public EventReference playerJump { get; private set; }
        [field: SerializeField] public EventReference playerAttack { get; private set; }
        [field: SerializeField] public EventReference playerHurt { get; private set; }
        [field: SerializeField] public EventReference playerDeath { get; private set; }
        [field: SerializeField] public EventReference playerEcolocation { get; private set; }
        
        [field:Header("Ground Enemy SFX")]
        
        [field: SerializeField] public EventReference enemyAttack { get; private set; }
        [field: SerializeField] public EventReference enemyHurt { get; private set; }
        [field: SerializeField] public EventReference enemyDeath { get; private set; }
        
        [field:Header("Flying Enemy SFX")]
     //   [field: SerializeField] public EventReference flyingEnemyWings { get; private set; }
     //   [field: SerializeField] public EventReference flyingEnemyAttack { get; private set; }
      //  [field: SerializeField] public EventReference flyingEnemyHurt { get; private set; }
     //   [field: SerializeField] public EventReference flyingEnemyDeath { get; private set; }
        
        [field: Header("Music")] 
        [field: SerializeField] public EventReference music { get; private set; }
        
        [field: Header("Ambience")]
        [field: SerializeField] public EventReference ambience { get; private set; }
      
        [field: Header("Collectable SFX")]
        [field: SerializeField] public EventReference coinCollected { get; private set;}
        [field: SerializeField] public EventReference luminCollected { get; private set;}
        
        [field: Header("Audio By Distance")]
        //[field: SerializeField] public EventReference ChestIdle { get; private set; }
        
        [field: Header(("Explosion"))]
        [field: SerializeField] public EventReference explode { get; private set; }
        
        [field: Header("UI")]
        [field: SerializeField] public EventReference uiopen { get; private set; }
        [field: SerializeField] public EventReference uiclose { get; private set; }
        [field: SerializeField] public EventReference ui { get; private set; }
        
    }
}