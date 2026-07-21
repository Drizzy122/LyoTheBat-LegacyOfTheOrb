using UnityEngine;
using UnityEngine.Rendering.Universal;
using Platformer;

[CreateAssetMenu(fileName = "SonarPulse", menuName = "Abilities/SonarPulseStrategy")]
public class SonarPulseStrategy : AbilityStrategy
{
    #region Variables
    
    [field: Header("SonarPulse Settings")] 
    [SerializeField] LayerMask detectionLayer;
    [SerializeField] float detectionRadius = 30f; 
    [SerializeField] float detectionDuration = 3f;
        
    [field: Header("VFX Settings")]
    public GameObject terrainScanPrefab;
    [SerializeField] ScriptableRendererFeature xRayRenderFeature;
    #endregion
    
    public override void Execute(GameObject player)
    {
        // Particle System for terrain scan
        if (terrainScanPrefab != null)
        {
            GameObject vfx = Instantiate(terrainScanPrefab, player.transform.position, Quaternion.identity, player.transform);
            Destroy(vfx, 3f); // Clean it up after 3 seconds
        }

        // Screen dims for the scan window, then eases back on its own
        SonarVisionEffect.Instance?.Play(detectionDuration);

        // 2 Highlight Objects

        Collider[] detectedObjects = Physics.OverlapSphere(player.transform.position, detectionRadius, detectionLayer);
        bool foundSomething = false;
        foreach (Collider collider in detectedObjects)
        {
            if (collider.CompareTag("Enemy") || collider.CompareTag("Collectible"))
            {
                foundSomething = true;
                break; 
            }
        }


        if (foundSomething)
        {
            PlayerAbilities abilities = player.GetComponent<PlayerAbilities>();
            if (abilities != null)
            {
                abilities.TriggerXRay(xRayRenderFeature, detectionDuration);
            }
        }
    }
}