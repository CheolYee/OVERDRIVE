using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Gamelib.EventSystem;
using JetBrains.Annotations;
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
        private struct SaveFileInfo
        {
            [UsedImplicitly] public int id;
            [UsedImplicitly] public string fileName;
            [UsedImplicitly] public string typeName;
            [UsedImplicitly] public string objectName;
        }

        [Serializable]
        private struct SaveFileIndex
        {
            [UsedImplicitly] public List<SaveFileInfo> items;
        }

        [Header("Channel")]
        [field: SerializeField] public EventChannelSO SystemChannel { get; private set; }

        [Header("Save Settings")]
        [SerializeField] private string saveFolderName = "SaveData";
        [SerializeField] private bool applyJsonFilesToGameData = true;

        [Header("Auto Save")]
        [SerializeField] private bool saveOnApplicationPause = true;
        [SerializeField] private bool saveOnApplicationQuit = true;

        [Header("Debug")]
        [SerializeField] private bool logSavePathOnAwake = true;
        [SerializeField] private bool logSaveLoad = true;

        private readonly List<SaveData> _unusedData = new();

        private string SaveFolderPath => Path.Combine(Application.persistentDataPath, saveFolderName);

        private void Awake()
        {
            Debug.Assert(SystemChannel != null, $"{name} : SystemChannel is null");

            EnsureSaveDirectoryExists();

            if (SystemChannel != null)
            {
                SystemChannel.AddListener<SavePrefEvent>(HandleSavePrefEvent);
                SystemChannel.AddListener<LoadPrefEvent>(HandleLoadPrefEvent);
            }

            if (logSavePathOnAwake)
                Debug.Log($"[DataManager] Save Path : {SaveFolderPath}");
        }

        private void OnDestroy()
        {
            if (SystemChannel == null)
                return;

            SystemChannel.RemoveListener<SavePrefEvent>(HandleSavePrefEvent);
            SystemChannel.RemoveListener<LoadPrefEvent>(HandleLoadPrefEvent);
        }

        private void OnApplicationQuit()
        {
            if (saveOnApplicationQuit)
                SaveToJsonFiles();
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus && saveOnApplicationPause)
                SaveToJsonFiles();
        }

        private void HandleSavePrefEvent(SavePrefEvent evt)
        {
            SaveToJsonFiles();
        }

        private void HandleLoadPrefEvent(LoadPrefEvent evt)
        {
            if (!applyJsonFilesToGameData)
            {
                Debug.Log("[DataManager] applyJsonFilesToGameData is false. Load skipped.");
                return;
            }

            LoadFromJsonFiles();
        }

        private void EnsureSaveDirectoryExists()
        {
            if (!Directory.Exists(SaveFolderPath))
                Directory.CreateDirectory(SaveFolderPath);
        }

        private void SaveToJsonFiles()
        {
            EnsureSaveDirectoryExists();

            IEnumerable<ISaveable> savableObjects =
                FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None).OfType<ISaveable>();

            List<ISaveable> savableList = savableObjects
                .Where(s => s.SaveId != null)
                .ToList();

            HashSet<int> usedIds = new();

            foreach (ISaveable savable in savableList)
            {
                int id = savable.SaveId.Id;

                if (!usedIds.Add(id))
                {
                    Debug.LogWarning($"[DataManager] Duplicate SaveId detected. id : {id}, type : {savable.GetType().Name}");
                    continue;
                }

                string filePath = GetSaveFilePath(id, savable);
                string json = savable.GetSaveData();

                File.WriteAllText(filePath, json);
            }

            foreach (SaveData saveData in _unusedData)
            {
                if (usedIds.Contains(saveData.id))
                    continue;

                string filePath = GetUnusedSaveFilePath(saveData.id);
                File.WriteAllText(filePath, saveData.data);
            }

            SaveIndexFile(savableList);

            if (logSaveLoad)
                Debug.Log($"[DataManager] Saved json files to : {SaveFolderPath}");
        }

        private void LoadFromJsonFiles()
        {
            EnsureSaveDirectoryExists();

            IEnumerable<ISaveable> savableObjects =
                FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None).OfType<ISaveable>();

            Dictionary<int, ISaveable> savableMap = savableObjects
                .Where(s => s.SaveId != null)
                .GroupBy(s => s.SaveId.Id)
                .ToDictionary(g => g.Key, g => g.First());

            _unusedData.Clear();

            string[] jsonFiles = Directory.GetFiles(SaveFolderPath, "*.json");

            foreach (string filePath in jsonFiles)
            {
                string fileName = Path.GetFileName(filePath);

                if (string.Equals(fileName, "index.json", StringComparison.OrdinalIgnoreCase))
                    continue;

                if (!TryParseSaveIdFromFileName(fileName, out int id))
                {
                    Debug.LogWarning($"[DataManager] Failed to parse SaveId from file name : {fileName}");
                    continue;
                }

                string json = File.ReadAllText(filePath);

                if (savableMap.TryGetValue(id, out ISaveable savable))
                {
                    savable.RestoreData(json);
                }
                else
                {
                    _unusedData.Add(new SaveData
                    {
                        id = id,
                        data = json
                    });
                }
            }

            if (logSaveLoad)
                Debug.Log($"[DataManager] Loaded json files from : {SaveFolderPath}");
        }

        private void SaveIndexFile(IEnumerable<ISaveable> savableObjects)
        {
            List<SaveFileInfo> items = new();

            foreach (ISaveable savable in savableObjects)
            {
                if (savable == null || savable.SaveId == null)
                    continue;

                MonoBehaviour mono = savable as MonoBehaviour;
                string typeName = savable.GetType().Name;

                items.Add(new SaveFileInfo
                {
                    id = savable.SaveId.Id,
                    fileName = GetSaveFileName(savable.SaveId.Id, savable),
                    typeName = typeName,
                    objectName = mono != null ? mono.gameObject.name : typeName
                });
            }

            SaveFileIndex index = new SaveFileIndex
            {
                items = items
            };

            string json = JsonUtility.ToJson(index, true);
            string path = Path.Combine(SaveFolderPath, "index.json");
            File.WriteAllText(path, json);
        }

        private string GetSaveFilePath(int id, ISaveable savable)
        {
            return Path.Combine(SaveFolderPath, GetSaveFileName(id, savable));
        }

        private string GetSaveFileName(int id, ISaveable savable)
        {
            string typeName = savable.GetType().Name;
            typeName = MakeSafeFileName(typeName);
            return $"{id}_{typeName}.json";
        }

        private string GetUnusedSaveFilePath(int id)
        {
            return Path.Combine(SaveFolderPath, $"{id}_UnusedData.json");
        }

        private bool TryParseSaveIdFromFileName(string fileName, out int id)
        {
            id = -1;

            string nameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);
            string[] split = nameWithoutExtension.Split('_');

            if (split.Length == 0)
                return false;

            return int.TryParse(split[0], out id);
        }

        private string MakeSafeFileName(string fileName)
        {
            foreach (char c in Path.GetInvalidFileNameChars())
            {
                fileName = fileName.Replace(c.ToString(), string.Empty);
            }

            return fileName;
        }

        [ContextMenu("Save Now")]
        public void SaveNow()
        {
            SaveToJsonFiles();
        }

        [ContextMenu("Load Now")]
        public void LoadNow()
        {
            if (!applyJsonFilesToGameData)
            {
                Debug.LogWarning("[DataManager] applyJsonFilesToGameData is false. Load skipped.");
                return;
            }

            LoadFromJsonFiles();
        }

        [ContextMenu("Clear Save Files")]
        public void ClearSaveFiles()
        {
            if (!Directory.Exists(SaveFolderPath))
            {
                _unusedData.Clear();
                return;
            }

            string[] files = Directory.GetFiles(SaveFolderPath);

            foreach (string file in files)
            {
                File.Delete(file);
            }

            _unusedData.Clear();
            Debug.Log("[DataManager] Cleared all save files.");
        }

        [ContextMenu("Open Save Folder")]
        public void OpenSaveFolder()
        {
            EnsureSaveDirectoryExists();
            Application.OpenURL("file://" + SaveFolderPath);
        }
    }
}