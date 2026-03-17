using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork {

    public struct PortraitChangeEvent
    {
        static PortraitChangeEvent e;
        public int portraitNum;
        public PortraitChangeEvent(int _portraitNum)
        {
            portraitNum = _portraitNum;
        }
        public static void RefreshPortrait(int portraitNo)
        {
            e.portraitNum = portraitNo;
            EventManager.TriggerEvent(e);
        }
        public static void RefreshPortrait(string portraitNo)
        {
            int defaultValue = 0;
            int.TryParse(portraitNo, out defaultValue);
            e.portraitNum = defaultValue;
            EventManager.TriggerEvent(e);
        }
    }
    public class PortraitPopup : Popup<PopupData>
    {
        [SerializeField] private GameObject portraitBox;
        [SerializeField] private Transform contents;
        [SerializeField] private TableViewGrid tableView;
        public delegate void callback();
        callback SaveCallBack;
        string currentSelectKey;
        List<CharBaseData> portraitCharbaseList;
        private PortraitBox lastSelectBox;

        bool isInit = false;
        bool isDefaultPortrait=false;
        public void SelectBox(string tag, PortraitBox box)
        {
            lastSelectBox?.SetCheck(false);
            currentSelectKey = tag;
            lastSelectBox = box;
        }

        public override void InitUI()
        {
            if (isInit==false)
            {
                tableView.OnStart();
                isInit = true;
            }


            portraitCharbaseList = new List<CharBaseData>();
            foreach(var dragon in User.Instance.DragonData.GetAllUserDragons())
            {
                portraitCharbaseList.Add(dragon.BaseData);
            }

            currentSelectKey = User.Instance.UserData.UserPortrait;

            int key = string.IsNullOrEmpty(currentSelectKey) ? 0 : int.Parse(currentSelectKey);
            portraitCharbaseList.Sort((CharBaseData a, CharBaseData b) =>
            {
                if (key == a.KEY)
                {
                    return 1;
                }
                else if (key == b.KEY)
                {
                    return -1;
                }
                else
                {
                    var ret = User.Instance.DragonData.IsFavorite(a.KEY).CompareTo(User.Instance.DragonData.IsFavorite(b.KEY));
                    if(ret == 0)
                    {
                        if (a.GRADE == b.GRADE)
                        {
                            return a.ELEMENT - b.ELEMENT;
                        }
                        else
                        {
                            return a.GRADE - b.GRADE;
                        }
                    }
                    return ret;
                }
            });
            portraitCharbaseList.Reverse();

            isDefaultPortrait = true;
            List<ITableData> list = new List<ITableData>();
            foreach (var item in portraitCharbaseList)
            {
                if(item.KEY == key)
                {
                    isDefaultPortrait = false;
                }
                list.Add(item);
            }

            tableView.SetDelegate(new TableViewDelegate(list, (GameObject node, ITableData item) => {
                if (node == null)
                {
                    return;
                }
                var frame = node.GetComponent<PortraitBox>();
                if (frame == null)
                {
                    return;
                }
                
                if (item ==null)
                {
                    frame.Init(null, isDefaultPortrait, SelectBox);
                }
                else
                {
                    var charData = (CharBaseData)item;
                    if (key == charData.KEY)
                    {
                        frame.Init(charData, true, SelectBox);
                        lastSelectBox = frame;
                    }
                    else
                    {
                        frame.Init(charData, false, SelectBox);
                    }
                }

            }));
            tableView.ReLoad();
        }
        public void OnClickChange()
        {
            if (int.TryParse(User.Instance.UserData.UserPortrait, out int tag))
            {
                if (Town.TownDragonsDic.ContainsKey(tag))
                {
                    Town.TownDragonsDic[tag].Data.RemoveDragonState(eDragonState.Guild);
                    Town.TownDragonsDic[tag].OnEvent(new DragonShowEvent());
                }
            }

            User.Instance.UserData.UpdatePortrait(currentSelectKey);
            PortraitChangeEvent.RefreshPortrait(currentSelectKey);

            WWWForm param = new WWWForm();
            param.AddField("icon", currentSelectKey);
            NetworkManager.Send("user/icon", param, (JObject jsonData) => {
                SaveCallBack?.Invoke();
            });
            ClosePopup();
        }
        public void SetUICallback(callback cb)
        {
            if(cb != null) { 
                SaveCallBack = cb;
            }
        }
    }
}