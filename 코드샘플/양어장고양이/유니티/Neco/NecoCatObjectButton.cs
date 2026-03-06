using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Newtonsoft.Json.Linq;
using SRF;

public class NecoCatObjectButton : MonoBehaviour
{
    public GameObject Heart;
    public GameObject Coin;

    protected void Awake()
    {
        Heart.SetActive(false);
        Coin.SetActive(false);

        Button button = gameObject.GetComponentOrAdd<Button>();
        if(button.onClick.GetPersistentEventCount() == 0)
            button.onClick.AddListener(OnCatButton);
    }

    public void OnCatButton()
    {
        if (neco_data.GetPrologueSeq() == neco_data.PrologueSeq.고양이10번터치가이드퀘스트)
        {
            HeartEffect();
            return;
        }

        NecoCatSpotContainer container = transform.GetComponentInParent<NecoCatSpotContainer>();
        if (container != null)
        {
            if (container.EventIcon != null && container.EventIcon.gameObject.activeInHierarchy)
            {
                container.OnCatInfo();
                return;
            }
        }
        
        
        if (Random.value < 0.10f)
        {
            if (NecoCanvas.GetCurTime() > neco_data.Instance.GetEnalbeCoinTime())
            {
                neco_data.Instance.SetEnableCoinTime(NecoCanvas.GetCurTime() + 1);
                CoinEffect();
            }
            else
            {
                HeartEffect();
            }
        }
        else
        {
            HeartEffect();
        }
    }

    public void CoinEffect()
    {
        WWWForm data = new WWWForm();
        
        int level = (int)neco_data.Instance.GetFishfarmLevel();
        int cnt = Random.Range(1, (int)(level * 0.5f));
        data.AddField("gold", cnt);

        NetworkManager.GetInstance().SendPostSimple("chore", 3, data, (response) =>
        {
            JObject root = JObject.Parse(response);
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
                if (uri == "chore")
                {
                    JToken resultCode = row["rs"];
                    if (resultCode != null && resultCode.Type == JTokenType.Integer)
                    {
                        int rs = resultCode.Value<int>();
                        if (rs == 0)
                        {
                            if (row.ContainsKey("rew"))
                            {
                                JObject income = (JObject)row["rew"];
                                if (income.ContainsKey("gold"))
                                {
                                    if(income["gold"].Value<uint>() > 0)
                                    {
                                        GameObject coin = Instantiate(Coin);
                                        coin.transform.SetParent(transform);
                                        coin.transform.localPosition = Vector3.zero;
                                        coin.transform.localScale = Vector3.one;
                                        coin.SetActive(true);

                                        coin.transform.DOLocalMoveX(Random.Range(-50, 50), 2);
                                        coin.transform.DOLocalMoveY(100, 2).OnComplete(() => {
                                            Destroy(coin);
                                        });

                                        coin.GetComponent<Image>().DOColor(new Color(1, 1, 1, 0), 2);
                                    }
                                }

                                if (income.ContainsKey("item"))
                                {
                                    JArray item = (JArray)income["item"];
                                    foreach (JObject rw in item)
                                    {
                                        GameObject dice = Instantiate(Resources.Load<GameObject>("Prefabs/Neco/touch_dice"));
                                        dice.transform.SetParent(transform);
                                        dice.transform.localPosition = Vector3.zero;
                                        dice.transform.localScale = Vector3.one;
                                        dice.SetActive(true);

                                        dice.transform.DOLocalMoveX(Random.Range(-50, 50), 2);
                                        dice.transform.DOLocalMoveY(100, 2).OnComplete(() => {
                                            Destroy(dice);
                                        });

                                        dice.GetComponent<Image>().DOColor(new Color(1, 1, 1, 0), 2);
                                    }
                                }
                            }
                        }
                    }
                }
                if (uri == "user")
                {
                    NetworkManager.GetInstance().OnResponseUser(row);
                    RefreshResource();
                }
                if (uri == "item")
                {
                    NetworkManager.GetInstance().OnResponseItem(row);
                }
            }
        });

        if (neco_data.GetPrologueSeq() == neco_data.PrologueSeq.방문고양이터치 || neco_data.GetPrologueSeq() == neco_data.PrologueSeq.골드받기)
        {
            MapObjectController mapController = NecoCanvas.GetGameCanvas().GetCurMapController();
            if (mapController != null)
            {
                mapController.SendMessage("OnGuideQuestCheckOut", neco_data.PrologueSeq.방문고양이터치, SendMessageOptions.DontRequireReceiver);
                mapController.SendMessage("OnGuideQuestCheckOut", neco_data.PrologueSeq.골드받기, SendMessageOptions.DontRequireReceiver);
            }
        }

        AudioManager.GetInstance().PlayNyangSound(1.0f);
    }

    public void HeartEffect()
    {
        GameObject heart = Instantiate(Heart);
        heart.transform.SetParent(transform);

        RectTransform rt = transform as RectTransform;
        Vector2 bound = rt.sizeDelta;
        bound /= 2;

        heart.transform.localPosition = new Vector3(Random.Range(-bound.x, bound.x), Random.Range(-bound.y, bound.y), 0);        
        heart.transform.localScale = Vector3.one * ((Random.value * 0.5f) + 0.5f);
        heart.SetActive(true);

        heart.transform.DOLocalMoveX(Random.Range(-50, 50), 2);
        heart.transform.DOLocalMoveY(heart.transform.localPosition.y + 100, 2).OnComplete(() => {
            Destroy(heart);
        });

        heart.GetComponent<Image>().DOColor(new Color(1, 1, 1, 0), 2);

        if (neco_data.GetPrologueSeq() == neco_data.PrologueSeq.고양이10번터치가이드퀘스트)
        {
            MapObjectController mapController = NecoCanvas.GetGameCanvas().GetCurMapController();
            if (mapController != null)
            {
                mapController.SendMessage("열번만지기시키기완료", SendMessageOptions.DontRequireReceiver);                
            }
        }

        if(neco_data.GetPrologueSeq() == neco_data.PrologueSeq.방문고양이터치 || neco_data.GetPrologueSeq() == neco_data.PrologueSeq.하트30회)
        {
            MapObjectController mapController = NecoCanvas.GetGameCanvas().GetCurMapController();
            if (mapController != null)
            {
                mapController.SendMessage("OnGuideQuestCheckOut", neco_data.PrologueSeq.방문고양이터치, SendMessageOptions.DontRequireReceiver);
                mapController.SendMessage("OnGuideQuestCheckOut", neco_data.PrologueSeq.하트30회, SendMessageOptions.DontRequireReceiver);
            }
        }
    }

    void RefreshResource()
    {
        NecoCanvas.GetUICanvas().RefreshTopUILayer(TOP_UI_PANEL_TYPE.RESOURCE);
    }
}
