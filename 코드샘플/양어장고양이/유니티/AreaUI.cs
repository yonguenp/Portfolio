using Spine.Unity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AreaUI : MonoBehaviour
{
    public WorldCanvas worldCanvas;
    public List<AreaItem> areaObjects;
    public AreaInfoUI areaInfo;
    public StageInfoUI stageInfo;
    public WorldCat worldCatCloneTarget;
    public ScrollRect mapScroller;
    public Transform MapObjectUItransform;
    public GameObject BackButton;
    public WorldCatManager worldCatManager;

    List<WorldCat> worldCatList = new List<WorldCat>();

    private void OnEnable()
    {
        foreach(AreaItem item in areaObjects)
        {
            item.gameObject.SetActive(true);
            item.SetAreaUI(this);
            item.SetCursor(false);
        }

        areaInfo.Close();
        BackButton.SetActive(true);
        ClearWorldCat();

        //worldCatManager.SetCatAnimation("C0_G0", "");
    }

    private void OnDisable()
    {
        foreach (AreaItem item in areaObjects)
        {
            item.gameObject.SetActive(false);
        }

        areaInfo.Close();
        stageInfo.Close();
        BackButton.SetActive(false);
        ClearWorldCat();
    }

    public void OnAreaItemSelected(AreaItem target)
    {
        foreach(AreaItem item in areaObjects)
        {
            item.SetCursor(target == item);
        }

        stageInfo.Close();
        
        if (target == null)
            return;

        areaInfo.Open(target);

        UIExtensions.ScrollToCenter(mapScroller, target.transform as RectTransform, RectTransform.Axis.Horizontal);
        UIExtensions.ScrollToCenter(mapScroller, target.transform as RectTransform, RectTransform.Axis.Vertical);
        mapScroller.verticalNormalizedPosition += 0.2f;
    }

    public void ClearWorldCat()
    {
        worldCatCloneTarget.gameObject.SetActive(false);
        foreach (WorldCat cat in worldCatList)
        {
            Destroy(cat.gameObject);
        }

        worldCatList.Clear();
        worldCatManager.Clear();
    }

    public void CreateWorldCat(Transform rs)
    {
        users userData = GameDataManager.Instance.GetUserData();
        List<uint> unlocked_area = ((List<uint>)userData.data["unlocked_area"]);
        
        List<game_data> stages = GameDataManager.Instance.GetGameData(GameDataManager.DATA_TYPE.STAGE);
        List<stage> randomStage = new List<stage>();
        
        foreach (game_data gd in stages)
        {
            stage stage = (stage)gd;
            foreach (uint area in unlocked_area)
            {
                if (stage.GetLocationID() == area)
                {
                    if (stage.GetMaxStar() > stage.GetCurStar())
                    {
                        bool isNew = true;
                        foreach (WorldCat wc in worldCatList)
                        {
                            if (wc.GetStageData() == stage)
                            {
                                isNew = false;
                                break;                                
                            }
                        }           
                        if(isNew)
                            randomStage.Add(stage);
                    }
                }
            }
        }

        if (randomStage.Count > 0)
        {
            worldCatList.Add(worldCatCloneTarget.CloneItem(MapObjectUItransform, rs.GetComponent<WaypointsFree.WaypointsGroup>(), randomStage[Random.Range(0, randomStage.Count)]));
        }
    }

    public void SwitchCat(WorldCat cat)
    {
        stage data = cat.GetStageData();
        Transform rs = cat.transform.parent;

        foreach (WorldCat wc in worldCatList)
        {
            if(wc == cat)
            {
                worldCatList.Remove(wc);
                Destroy(cat.gameObject);
                break;
            }
        }

        //List<stage> randomStage = new List<stage>();
        //List<game_data> stages = GameDataManager.Instance.GetGameData(GameDataManager.DATA_TYPE.STAGE);
        //foreach (game_data gd in stages)
        //{
        //    stage stage = (stage)gd;
        //    if (stage.GetMaxStar() > stage.GetCurStar())
        //    {
        //        if (stage.GetLocationID() == data.GetLocationID())
        //        {
        //            bool isNew = true;
        //            foreach (WorldCat wc in worldCatList)
        //            {
        //                if(wc.GetStageData() == stage)
        //                {
        //                    isNew = false;
        //                    break;
        //                }
        //            }
        //            if(isNew)
        //                randomStage.Add(stage);
        //        }
        //    }
        //}

        //if (randomStage.Count <= 0)
        //    return;

        //stage choicedStage = randomStage[Random.Range(0, randomStage.Count)];

        //worldCatList.Add(wordCatCloneTarget.CloneItem(rs, choicedStage));
    }

    public void OnWorldCatSelected(WorldCat cat)
    {
        foreach (AreaItem item in areaObjects)
        {
            item.SetCursor(false);
        }

        UIExtensions.ScrollToCenter(mapScroller, cat.transform as RectTransform, RectTransform.Axis.Horizontal);
        UIExtensions.ScrollToCenter(mapScroller, cat.transform as RectTransform, RectTransform.Axis.Vertical);

        areaInfo.Close();
        stageInfo.Open(cat.GetStageData());
    }

    public void StageStart(stage stageData)
    {
        ClearWorldCat();
        worldCanvas.OnStageSelected(stageData);
        areaInfo.Close();
        stageInfo.Close();
    }

    public void AutoStart(List<stage> stageData)
    {
        List<uint> listStageID = new List<uint>();
        foreach(stage st in stageData)
        {
            if(st != null)
                listStageID.Add(st.GetStageID());
        }
        worldCanvas.OnAutoStageSelected(listStageID);
    }
}
