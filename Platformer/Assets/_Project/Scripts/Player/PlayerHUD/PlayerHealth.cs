using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using KBCore.Refs;
using Unity.Cinemachine;

namespace Platformer
{
    public class PlayerHealth : ValidatedMonoBehaviour
    {
        [field: Header("References")] 
        [field: SerializeField, Anywhere] CinemachineOrbitalFollow freeLookCam;
        [field: SerializeField, Anywhere] Renderer meshRenderer;
        [field: SerializeField, Anywhere] FloatEventChannel playerHealthChannel;
        
        [field: Header("Health")] 
        [field: SerializeField] float maxHealth;
        [field: SerializeField] public bool isDead;
        public float currentHealth { get; private set; }
        public bool IsInvulnerable { get; private set; }
        
        [Header("iFrames")] 
        [field: SerializeField] float iFramesDuration;
        [field: SerializeField] int numberOfFlashes;
        
        private void Awake()
        {
            currentHealth = maxHealth;
            meshRenderer = GetComponentInChildren<Renderer>();
        }

        public void Start()
        {
            PublishHealthPercentage();
        }

        public void AddHealth(float value)
        {
            currentHealth = Mathf.Clamp(currentHealth + value, 0, maxHealth);
            PublishHealthPercentage();
        }

        void PublishHealthPercentage()
        {
            if (playerHealthChannel != null)
            {
                playerHealthChannel.Invoke(currentHealth / (float) maxHealth); 
            }
        }
        public void TakeDamage(float damage)
        {
            currentHealth = Mathf.Clamp(currentHealth - damage, 0, maxHealth);
            if (currentHealth > 0)
            {
                HandleDamage();
                StartCoroutine(Invunerability());
            }
            else
            {
                HandleDeath();
            }
            PublishHealthPercentage();
        }
        public void HandleDamage() => AudioManager.instance.PlayOneShot(FMODEvents.instance.playerHurt, this.transform.position);

        public void HandleDeath()
        {
            if (!isDead)
            {
                isDead = true;
                AudioManager.instance.PlayOneShot(FMODEvents.instance.playerDeath, this.transform.position);
                AudioManager.instance.StopMusic();
                if (freeLookCam != null)
                {
                    freeLookCam.gameObject.SetActive(false);
                }

                Invoke(nameof(RestartScene), 5f); // Restart scene after  seconds
            }
        }
        private IEnumerator Invunerability()
        {
            IsInvulnerable = true;
            Physics.IgnoreLayerCollision(6, 7, true); // Adjust layer numbers as needed
            for (int i = 0; i < numberOfFlashes; i++)
            {
                meshRenderer.material.color = new Color(1, 0, 0, 0.1f);
                yield return new WaitForSeconds(iFramesDuration / (numberOfFlashes * 2));
                meshRenderer.material.color = Color.white;
                yield return new WaitForSeconds(iFramesDuration / (numberOfFlashes * 2));
            }

            Physics.IgnoreLayerCollision(6, 7, false);
            IsInvulnerable = false;
        }
        private void RestartScene() => SceneManager.LoadScene("Game");
    }
}