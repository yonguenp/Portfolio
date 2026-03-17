using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RecipeInfoItem : MonoBehaviour
{
    public Image outputIcon;
    public Text outputName;
    public Text outputEffectDesc;
    public Text outputDesc;

    public Image inputIcon;
    public Text inputCount;

    private List<KeyValuePair<uint, uint>> input;
    private KeyValuePair<uint, uint> output;
    public void SetRecipeInfoData(List<KeyValuePair<uint, uint>> _input, KeyValuePair<uint, uint> _output)
    {
        input = _input;
        output = _output;

        foreach (KeyValuePair<uint, uint> need in input)
        {
            items item = items.GetItem(need.Key);
            inputIcon.sprite = item.GetItemIcon();
            inputCount.text = "x" + need.Value;            
        }

        items outputItem = items.GetItem(output.Key);
        outputIcon.sprite = outputItem.GetItemIcon();
        outputName.text = outputItem.GetItemName();
        outputDesc.text = outputItem.GetItemDesc();
        
        List<game_data> fullness = GameDataManager.Instance.GetGameData(GameDataManager.DATA_TYPE.FOOD_FULLNESS);
        object obj;
        
        foreach (game_data f in fullness)
        {
            if (f.data.TryGetValue("item_id", out obj))
            {
                if (output.Key == (uint)obj)
                {
                    if (f.data.TryGetValue("fullness", out obj))
                        outputEffectDesc.text = "포만감+" + ((uint)obj).ToString();
                }
            }
        }
    }

}
