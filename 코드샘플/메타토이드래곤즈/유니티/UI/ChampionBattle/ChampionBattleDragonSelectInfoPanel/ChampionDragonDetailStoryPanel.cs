using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/*
 * 드래곤 Info쪽에서 스토리 버튼 클릭하면 
 * 드래곤의 이야기 서브 팝업 나옴
 */
namespace SandboxNetwork
{
    public class ChampionDragonDetailStoryPanel : ChampionDragonDetailSubPanel
    {
        [SerializeField]
        Text dragonName = null;
        [SerializeField]
        Text desc = null;

        public override void ShowPanel(VoidDelegate _successCallback = null)
        {
            base.ShowPanel(_successCallback);
        }

        public override void HidePanel()
        {
            base.HidePanel();
        }

        public override void Init()
        {
            base.Init();
            SetLabel();
        }

        void SetLabel()
        {
            if(dragonBase != null && dragonName != null && desc != null)
            {
                dragonName.text = StringData.GetStringByStrKey(dragonBase._NAME);
                desc.text = dragonBase._DESC == "0" ? StringData.GetStringByStrKey("스토리없는드래곤") : StringData.GetStringByStrKey(dragonBase._DESC);
            }
        }
    }
}
