using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TopUIGroup : UIGroup
{
    public enum TOP_UI { LEVEL, MONEY, CONFIG };
    public enum UI_STATE { NORMAL, HIDE, };

    //level control
    public Text curLevel;
    private uint curDisplayLevel = 0;
    public Image curExpGuage;
    private uint curDisplayExp = 0;

    //money control
    public Text curMoney;

    //config control
    public Button configButton;

    //levelup effect
    public AppearPanel CatAppearPanel;
    public RewardListUI RewardListUI;

    private Coroutine ExpAnimation = null;
    public void Awake()
    {
        configButton.onClick.AddListener(OnConfigButton);
    }

    public override void SetUI(bool enable)
    {
        foreach(GameObject ui in UI)
        {
            ui.SetActive(enable);
        }

        if (enable)
        {
            Refresh();
        }
    }

    public override void Refresh()
    {
        users user = GameDataManager.Instance.GetUserData();
        if (user == null)
            return;


        object obj;
        if (curDisplayLevel == 0)
        {
            if (user.data.TryGetValue("level", out obj))
            {
                curDisplayLevel = (uint)obj;
            }
        }
        curLevel.text = curDisplayLevel.ToString();

        if (ExpAnimation != null)
            StopCoroutine(ExpAnimation);

        float curRatio = 0.0f;
        if (user.data.TryGetValue("exp", out obj))
        {
            if (curDisplayExp == 0)
            {
                curDisplayExp = (uint)obj;
                List<game_data> exp_table = GameDataManager.Instance.GetGameData(GameDataManager.DATA_TYPE.EXP);
                int curLevelIndex = (int)curDisplayLevel - 1;
                int nextLevelIndex = (int)curDisplayLevel;
                if (exp_table.Count > curLevelIndex && exp_table.Count > nextLevelIndex)
                {
                    object cur;
                    object next;
                    if (exp_table[curLevelIndex].data.TryGetValue("exp", out cur) && exp_table[nextLevelIndex].data.TryGetValue("exp", out next))
                    {
                        uint curExpRange = (uint)next - (uint)cur;
                        uint curLevelExpGain = curDisplayExp - (uint)cur;

                        curRatio = (float)((double)curLevelExpGain / curExpRange);                        
                    }
                }

                if (curRatio > 1.0f)
                {
                    curRatio = 1.0f;
                    curExpGuage.fillAmount = curRatio;
                    Refresh();
                    return;
                }
                else
                    curExpGuage.fillAmount = curRatio;
            }
            else
            {
                ExpAnimation = StartCoroutine(PlayExpAnimation((uint)obj));
            }
        }


        uint money = 0;
        if (user.data.TryGetValue("gold", out obj))
        {
            money = (uint)obj;            
        }
        curMoney.text = money.ToString("n0");
    }

    public void OnConfigButton()
    {
        FarmUIPanel.FarmCanvas.GameManager.ConfigUI.OnShowConfigUI();
    }

    public IEnumerator PlayExpAnimation(uint targetExp)
    {
        uint goalExp = targetExp;
        float targetRatio = curExpGuage.fillAmount;

        List<game_data> exp_table = GameDataManager.Instance.GetGameData(GameDataManager.DATA_TYPE.EXP);
        int curLevelIndex = System.Convert.ToInt32(curLevel.text) - 1;
        int nextLevelIndex = System.Convert.ToInt32(curLevel.text);
        if (exp_table.Count > curLevelIndex && exp_table.Count > nextLevelIndex)
        {
            object cur;
            object next;
            if (exp_table[curLevelIndex].data.TryGetValue("exp", out cur) && exp_table[nextLevelIndex].data.TryGetValue("exp", out next))
            {
                int curExpRange = System.Convert.ToInt32(next) - System.Convert.ToInt32(cur);
                int curLevelExpGain = System.Convert.ToInt32(targetExp) - System.Convert.ToInt32(cur);
                float curRatio = ((float)curLevelExpGain / curExpRange);
                targetRatio = curRatio;
                if (targetRatio >= 1.0f)//levelup?
                {
                    targetRatio = 1.0f;
                    targetExp = (uint)next;
                }
            }
        }

        float perRatio = (targetRatio - curExpGuage.fillAmount) * 0.1f;
        float perExpVal = (targetExp - curDisplayExp) * 0.1f;
        while (curExpGuage.fillAmount < targetRatio)
        {
            curExpGuage.fillAmount += perRatio;
            curDisplayExp += System.Convert.ToUInt32(perExpVal);

            if (curExpGuage.fillAmount > targetRatio)
            {
                curExpGuage.fillAmount = targetRatio;
                curDisplayExp = targetExp;
            }
            yield return new WaitForSeconds(0.1f);
        }

        curExpGuage.fillAmount = targetRatio;
        curDisplayExp = targetExp;

        if (targetRatio >= 1.0f)
        {
            curExpGuage.fillAmount = 0.0f;
            curDisplayLevel += 1;
            curLevel.text = curDisplayLevel.ToString();

            cat_def open_cat = null;
            List<game_data> cats = GameDataManager.Instance.GetGameData(GameDataManager.DATA_TYPE.CAT_DEF);
            foreach (game_data data in cats)
            {
                cat_def cat = (cat_def)data;
                if (cat.GetApearLevel() == curDisplayLevel)
                {
                    open_cat = cat;
                }
            }

            if (open_cat != null)
            {
                yield return CatAppearPanel.PlayAppearMovie(open_cat);

                FarmUIPanel.FarmCanvas.WorldCatManager.SetCatAnimation(open_cat);
            }

            if (RewardListUI.ShowLevelUpRewardList(curDisplayLevel))
            {
                while (RewardListUI.isShow)
                {
                    yield return new WaitForSeconds(0.01f);
                }
            }

            ExpAnimation = StartCoroutine(PlayExpAnimation(goalExp));
        }
        else
        {
            curDisplayExp = 0;
            curDisplayLevel = 0;
            Refresh();
        }
    }

}
