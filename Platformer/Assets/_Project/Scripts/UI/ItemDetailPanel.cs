using UnityEngine;
using UnityEngine.UIElements;

namespace Platformer
{
    /// <summary>
    /// Helper that fills a detail-panel VisualElement with an item's info:
    /// rarity-tinted header band, name, description, and Destiny-style stat bars.
    /// Used by both InventoryUIController and CharacterUIController.
    /// </summary>
    public static class ItemDetailPanel
    {
        public static void Show(VisualElement panel, InventoryItem item)
        {
            if (panel == null || item?.data == null) return;

            string rarityClass = item.data.rarity.ToString().ToLower();

            // Rarity band: stripe on the panel edge + tinted header background.
            foreach (Rarity r in System.Enum.GetValues(typeof(Rarity)))
                panel.RemoveFromClassList($"detail-panel--{r.ToString().ToLower()}");
            panel.AddToClassList($"detail-panel--{rarityClass}");

            // Icon
            var icon = panel.Q<VisualElement>("detail-icon");
            if (icon != null)
            {
                icon.style.backgroundImage = item.data.icon != null
                    ? new StyleBackground(item.data.icon)
                    : new StyleBackground();
            }

            // Name
            var nameLabel = panel.Q<Label>("detail-name");
            if (nameLabel != null) nameLabel.text = item.data.displayName;

            // Rarity label + colored class
            var rarityLabel = panel.Q<Label>("detail-rarity");
            if (rarityLabel != null)
            {
                rarityLabel.text = item.data.rarity.ToString().ToUpper();
                foreach (Rarity r in System.Enum.GetValues(typeof(Rarity)))
                    rarityLabel.RemoveFromClassList($"detail-rarity--{r.ToString().ToLower()}");
                rarityLabel.AddToClassList($"detail-rarity--{rarityClass}");
            }

            // Description
            var descLabel = panel.Q<Label>("detail-description");
            if (descLabel != null) descLabel.text = item.data.description ?? "";

            // Stats — varies by item type
            var stats = panel.Q<VisualElement>("detail-stats");
            if (stats != null)
            {
                stats.Clear();
                FillStats(stats, item.data);
            }

            panel.style.display = DisplayStyle.Flex;
        }

        public static void Hide(VisualElement panel)
        {
            if (panel != null) panel.style.display = DisplayStyle.None;
        }

        // ─── per-type stat rendering ───────────────────────────────────────────

        // Bar normalization maxes — tune as item power grows.
        const float MaxDamage = 50f;
        const float MaxAttackSpeed = 3f;
        const float MaxRange = 5f;
        const float MaxKnockback = 3f;
        const float MaxDefense = 50f;
        const float MaxHealthBonus = 50f;
        const float MaxMobilityBonus = 20f;
        const float MaxHeal = 100f;

        static void FillStats(VisualElement stats, ItemData data)
        {
            switch (data)
            {
                case WeaponData w:
                    AddStatBar(stats, "DAMAGE",       w.damage,      MaxDamage,      w.damage.ToString());
                    AddStatBar(stats, "ATTACK SPEED", w.attackSpeed, MaxAttackSpeed, w.attackSpeed.ToString("0.0"));
                    AddStatBar(stats, "RANGE",        w.range,       MaxRange,       w.range.ToString("0.0"));
                    AddStatBar(stats, "KNOCKBACK",    w.knockback,   MaxKnockback,   w.knockback.ToString("0.0"));
                    break;

                case ArmorData a:
                    AddStatRow(stats, "SLOT", a.slot.ToString().ToUpper());
                    AddStatBar(stats, "DEFENSE", a.defense, MaxDefense, a.defense.ToString());
                    if (a.healthBonus   != 0) AddStatBar(stats, "HEALTH",   a.healthBonus,   MaxHealthBonus,   $"+{a.healthBonus}");
                    if (a.mobilityBonus != 0) AddStatBar(stats, "MOBILITY", a.mobilityBonus, MaxMobilityBonus, $"+{a.mobilityBonus}");
                    break;

                case ConsumableData c:
                    if (c.healAmount > 0) AddStatBar(stats, "HEAL", c.healAmount, MaxHeal, $"+{c.healAmount}");
                    break;
            }
        }

        /// <summary>Label + filled bar + value, Destiny-style.</summary>
        static void AddStatBar(VisualElement parent, string label, float value, float max, string valueText)
        {
            var row = new VisualElement();
            row.AddToClassList("detail-stat-row");

            var l = new Label(label);
            l.AddToClassList("detail-stat-label");
            row.Add(l);

            var track = new VisualElement();
            track.AddToClassList("stat-bar-track");
            var fill = new VisualElement();
            fill.AddToClassList("stat-bar-fill");
            fill.style.width = Length.Percent(Mathf.Clamp01(max > 0f ? value / max : 0f) * 100f);
            track.Add(fill);
            row.Add(track);

            var v = new Label(valueText);
            v.AddToClassList("detail-stat-value");
            row.Add(v);

            parent.Add(row);
        }

        /// <summary>Plain label/value row for non-numeric stats (e.g. armor slot).</summary>
        static void AddStatRow(VisualElement parent, string label, string value)
        {
            var row = new VisualElement();
            row.AddToClassList("detail-stat-row");

            var l = new Label(label);
            l.AddToClassList("detail-stat-label");
            row.Add(l);

            var spacer = new VisualElement();
            spacer.style.flexGrow = 1;
            row.Add(spacer);

            var v = new Label(value);
            v.AddToClassList("detail-stat-value");
            v.style.width = StyleKeyword.Auto;
            row.Add(v);

            parent.Add(row);
        }
    }
}
