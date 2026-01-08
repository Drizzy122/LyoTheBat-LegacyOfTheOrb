using UnityEngine;
using UnityEngine.VFX;

namespace Platformer
{
    public class LuminOrbs : MonoBehaviour, IDataPersistence
    {
        [ContextMenu("Generate guid for id")]
        private void GenerateGuid()
        {
            id = System.Guid.NewGuid().ToString();
        }
        
        [SerializeField] private VisualEffect visual;
        [SerializeField] private string id;
        [SerializeField] private bool collected = false;

        public void LoadData(GameData data)
        {
            data.luminCollected.TryGetValue(id, out collected);
            if (collected)
            {
                visual.gameObject.SetActive(false);
                Destroy();
            }
        }

        public void SaveData(GameData data)
        {
            if (data.luminCollected.ContainsKey(id))
            {
                data.luminCollected.Remove(id);
            }

            data.luminCollected.Add(id, collected);
        }

        private void OnTriggerEnter()
        {
            if (!collected)
            {
                LuminCollected();
            }
        }

        private void LuminCollected()
        {
            collected = true;
            visual.gameObject.SetActive(false);
            GameEventsManager.instance.miscEvents.LuminCollected();
            AudioManager.instance.PlayOneShot(FMODEvents.instance.luminCollected, this.transform.position);
           
        }
        
        private void Destroy() 
        {
            Destroy(this.gameObject);
        }
    }
}