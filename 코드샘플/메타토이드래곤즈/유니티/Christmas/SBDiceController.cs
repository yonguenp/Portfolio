using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace SandboxNetwork
{
    public class SBDiceController : MonoBehaviour
    {
        [SerializeField]
        List<Dice2D> diceList = new List<Dice2D>();

        [SerializeField]
        Animator anim = null;

        const string ANIMATOR_PREFIX_NAME = "dice_animation";

        public delegate void ListDelegate(List<int> _indexList);

//#if UNITY_EDITOR
//        public void Update()
//        {
//            DiceCheat();
//        }
//#endif

//#if UNITY_EDITOR
//        void DiceCheat()
//        {
//            if (Input.GetKeyDown(KeyCode.Alpha1))
//            {
//                OnSingleRandomAuto();
//            }
//            if (Input.GetKeyDown(KeyCode.Alpha2))
//            {
//                OnSingleRandomDoubleAuto();
//            }
//        }
//#endif
        public void SetDiceIdle()
        {
            if (diceList != null && diceList.Count > 0)
            {
                foreach (var dice in diceList)
                {
                    if (dice == null)
                        continue;
                    dice.OnRandomDiceIdle();
                }
            }
        }

        [ContextMenu("Random Single Dice Jump Shot")]
        public void OnSingleRandomAuto(int _resultNum = -1,float _startDelay = 0, VoidDelegate _callback = null)
        {
            if (diceList == null || diceList.Count <= 0)
                return;

            SetVisibleOffAllDice();

            if(_resultNum <= 0)//문제가 있음.
                _resultNum = SBFunc.Random(1, 7);

            diceList[0].SetVisible(true);
            diceList[0].OnRandomDiceJumpDrop(_resultNum, _startDelay, () => {
                _callback?.Invoke();
            });

            Debug.Log("Single Dice Result : " + _resultNum);
        }

        [ContextMenu("Random Dice Double Jump Shot")]
        public void OnSingleRandomDoubleAuto()
        {
            if (diceList == null || diceList.Count <= 0)
                return;

            SetVisibleOffAllDice();

            var currentResultNum = 0;

            foreach (var dice in diceList)
            {
                if (dice == null)
                    continue;

                dice.SetVisible(true);
                var resultNum = SBFunc.Random(1, 7);
                dice.OnRandomDiceJumpDrop(resultNum, 3);
                currentResultNum += resultNum;
            }

            Debug.Log("Double Dice Result : " + currentResultNum);
        }

        public void SetVisibleOffAllDice()
        {
            foreach (var dice in diceList)
            {
                if (dice == null)
                    continue;

                dice.gameObject.SetActive(false);
                dice.SetResultCallBack(null);
            }
        }

        public void OnDiceAnimator(int _resultNum)
        {
            if(anim != null)
            {
                diceList[0].SetVisible(true);
                anim.Play(ANIMATOR_PREFIX_NAME + _resultNum.ToString(),0,0);
            }
        }

        public void SetDiceLayer()
        {
            foreach (var dice in diceList)
            {
                if (dice == null)
                    continue;

                dice.SetDiceLayer();
            }
        }

        public void PlaySfxSound(string _sfxName)
        {
            if (!diceList[0].gameObject.activeInHierarchy)
                return;

            SoundManager.Instance?.PlaySFX(_sfxName);
        }
    }
}
