using KBCore.Refs;
using UnityEngine;

namespace Platformer
{
    public class Collectable : ValidatedMonoBehaviour, IDataPersistence
    {
        [field: Header("Settings")]
        [field: SerializeField] CollectableType collectibleType;
        [field: SerializeField] string id;
        [field: SerializeField] bool isCollected;
        
        [field: Header("Visuals")]
        [field: SerializeField, Anywhere] GameObject visualObject;
       // [field: SerializeField, Anywhere] ParticleSystem collectParticle;
        
        [ContextMenu("Generate guid for id")]
        private void GenerateGuid() => id = System.Guid.NewGuid().ToString();

        private void Awake()
        {
            //collectParticle.Stop();
        }
        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                Collect();
            }
        }
        void Collect()
        {
            isCollected = true;
            //collectParticle.Play();
            Destroy(gameObject);

            switch (collectibleType)
            {
                case CollectableType.Coin:
                    GameEventsManager.instance.miscEvents.CoinCollected();
                    AudioManager.instance.PlayOneShot(FMODEvents.instance.coinCollected, this.transform.position);
                    break;
                case CollectableType.LuminOrb:
                    GameEventsManager.instance.miscEvents.LuminCollected();
                    AudioManager.instance.PlayOneShot(FMODEvents.instance.luminCollected, this.transform.position);
                    break;
                case CollectableType.Ecliptium:
                    GameEventsManager.instance.miscEvents.EcliptiumCollected();
                    AudioManager.instance.PlayOneShot(FMODEvents.instance.luminCollected, this.transform.position);
                    break;
            }
        }
        public void LoadData(GameData data)
        {
            switch (collectibleType)
            {
                case CollectableType.Coin:
                    data.coinsCollected.TryGetValue(id, out isCollected);
                    break;
                case CollectableType.LuminOrb:
                    data.luminCollected.TryGetValue(id, out isCollected);
                    break;
                case CollectableType.Ecliptium:
                    data.ecliptiumCollected.TryGetValue(id, out isCollected);
                    break;
            }
            if (isCollected)
            {
                visualObject.gameObject.SetActive(false);
              //  collectParticle.Stop();
                Destroy(gameObject, 0.5f);
            }
        }
        public void SaveData(GameData data)
        {
            switch (collectibleType)
            {
                case CollectableType.Coin:
                    if (data.coinsCollected.ContainsKey(id))
                    {
                        data.coinsCollected.Remove(id);
                    }
                    data.coinsCollected.Add(id, isCollected); 
                    break;
                case CollectableType.LuminOrb:
                    if (data.luminCollected.ContainsKey(id))
                    {
                        data.luminCollected.Remove(id);
                    }
                    data.luminCollected.Add(id, isCollected); 
                    break;
                case CollectableType.Ecliptium:
                    if (data.ecliptiumCollected.ContainsKey(id))
                    {
                        data.ecliptiumCollected.Remove(id);
                    }
                    data.ecliptiumCollected.Add(id, isCollected); 
                    break;
            }
        }
    }
}