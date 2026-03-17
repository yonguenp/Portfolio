using SandboxNetwork;
using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Coffee.UIEffects;
using Spine.Unity;

public class BuildingProductUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, EventListener<PopupEvent>
{
    [SerializeField] Image itemClone;
    [SerializeField] GameObject productPanel;
    [SerializeField] GameObject idlePanel;

    private List<ProductReward> products = null;
    private List<Image> bubbles = new List<Image>();
    private Building parent = null;
    private bool bPress = false;

    Building.ProductState curState = Building.ProductState.UNKNOWN;

    public void Clear()
    {
        curState = Building.ProductState.UNKNOWN;
        itemClone.gameObject.SetActive(false);
        
        productPanel.SetActive(false);
        idlePanel.SetActive(false);

        if (products != null)
            products.Clear();
        products = null;
        ProductsClear();
    }

    public void OnDestroy()
    {
        ProductsClear();
    }
    private void OnEnable()
    {
        EventManager.AddListener(this);
    }
    private void OnDisable()
    {
        EventManager.RemoveListener(this);
    }

    public virtual void OnEvent(PopupEvent e)
    {
        if (e.popupOn)
        {
            foreach (var eff in bubbles)
            {
                eff.gameObject.SetActive(false);
            }
        }
        else
        {
            foreach (var eff in bubbles)
            {
                eff.gameObject.SetActive(true);
            }
        }
    }

    void ProductsClear()
    {
        foreach (Transform item in itemClone.transform.parent)
        {
            if (item == itemClone.transform)
                continue;

            item.DOKill();
            Destroy(item.gameObject);
        }

        bubbles.Clear();

        if (parent != null)
        {
            parent.GetSpine()?.Skeleton.SetColor(Color.white);
        }
    }

    public void TouchEffectClear()
    {
        if (!bPress)
            return;

        bPress = false;

        if (parent != null)
        {
            parent.GetSpine()?.Skeleton.SetColor(Color.white);
        }

        foreach (var eff in bubbles)
        {
            eff.DOColor(Color.white, 0.8f);
            eff.transform.DOScale(new Vector3(eff.transform.localScale.x < 0 ? -1f : 1.0f, 1.0f, 1.0f), 1.0f);
        }
    }


    public void SetParentBuilding(Building building)
    {
        parent = building;

        (transform as RectTransform).localPosition = new Vector2(0, -10.0f);

        Clear();
    }

    public void SetState(Building.ProductState state)
    {
        if (state == Building.ProductState.COMPLETED_ALL)
            state = Building.ProductState.RUNNING;

        if (curState == state)
            return;

        switch (state)
        {
            case Building.ProductState.QUEUE_EMPTY:
                SetIdle();
                break;
            case Building.ProductState.RUNNING:                
            case Building.ProductState.COMPLETED_ALL:
                SetProducts();
                break;
            default:                
                Clear();
                break;
        }
    }
    protected void SetIdle()
    {
        if (curState == Building.ProductState.QUEUE_EMPTY)
            return;

        Clear();

        curState = Building.ProductState.QUEUE_EMPTY;

        idlePanel.SetActive(true);
    }

    protected void SetProducts()
    {
        if (curState == Building.ProductState.RUNNING)
            return;

        Clear();

        curState = Building.ProductState.RUNNING;
    }

    public void SetProducts(List<ProductReward> p)
    {
        if (products == p)
            return;

        SetProducts();
        ProductsClear();

        products = p;

        if (products.Count <= 0)
        {
            productPanel.SetActive(false);
            return;
        }

        productPanel.SetActive(true);

        itemClone.gameObject.SetActive(true);
        
        int index = 0;

        bubbles.Clear();

        foreach (var product in products)
        {
            Sprite icon = null;
            switch (product.GoodType)
            {
                case eGoodType.GOLD:        // 골드
                    icon = ResourceManager.GetResource<Sprite>(eResourcePath.ItemIconPath, "gold");
                    break;
                case eGoodType.GEMSTONE:    // 젬스톤
                    icon = ResourceManager.GetResource<Sprite>(eResourcePath.ItemIconPath, "gemstone");
                    break;
                case eGoodType.ARENA_TICKET:   // 아레나 티켓
                    icon = ResourceManager.GetResource<Sprite>(eResourcePath.ItemIconPath, "item_pvp_ticket_1");
                    break;

                case eGoodType.ENERGY: // 에너지
                    icon = ResourceManager.GetResource<Sprite>(eResourcePath.ItemIconPath, "energy");
                    break;

                case eGoodType.ITEM: // 아이템
                    if (product.BaseData != null)
                    {
                        icon = product.BaseData.ICON_SPRITE;
                    }
                    break;
            }

            if (icon == null)
                continue;

            GameObject clone = Instantiate(itemClone.gameObject, itemClone.transform.parent);
            var effect = clone.GetComponent<Image>();
            bubbles.Add(effect);

            foreach (Transform child in clone.transform)
            {
                child.gameObject.SetActive(false);
            }

            var item = clone.transform.Find("Item");
            item.gameObject.SetActive(true);
            item.GetComponent<Image>().sprite = icon;

            clone.transform.localScale = Vector3.zero;
            clone.transform.localPosition = itemClone.transform.localPosition;

            Vector3 offset = GetLocalPosition(products.Count, index);
            float showTime = 0.5f + SBFunc.RandomValue;
            clone.transform.DOLocalMove(offset, showTime).SetEase(Ease.OutBack).OnComplete(()=> {
                clone.transform.DOLocalJump(clone.transform.localPosition, 2.0f + SBFunc.RandomValue, 3, 2.0f + SBFunc.RandomValue).SetLoops(-1, LoopType.Yoyo);
            });
            clone.transform.DOScale(Vector3.one, showTime).SetEase(Ease.OutBack);
             
            clone.transform.SetAsFirstSibling();

            index++;
        }

        itemClone.gameObject.SetActive(false);
    }

