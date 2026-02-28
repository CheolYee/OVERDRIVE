using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Agents.Players
{
    [CreateAssetMenu(fileName = "Level Data", menuName = "Combat/Level Data", order = 30)]
    public class LevelDataSo : ScriptableObject
    {
        [Serializable]
        public struct LevelData
        {
            public int currentLevel;
            public int requiredExp;
        }
        
        public int statPerLevelUp;
        public int skillPerLevelUp;
        public List<LevelData> levelDataList;
        
        public int GetRequiredExp(int currentlevel)
        {
            int targetIndex =  levelDataList.FindIndex(x => x.currentLevel == currentlevel);
            return targetIndex < 0 ? -1 : levelDataList[targetIndex].requiredExp;
        }
        
        public bool IsMaxLevel(int currentlevel)
        {
            return levelDataList.Last().currentLevel == currentlevel;
        }

        private void OnValidate()
        {
            if (levelDataList == null) return;
            for (int i = 1; i < levelDataList.Count; i++)
            {
                if (levelDataList[i - 1].currentLevel + 1 != levelDataList[i].currentLevel)
                {
                    Debug.LogWarning($"LevelDataSo : 레벨은 반드시 오름차순으로 채워져 있어야 하고, 1씩 증가해야 합니다. 오류 레벨 : {levelDataList[i].currentLevel} (인덱스 {i})");
                }
            }
        }
    }
}