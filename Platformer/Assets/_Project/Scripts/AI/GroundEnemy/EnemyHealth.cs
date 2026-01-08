using System.Collections;
using UnityEngine;
using UnityEngine.VFX;
using KBCore.Refs;
using System;

namespace Platformer
{
    public class EnemyHealth : ValidatedMonoBehaviour
    {
        [field: Header("Health Settings")]
        [field: SerializeField] float maxHealth;
        [field: SerializeField, Anywhere] FloatEventChannel enemyHealthChannel;
        public float currentHealth { get; private set; }
        [field: SerializeField] public bool isDead;

        [field: Header("Dissolve Settings")]
        [field: SerializeField] SkinnedMeshRenderer skinnedMesh;
        [field: SerializeField] Material[] skinnedMaterials;
        [field: SerializeField] float dissolveRate = 0.0125f;
        [field: SerializeField] float refreshRate = 0.025f;
        [field: SerializeField] VisualEffect VFXGraph;
        
        public event Action<float> OnHit;
        
        public void Start() => PublishHealthPercentage();

        private void Awake()
        {
            if (skinnedMesh != null)
            {
                skinnedMaterials = skinnedMesh.materials;
                currentHealth = maxHealth;
            }
            VFXGraph.Stop();
        }
        
        void PublishHealthPercentage()
        {
            if (enemyHealthChannel != null)
            {
                enemyHealthChannel.Invoke(currentHealth / (float) maxHealth); 
            }
        }

        public void TakeDamage(float damage, float knockBackTime)
        {
            currentHealth = Mathf.Clamp(currentHealth - damage, 0, maxHealth);
            if (currentHealth > 0)
            {
                OnHit?.Invoke(knockBackTime);
                AudioManager.instance.PlayOneShot(FMODEvents.instance.enemyHurt, this.transform.position);
            }
            else
            {
                HandleDeath();
            }
            PublishHealthPercentage();
        }
        public void HandleDeath()
        {
            if (!isDead)
            {
                isDead = true;
                AudioManager.instance.PlayOneShot(FMODEvents.instance.enemyDeath, this.transform.position);
                GameEventsManager.instance.enemyEvents.EnemyDeath();
            }
            StartCoroutine(DissolveCo());
        }
        IEnumerator DissolveCo()
        {
            if (VFXGraph != null)
            {
                VFXGraph.Play();
            }
            if (skinnedMaterials.Length > 0)
            {
                float counter = 0;
                while (skinnedMaterials[0].GetFloat("_DissolveAmount") < 1)
                {
                    counter += dissolveRate;
                    for (int i = 0; i < skinnedMaterials.Length; i++)
                    {
                        skinnedMaterials[i].SetFloat("_DissolveAmount", counter);
                    }
                    yield return new WaitForSeconds(refreshRate);
                }
            }
        }
    }
}