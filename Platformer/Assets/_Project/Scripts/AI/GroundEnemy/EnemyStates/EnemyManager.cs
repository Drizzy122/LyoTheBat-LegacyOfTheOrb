using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Platformer 
{
    public class EnemyManager : MonoBehaviour
    {
        public static EnemyManager instance;
        
        [Header("Combat Settings")]
        [SerializeField] float minTimeBetweenAttacks = 0.5f;
        [SerializeField] float maxTimeBetweenAttacks = 1.5f;
        
        public List<Enemy> engagedEnemies = new List<Enemy>();
        private Coroutine combatLoop;
        
        // Tracks who just attacked so we don't pick them again immediately
        private Enemy lastAttacker; 

        private void Awake() 
        {
            if (instance == null) instance = this;
            else Destroy(gameObject);
        }

        public void RegisterEnemy(Enemy enemy) 
        {
            if (!engagedEnemies.Contains(enemy)) 
            {
                engagedEnemies.Add(enemy);
                if (combatLoop == null) 
                {
                    combatLoop = StartCoroutine(AI_Loop());
                }
            }
        }

        public void UnregisterEnemy(Enemy enemy) 
        {
            if (engagedEnemies.Contains(enemy)) 
            {
                engagedEnemies.Remove(enemy);
            }
            
            // If the guy who just attacked dies, clear the record
            if (lastAttacker == enemy) lastAttacker = null;
        }

        // The method we added earlier for the Final Blow Camera!
        public int AliveEnemyCount()
        {
            return engagedEnemies.Count;
        }

        // Mix and Jam's random enemy picker!
        public Enemy RandomEnemy()
        {
            if (engagedEnemies.Count == 0) return null;
            return engagedEnemies[Random.Range(0, engagedEnemies.Count)];
        }

        IEnumerator AI_Loop() 
        {
            while (engagedEnemies.Count > 0) 
            {
                // 1. Organic delay between attacks
                yield return new WaitForSeconds(Random.Range(minTimeBetweenAttacks, maxTimeBetweenAttacks));
                
                // 2. Filter for ready enemies
                List<Enemy> readyEnemies = new List<Enemy>();
                foreach (var enemy in engagedEnemies)
                {
                    // Skip enemies whose personal attack timer is still cooling down —
                    // their Attack() would silently deal zero damage
                    if (enemy.isWaitingForTurn && !enemy.IsAttackOnCooldown)
                    {
                        // Add them to the pool IF they didn't just attack, OR if they are the only one left
                        if (enemy != lastAttacker || engagedEnemies.Count == 1)
                        {
                            readyEnemies.Add(enemy);
                        }
                    }
                }

                // 3. Pick the attacker
                if (readyEnemies.Count > 0)
                {
                    Enemy chosenEnemy = readyEnemies[Random.Range(0, readyEnemies.Count)];

                    lastAttacker = chosenEnemy;
                    chosenEnemy.GiveAttackOrder();

                    // 4. Mix & Jam Secret Sauce: PAUSE the manager until this enemy finishes their attack!
                    // Timeout guard: if the attacker gets kited, stunned, or stuck, release the
                    // token after a few seconds instead of freezing the whole fight forever.
                    float deadline = Time.time + 5f;
                    yield return new WaitUntil(() => chosenEnemy == null || chosenEnemy.isWaitingForTurn || engagedEnemies.Count == 0 || Time.time > deadline);

                    if (chosenEnemy != null && Time.time > deadline && !chosenEnemy.isWaitingForTurn)
                    {
                        chosenEnemy.AbortAttack();
                    }
                }
            }
            combatLoop = null; 
        }
    }
}