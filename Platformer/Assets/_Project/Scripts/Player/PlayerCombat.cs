using System.Collections;
using UnityEngine;
using DG.Tweening;
using KBCore.Refs;
using UnityEngine.VFX;
using Unity.Cinemachine;

namespace Platformer
{
    [System.Serializable]
    public class ComboSlash 
    {
        public GameObject slashObj;
        public float activeDuration = 1f; // How long the slash stays visible
    }

    public class PlayerCombat : MonoBehaviour
    {
        [field: Header("References")]
        [field: SerializeField, Anywhere] CinemachineImpulseSource impulseSource;
        [field: SerializeField] public EnemyDetection enemyDetection; // Drag the new script here!
       
        [field: Header("Attack Settings")]
        [field: SerializeField] [Range(0, 10)] float lightAttackDistance = 1f;
        [field: SerializeField] int lightAttackDamage = 10;
        [field: SerializeField] [Range(0, 10)] float heavyAttackDistance = 5f;
        [field: SerializeField] int heavyAttackDamage = 20;
        [field: SerializeField] float knockbackTime = 0.5f;
        
        [field: Header("Freeflow Settings")]
        [field: SerializeField] [Range(0, 20)] float targetingRadius = 8f; 
        [field: SerializeField] float slideDuration = 0.2f;
        
        [field: Header("Camera Shake Settings")]
        [field: SerializeField] float minShakeForce = 0.5f; 
        [field: SerializeField] float shakeDistanceMultiplier = 0.2f;
        
        [SerializeField] private GameObject lastHitCamera;
        [SerializeField] private Transform lastHitFocusObject;
        
        [field: Header("VFX Settings")]
        [field: SerializeField] VisualEffect slashVFX;
        [field: SerializeField] ComboSlash[] comboSlashes;
        
        
        
        
        [field: Header("Combo Settings")]
        [field: SerializeField] string[] comboAnimations = { "Attack1", "Attack2", "Attack3" };
        
        private float comboResetWindow = 1.5f;
        private int comboCounter = -1;
        private float lastAttackTime;

        void Awake()
        {
            if (slashVFX != null)
            {
                slashVFX.Stop();
            }
        }

        void Start()
        {
            impulseSource = GetComponentInChildren<CinemachineImpulseSource>();
        }
        
        public void PlaySlashVFX()
        {
            // Make sure we have a valid combo counter and an assigned slash
            if (comboCounter >= 0 && comboCounter < comboSlashes.Length)
            {
                GameObject currentSlash = comboSlashes[comboCounter].slashObj;
                float duration = comboSlashes[comboCounter].activeDuration;

                if (currentSlash != null)
                {
                    // Turn the slash on
                    currentSlash.SetActive(true);

                    // Since you already use DOTween, we can use it instead of a Coroutine to turn it off!
                    DOVirtual.DelayedCall(duration, () => 
                    {
                        currentSlash.SetActive(false);
                    });
                }
            }
        }
        
        private void TriggerCameraShake(float distance)
        {
            if (impulseSource != null)
            {
                // Now it uses your Inspector variables instead of Magic Numbers!
                float shakeForce = Mathf.Max(minShakeForce, shakeDistanceMultiplier * distance);
                
                impulseSource.GenerateImpulseWithForce(shakeForce);
            }
        }
       

        public void LightAttack(Vector3 inputDirection)
        {
            float lungeDistance = 0f;
            Transform target = enemyDetection.CurrentTarget();
            AudioManager.instance.PlayOneShot(FMODEvents.instance.playerAttack, this.transform.position);
            if (target != null)
            {
                lungeDistance = Vector3.Distance(transform.position, target.position);
                
                Vector3 directionToTarget = (target.position - transform.position).normalized;
                directionToTarget.y = 0; 

                Vector3 stopPosition = target.position - (directionToTarget * 1.2f);

                transform.DOLookAt(target.position, 0.1f, AxisConstraint.Y);
                transform.DOMove(stopPosition, slideDuration).SetEase(Ease.OutQuad);
            }
            else if (inputDirection != Vector3.zero)
            {
                transform.rotation = Quaternion.LookRotation(inputDirection);
            }

            // Trigger the camera shake at the exact same time the damage is dealt!
            DOVirtual.DelayedCall(slideDuration, () => 
            {
                DealDamageInFront();
                TriggerCameraShake(lungeDistance); 
            });
            
            // Only trigger Cinematic Camera if they are the last enemy or have low HP!
            
            
            
        }
        
       
        
        
        private void DealDamageInFront()
        {
            Vector3 attackPos = transform.position + transform.forward * lightAttackDistance;
            Collider[] hitEnemies = Physics.OverlapSphere(attackPos, lightAttackDistance);
            
            
            foreach (var hit in hitEnemies)
            {
                if (hit.CompareTag("Enemy"))
                {
                    if(hit.TryGetComponent<Health>(out Health enemyHealth))
                    {
                        enemyHealth.TakeDamage(lightAttackDamage, knockbackTime);
                        // THE NEW SPOT FOR THE FINAL BLOW CAMERA
                        if (EnemyManager.instance != null && EnemyManager.instance.AliveEnemyCount() <= 1)
                        {
                            TriggerFinalBlowCamera(hit.transform);
                        }
                    }
                }
                else if (hit.CompareTag("Destructable"))
                {
                    if (hit.TryGetComponent<FractureObject>(out FractureObject fractureObject))
                    {
                        fractureObject.Explode();
                    }
                }
            }
        }
        
