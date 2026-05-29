using UnityEngine;

namespace Platformer
{
    public class PlayerPersistence : MonoBehaviour, IDataPersistence
    {
        public void LoadData(GameData data) => transform.position = data.playerPosition;
        public void SaveData(GameData data) => data.playerPosition = transform.position;
    }
}
