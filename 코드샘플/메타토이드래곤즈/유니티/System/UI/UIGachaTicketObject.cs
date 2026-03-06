using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork
{
    public class UIGachaTicketObject: UIObject, EventListener<ItemFrameEvent>
    {
        [SerializeField] private Text AmountLabel = null;
		[SerializeField] private int itemNo = 0;

        void OnEnable()
        {
            EventManager.AddListener(this);
        }

        void OnDisable()
        {
            EventManager.RemoveListener(this);
        }
        public override void Init()
        {
            base.Init();
        }
        public override void InitUI(eUIType targetType)
        {
            base.InitUI(targetType);
            RefreshTicketCount();
        }
        public override bool RefreshUI(eUIType targetType)
        {
            if (base.RefreshUI(targetType))
            {
                RefreshTicketCount();
            }
            
            return curSceneType != targetType;
        }

        public void RefreshTicketCount()
        {
            if (AmountLabel != null)
            {
                InventoryItem useritem = User.Instance.GetItem(itemNo);
                if(useritem == null || useritem.Amount <= 0)
                {
                    AmountLabel.text = "0";
                    return;
                }
                
                AmountLabel.text = SBFunc.CommaFromNumber(useritem.Amount);
            }
        }

		public void OnEvent(ItemFrameEvent eventType)
		{
			switch (eventType.Event)
			{
				case ItemFrameEvent.ItemFrameEventEnum.ITEM_UPDATE:
					RefreshTicketCount();
					break;
			}
		}
	}
}
