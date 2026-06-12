using System;
using System.Collections;
using ImprovedTimers;
using UnityEngine;
using DG.Tweening;
using KBCore.Refs;
using UnityEngine.VFX;
using Unity.Cinemachine;
using UnityEngine.Serialization;

namespace Platformer
{
    [System.Serializable]
    public class ComboSlash
    {
        public GameObject slashObj;
        public float activeDuration = 1f;
    }

    public class PlayerCombat : MonoBehaviour, IEntity
    {
        [field: Header("References")]
        [field: SerializeField, Anywhere] CinemachineImpulseSource impulseSource;
        [field: SerializeField, Anywhere] InputReader input;
        [field: SerializeField] public EnemyDetection enemyDetection;



        [field: Header("Attack Settings")]
        [field: SerializeField] [Range(0, 10)] float lightAttackDistance = 1f;
        [field: SerializeField] int lightAttackDamage = 10;
        [field: SerializeField] int punchDamage = 5;
        [field: SerializeField] float knockbackTime = 0.5f;
        [field: SerializeField] float attackCoolDown = 0.5f;
        [field: SerializeField] float blastAttackCoolDown = 0.5f;

        private CountdownTimer attackTimer;
        private CountdownTimer blastAttackTimer;

        public bool IsAttacking => attackTimer != null && attackTimer.IsRunning;
        public bool IsBlastAttacking => blastAttackTimer != null && blastAttackTimer.IsRunning;

       
        [field: Header("Blast Settings")]
        [field: SerializeField] SpellStragedy blastStrategy;
        [field: SerializeField] Transform blastPoint;

        [field: Header("Freeflow Settings")]
        [field: SerializeField] [Range(0, 20)] float targetingRadius = 8f;
        [field: SerializeField] float slideDuration = 0.2f;

        [field: Header("Camera Shake Settings")]
        [field: SerializeField] float minShakeForce = 0.5f;
        [field: SerializeField] float shakeDistanceMultiplier = 0.2f;


        [field: Header("VFX Settings")]
        [field: SerializeField] VisualEffect slashVFX;
        [field: SerializeField] ComboSlash[] comboSlashes;

        private PlayerMovement playerMovement;
        private PlayerEquipment playerEquipment;

        // Derived from whatever weapon is currently IN HAND (active slot).
        public bool hasWeapon => playerEquipment != null && playerEquipment.ActiveWeapon != null;

        public int luminCharges { get; private set; }

        void OnEnable()
        {
            GameEventsManager.instance.miscEvents.onLuminCollected += OnLuminCollected;
            input.LightAttack += OnLightAttack;
            input.BlastAttack += OnBlastAttack;
            input.SwapWeapon += OnSwapWeapon;
        }

        void OnDisable()
        {
            GameEventsManager.instance.miscEvents.onLuminCollected -= OnLuminCollected;
            input.LightAttack -= OnLightAttack;
            input.BlastAttack -= OnBlastAttack;
            input.SwapWeapon -= OnSwapWeapon;
        }

        void OnSwapWeapon() => playerEquipment?.SwapActiveWeapon();

        void OnLuminCollected() => luminCharges += 5;

        void OnLightAttack()
        {
            if (!IsAttacking) attackTimer.Start();
        }

        void OnBlastAttack()
        {
            if (!IsBlastAttacking && luminCharges > 0) blastAttackTimer.Start();
        }

        public void CancelActions()
        {
            attackTimer?.Stop();
            blastAttackTimer?.Stop();
        }

        

      



        void Awake()
        {
            playerMovement = GetComponent<PlayerMovement>();
            playerEquipment = GetComponent<PlayerEquipment>();
            attackTimer = new CountdownTimer(attackCoolDown);
            blastAttackTimer = new CountdownTimer(blastAttackCoolDown);

            foreach (ComboSlash combo in comboSlashes)
            {

                if (combo != null && combo.slashObj != null)
                {
                    combo.slashObj.SetActive(false);
                }
            }
        }

