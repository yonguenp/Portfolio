using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork
{
    public class ChampionDragonDetailPopup : Popup<PopupBase>
    {
        [SerializeField]
        Text titlePopupLabel = null;
        [SerializeField]
        public int maxCnt = 0;

        [SerializeField]
        ChampionDragonDetailLayer championDragonDetailLayer = null;

        public int DragonTag { get { return Dragon != null ? Dragon.Tag : -1; } }
        public int PetTag { get { return Pet != null ? Pet.Tag : -1; } }

        //클릭한 드래곤의 덱 내 모든 드래곤 (5마리)
        public List<int> DragonInfoList { get; private set; } = new List<int>();

        public List<int> SelectedDragonList { get; private set; } = new List<int>();

        //드래곤 장비 관련 저장값
        public int PartTag { get; private set; } = 0;

        public UserDragonData Dragons { get; private set; } = null;
        public ChampionDragon Dragon { get; private set; } = null;
        public ChampionPet Pet { get; private set; } = null;

        UserPetData petData = null;

        public UserPetData PetData
        {
            get
            {
                if (petData == null)
                {
                    petData = new UserPetData();
                    foreach (var petBase in ChampionManager.GetSelectablePets())
                    {
                        petData.AddPet(new ChampionPet(petBase));
                    }
                }

                return petData;
            }
        }

        public void Clear()
        {
            Dragons = null;
            Dragon = null;
            Pet = null;
        }

        public void PetClear()
        {
            Pet = null;
        }

        public void SetData(UserDragonData _dragons, ChampionDragon _dragon)
        {
            Dragons = _dragons;
            Dragon = _dragon;
            Pet = Dragon.ChampionPet;

            ClearDragonInfoList();
            foreach (var dragon in Dragons.GetAllUserDragons())
            {
                DragonInfoList.Add(dragon.Tag);
            }

            if (championDragonDetailLayer != null)
                championDragonDetailLayer.Init();
        }

        public static void OpenPopup(ChampionDragon _dragon, UserDragonData _dragons)
        {
            var popup = PopupManager.OpenPopup<ChampionDragonDetailPopup>();
            popup.SetData(_dragons, _dragon);
        }

        public void onClickCloseBtn()
        {
            ClosePopup();
        }

        public override void InitUI()
        {

        }


        void RefreshTitleLabel(int labelIndex)
        {
            if (labelIndex <= 0)
            {
                return;
            }

            if (titlePopupLabel != null)
            {
                titlePopupLabel.text = StringData.GetStringByIndex(labelIndex);
            }
        }
        public void ClearDragonInfoList()
        {
            if (DragonInfoList == null)
                DragonInfoList = new List<int>();
            DragonInfoList.Clear();
        }
        
        public void ClearSelectedDragonList()
        {
            if (SelectedDragonList == null)
                SelectedDragonList = new List<int>();
            SelectedDragonList.Clear();
        }

        public override void OnClickDimd()
        {
            if (championDragonDetailLayer.IsPetInfoPanel())
            {
                championDragonDetailLayer.OnClickExitPetInfo();
                return;
            }
            Clear();
            base.ClosePopup();

        }
        public override void BackButton()
        {
            Clear();
            base.ClosePopup();
        }

        public override void ClosePopup()
        {
            Clear();
            base.ClosePopup();
        }


    }
}

