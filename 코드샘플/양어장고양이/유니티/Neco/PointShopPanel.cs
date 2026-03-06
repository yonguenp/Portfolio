using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PointShopPanel : MonoBehaviour
{
    [Header("[Point Item Info List]")]
    public GameObject pointItemListScrollContainer;
    public GameObject pointItemListCloneObject;

    public GameObject pointNormalItemListScrollContainer;
    public GameObject pointNormalItemListCloneObject;

    [Header("[Point Price Layer]")]
    public Text pointAmountText;

    [Header("[Layout List]")]
    public RectTransform layoutRect;

    NecoShopPanel rootParentPanel;
    TapjoyUnity.TJPlacement offerwallPlacement = null;
    bool UserIDSet = false;
    private void OnEnable()
    {
#if UNITY_EDITOR
        return;
#endif
        TapjoyUnity.Tapjoy.OnConnectSuccess += HandleConnectSuccess;
        TapjoyUnity.Tapjoy.OnConnectFailure += HandleConnectFailure;

        TapjoyUnity.Tapjoy.OnSetUserIDSuccess += HandleSetUserIDSuccess;
        TapjoyUnity.Tapjoy.OnSetUserIDFailure += HandleSetUserIDFailure;
    }
    void OnDisable()
    {
#if UNITY_EDITOR
        return;
#endif
        TapjoyUnity.Tapjoy.OnConnectSuccess -= HandleConnectSuccess;
        TapjoyUnity.Tapjoy.OnConnectFailure -= HandleConnectFailure;

        TapjoyUnity.Tapjoy.OnSetUserIDSuccess -= HandleSetUserIDSuccess;
        TapjoyUnity.Tapjoy.OnSetUserIDFailure -= HandleSetUserIDFailure;
    }

    public void HandleConnectSuccess()
    {
        Debug.Log("탭조이 연결 성공");
    }

    public void HandleConnectFailure()
    {
        Debug.Log("탭조이 연결 실패");
    }

    private void HandleSetUserIDSuccess()
    {
        UserIDSet = true;
    }

    private void HandleSetUserIDFailure(string error)
    {
        UserIDSet = false;
    }

    public void OnClickPointShop(int shopNumber)
    {
#if UNITY_EDITOR        
        NecoCanvas.GetPopupCanvas().OnSystemMessagePopupShow("에디터", "에디터에서는 사용할 수 없습니다.");
        return;
#endif
        switch (shopNumber)
        {
            case 0:
                {
                    if (!TapjoyUnity.Tapjoy.IsConnected)
                    {
                        NecoCanvas.GetPopupCanvas().OnToastPopupShow(LocalizeData.GetText("LOCALIZE_336"));
                        TapjoyUnity.Tapjoy.Connect();
                    }
                    else if(!UserIDSet)
                    {
                        NecoCanvas.GetPopupCanvas().OnToastPopupShow(LocalizeData.GetText("LOCALIZE_336"));
                        TapjoyUnity.Tapjoy.SetUserID(NetworkManager.GetInstance().UserNo.ToString());
                    }
                    else
                    {
                        if (offerwallPlacement == null)
                        {
                            offerwallPlacement = TapjoyUnity.TJPlacement.CreatePlacement("hahaha_offerwall");
                            offerwallPlacement.RequestContent();
                        }

                        if (offerwallPlacement.IsContentAvailable())
                        {   
                            offerwallPlacement.ShowContent();
                            offerwallPlacement.RequestContent();
                        }
                        else
                        {
                            NecoCanvas.GetPopupCanvas().OnToastPopupShow(LocalizeData.GetText("LOCALIZE_337"));
                        }
                    }
                }break;
            default:
                //NecoCanvas.GetPopupCanvas().OnToastPopupShow("ERROR");
                break;
        }
    }

    public void InitPointShopPanel(NecoShopPanel parentPanel)
    {
        rootParentPanel = parentPanel;

        ClearData();

        SetPoinItemList();

        uint userPoint  = rootParentPanel.GetUserResource(NecoShopPanel.SHOP_RESOURCE_TYPE.POINT);
        pointAmountText.text = userPoint.ToString("n0");

        pointItemListScrollContainer.GetComponent<RectTransform>().anchoredPosition = Vector3.zero;

        if (gameObject.activeInHierarchy)
        {
            StartCoroutine(RefreshLayout());
        }

#if UNITY_EDITOR        
        return;
#endif
        if (!TapjoyUnity.Tapjoy.IsConnected)
        {   
            TapjoyUnity.Tapjoy.Connect();
        }

        TapjoyUnity.Tapjoy.SetUserID(NetworkManager.GetInstance().UserNo.ToString());
    }

    void SetPoinItemList()
    {
        if (pointItemListScrollContainer == null || pointItemListCloneObject == null) { return; }

        foreach (Transform child in pointItemListScrollContainer.transform)
        {
            if (child.gameObject != pointItemListCloneObject)
            {
                Destroy(child.gameObject);
            }
        }

        pointItemListCloneObject.SetActive(true);

        List<neco_shop> pointItemList = neco_shop.GetNecoShopListByType("pointshop");

        // 포인트 아이템 데이터 리스트 정렬
        // ...

        // 포인트 아이템 데이터 리스트 생성
        foreach (neco_shop shopData in pointItemList)
        {
            GameObject pointItem = Instantiate(pointItemListCloneObject);
            pointItem.transform.SetParent(pointItemListScrollContainer.transform);
            pointItem.transform.localScale = pointItemListCloneObject.transform.localScale;
            pointItem.transform.localPosition = pointItemListCloneObject.transform.localPosition;

            pointItem.GetComponent<ShopPointItemInfo>().SetPointItemInfoData(shopData, rootParentPanel);
        }

        pointItemListCloneObject.SetActive(false);

        foreach (Transform child in pointNormalItemListScrollContainer.transform)
        {
            if (child.gameObject != pointNormalItemListCloneObject)
            {
                Destroy(child.gameObject);
            }
        }

        pointNormalItemListCloneObject.SetActive(true);

        List<neco_shop> pointNormalItemList = neco_shop.GetNecoShopListByType("pointshop_normal");
        foreach (neco_shop shopData in pointNormalItemList)
        {
            GameObject pointItem = Instantiate(pointNormalItemListCloneObject);
            pointItem.transform.SetParent(pointNormalItemListScrollContainer.transform);
            pointItem.transform.localScale = pointNormalItemListCloneObject.transform.localScale;
            pointItem.transform.localPosition = pointNormalItemListCloneObject.transform.localPosition;

            pointItem.GetComponent<ShopCashItemInfo>().SetCatLeafItemData(shopData, rootParentPanel);
        }

        pointNormalItemListCloneObject.SetActive(false);
    }

    public void RefreshData()
    {
        ClearData();

        SetPoinItemList();

        uint userPoint = rootParentPanel.GetUserResource(NecoShopPanel.SHOP_RESOURCE_TYPE.CAT_LEAF);
        pointAmountText.text = userPoint.ToString("n0");

        if (gameObject.activeInHierarchy)
        {
            StartCoroutine(RefreshLayout());
        }
    }

    void ClearData()
    {

    }

    IEnumerator RefreshLayout()
    {
        // 원인 불명.. 2프레임에 걸쳐 최소 2회 갱신해야 정상 작동함

        yield return new WaitForEndOfFrame();

        if (layoutRect != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(layoutRect);
        }

        yield return new WaitForEndOfFrame();

        if (layoutRect != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(layoutRect);
        }
    }
}
