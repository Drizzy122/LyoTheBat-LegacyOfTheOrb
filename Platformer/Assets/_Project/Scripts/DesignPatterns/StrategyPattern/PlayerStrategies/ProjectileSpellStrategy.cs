using Platformer;
using UnityEngine;


[CreateAssetMenu(fileName = "ProjectileSpellStrategy", menuName ="spells/ProjectileSpawnerStrategy")]
public class ProjectileSpellStrategy : SpellStragedy
{
    public GameObject projectilePrefab;
    public float speed = 10f;
    public float duration = 10f;

    [Header("Multi-shot")]
    [Min(1)] public int baseProjectileCount = 1;
    [Tooltip("Degrees between projectiles when firing more than one.")]
    public float spreadAngle = 10f;

    public override void CastSpell(Transform origin, Transform caster = null)
    {
        if (projectilePrefab == null) return;

        // Ability tree bonus: Power branch adds extra projectiles per cast.
        int count = baseProjectileCount;
        if (caster != null)
        {
            var tree = caster.GetComponent<AbilityTree>();
            if (tree != null)
                count += Mathf.RoundToInt(tree.GetStat(AbilityTree.StatBlastProjectiles));
        }
        count = Mathf.Max(1, count);

        // Fan the shots out evenly around the forward direction.
        float startYaw = -(count - 1) * 0.5f * spreadAngle;
        for (int i = 0; i < count; i++)
        {
            var rotation = origin.rotation * Quaternion.Euler(0f, startYaw + i * spreadAngle, 0f);
            new ProjectileBuilder()
                .WithProjectilePrefab(projectilePrefab)
                .WithSpeed(speed)
                .WithDuration(duration)
                .WithCaster(caster)
                .Build(origin.position, rotation);
        }
    }
}
