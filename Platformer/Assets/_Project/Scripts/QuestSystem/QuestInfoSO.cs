using UnityEngine;

namespace Platformer
{
    [CreateAssetMenu(fileName = "QuestInfoSO", menuName = "ScriptableObjects/QuestInfoSO", order = 1)]
    public class QuestInfoSO : ScriptableObject
    {
        [field: SerializeField] public string id { get; private set; }

        [Header("General")] 
        public string displayName;

        [Header("Requirements")] 
        public int levelRequirement;
        public QuestInfoSO[] questPrerequisites;

        [Header("Steps")] 
        public GameObject[] questStepPrefabs;

        [Header("Rewards")] 
        public int experienceReward;
        public int goldReward;

        // ensure the id is always the name of the Scriptable Object asset
        private void OnValidate()
        {
            #if UNITY_EDITOR
            id = this.name;
            UnityEditor.EditorUtility.SetDirty(this);
            #endif
        }
    }
}