using System.Collections;
using System.Collections.Generic;
using Platformer;
using UnityEngine;

public class PlayerLevelManager : MonoBehaviour, IDataPersistence
{
    [Header("Configuration")]
    [SerializeField] private int startingLevel = 1;
    [SerializeField] private int startingExperience = 0;
    [SerializeField] private int maxLevel = 20;

    private int currentLevel;
    private int currentExperience;

    private void Awake()
    {
        currentLevel = startingLevel;
        currentExperience = startingExperience;
    }

    // ── IDataPersistence ── (load runs before Start, so the Start broadcast
    // below announces the restored level to the HUD/ability tree)
    public void LoadData(GameData data)
    {
        currentLevel = Mathf.Clamp(data.playerLevel, startingLevel, maxLevel);
        currentExperience = Mathf.Max(0, data.playerExperience);
    }

    public void SaveData(GameData data)
    {
        data.playerLevel = currentLevel;
        data.playerExperience = currentExperience;
    }

    private void OnEnable()
    {
        GameEventsManager.instance.playerEvents.onExperienceGained += ExperienceGained;
    }

    private void OnDisable() 
    {
        GameEventsManager.instance.playerEvents.onExperienceGained -= ExperienceGained;
    }

    private void Start()
    {
        GameEventsManager.instance.playerEvents.PlayerLevelChange(currentLevel);
        GameEventsManager.instance.playerEvents.PlayerExperienceChange(currentExperience);
    }

    private void ExperienceGained(int experience)
    {
        // At the level cap, XP no longer accumulates.
        if (currentLevel >= maxLevel)
        {
            GameEventsManager.instance.playerEvents.PlayerExperienceChange(currentExperience);
            return;
        }

        currentExperience += experience;
        // check if we're ready to level up
        while (currentExperience >= GlobalConstants.experienceToLevelUp && currentLevel < maxLevel)
        {
            currentExperience -= GlobalConstants.experienceToLevelUp;
            currentLevel++;
            GameEventsManager.instance.playerEvents.PlayerLevelChange(currentLevel);
        }

        // Hit the cap mid-gain: discard the overflow.
        if (currentLevel >= maxLevel) currentExperience = 0;

        GameEventsManager.instance.playerEvents.PlayerExperienceChange(currentExperience);
    }
}