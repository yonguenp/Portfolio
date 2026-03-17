using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ContentLocker : MonoBehaviour
{
    public static uint ContentGuideDoneSeq = 47;

    public uint ContentID = 0;
    public GameObject guidePrefab;
    public RectTransform targetUIRectTransform;
    public Sprite targetSprite;
    public float delay = 0.0f;

    private bool bSameSeq = false;
    private bool bGuideShown = false;
    private bool bForceActive = false;
    private GameObject ContentGuideUI = null;

    public void ClearForce()
    {
        bSameSeq = false;
        bGuideShown = false;
        bForceActive = false;

        if (ContentGuideUI)
            Destroy(ContentGuideUI);
        ContentGuideUI = null;
    }

    private void OnEnable()
    {
        RefreshUnlockStatus();
    }

    public static void CheckContentSeqDone(uint targetSeq)
    {
        if (ContentLocker.GetCurContentSeq() == targetSeq)
        {
            ContentLocker.SendContentSeq((int)targetSeq + 1);
        }
    }

    public static uint GetCurContentSeq()
    {
        uint curContentSeq = 0;
        game_data userData = GameDataManager.Instance.GetUserData();
        if (userData != null)
        {
            object obj;
            if (userData.data.TryGetValue("contents", out obj))
            {
                curContentSeq = (uint)obj;
            }
        }

        return curContentSeq;
    }

    public static void SendContentSeq(int index)
    {
#if UNITY_EDITOR
        GameObject canvas = GameObject.Find("SAMANDA_CANVAS");
        if (canvas == null)
        {
            GameDataManager.Instance.GetUserData().data["contents"] = (uint)index;
            UpdateUnlockSeq();
            return;
        }
#endif
        WWWForm data = new WWWForm();
        data.AddField("api", "user");
        data.AddField("val", index);

        NetworkManager.GetInstance().SendApiRequest("user", 4, data, (response) => {

        });
    }

    bool IsUnlockedContent()
    {
        if (ContentID == 0)
            return false;

        uint curSeq = GetCurContentSeq();

        if (curSeq == 0)
            return false;

        bSameSeq = curSeq == ContentID;

        return curSeq >= ContentID;
    }

    public bool IsNeedUnlockEffect()
    {
        //int effectSeq = PlayerPrefs.GetInt("UnlockEffect", 0);
        //if (effectSeq == ContentID)
        //    return false;

        if (guidePrefab == null)
            return false;

        return true;
    }

    public void RefreshUnlockStatus()
    {
        if (IsUnlockedContent())
        {
            if (bSameSeq && !bGuideShown && IsNeedUnlockEffect() && gameObject.activeInHierarchy)
            {
                ShowUnlockEffect();
            }
        }
    }

    public void ShowUnlockEffect()
    {
        if (guidePrefab == null)
            return;

        if (bGuideShown)
            return;

        bGuideShown = true;

        GameObject guide = Instantiate(guidePrefab);
        ContentGuide contentGuide = guide.GetComponent<ContentGuide>();
        List<string> GuideText = new List<string>();
        string key = "";
        string value = "";
        int index = 0;
        do
        {
            key = "CS" + ContentID.ToString("00") + "-" + index.ToString("00");
            value = LocalizeData.GetText(key);

            if(key != value)
            {
                GuideText.Add(value);
            }
            index++;
        } while (key != value);

        contentGuide.SetGuide((int)ContentID, GuideText.ToArray(), targetUIRectTransform, targetSprite, GetComponent<Button>(), delay);
        ContentGuideUI = guide;
        //PlayerPrefs.SetInt("UnlockEffect", (int)ContentID);
    }

    [ContextMenu("FORCE ON")]
    public void ForceActive()
    {
        bForceActive = true;
        RefreshUnlockStatus();
    }

    [ContextMenu("TEST")]
    public void UnlockEffectTest()
    {
        bGuideShown = false;
        ShowUnlockEffect();
    }
    
    [ContextMenu("LockID")]
    public void ContentLockID()
    {
        Dictionary<uint, List<ContentLocker>> lockers = new Dictionary<uint, List<ContentLocker>>();

        GameObject[] roots = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();
        foreach (GameObject root in roots)
        {
            if (root.GetComponent<Canvas>() != null)
            {
                ContentLocker[] contentLockers = root.GetComponentsInChildren<ContentLocker>(true);
                foreach (ContentLocker contentLocker in contentLockers)
                {
                    if (!lockers.ContainsKey(contentLocker.ContentID))
                        lockers[contentLocker.ContentID] = new List<ContentLocker>();

                    lockers[contentLocker.ContentID].Add(contentLocker);
                }
            }
        }

        foreach(KeyValuePair<uint, List<ContentLocker>> pair in lockers)
        {
            List<ContentLocker> list = (pair.Value);
            if(list.Count > 1)
            {
                foreach (ContentLocker locker in list)
                {
                    Debug.Log(locker.gameObject.name + " : " + pair.Key);
                }
            }
        }
    }

    public static void UpdateUnlockSeq()
    {
        GameObject[] roots = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();
        foreach(GameObject root in roots)
        {
            if (root.GetComponent<Canvas>() != null)
            {
                ContentLocker[] contentLockers = root.GetComponentsInChildren<ContentLocker>(true);
                foreach (ContentLocker contentLocker in contentLockers)
                {
                    contentLocker.RefreshUnlockStatus();
                }
            }
        }
    }

    public static uint GetCurPivotSeq(uint Seq)
    {
        object obj;
        
        if (Seq >= 1 && Seq <= 4)//탐험-실패
        {
            Seq = 1;//to world
        }
        else if(Seq == 5)
        {
            Seq = 1;

            users user = GameDataManager.Instance.GetUserData();
            if (user != null)
            {
                if (user.data.TryGetValue("gold", out obj))
                {
                    uint money = (uint)obj;
                    if (money >= 10)
                        Seq = 6;
                }
            }
        }
        else if (Seq >= 6 && Seq <= 7)//낚시
        {
            Seq = 6;//to world
        }
        else if(Seq == 8)
        {
            Seq = 6;

            uint itemID = 11;
            List<game_data> user_items = GameDataManager.Instance.GetGameData(GameDataManager.DATA_TYPE.USER_ITEMS);
            foreach (game_data item in user_items)
            {
                if(((user_items)item).GetItemID() == itemID)
                {
                    if (((user_items)item).GetAmount() >= 2)
                        Seq = 9;
                }
            }
        }
        else if (Seq >= 9 && Seq <= 15)//요리하기
        {
            Seq = 9;//to world
        }
        else if (Seq == 16)
        {
            Seq = 9;

            uint itemID = 14;
            List<game_data> user_items = GameDataManager.Instance.GetGameData(GameDataManager.DATA_TYPE.USER_ITEMS);
            foreach (game_data item in user_items)
            {
                if (((user_items)item).GetItemID() == itemID)
                {
                    if (((user_items)item).GetAmount() >= 1)
                        Seq = 18;
                }
            }
        }
        else if (Seq >= 17 && Seq <= 21)//밥주기, 레벨업
        {
            Seq = 18;//to world
        }
        else if (Seq == 22)
        {
            Seq = 18;

            users user = GameDataManager.Instance.GetUserData();
            if (user != null)
            {
                if (user.data.TryGetValue("level", out obj))
                {
                    uint level = (uint)obj;
                    if (level >= 2)
                        Seq = 25;
                }
            }
        }
        else if (Seq >= 23 && Seq <= 26)//탐험 성공
        {
            Seq = 25;//to world            
        }
        else if(Seq == 27)
        {
            Seq = 25;

            uint cardID = 1021;
            List<game_data> user_items = GameDataManager.Instance.GetGameData(GameDataManager.DATA_TYPE.USER_CARD);
            foreach (game_data card in user_items)
            {
                if (((user_card)card).GetCardID() == cardID)
                {
                    Seq = 29;
                }
            }
            //users user = GameDataManager.Instance.GetUserData();
            //if (user != null)
            //{
            //    if (user.data.TryGetValue("level", out obj))
            //    {
            //        uint level = (uint)obj;
            //        if (level >= 3)
            //            Seq = 29;
            //    }
            //}
        }        
        else if (Seq >= 28 && Seq <= 39)//앨범
        {
            Seq = 29;//to main
        }
        else if (Seq >= 40 && Seq <= 41)//사진
        {
            Seq = 40;//to main
        }
        else if (Seq >= 42 && Seq <= 43)//뽑
        {
            Seq = 43;//to shop
        }
        else if (Seq >= 44 && Seq <= 45)//뽑
        {
            Seq = 45;//to shop
        }

        return Seq;
    }
}
