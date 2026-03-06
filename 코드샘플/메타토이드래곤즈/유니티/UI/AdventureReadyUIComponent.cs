using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork { 
    public class AdventureReadyUIComponent : MonoBehaviour
    {
        [Header("world Info")]
        [SerializeField] private Text worldNameLabel = null;
        [SerializeField] private Text stageNameLabel = null;

        WorldStageInfo currentWorldStageInfo = null;
        int worldIndex = 0;
        int stageIndex = 0;

        public void SetData(int world, int stage, WorldStageInfo worldInfo)
        {
            worldIndex = world;
            stageIndex = stage;
            currentWorldStageInfo = worldInfo;
            Refresh();

        }
        private void Refresh()
        {
            if(worldNameLabel != null)
            {
                var data = WorldData.GetByWorldNumber(worldIndex);
                if(worldNameLabel != null)
                {
                    worldNameLabel.text = string.Format(StringData.GetStringByIndex(100000946), StringData.GetStringByIndex(data._NAME));
                }
                if(stageNameLabel!= null)
                {
                    stageNameLabel.text = string.Format("{0} {1}-{2}", StringData.GetStringByIndex(data._NAME), worldIndex,stageIndex);
                }
            }
        }
    }
}