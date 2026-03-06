using SandboxNetwork;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WorldBossRewardInfoClone : MonoBehaviour
{
    [SerializeField]
    Text levelText;
    [SerializeField]
    ItemFrame itemSample;
    [SerializeField]
    string level;


    private void OnEnable()
    {
        foreach(Transform child in itemSample.transform.parent)
        {
            if (child == itemSample.transform)
                continue;

            Destroy(child.gameObject);
        }

        int lv = int.Parse(level);
        levelText.text = level;
        if (lv >= 21)
            levelText.text = level + "~";

        itemSample.gameObject.SetActive(true);

        var data = WorldBossLevelData.Get(WorldBossManager.Instance.UISelectBossKey, lv);
        if (data != null)
        {
            string strReward = data.REWARD_DESC;
            
            foreach (var pair in strReward.Split(","))
            {
                var reward = pair.Split(":");

                if (reward.Length == 2)
                {
                    var obj = Instantiate(itemSample.gameObject, itemSample.transform.parent);
                    ItemFrame frame = obj.GetComponent<ItemFrame>();

                    if (int.TryParse(reward[0], out int res))
                    {
                        frame.SetFrameItemInfo(res, 1);
                        frame.SetCustomText(reward[1]);
                    }
                    else
                    {
                        Debug.LogError("reward parse error : " + reward);
                    }
                }
            }
        }
        itemSample.gameObject.SetActive(false);
    }
}
