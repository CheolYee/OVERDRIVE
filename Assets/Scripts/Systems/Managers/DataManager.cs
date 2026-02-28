using System;
using System.Collections.Generic;
using System.Linq;
using Gamelib.EventSystem;
using Systems.CoreSystem;
using Systems.GameEvents;
using UnityEngine;

namespace Systems.Managers
{
    public class DataManager : MonoBehaviour
    {
        [Serializable]
        public struct SaveData
        {
            public int id;
            public string data;
        }
        
        [Serializable]
        public struct DataCollection
        {
            public List<SaveData> collection;
        }

        [SerializeField] private string prefKey = "saveData";

        //이번씬에서 사용하지 않는 세이브된 데이터를 가지고 있다.
        private List<SaveData> _unUsedData = new List<SaveData>();
        [field: SerializeField] public EventChannelSO SystemChannel { get; private set; }

        private void Awake()
        {
            SystemChannel.AddListener<SavePrefEvent>(HandleSavePrefEvent);
            SystemChannel.AddListener<LoadPrefEvent>(HandleLoadPrefEvent);
        }

        private void OnDestroy()
        {
            SystemChannel.RemoveListener<SavePrefEvent>(HandleSavePrefEvent);
            SystemChannel.RemoveListener<LoadPrefEvent>(HandleLoadPrefEvent);
        }

        #region 데이터 세이브 로직

        private void HandleSavePrefEvent(SavePrefEvent evt)
        {
            string saveData = GetSceneSaveData();
            PlayerPrefs.SetString(prefKey, saveData);
            Debug.Log($"Save Data : {saveData}");
        }

        private string GetSceneSaveData()
        {
            IEnumerable<ISaveable> saveableObjects = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None).OfType<ISaveable>();
            
            List<SaveData> toSaveData = new List<SaveData>();
            foreach (ISaveable saveable in saveableObjects)
            {
                toSaveData.Add(new SaveData{id = saveable.SaveId.Id, data = saveable.GetSaveData()});
            }
            toSaveData.AddRange(_unUsedData); //이번 씬에서 사용하지 않았던 데이터도 같이 저장한다.
            DataCollection dataCollection = new DataCollection{collection = toSaveData};
            
            return JsonUtility.ToJson(dataCollection);
        }
        
        #endregion

        #region 데이터 로드 로직
        private void HandleLoadPrefEvent(LoadPrefEvent evt)
        {
            string loadJson = PlayerPrefs.GetString(prefKey, string.Empty);
            RestoreData(loadJson);
        }

        private void RestoreData(string json)
        {
            IEnumerable<ISaveable> saveables = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None).OfType<ISaveable>();
            DataCollection parsedData = string.IsNullOrEmpty(json) 
                ? new DataCollection() 
                : JsonUtility.FromJson<DataCollection>(json);
            
            _unUsedData.Clear();

            if (parsedData.collection != null)
            {
                foreach (var saveData in parsedData.collection)
                {
                    ISaveable saveable = saveables.FirstOrDefault(s => s.SaveId.Id == saveData.id);
                    if (saveable != null)
                        saveable.RestoreData(saveData.data);
                    else
                        _unUsedData.Add(saveData);
                }
            }
        }

        #endregion
        
        [ContextMenu("Clear Pref Data")]
        public void ClearPrefData()
        {
            PlayerPrefs.DeleteKey(prefKey);
        }
    }
}