using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class PackageShopPanel : MonoBehaviour
{
    [Header("[Package Item Info List]")]
    public GameObject packageListScrollContainer;
    //public GameObject packageListCloneObject;

    [Header("[Layout List]")]
    public RectTransform layoutRect;

    NecoShopPanel rootParentPanel;

    public void InitPackageShopPanel(NecoShopPanel parentPanel)
    {
        rootParentPanel = parentPanel;

        ClearData();

        SetPackageList();

        packageListScrollContainer.GetComponent<RectTransform>().anchoredPosition = Vector3.zero;

        if (gameObject.activeInHierarchy)
        {
            StartCoroutine(RefreshLayout());
        }
    }

    void SetPackageList()
    {
        if (packageListScrollContainer == null) { return; }

        foreach (Transform child in packageListScrollContainer.transform)
        {
            Destroy(child.gameObject);
        }

        WWWForm data = new WWWForm();
        data.AddField("api", "iap");
        data.AddField("op", 3);

        NetworkManager.GetInstance().SendApiRequest("iap", 3, data, (res) =>
        {
            JObject root = JObject.Parse(res);
            JToken apiToken = root["api"];
            if (null == apiToken || apiToken.Type != JTokenType.Array
                || !apiToken.HasValues)
            {
                return;
            }

            JArray apiArr = (JArray)apiToken;
            foreach (JObject row in apiArr)
            {
                string uri = row.GetValue("uri").ToString();
                if (uri == "iap")
                {
                    JToken resultCode = row["rs"];
                    if (resultCode != null && resultCode.Type == JTokenType.Integer)
                    {
                        int rs = resultCode.Value<int>();
                        if (rs == 0)
                        {
                            neco_data.Instance.SetPurchaseHistory((JObject)row["cnt"]);

                            System.DateTime curTime = new System.DateTime(1970, 1, 1, 0, 0, 0, 0);
                            curTime = curTime.AddSeconds(NecoCanvas.GetCurTime());

                            //Todo : 가중치 값에 우선적으로 패키지 리스트 나열
                            List<neco_shop> pkgList = new List<neco_shop>();
                            List<neco_shop> compPkgList = new List<neco_shop>();

                            List<neco_shop> eventsaleList = neco_shop.GetNecoShopListByType("event");
                            foreach (neco_shop shopData in eventsaleList)
                            {
                                if (neco_data.Instance.GetEventSaleProductRemain(shopData.GetNecoShopID()) <= 0)
                                {
                                    continue;
                                }

                                string resourcePath = shopData.GetNecoShopPackageResource();
                                if (!string.IsNullOrEmpty(resourcePath))
                                {
                                    GameObject packegePrefab = Resources.Load<GameObject>(resourcePath);
                                    if (packegePrefab == null)
                                        continue;

                                    //구매한 상품은 아래로
                                    uint currentPurchaseCount = neco_data.Instance.GetPurchaseCount(shopData.GetNecoShopID());
                                    uint limitPurchaseCount = shopData.GetNecoShopPurchaseLimit();
                                    if (currentPurchaseCount >= limitPurchaseCount)
                                    {
                                        compPkgList.Add(shopData);
                                        continue;
                                    }

                                    GameObject packageItem = Instantiate(packegePrefab);
                                    packageItem.transform.SetParent(packageListScrollContainer.transform);
                                    packageItem.transform.localScale = packegePrefab.transform.localScale;
                                    //packageItem.transform.localPosition = packageListCloneObject.transform.localPosition;

                                    packageItem.GetComponent<ShopPackageInfo>().SetPackageInfoData(shopData, rootParentPanel);
                                }
                            }

                            List<neco_shop> timesaleList = neco_shop.GetNecoShopListByType("timesale");
                            foreach (neco_shop shopData in timesaleList)
                            {
                                //구매한 상품은 활성화되지 않음 따로 정렬할 필요 X
                                if (neco_data.Instance.GetTimeSaleProductRemain(shopData.GetNecoShopID()) < NecoCanvas.GetCurTime())
                                {
                                    continue;
                                }

                                string resourcePath = shopData.GetNecoShopPackageResource();
                                if (!string.IsNullOrEmpty(resourcePath))
                                {
                                    GameObject packegePrefab = Resources.Load<GameObject>(resourcePath);
                                    if (packegePrefab == null)
                                        continue;

                                    GameObject packageItem = Instantiate(packegePrefab);
                                    packageItem.transform.SetParent(packageListScrollContainer.transform);
                                    packageItem.transform.localScale = packegePrefab.transform.localScale;
                                    //packageItem.transform.localPosition = packageListCloneObject.transform.localPosition;

                                    packageItem.GetComponent<ShopPackageInfo>().SetPackageInfoData(shopData, rootParentPanel);
                                }
                            }

                            List<neco_shop> packageList = neco_shop.GetNecoShopListByType("package");
                            List<neco_shop> completePackages = new List<neco_shop>();
                            
                            foreach (neco_shop shopData in packageList)
                            {
                                string resourcePath = shopData.GetNecoShopPackageResource();
                                if (!string.IsNullOrEmpty(resourcePath))
                                {
                                    GameObject packegePrefab = Resources.Load<GameObject>(resourcePath);
                                    if (packegePrefab == null)
                                        continue;

                                    uint currentPurchaseCount = neco_data.Instance.GetPurchaseCount(shopData.GetNecoShopID());
                                    uint limitPurchaseCount = shopData.GetNecoShopPurchaseLimit();
                                    if(limitPurchaseCount > 0 && currentPurchaseCount >= limitPurchaseCount)
                                    {
                                        compPkgList.Add(shopData);
                                        continue;
                                    }

                                    pkgList.Add(shopData);
                                    //GameObject packageItem = Instantiate(packegePrefab);
                                    //packageItem.transform.SetParent(packageListScrollContainer.transform);
                                    //packageItem.transform.localScale = packegePrefab.transform.localScale;
                                    ////packageItem.transform.localPosition = packageListCloneObject.transform.localPosition;

                                    //packageItem.GetComponent<ShopPackageInfo>().SetPackageInfoData(shopData, rootParentPanel);
                                }
                            }

                            //compPkgList에 담는 것으로 쇼부
                            //foreach (neco_shop shopData in completePackages)
                            //{
                            //    string resourcePath = shopData.GetNecoShopPackageResource();
                            //    if (!string.IsNullOrEmpty(resourcePath))
                            //    {
                            //        GameObject packegePrefab = Resources.Load<GameObject>(resourcePath);
                            //        if (packegePrefab == null)
                            //            continue;

                            //        pkgList.Add(shopData);
                            //        GameObject packageItem = Instantiate(packegePrefab);
                            //        packageItem.transform.SetParent(packageListScrollContainer.transform);
                            //        packageItem.transform.localScale = packegePrefab.transform.localScale;
                            //        //packageItem.transform.localPosition = packageListCloneObject.transform.localPosition;

                            //        packageItem.GetComponent<ShopPackageInfo>().SetPackageInfoData(shopData, rootParentPanel);
                            //    }
                            //}

                            pkgList.Sort((neco_shop pkgA, neco_shop pkgB) =>
                            {
                                //후에 GetNecoShopGoodsID 를 GetNecoShopWeight으로 변경
                                if (pkgA.GetNecoShopOrder() < pkgB.GetNecoShopOrder())
                                    return -1;
                                else if (pkgA.GetNecoShopOrder() > pkgB.GetNecoShopOrder())
                                    return 1;
                                else
                                    return 0;
                            });

                            //판매 완료한 패키지 sorting된 상품 아래에 배치
                            pkgList.AddRange(compPkgList);

                            foreach (neco_shop pkgPref in pkgList)
                            {
                                string resourcePath = pkgPref.GetNecoShopPackageResource();
                                GameObject packegePrefab = Resources.Load<GameObject>(resourcePath);

                                GameObject packageItem = Instantiate(packegePrefab);
                                packageItem.transform.SetParent(packageListScrollContainer.transform);
                                packageItem.transform.localScale = packegePrefab.transform.localScale;
                                //packageItem.transform.localPosition = packageListCloneObject.transform.localPosition;

                                packageItem.GetComponent<ShopPackageInfo>().SetPackageInfoData(pkgPref, rootParentPanel);
                            }
                        }
                        else
                        {
                            string msg = rs.ToString();
                            switch (rs)
                            {
                                case 1: msg = LocalizeData.GetText("LOCALIZE_199"); break;
                                case 2: msg = LocalizeData.GetText("LOCALIZE_507"); break;
                                case 3: msg = LocalizeData.GetText("LOCALIZE_375"); break;
                                case 4: msg = LocalizeData.GetText("LOCALIZE_199"); break;
                                case 5: msg = LocalizeData.GetText("LOCALIZE_199"); break;
                                case 6: msg = LocalizeData.GetText("LOCALIZE_333"); break;
                            }

                            NecoCanvas.GetPopupCanvas().OnSystemMessagePopupShow(LocalizeData.GetText("LOCALIZE_360"), msg);
                        }
                    }
                }
            }
        });
    }


    public void RefreshData()
    {
        ClearData();

        SetPackageList();

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
