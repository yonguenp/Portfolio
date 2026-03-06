using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class RewardItemUI : MonoBehaviour
{
    [SerializeField] ParticleSystem effect;
    [SerializeField] UIBundleItem item;
    [SerializeField] Text name;

    public void SetActive(bool enable)
    {
        gameObject.SetActive(enable);
    }

    public void SetRewardInfo(SBWeb.ResponseReward reward)
    {
        if (effect != null)
        {
            effect.Play();
            item.transform.DOScale(1.0f, 0.2f).From().SetEase(Ease.OutBounce);
        }
        
        item.SetReward(reward);

        switch (reward.Type)
        {
            case SBWeb.ResponseReward.RandomRewardType.GOLD:
                name.text = StringManager.GetString("gold_name");
                break;
            case SBWeb.ResponseReward.RandomRewardType.DIA:
                name.text = StringManager.GetString("dia_name");
                break;
            case SBWeb.ResponseReward.RandomRewardType.ITEM:
                name.text = ItemGameData.GetItemData(reward.Id).GetName();
                break;
            case SBWeb.ResponseReward.RandomRewardType.CHARACTER:
                name.text = CharacterGameData.GetCharacterData(reward.Id).GetName();
                break;
        }
    }
}
