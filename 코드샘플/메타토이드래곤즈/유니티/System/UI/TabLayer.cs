using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SandboxNetwork
{
    public class TabLayer : MonoBehaviour
    {
        protected TabTypePopupData Datas { get; private set; } = null;
        public int LayerIndex { get; protected set; } = -1;

        public int titleIndex = 0;

        public int SubLayerIndex { get; private set; } = -1;
        public virtual void InitUI(TabTypePopupData datas = null)//데이터가 있는 갱신
        {
            if(datas != null)
                Datas = datas;
        }

        public virtual void RefreshUI()//데이터 유지 갱신
        {
        }
        public virtual void RefreshReddot()//레드닷 갱신
        {
        }

        public virtual void ServerUpdate(JToken _jsonData, bool _flag) 
        { 
        }
        public virtual void RefreshUIButtonState()
        {
        }
        public virtual GameObject[] GetSubLayerList()
        {
            return null;
        }

        public virtual void SetLayerIndex(int _index)
        {
            LayerIndex = _index;
        }

        public virtual void SetSubLayerIndex(int _index)
        {
            SubLayerIndex = _index;
        }

        public void InitSubLayerIndex()
        {
            SubLayerIndex = -1;
        }
        public void ClearData()
        {
            Datas = null;
        }
        public virtual string GetTitleText()
        {
            if (titleIndex <= 0)
                return "";

            return StringData.GetStringByIndex(titleIndex);
        }
    }
}