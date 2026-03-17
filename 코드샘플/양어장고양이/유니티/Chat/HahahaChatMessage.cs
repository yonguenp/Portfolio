using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class HahahaChatMessage : MonoBehaviour
{
    [System.Serializable]
    public struct AdminChatResourceStruct
    {
        public Sprite adminProfile;
        public Sprite adminChatBg;
        public Color bgColor;
        public Vector2 nameOutline;
        public Color nameOutlineColor;
        public Color textColor;
    }

    public Text user_name;
    public Text[] user_message;
    public GameObject image_group;
    public RawImage user_image;
    public Image profile_image;
    public RectTransform ChildPanel;
    public Text card_memo;
    public Text card_date;
    public bool is_mine;
    //public Text msg_time;

    private bool bAlphaAction = true;
    private float curAlpha = 1.0f;
    private float delayTime = 2.0f;
    private float curHeight = 0;

    public Image LevelImage;

    [Space]
    [Header("관리자 채팅 리소스")]
    
    public AdminChatResourceStruct[] adminChatRes;
        
    [Space]

    private JObject CardJsonData = null;
    private string accountNo = "";
    private string userName = "";
    private string userMsg = "";
    public void SetMessage(string _accountNo, string name, string message, bool isImage = false, string url = "", neco_admin.ADMIN_TYPE admin_type = neco_admin.ADMIN_TYPE.NONE)
    {
        accountNo = _accountNo;

        string[] spliter = {
            "[e960cdb67f2cb7488f16347705580180",
            "e960cdb67f2cb7488f16347705580180]"
        };

        string[] levChecker = name.Split(new string[] { spliter[1] }, StringSplitOptions.None);
        string level = levChecker.Length > 1 ? name.Substring(name.IndexOf(spliter[0]) + spliter[0].Length, name.IndexOf(spliter[1]) - name.IndexOf(spliter[0]) - spliter[0].Length) : "";
        name = levChecker[1];

        if (admin_type != neco_admin.ADMIN_TYPE.NONE)
        {
            Outline oline = user_name.gameObject.GetComponent<Outline>() == null ? user_name.gameObject.AddComponent<Outline>() : user_name.gameObject.GetComponent<Outline>();
            oline.effectDistance = adminChatRes[(int)admin_type -1].nameOutline;
            oline.effectColor = adminChatRes[(int)admin_type - 1].nameOutlineColor;

            LevelImage.gameObject.SetActive(true);
            LevelImage.sprite = adminChatRes[(int)admin_type - 1].adminProfile;

            user_name.color = Color.white;

            Image panelBg = ChildPanel.GetComponent<Image>();
            panelBg.sprite = adminChatRes[(int)admin_type - 1].adminChatBg;
            panelBg.color = adminChatRes[(int)admin_type - 1].bgColor;

            foreach (Text textObj in user_message)
            {
                textObj.color = adminChatRes[(int)admin_type - 1].textColor;
            }
        }
        else
        {
            if (!string.IsNullOrEmpty(level))
            {
                //List<game_data> necoData = GameDataManager.Instance.GetGameData(GameDataManager.DATA_TYPE.NECO_CAT);
                //neco_cat data = null;
                //while (data == null)
                //{
                //    data = (neco_cat)necoData[UnityEngine.Random.Range(0, necoData.Count)];
                //    if(data != null)
                //    {
                //        if(string.IsNullOrEmpty(data.GetIconPath()))
                //        {
                //            data = null;
                //        }
                //    }
                //}
                int index = (int.Parse(level) - 1);
                if (index < 0)
                    index = 0;

                Sprite[] sprites = Resources.LoadAll<Sprite>("Sprites/Neco/Ui/Idcard");
                if (neco_admin.GetAdmin(uint.Parse(accountNo)) != neco_admin.ADMIN_TYPE.NONE)
                {
                    LevelImage.gameObject.SetActive(true);
                }
                else if (sprites.Length > index)
                {
                    LevelImage.sprite = sprites[index];
                    LevelImage.gameObject.SetActive(true);
                }
                else
                {
                    LevelImage.gameObject.SetActive(false);
                }
            }
            else
            {
                LevelImage.gameObject.SetActive(false);
            }
        }        

        user_name.text = name;
        userName = name;

        if (isImage)
        {
            string[] data = message.Split(',');
            if (data.Length != 2)
                return;

            foreach(Text msgObj in user_message)
            {
                msgObj.gameObject.SetActive(false);
            }

            if (image_group)
            {
                image_group.SetActive(true);

                Texture2D texture = new Texture2D(64, 64);

                texture.LoadImage(System.Convert.FromBase64String(data[1]));
                user_image.gameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(texture.width, texture.height);
                user_image.texture = texture;
            }
        }
        else
        {
            foreach (Text msgObj in user_message)
            {
                msgObj.gameObject.SetActive(false);
            }

            if (image_group)
            {
                image_group.SetActive(false);
            }

            string[] checker = message.Split(new string[] { spliter[1] }, StringSplitOptions.None);

            string cardData = checker.Length > 1 ? message.Substring(message.IndexOf(spliter[0]) + spliter[0].Length, message.IndexOf(spliter[1]) - message.IndexOf(spliter[0]) - spliter[0].Length) : "";
            if (!string.IsNullOrEmpty(cardData))
            {   
                user_message[0].transform.parent.gameObject.SetActive(false);
                image_group.SetActive(true);
                user_image.gameObject.SetActive(true);

                JObject json = JObject.Parse(cardData);
                JToken jtk;
                uint cardNo = 0;
                float x = 0.0f;
                float y = 0.0f;
                float w = 0.0f;
                float h = 0.0f;
                string memo = "";
                uint date = 0;

                if (json.TryGetValue("CardNo", out jtk))
                {
                    cardNo = jtk.Value<uint>();
                }
                if (json.TryGetValue("x", out jtk))
                {
                    x = jtk.Value<float>();
                }
                if (json.TryGetValue("y", out jtk))
                {
                    y = jtk.Value<float>();
                }
                if (json.TryGetValue("w", out jtk))
                {
                    w = jtk.Value<float>();
                }
                if (json.TryGetValue("h", out jtk))
                {
                    h = jtk.Value<float>();
                }
                if (json.TryGetValue("memo", out jtk))
                {
                    memo = jtk.Value<string>();
                }
                if (json.TryGetValue("date", out jtk))
                {
                    date = jtk.Value<uint>();
                }
                CardJsonData = json;
                CardJsonData.Add("owner", name);

                object obj;
                List<game_data> card_list = GameDataManager.Instance.GetGameData(GameDataManager.DATA_TYPE.CARD_DEFINE);
                foreach (game_data data in card_list)
                {
                    if (data.data.TryGetValue("card_id", out obj))
                    {
                        if ((uint)obj == cardNo)
                        {
                            //user_image.GetComponent<AspectRatioFitter>().enabled = false;
                            //user_image.GetComponent<LayoutElement>().enabled = false;

                            card_define cardDefineData = (card_define)data;
                            user_image.texture = Resources.Load<Sprite>(cardDefineData.GetResourcePath()).texture;
                            user_image.uvRect = new Rect(x, y, w, h);

                            RectTransform rt = user_image.GetComponent<RectTransform>();
                            float ratio = (h / w) * 0.75f;
                            Vector2 size = rt.sizeDelta;
                            size.y = size.x * ratio;
                            float offsetY = rt.sizeDelta.y - size.y;

                            RectTransform parent = (user_image.transform.parent as RectTransform);
                            if (size.x > parent.sizeDelta.x - 10)
                            {
                                size.x = parent.sizeDelta.x - 10;
                            }

                            rt.sizeDelta = size;

                            LayoutElement le = image_group.GetComponent<LayoutElement>();
                            le.minHeight -= offsetY;
                            le.preferredHeight -= offsetY;
                        }
                    }
                }

                card_memo.text = memo;
                userMsg = memo;
                //if (date > 0)
                //{
                //    DateTime time = new DateTime(1970, 1, 1, 0, 0, 0, 0).AddSeconds(date);
                //    card_date.text = time.ToString(string.Format("yyyy.MM.dd"));
                //}
                //else
                    card_date.text = "";
            }
            else
            {
                user_message[0].transform.parent.gameObject.SetActive(true);
                user_image.gameObject.SetActive(false);

                if (message.Length > 19)
                {
                    string firstline = message.Substring(0, 18);
                    user_message[0].gameObject.SetActive(true);
                    user_message[0].text = firstline;
                    string seconline = message.Substring(18);
                    if (seconline.Length > 19)
                    {
                        string thirdline = seconline.Substring(18);
                        seconline = seconline.Substring(0, 18);
                        if (thirdline.Length > 19)
                        {
                            thirdline = thirdline.Substring(0, 18);
                        }
                        user_message[2].gameObject.SetActive(true);
                        user_message[2].text = thirdline;
                    }

                    user_message[1].gameObject.SetActive(true);
                    user_message[1].text = seconline;
                }
                else
                {
                    user_message[0].gameObject.SetActive(true);
                    user_message[0].text = message;
                }

                userMsg = message;
            }
        }

        if (!string.IsNullOrEmpty(url) && profile_image)
            StartCoroutine(OnStartDownloadProfile(url));

        StartCoroutine(RefreshLayout());
    }


    public IEnumerator OnStartDownloadProfile(string url)
    {
        if (profile_image)
        {
            UnityWebRequest request = UnityWebRequestTexture.GetTexture(url);
            yield return request.SendWebRequest();
            if (request.isNetworkError || request.isHttpError)
                Debug.Log(request.error);
            else
            {
                Texture2D tex = ((DownloadHandlerTexture)request.downloadHandler).texture;
                Sprite sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(tex.width / 2, tex.height / 2));
                profile_image.overrideSprite = sprite;
            }
        }
    }

    public void SetOpacityForce(float alpha, bool action)
    {
        //iTween tween = GetComponent<iTween>();
        //if (tween)
        //{
        //    Destroy(tween);
        //    iTween.ValueTo(gameObject, iTween.Hash("from", user_name.color.a, "to", alpha, "easetype", iTween.EaseType.easeInQuad, "time", 0.5f, "onupdate", "SetOpacity", "onupdatetarget", gameObject));
        //}
        //else

        SetOpacity(alpha);
        bAlphaAction = action;
        if (bAlphaAction)
            delayTime = 2.0f;
    }

    public void SetOpacity(float alpha)
    {        
        curAlpha = alpha;
        if(curAlpha < 0)
        {
            bAlphaAction = false;
        }

        Image img = gameObject.GetComponent<Image>();
        if (img != null)
        {
            Color c = img.color;
            c.a = curAlpha;
            img.color = c;
        }

        Color color = user_name.color;
        color.a = curAlpha;
        user_name.color = color;

        foreach (Text msgObj in user_message)
        {
            if (msgObj.gameObject.activeSelf)
            {
                color = msgObj.color;
                color.a = curAlpha;
                msgObj.color = color;
            }
        }

        if (user_image)
        {
            color = user_image.color;
            color.a = curAlpha;
            user_image.color = color;
        }
        if (profile_image)
        {
            color = profile_image.color;
            color.a = curAlpha;
            profile_image.color = color;
        }

        //color = msg_time.color;
        //color.a = curAlpha;
        //msg_time.color = color;
    }

    private void Update()
    {
        if(bAlphaAction)
        {
            if(delayTime > 0)
            {
                delayTime -= Time.deltaTime;
                return;
            }

            SetOpacity(curAlpha - Time.deltaTime);
        }

        if(curHeight != ChildPanel.sizeDelta.y)
        {
            if (transform.parent == null)
            {
                Destroy(gameObject);
                return;
            }

            RectTransform curRt = (transform as RectTransform);
            Vector2 size = curRt.sizeDelta;
            size.y = ChildPanel.sizeDelta.y;
            curRt.sizeDelta = size;
            curHeight = size.y;
            
            LayoutRebuilder.ForceRebuildLayoutImmediate(transform.parent as RectTransform);
        }

        if(transform.parent == null)
        {
            Destroy(this.gameObject);
        }

        if (user_image.gameObject.activeInHierarchy)
        {
            RectTransform parent = (user_image.transform.parent as RectTransform);
            RectTransform picture = user_image.transform as RectTransform;
            Vector2 size = picture.sizeDelta;
            size.x = parent.sizeDelta.x - 10;
            picture.sizeDelta = size;
        }
    }

    public void updateSize(float val)
    {
        
    }

    public void OnCardImageSelect()
    {
        if(CardJsonData != null)
        {
            //Canvas canvas = GetComponentInParent<Canvas>();
            //GameCanvas gameCanvas = canvas.GetComponent<GameCanvas>();
            //((CardCanvas)gameCanvas.GameManager.CardCanvas).CardDetailPopup.OnShow(CardJsonData);
        }
    }

    public void OnNameSelect()
    {
        NecoCanvas.GetPopupCanvas().ShowIDChecker(accountNo, userName, userMsg);
    }

    IEnumerator RefreshLayout()
    {
        // 원인 불명.. 2프레임에 걸쳐 최소 2회 갱신해야 정상 작동함

        yield return new WaitForEndOfFrame();

        RectTransform layoutRect = transform as RectTransform;

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