    public void SetData(List<UserDragon> dragons)//for world trip, exchange
    {
        SetProducts();
        ProductsClear();

        itemClone.gameObject.SetActive(true);

        if (dragons.Count <= 0)
        {
            productPanel.SetActive(false);
            return;
        }

        productPanel.SetActive(true);

        int index = 0;
        foreach (var dragon in dragons)
        {
            Sprite icon = dragon.BaseData.GetThumbnail();

            if (icon == null)
                continue;

            GameObject clone = Instantiate(itemClone.gameObject, itemClone.transform.parent);
            var effect = clone.GetComponent<Image>();
            bubbles.Add(effect);

            foreach(Transform child in clone.transform)
            {
                child.gameObject.SetActive(false);
            }

            var mask = clone.transform.Find("mask");
            mask.gameObject.SetActive(true);
            RectTransform item = mask.Find("Thumnail") as RectTransform;
            item.GetComponent<Image>().sprite = icon;
            item.sizeDelta = new Vector2(40, 40);

            clone.transform.localScale = Vector3.zero;
            clone.transform.localPosition = itemClone.transform.localPosition;



            Vector3 offset = GetLocalPosition(dragons.Count, index);
            float showTime = 0.5f + SBFunc.RandomValue;
            clone.transform.DOLocalMove(offset, showTime).SetEase(Ease.OutBack).OnComplete(() => {
                clone.transform.DOLocalJump(clone.transform.localPosition, 2.0f + SBFunc.RandomValue, 3, 2.0f + SBFunc.RandomValue).SetLoops(-1, LoopType.Yoyo);
            });
            clone.transform.DOScale(new Vector3(clone.transform.position.x > 0 ? -1.0f : 1.0f, 1.0f, 1.0f), showTime).SetEase(Ease.OutBack);

            clone.transform.SetAsFirstSibling();

            index++;
        }

        itemClone.gameObject.SetActive(false);
    }

    public bool TryHarvest(eHarvestType harvestType)
    {
        if(bubbles.Count > 0 && parent != null)
        {
            BuildingBaseData buildingData = BuildingBaseData.GetBuildingDataWithTag(parent.BTag);
            if (buildingData != null)
            {
                if (buildingData.TYPE == (int)eBuildingType.MATERIAL || buildingData.TYPE == (int)eBuildingType.PRODUCT)
                {
                    parent.OnHarvestAll();              // 23.6.21 - 타운에서 획득 시 모두 획득으로 변경
                }
                else
                {
                    parent.OnHarvest(harvestType);
                }
            }
            return true;
        }

        return false;
    }

    public Vector3 GetLocalPosition(int count, int index)
    {
        Vector2 pivot = itemClone.transform.localPosition;
        int row = count / 5;
        pivot.y -= (row * 10.0f);

        count = (count - 1) % 5;
        switch(count)
        {
            case 0:                
            case 2:
            case 4:
                break;
            case 1:                
            case 3:
                pivot.x += 15.0f;
                break;
        }

        switch (index % 5)
        {
            case 0:
                pivot.x += 0.0f;
                pivot.y += (index / 5 * 25.0f);
                break;
            case 1:
                pivot.x += -30.0f;
                pivot.y += (index / 5 * 25.0f) + 10.0f;
                break;
            case 2:
                pivot.x += 30.0f;
                pivot.y += (index / 5 * 25.0f) + 10.0f;
                break;
            case 3:
                pivot.x += -60.0f;
                pivot.y += (index / 5 * 25.0f);
                break;
            case 4:
                pivot.x += 60.0f;
                pivot.y += (index / 5 * 25.0f);
                break;
        }

        return pivot;
    }

    public Vector3 GetWorldPosition(int count, int index)
    {
        return itemClone.transform.TransformPoint(GetLocalPosition(count, index));
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        bPress = true;

        if (parent != null)
        {
            parent.GetSpine()?.Skeleton.SetColor(new Color(0.9f,0.9f,0.9f,1.0f));
        }

        foreach (var eff in bubbles)
        {
            eff.DOColor(Color.clear, 0.8f);
            eff.transform.DOScale(new Vector3(eff.transform.localScale.x < 0 ? -1f : 1.0f, 1.0f, 1.0f) * 1.5f, 1.0f);
        }
    }

    public void OnPointerExit(PointerEventData eventDate)
    {
        TouchEffectClear();
    }
        
    public List<ProductReward> GetProductRewards()
    {
        return products;
    }
}