        public void HeavyAttack()
        {
            Vector3 attackPos = transform.position;
            Collider[] hitEnemies = Physics.OverlapSphere(attackPos, heavyAttackDistance);
            AudioManager.instance.PlayOneShot(FMODEvents.instance.playerAttack, transform.position);

            foreach (var enemy in hitEnemies)
            {
                if (enemy.CompareTag("Enemy"))
                {
                    enemy.GetComponent<Health>().TakeDamage(heavyAttackDamage, knockbackTime);
                }
            }
        }
        
        public string GetNextComboAnimation()
        {
            if (Time.time - lastAttackTime > comboResetWindow)
            {
                comboCounter = -1;
            }
            
            lastAttackTime = Time.time;
            comboCounter = (int)Mathf.Repeat(comboCounter + 1, comboAnimations.Length);
            return comboAnimations[comboCounter];
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position + transform.forward * lightAttackDistance, lightAttackDistance);

            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, heavyAttackDistance);

            Gizmos.color = Color.pink;
            Gizmos.DrawWireSphere(transform.position, targetingRadius);
        }
        
       
        public void CounterCheck()
        {
            Enemy bestCounterTarget = null;
            float closestDistance = float.MaxValue;

            // 1. Efficient Scan: Check the Manager's list of active fighters
            if (EnemyManager.instance != null)
            {
                foreach (Enemy enemy in EnemyManager.instance.engagedEnemies)
                {
                    // Check if they are preparing to lunge AND within your targeting radius
                    if (enemy.isPreparingAttack)
                    {
                        float distance = Vector3.Distance(transform.position, enemy.transform.position);
                        if (distance < targetingRadius && distance < closestDistance)
                        {
                            closestDistance = distance;
                            bestCounterTarget = enemy;
                        }
                    }
                }
            }

            if (bestCounterTarget != null)
            {
                // 2. STOP THE ENEMY: Call the new CancelAttack() we wrote in Enemy.cs
                bestCounterTarget.CancelAttack();

                // 3. SNAPPY MOVEMENT: Spin and snap to the enemy position
                Vector3 dirToTarget = (bestCounterTarget.transform.position - transform.position).normalized;
                dirToTarget.y = 0;
                Vector3 stopPosition = bestCounterTarget.transform.position - (dirToTarget * 1.5f);

                transform.DOLookAt(bestCounterTarget.transform.position, 0.1f, AxisConstraint.Y);
                transform.DOMove(stopPosition, slideDuration).SetEase(Ease.OutQuad);

                // 4. FEEDBACK: Sound and Camera Effects
                AudioManager.instance.PlayOneShot(FMODEvents.instance.playerAttack, transform.position);
        
                if (bestCounterTarget.enemyHealth != null)
                {
                    // Deal heavy damage and trigger that beautiful Final Blow camera
                    bestCounterTarget.enemyHealth.TakeDamage(lightAttackDamage * 2, knockbackTime); 
                    TriggerCameraShake(10f);
                    
                    
                }
            }
        }
        
        private IEnumerator FinalBlowCoroutine(Transform target)
        {
            // 1. Enter Slow Motion
            Time.timeScale = 0.5f; 
            
            // 2. Turn on the cinematic camera
            if (lastHitCamera != null) lastHitCamera.SetActive(true);
            
            // 3. Move the focus object to the enemy being hit
            if (lastHitFocusObject != null && target != null) 
            {
                lastHitFocusObject.position = target.position;
            }

            // 4. Wait for 2 real-world seconds (ignoring the slow motion)
            yield return new WaitForSecondsRealtime(2f);

            // 5. Turn the camera off and restore normal time
            if (lastHitCamera != null) lastHitCamera.SetActive(false);
            Time.timeScale = 1f; 
        }
        
        public void TriggerFinalBlowCamera(Transform target)
        {
            StartCoroutine(FinalBlowCoroutine(target));
        }
    }
}