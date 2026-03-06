using DG.Tweening;
using SandboxNetwork;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public struct InventoryIncomeEvent
{

    private static InventoryIncomeEvent obj;
    public List<Asset> rewards;
    public int buildingTag;
    public eHarvestType type;

    public static void Send(List<Asset> rewards, int buildingTag, eHarvestType type = eHarvestType.GET_BUILDING)
    {
        obj.rewards = rewards;
        obj.buildingTag = buildingTag;
        obj.type = type;
        
        EventManager.TriggerEvent(obj);
    }
}

public class InventoryAnimation : MonoBehaviour, EventListener<InventoryIncomeEvent>
{
    [SerializeField] GameObject inventory = null;
    [SerializeField] GameObject itemClone = null;
    [SerializeField] ParticleSystem inventoryParticle = null;
    private Coroutine animationCoroutine = null;
    private List<GameObject> items = new List<GameObject>();
    

    void OnEnable()
    {
        EventManager.AddListener<InventoryIncomeEvent>(this);
        itemClone.SetActive(false);
        inventory.SetActive(false);
    }

    void OnDisable()
    {
        if (animationCoroutine != null)
            StopCoroutine(animationCoroutine);
        animationCoroutine = null;
        EventManager.RemoveListener<InventoryIncomeEvent>(this);

        foreach(var item in items)
        {
            item.transform.DOKill();
            item.transform.Find("itemIcon").DOKill();
            Destroy(item);
        }
        items.Clear();

        inventory.transform.DOKill();

        itemClone.SetActive(false);
        inventory.SetActive(false);
        inventoryParticle.Stop();
    }

    public void OnEvent(InventoryIncomeEvent eventType)
    {
        if (animationCoroutine != null)
            StopCoroutine(animationCoroutine);

        animationCoroutine = StartCoroutine(AnimCoroutine(eventType.rewards, eventType.buildingTag, eventType.type));
    }
    [ContextMenu("TestAnimation")]
    public void TestAnimation()
    {
        Asset temp = new Asset(40000001, 1);
        List<Asset> tmplist = new List<Asset>();
        for(int i = 0; i < 100; i++)
        {
            tmplist.Add(temp);
        }

        if (animationCoroutine != null)
            StopCoroutine(animationCoroutine);

        animationCoroutine = StartCoroutine(AnimCoroutine(tmplist, 1001, eHarvestType.GET_BUILDING_TYPE));
    }
    IEnumerator AnimCoroutine(List<Asset> rewards, int buildingTag, eHarvestType type)
    {
        inventory.transform.localScale = Vector3.one;
        inventory.SetActive(true);
        inventoryParticle.Stop();

        Vector2 goalPos = new Vector2(inventory.transform.localPosition.x, inventory.transform.localPosition.y);

        List<KeyValuePair<BuildingProductUI, List<Asset>>> ui = new List<KeyValuePair<BuildingProductUI, List<Asset>>>();
        Building building = Town.Instance.GetBuilding(buildingTag);
        if (building != null)
        {
            if (type != eHarvestType.GET_BUILDING_TYPE)
            {
                ui.Add(new KeyValuePair<BuildingProductUI, List<Asset>>(building.GetProductUI(), rewards));
            }
            else if(type == eHarvestType.GET_BUILDING_TYPE)
            {
                foreach (var b in Town.Instance.GetSameTypeBuildings(building))
                {
                    if (b != null)
                    {
                        var rew = b.GetProductUI().GetProductRewards();
                        if (rew != null)
                        {
                            ui.Add(new KeyValuePair<BuildingProductUI, List<Asset>>(b.GetProductUI(), new List<Asset>(rew)));
                        }
                    }
                }
            }
        }

        float animTime = 0.5f;
        float maxAnimTime = 0.0f;
        int maxObjCount = 0;
        inventory.transform.DOKill();
        
        for (int u = 0; u < ui.Count; u++)
        {
            int rewardCount = ui[u].Value.Count;
            if (rewardCount <= 0)
                continue;

            int objCount = 0;
            float boxAnimTime = 0.25f / rewardCount;
            maxAnimTime = Mathf.Max(boxAnimTime, maxAnimTime);
            for (int j = 0; j < rewardCount; j++)
            {
                var reward = ui[u].Value[j % rewardCount];

                Vector2 startPoint = (new Vector2(SBFunc.RandomValue, SBFunc.RandomValue).normalized * 100.0f) + goalPos;
                if (ui[u].Key != null)
                {
                    int index = j % ui[u].Value.Count;
                    startPoint = transform.InverseTransformPoint(UICanvas.Instance.GetCamera().ScreenToWorldPoint(Camera.main.WorldToScreenPoint(ui[u].Key.GetWorldPosition(rewardCount, j))));
                }

                if (!ItemFrame.IsInventoryItem(reward.ItemNo))
                    continue;

                //for (int i = 0; i < reward.Amount; i++)
                {
                    var itemObj = Instantiate(itemClone, itemClone.transform.parent);
                    itemObj.SetActive(true);
                    items.Add(itemObj);

                    var itemFrame = itemObj.GetComponent<ItemFrame>();
                    var itemIcon = itemObj.transform.Find("itemIcon");
                    itemFrame.SetInventoryAnimationItem(reward.ItemNo);

                    itemObj.transform.localPosition = startPoint;
                    itemObj.transform.localScale = Vector3.one;

                    float delay = (boxAnimTime * objCount) + (0.25f * SBFunc.RandomValue);
                    itemIcon.localScale = Vector3.one * 0.8f;

                    itemIcon.transform.DOScale(Vector3.one, delay);
                    itemObj.transform.DOLocalMoveY(startPoint.y + 10.0f, delay).OnComplete(() =>
                    {
                        itemObj.transform.DOKill();
                        itemIcon.DOScale(Vector3.one * 0.2f, animTime).SetEase(Ease.InQuad);
                        itemObj.transform.DOLocalMove(goalPos, animTime).SetEase(Ease.InQuad).OnComplete(() =>
                        {
                            itemIcon.transform.DOKill();
                            itemObj.transform.DOKill();

                            items.Remove(itemObj);
                            itemIcon.gameObject.SetActive(false);
                            Destroy(itemObj, 0.5f);

                            inventory.transform.localScale = Vector3.one * 1.2f;
                            inventory.transform.DOScale(Vector3.one, boxAnimTime * 0.5f).SetEase(Ease.InOutBounce);
                            inventoryParticle.Stop();
                            inventoryParticle.Play();
                        });

                    });

                    objCount++;
                }
            }

            maxObjCount = Mathf.Max(objCount, maxObjCount);
        }

        if (maxObjCount > 0)
        {
            SoundManager.Instance.PlaySFX("sfx_batte_star");

            yield return SBDefine.GetWaitForSeconds(animTime);
            
            SoundManager.Instance.PlaySFX("sfx_product_itemget");

            yield return SBDefine.GetWaitForSeconds((maxAnimTime * maxObjCount + 1.5f));

            SoundManager.Instance.PlaySFX("sfx_bag_incoming");
        }

        inventory.SetActive(false);
    }
}
