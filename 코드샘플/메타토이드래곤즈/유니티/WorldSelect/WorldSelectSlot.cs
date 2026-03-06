using Coffee.UIEffects;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork {
    public class WorldSelectSlot : MonoBehaviour
    {
        [SerializeField] private int worldIndex = 0;
        public int WorldIndex { get { return worldIndex; } }

        [SerializeField] private GameObject selectedNode = null;
        [SerializeField] private GameObject starInfoLayerNode = null;
        [SerializeField] private Image worldImageSprite = null;
        [SerializeField] private Text worldNameLabel = null;
        [SerializeField] private Text starLabel = null;
        [SerializeField] private GameObject lockLayerNode = null;
        [SerializeField] private Text lockLabel = null;
        [SerializeField] private StageScene StageScene = null;
        [SerializeField] private Sprite SelectNameLabelImg = null;
        
        [SerializeField] private Sprite[] NonSelectNameLabelImg = null;

        [SerializeField] private Image NameLabelImg = null;

        [SerializeField] private Sprite[] starIcon = null;
        [SerializeField] private Image StarImg = null;

        [SerializeField] private Color[] diffColor = new Color[] { Color.white, Color.cyan, Color.red };

        WorldData currentWorldData = null;
        int userClearWorldNumber=0;
        int playAbleWorldNumber = 0;


        public void Init(int index)
        {
            worldIndex = index;
            currentWorldData = WorldData.GetByWorldNumber(worldIndex);
            playAbleWorldNumber = GameConfigTable.GetLastWorld();

            if(currentWorldData == null || worldIndex > GameConfigTable.GetLastWorld())
                SetEmptyWorldInfoSlot();
            else
                SetWorldInfoSlot();
        }
        void SetWorldInfoSlot()
        {
            int difficult = CacheUserData.GetInt("adventure_difficult", 1);
            var worldInfo = StageManager.Instance;
            if(worldInfo == null)
            {
                StageManager.Instance.SetWorld(StageManager.Instance.AdventureProgressData.GetLatestWorld(difficult));
            }
            userClearWorldNumber = StageManager.Instance.AdventureProgressData.GetLatestWorld(difficult);

            worldNameLabel.text = string.Format("{0} - {1}", currentWorldData.NUM, StringData.GetStringByIndex(currentWorldData._NAME));

            int stageCount = StageBaseData.GetByAdventureWorld(currentWorldData.NUM).Count;
            starLabel.text = string.Format("{0} / {1}", GetWorldStarCount(), stageCount*3);
            switch(difficult)
            {
                case 2:
                    selectedNode.GetComponent<Image>().color = diffColor[1];
                    StarImg.sprite = starIcon[1];
                    break;
                case 3:
                    selectedNode.GetComponent<Image>().color = diffColor[2];
                    StarImg.sprite = starIcon[2];
                    break;
                case 1:
                default:
                    selectedNode.GetComponent<Image>().color = diffColor[0];
                    StarImg.sprite = starIcon[0];
                    break;
            }
            
            bool isOpen = worldIndex <= userClearWorldNumber;

            lockLayerNode.SetActive(false);

            string path = string.Format("worldmap_{0:D2}", worldIndex);
            switch (difficult)
            {
                case 2:
                    path += "_1";
                    break;
                case 3:
                    path += "_2";
                    break;
                case 1:
                default:
                    break;
            }
            worldImageSprite.sprite = ResourceManager.GetResource<Sprite>(eResourcePath.WorldSelectImgPath, path);

            if (worldImageSprite != null)
            {
                var uiEffect = worldImageSprite.GetComponent<UIEffect>();
                if (uiEffect == null)
                    uiEffect = worldImageSprite.gameObject.AddComponent<UIEffect>();

                uiEffect.effectMode = isOpen ? EffectMode.None : EffectMode.Grayscale;
            }
            if(lockLabel != null)
            {
                lockLabel.gameObject.SetActive(!isOpen);
                if (!isOpen)
                    lockLabel.text = StringData.GetStringByStrKey("월드메세지");
            }
            if (worldInfo == null)
            {
                selectedNode.SetActive(false);
                return;
            }
            bool isSelect = StageManager.Instance.World == worldIndex;
            selectedNode.SetActive(isSelect);
            NameLabelImg.sprite = isSelect ? SelectNameLabelImg : NonSelectNameLabelImg[difficult - 1];
        }

        void SetEmptyWorldInfoSlot()
        {
            worldNameLabel.text = StringData.GetStringByIndex(100001245);
            starInfoLayerNode.SetActive(false);
            lockLayerNode.SetActive(false);
            lockLabel.text = StringData.GetStringByStrKey("월드업데이트메세지");
        }

        int GetWorldStarCount()
        {
            int result = 0;
            var worldStageList = StageManager.Instance.AdventureProgressData.GetWorldStages(worldIndex, CacheUserData.GetInt("adventure_difficult", 1));

            if(worldStageList !=null && worldStageList.Count > 0)
            {
                for(int i = 0; i < worldStageList.Count; ++i)
                {
                    result += worldStageList[i];
                }
            }
            return result;
        }


        public void OnClickWorldSlot()
        {

            //미구현 월드
            if (worldIndex > playAbleWorldNumber)
            {
                ToastManager.On(100000326);
                //토스트 메세지 100000326
                return;
            }
            //미 오픈 월드
            if (worldIndex > userClearWorldNumber)
            {
                ToastManager.On(100000628);
                //토스트 메세지 100000628
                return;
            }
            
            UIManager.Instance.InitUI(eUIType.Adventure);
            StageScene.ChangeWorld(worldIndex, CacheUserData.GetInt("adventure_difficult", 1));
            //LoadingManager.ImmediatelySceneLoad("AdventureStageSelect");

        }
        public void SetWorldSprite(Sprite sprite)
        {
            worldImageSprite.sprite = sprite;
        }

        public void RefreshWorldInfoSlot()
        {
            int difficult = CacheUserData.GetInt("adventure_difficult", 1);
            bool isSelect = StageManager.Instance.World == worldIndex;
            if (selectedNode != null)
                selectedNode.SetActive(isSelect);
            if(NameLabelImg != null)
                NameLabelImg.sprite = isSelect ? SelectNameLabelImg : NonSelectNameLabelImg[difficult - 1];
        }
    }
}