        void Update()
        {
            HandleWeaponVisibility();

            if (enemyDetection != null && playerMovement != null)
            {
                enemyDetection.ScanForEnemies(playerMovement.GetAdjustedMovementDirection());
            }
        }

        private void HandleWeaponVisibility()
        {
            if (playerEquipment == null) return;

            bool shouldHide = playerMovement != null &&
                              (playerMovement.IsGliding || playerMovement.wallClimbimg || playerMovement.InWater);

            // Active weapon + shield both tuck away while gliding/climbing/swimming.
            var weaponVisual = playerEquipment.GetEquippedVisual(playerEquipment.ActiveWeaponSlot);
            if (weaponVisual != null) weaponVisual.SetActive(!shouldHide);

            var shieldVisual = playerEquipment.GetEquippedVisual(EquipSlot.Shield);
            if (shieldVisual != null) shieldVisual.SetActive(!shouldHide);
        }

        /// <summary>Damage to apply on a successful melee hit. Pulls from the ACTIVE weapon when present.</summary>
        int CurrentMeleeDamage()
        {
            if (!hasWeapon) return punchDamage;
            var weapon = playerEquipment.ActiveWeapon.data as WeaponData;
            return weapon != null ? weapon.damage : lightAttackDamage;
        }

        

        void Start()
        {
            impulseSource = GetComponentInChildren<CinemachineImpulseSource>();
        }
        public void PlaySlashVFX(int slashIndex)
        {
            if (slashIndex >= 0 && slashIndex < comboSlashes.Length)
            {
                GameObject currentSlash = comboSlashes[slashIndex].slashObj;
                float duration = comboSlashes[slashIndex].activeDuration;

                if (currentSlash != null)
                {
                    currentSlash.SetActive(false);
                    currentSlash.SetActive(true);
                    DOVirtual.DelayedCall(duration, () =>
                    {
                        if (currentSlash != null)
                        {
                            currentSlash.SetActive(false);
                        }
                    });
                }
            }
        }

        private void TriggerCameraShake(float distance)
        {
            if (impulseSource != null)
            {
                float shakeForce = Mathf.Max(minShakeForce, shakeDistanceMultiplier * distance);
                impulseSource.GenerateImpulseWithForce(shakeForce);
            }
        }


        public void lunge(Vector3 inputDirection)
        {
            float lungeDistance = 0f;
            Transform target = enemyDetection.CurrentTarget();
           
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

            DOVirtual.DelayedCall(slideDuration, () =>
            {
                LightAttack();
                TriggerCameraShake(lungeDistance);
            });
        }




        private void LightAttack()
        {
            Vector3 attackPos = transform.position + transform.forward * lightAttackDistance;
            Collider[] hitEnemies = Physics.OverlapSphere(attackPos, lightAttackDistance);
            AudioManager.instance.PlayOneShot(FMODEvents.instance.playerAttack, transform.position);

            int damage = CurrentMeleeDamage();
            foreach (var hit in hitEnemies)
            {
                if (hit.CompareTag("Enemy"))
                {
                    if(hit.TryGetComponent<Health>(out Health enemyHealth))
                    {
                        enemyHealth.TakeDamage(damage, knockbackTime);
                    }
                }
                else if (hit.CompareTag("Destructable"))
                {
                    if (hit.TryGetComponent<IDamageable>(out var damageable))
                        damageable.TakeDamage(damage, knockbackTime);
                }
            }
        }

        public void BlastAttack()
        {
            if (luminCharges <= 0) return;
            luminCharges--;

            AudioManager.instance.PlayOneShot(FMODEvents.instance.blastAttack, transform.position);
            blastStrategy?.CastSpell(blastPoint != null ? blastPoint : transform, transform);
        }

        void IEntity.Attack(Vector3 direction) => lunge(direction);
        void IEntity.Attack2() => BlastAttack();
        void IEntity.Attack3() => CounterCheck();


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


        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position + transform.forward * lightAttackDistance, lightAttackDistance);
           

            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(transform.position, targetingRadius);
        }
    }
}
