using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 주사위 보드판에 한 칸을 구성하는 슬롯. - 전체 데이터 제어는 diceboardController에서 할 예정.
/// 아이템 세팅은 itemPrefab을 튜닝해서 쓰는 걸로.
/// 보드 연출도 해야함.
/// </summary>
namespace SandboxNetwork
{
    public class DiceBoardSlot : MonoBehaviour
    {
        [SerializeField] GameObject startNode = null;
        [SerializeField] ItemFrame item = null;

        [SerializeField] GameObject glow = null;
        [SerializeField] Image bg = null;
        [SerializeField] List<Sprite> bgSprite = new List<Sprite>();


        [SerializeField] GameObject effectNode = null;

        Sequence startSlotSeq = null;
        public int SlotIndex { get; private set; }//diceBoard 데이터 테이블의 board_id , 시작 발판은 -1

        DiceBoardData data = null;

        public void SetSlotData(int _curIndex, DiceBoardData _data)
        {
            if (item == null)
                item = GetComponentInChildren<ItemFrame>();

            if(_data == null)
            {
                Debug.LogError("DiceBoardData is null :  board_id " + (SlotIndex));
                return;
            }

            data = _data;
            SlotIndex = _data.BOARD_ID;

            if (_data == null)
                Debug.LogError("DiceBoardData is null :  board_id" + (SlotIndex));
            else//홀더 일때와 아닐때 아이템 차이가 있음.
            {
                SetItemSlot(_data.REWARD_ID);
            }

            if(SlotIndex == 0 && startNode != null)
                SetSequence();

            SetBGSprite(_data.RARITY);
            SetGlow(_curIndex);
            SetEffectNodeVisible(false);
        }
        void SetSequence()
        {
            if (startSlotSeq != null)
                startSlotSeq.Kill();

            item.transform.Rotate(new Vector3(0, 90, 0));
            startNode.transform.Rotate(new Vector3(0f, 0f, 0f));

            startSlotSeq = DOTween.Sequence();
            startSlotSeq.AppendInterval(1.0f);
            startSlotSeq.Append(startNode.transform.DOLocalRotate(new Vector3(0f, 90f, 0f), 0.2f));
            startSlotSeq.Append(item.transform.DOLocalRotate(new Vector3(0f, 0f, 0f), 0.2f));
            startSlotSeq.AppendInterval(1.0f);
            startSlotSeq.Append(item.transform.DOLocalRotate(new Vector3(0f, 90f, 0f), 0.2f));
            startSlotSeq.Append(startNode.transform.DOLocalRotate(new Vector3(0f, 0f, 0f), 0.2f));
            
            startSlotSeq.SetLoops(-1);
            startSlotSeq.Play();
        }

        void SetItemSlot(int _itemGroupKey)//슬롯에 대한 보상은 단일 Asset 가져오기
        {
            var itemDataList = ItemGroupData.Get(_itemGroupKey);
            if (itemDataList == null || itemDataList.Count <= 0)
                return;

            if(itemDataList.Count > 2)//혹시 모를 로그
            {
                Debug.LogError("DiceBoardSlot itemList count is " + itemDataList.Count + " itemGroupKey : " + _itemGroupKey);
            }

            var slotItem = itemDataList[0].Reward;
            if (slotItem == null)
                return;

            if (item != null)
                item.SetFrameItem(slotItem);
        }

        void SetBGSprite(int _rarity)
        {
            if (bg == null || bgSprite == null || bgSprite.Count <= 0 || _rarity >= bgSprite.Count)
                return;

            bg.sprite = bgSprite[_rarity];
        }

        void SetGlow(int _curIndex)
        {
            if (glow != null)
                glow.SetActive(data.BOARD_ID == _curIndex);
        }

        public void SetEffectNodeVisible(bool _isVisible)
        {
            if (effectNode != null)
                effectNode.SetActive(_isVisible);
        }

        public void SetGlowVisible(bool _isVisible)//연출용으로 제어할 생각.
        {
            if (glow != null)
                glow.SetActive(_isVisible);
        }
    }
}

