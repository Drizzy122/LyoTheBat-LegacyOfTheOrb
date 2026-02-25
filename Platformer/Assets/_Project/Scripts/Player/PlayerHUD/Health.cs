using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using KBCore.Refs;
using Unity.Cinemachine;
using UnityEngine.VFX;
using System;

namespace Platformer
{
    public enum EntityHealth
    {
        Player,
        Enemy
    }
    public class Health : ValidatedMonoBehaviour, IDamageable
    {
        [field: Header("Configuration")]
        [field: SerializeField] EntityHealth entityHealth;
        [field: SerializeField, Anywhere] FloatEventChannel healthEventChannel;
        
        [field: Header("Health")] 
        [field: SerializeField] float maxHealth = 100f;
        
        [field: Header("Player Settings")]
        [field: SerializeField, Anywhere] CinemachineOrbitalFollow freeLookCam;
        [field: SerializeField, Anywhere] Renderer playerMeshRenderer;
        [field: SerializeField] public float iFramesDuration { get; private set; } = 2f;
        [field: SerializeField] int numberOfFlashes = 10;
        
        [field: Header("Enemy Settings")]
        [field: SerializeField] SkinnedMeshRenderer enemySkinnedMesh;
        [field: SerializeField] VisualEffect VFXGraph;
        [field: SerializeField] float dissolveRate = 0.0125f;
        [field: SerializeField] float refreshRate = 0.025f;
        
        [field: Header("State")]
        public float currentHealth { get; private set; }
        public bool isDead { get; private set; }
        public bool IsInvulnerable { get; private set; }

        private Material[] skinnedMaterials;
        public event Action<float> OnHit; // For Enemy Knockback

        private void Awake()
        {
            currentHealth = maxHealth;
            
            if (entityHealth == EntityHealth.Player)
            {
                if (playerMeshRenderer == null) 
                    playerMeshRenderer = GetComponentInChildren<Renderer>();
            }
            else if (entityHealth == EntityHealth.Enemy)
            {
                if (enemySkinnedMesh != null)
                    skinnedMaterials = enemySkinnedMesh.materials;
                
                if (VFXGraph != null) 
                    VFXGraph.Stop();
            }
        }

        private void Start() => PublishHealthPercentage();
        
        public void AddHealth(float value)
        {
            currentHealth = Mathf.Clamp(currentHealth + value, 0, maxHealth);
            PublishHealthPercentage();
        }
        
        private void PublishHealthPercentage()
        {
            if (healthEventChannel != null)
                healthEventChannel.Invoke(currentHealth / maxHealth);
        }
        
        public void TakeDamage(float damage, float knockBackTime = 0f)
        {
            currentHealth = Mathf.Clamp(currentHealth - damage, 0, maxHealth);
            if (currentHealth > 0)
            {
                HandleDamage(knockBackTime);
            }
            else
            {
                HandleDeath();
            }
            PublishHealthPercentage();
        }
        private void HandleDamage(float knockBackTime)
        {
            switch (entityHealth)
            {
                case EntityHealth.Player:
                    OnHit?.Invoke(knockBackTime);
                    AudioManager.instance.PlayOneShot(FMODEvents.instance.playerHurt, transform.position);
                    StartCoroutine(Invunerability());
                    break;

                case EntityHealth.Enemy:
                    OnHit?.Invoke(knockBackTime);
                    AudioManager.instance.PlayOneShot(FMODEvents.instance.enemyHurt, transform.position);
                    break;
            }
        }

        public void HandleDeath()
        {
            if (!isDead)
            {
                isDead = true;
                switch (entityHealth)
                {
                    case EntityHealth.Player:
                        AudioManager.instance.PlayOneShot(FMODEvents.instance.playerDeath, transform.position);
                        AudioManager.instance.StopMusic();
                        if (freeLookCam != null) freeLookCam.gameObject.SetActive(false);
                        Invoke(nameof(RestartScene), 5f);
                        break;

                    case EntityHealth.Enemy:
                        AudioManager.instance.PlayOneShot(FMODEvents.instance.enemyDeath, transform.position);
                        GameEventsManager.instance.enemyEvents.EnemyDeath();
                        StartCoroutine(DissolveCo());
                        break;
                }
            }
        }
        private IEnumerator Invunerability()
        {
            IsInvulnerable = true;
            Physics.IgnoreLayerCollision(6, 7, true); // Adjust layer numbers as needed
            for (int i = 0; i < numberOfFlashes; i++)
            {
                playerMeshRenderer.material.color = new Color(1, 0, 0, 0.1f);
                yield return new WaitForSeconds(iFramesDuration / (numberOfFlashes * 2));
                playerMeshRenderer.material.color = Color.white;
                yield return new WaitForSeconds(iFramesDuration / (numberOfFlashes * 2));
            }

            Physics.IgnoreLayerCollision(6, 7, false);
            IsInvulnerable = false;
        }
        private void RestartScene() => SceneManager.LoadScene("PlayGround");
        
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