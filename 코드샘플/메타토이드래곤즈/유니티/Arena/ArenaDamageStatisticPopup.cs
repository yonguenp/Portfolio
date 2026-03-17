using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


namespace SandboxNetwork
{
    public class ArenaDamageStatisticPopup : Popup<ArenaResultPopupData>
    {
        [SerializeField]
        private DragonPortraitFrame[] dragonSlot;
        [SerializeField]
        private Button[] tabBtns; // 0 - 전체, 1 - 공격, 2 - 방어

        [SerializeField] private statisticClone[] MyInfoUI;   // 0~5 : 통합  5~10 : 공격 10~15 : 방어
        [SerializeField] private statisticClone[] EnemyInfoUI;

        [SerializeField]
        private Transform[] myInfoTrans;
        [SerializeField]
        private Transform[] enemyInfoTrans;

        [SerializeField]
        private Text timeLabel;


        [SerializeField] GameObject playerWin;
        [SerializeField] GameObject playerLose;
        [SerializeField] Text playerBp;
        [SerializeField] Text playerNick;
        [SerializeField] GameObject enemyWin;
        [SerializeField] GameObject enemyLose;
        [SerializeField] Text enemyBp;
        [SerializeField] Text enemyNick;


        const int maxDragonCount = 5;

        public override void Init(ArenaResultPopupData data)
        {
            base.Init(data);
            //  basicInfoSetting();
            StatisticSetting();
            activeSetting();
            portraitSetting();
            onClickTab("0");

            //여기서 데이터 받아서 그 크기에 따라 myInfoTrans 나 enemyInfoTrans 의 children [ index ] 끄기
        }
        public void basicInfoSetting(bool isWin, int time)
        {
            playerBp.text = ArenaManager.Instance.battleInfo.myBP.ToString();
            playerNick.text = User.Instance.UserData.UserNick;
            enemyBp.text = ArenaManager.Instance.battleInfo.enemyBP.ToString();
            enemyNick.text = ArenaManager.Instance.battleInfo.enemyNick;

            playerWin.SetActive(isWin);
            playerLose.SetActive(!isWin);
            enemyWin.SetActive(!isWin);
            enemyLose.SetActive(isWin);
            timeLabel.text = SBFunc.TimeStringMinute(time);
        }

        void portraitSetting()
        {
            for (int i = 0; i < Data.MyDragonCount; ++i)
            {
                UserDragon dragon = new UserDragon();
                dragonSlot[i].SetCustomPotraitFrame(Data.MyDragonData[i].DTag, Data.MyDragonData[i].Level);
            }
            for (int i = 0; i < Data.EnemyDragonCount; ++i)
            {
                UserDragon dragon = new UserDragon();
                dragonSlot[i + 5].SetCustomPotraitFrame(Data.EnemyDragonData[i].DTag, Data.EnemyDragonData[i].Level);
            }
        }
        void activeSetting()
        {
            if (Data.MyDragonCount < maxDragonCount)
            {
                for (int i = Data.MyDragonCount; i < maxDragonCount; ++i)
                {
                    MyInfoUI[i].gameObject.SetActive(false);
                    MyInfoUI[i + 5].gameObject.SetActive(false);
                    MyInfoUI[i + 10].gameObject.SetActive(false);
                    dragonSlot[i].gameObject.SetActive(false);
                }
            }
            if (Data.EnemyDragonCount < maxDragonCount)
            {
                for (int i = Data.EnemyDragonCount; i < maxDragonCount; ++i)
                {
                    EnemyInfoUI[i].gameObject.SetActive(false);
                    EnemyInfoUI[i + 5].gameObject.SetActive(false);
                    EnemyInfoUI[i + 10].gameObject.SetActive(false);
                    dragonSlot[i + 5].gameObject.SetActive(false);
                }
            }
        }
        void StatisticSetting()
        {
            for (int i = 0; i < Data.MyDragonCount; ++i)
            {
                MyInfoUI[i].setAtkData(Data.MyDragonData[i].Damage, Data.MyAtk == 0 ? 0 : Data.MyDragonData[i].Damage / (float)Data.MyAtk, Data.MyAtk);
                MyInfoUI[i].setDefData(Data.MyDragonData[i].TakenDamage, Data.MyDragonData[i].TrueDamage, Data.MyDef == 0 ? 0 : Data.MyDragonData[i].TakenDamage / (float)Data.MyDef, Data.MyDef);
                MyInfoUI[i + 5].setAtkData(Data.MyDragonData[i].Damage, Data.MyAtk == 0 ? 0 : Data.MyDragonData[i].Damage / (float)Data.MyAtk, Data.MyAtk);
                MyInfoUI[i + 10].setDefData(Data.MyDragonData[i].TakenDamage, Data.MyDragonData[i].TrueDamage, Data.MyDef == 0 ? 0 : Data.MyDragonData[i].TakenDamage / (float)Data.MyDef, Data.MyDef);
            }
            for (int i = 0; i < Data.EnemyDragonCount; ++i)
            {
                EnemyInfoUI[i].setAtkData(Data.EnemyDragonData[i].Damage, Data.EnemyAtk == 0 ? 0 : Data.EnemyDragonData[i].Damage / (float)Data.EnemyAtk, Data.EnemyAtk);
                EnemyInfoUI[i].setDefData(Data.EnemyDragonData[i].TakenDamage, Data.EnemyDef == 0 ? 0 : Data.EnemyDragonData[i].TrueDamage, Data.EnemyDragonData[i].TakenDamage / (float)Data.EnemyDef, Data.EnemyDef);
                EnemyInfoUI[i + 5].setAtkData(Data.EnemyDragonData[i].Damage, Data.EnemyAtk == 0 ? 0 : Data.EnemyDragonData[i].Damage / (float)Data.EnemyAtk, Data.EnemyAtk);
                EnemyInfoUI[i + 10].setDefData(Data.EnemyDragonData[i].TakenDamage, Data.EnemyDragonData[i].TrueDamage, Data.EnemyDef == 0 ? 0 : Data.EnemyDragonData[i].TakenDamage / (float)Data.EnemyDef, Data.EnemyDef);
            }
        }

        public void onClickTab(string customED)
        {
            tabBtns[0].SetInteractable(true);
            tabBtns[1].SetInteractable(true);
            tabBtns[2].SetInteractable(true);

            myInfoTrans[0].gameObject.SetActive(false);
            myInfoTrans[1].gameObject.SetActive(false);
            myInfoTrans[2].gameObject.SetActive(false);
            enemyInfoTrans[0].gameObject.SetActive(false);
            enemyInfoTrans[1].gameObject.SetActive(false);
            enemyInfoTrans[2].gameObject.SetActive(false);

            int tabIndex = int.Parse(customED);
            myInfoTrans[tabIndex].gameObject.SetActive(true);
            enemyInfoTrans[tabIndex].gameObject.SetActive(true);
            tabBtns[tabIndex].SetInteractable(false);
            sliderTweenStart(tabIndex);

        }
        void sliderTweenStart(int index)
        {
            for (int i = 0; i < Data.MyDragonCount; ++i)
            {
                MyInfoUI[index * 5 + i].sliderTweenStart();
            }
            for (int i = 0; i < Data.EnemyDragonCount; ++i)
            {
                EnemyInfoUI[index * 5 + i].sliderTweenStart();
            }
        }

        public override void InitUI()
        {

        }
    }
}
