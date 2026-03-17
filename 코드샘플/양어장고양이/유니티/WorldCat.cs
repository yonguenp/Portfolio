using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WorldCat : MonoBehaviour
{
    public AreaUI areaUI;
    public Image panel;
    public Image thumbnail;
    public WaypointsFree.WaypointsTraveler wayPointMover;
    private stage curStageData;
    private WaypointsFree.WaypointsGroup targetWayPointer;

    public Transform minPos;
    public Transform maxPos;
    private float maxY = 0.0f;
    private float minY = 0.0f;
    private float maxS = 1.0f;
    private float minS = 0.7f;
    private float range = 0.0f;

    void Start()
    {
        maxY = maxPos.localPosition.y;
        minY = minPos.localPosition.y;
        range = maxY - minY;
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 scale = Vector3.one * (minS + (1.0f - (((range - ((transform.localPosition.y - maxY) * -1.0f)) / range))) * (maxS - minS));
        scale.z = 1.0f;
        transform.localScale = scale;
    }

    public WorldCat CloneItem(Transform parent, WaypointsFree.WaypointsGroup waypointsTarget, stage data)
    {
        gameObject.SetActive(true);

        GameObject listItem = Instantiate(gameObject);
        listItem.transform.SetParent(parent);

        RectTransform rt = listItem.GetComponent<RectTransform>();
        rt.localPosition = Vector3.zero;
        rt.localScale = Vector3.one;

        gameObject.SetActive(false);

        WorldCat ret = listItem.GetComponent<WorldCat>();
        ret.SetStageData(data, waypointsTarget);
        return ret;
    }

    public void OnCatSelect()
    {
        areaUI.OnWorldCatSelected(this);
    }

    public void SetStageData(stage data, WaypointsFree.WaypointsGroup waypointsTarget)
    {
        curStageData = data;
        targetWayPointer = waypointsTarget;
        thumbnail.sprite = Resources.Load<Sprite>(curStageData.GetThumbnailPath());

        Color color = panel.color;
        color.a = 0.0f;
        panel.color = color;
        
        color = thumbnail.color;
        color.a = 0.0f;
        thumbnail.color = color;

        Invoke("MoveStart", 0.1f);
    }

    public void MoveStart()
    {
        panel.DOFade(1.0f, 1.0f);
        thumbnail.DOFade(1.0f, 1.0f);
        WaypointsFree.WaypointsGroup wpg = targetWayPointer;
        if (wpg != null)
        {
            wayPointMover.StartLocalMove(wpg, () => {
                Color color = panel.color;
                color.a = 1.0f;
                panel.color = color;
                panel.DOFade(0.0f, 1.0f);
                color = thumbnail.color;
                color.a = 1.0f;
                thumbnail.color = color;
                thumbnail.DOFade(0.0f, 1.0f);

                Invoke("MoveDone", 1.0f);
            });
        }
    }

    public void MoveDone()
    {
        areaUI.SwitchCat(this);
    }

    public stage GetStageData()
    {
        return curStageData;
    }
}
