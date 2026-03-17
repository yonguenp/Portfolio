using System.Collections.Generic;
using UnityEngine;

public class CheatButton : MonoBehaviour
{
    private void Awake()
    {
#if !UNITY_EDITOR && !SB_TEST
        gameObject.SetActive(false);
#endif
    }

    public void OnGetAllCharacter()
    {
#if UNITY_EDITOR || SB_TEST
        OnGetCheatCharacter();
#endif
    }

    void OnGetCheatCharacter()
    {
#if UNITY_EDITOR || SB_TEST
        List<GameData> charList = GameDataManager.Instance.GetData(GameDataManager.DATA_TYPE.character);
        bool isAll = true;
        foreach (CharacterGameData charData in charList)
        {
            if (charData.use)
            {
                var ud = Managers.UserData.GetMyCharacterInfo(charData.GetID());
                if (ud == null)
                {
                    isAll = false;
                    break;
                }
            }
        }

        if (!isAll)
        {
            OnChore(1, 1);
        }
        else
        {
            int en = int.MaxValue;
            int sk = int.MaxValue;

            foreach (CharacterGameData charData in charList)
            {
                if (charData.use)
                {
                    var ud = Managers.UserData.GetMyCharacterInfo(charData.GetID());
                    if (ud != null)
                    {
                        if (ud.enchant < en)
                            en = ud.enchant;
                        if (ud.skillLv < sk)
                            sk = ud.skillLv;
                    }
                }
            }

            if(en == sk)
            {
                en += 1;
            }
            else
            {
                sk = en;
            }

            if(en > 5 || sk > 5)
            {
                OnChoreLevel();
                return;
            }

            OnChore(en, sk);
        }
#endif
    }
#if UNITY_EDITOR || SB_TEST
    void OnChore(int en, int sk)
    {
        List<GameData> charList = GameDataManager.Instance.GetData(GameDataManager.DATA_TYPE.character);
        GameData targetData = null;
        foreach (CharacterGameData charData in charList)
        {
            if (charData.use)
            {
                var ud = Managers.UserData.GetMyCharacterInfo(charData.GetID());
                if (ud == null)
                {
                    targetData = charData;
                    break;
                }
                else
                {
                    if(ud.enchant < en || ud.skillLv < sk)
                    {
                        targetData = charData;
                        break;
                    }
                }
            }
        }

        if (targetData == null)
        {
            PopupCanvas.Instance.ShowFadeText(StringManager.GetString($"치사하게 강화 {en} 등급, 스킬 {sk} 레벨 달성"));
            var lobby = Managers.Scene.CurrentScene as LobbyScene;
            if (lobby != null)
            {
                lobby.RefreshUI();
            }

            return;
        }

        SBWeb.OnObtainCharacter(targetData.GetID(), en, sk, () => { OnChore(en, sk); });
    }

    void OnChoreLevel()
    {
        int target = 0;
        foreach (var charData in Managers.UserData.MyCharacters)
        {
            if (charData.Value.characterData.use)
            {
                if (charData.Value.lv < 25)
                {
                    target = charData.Key;
                }
            }
        }

        if (target == 0)
        {
            OnChoreEquips();
            return;
        }

        SBWeb.OnLvCheat(target, 25, () => { OnChoreLevel(); });
    }

    void OnChoreEquips()
    {
        int target = 0;
        int[] equips = {
            9004,
            9008,
            9012,
            9016,
            9020,
            9024,
            9028,
            9032,
            9036,
            9040,
            9044,
            9048,
            9052,
            9056,
            9060,
            9064,
            9068,
            9072,
            9076,
            9080,
            9084,
            9088,
            9092,
            9096,
            9100,
            9104
        };

        for (int i = 0; i < equips.Length; i++)
        {
            bool need = true;
            foreach (var equip in Managers.UserData.MyEquips)
            {
                if (equip.Value.item_no == equips[i])
                {
                    need = false;
                    break;
                }
            }

            if(need)
            {
                target = equips[i];
                break;
            }
        }

        SBWeb.OnGetAllMaxEquips(equips, () => {

            PopupCanvas.Instance.ShowFadeText(StringManager.GetString("더 줄게 없다."));
            var lobby = Managers.Scene.CurrentScene as LobbyScene;
            if (lobby != null)
            {
                lobby.RefreshUI();
            }
        });
        
    }
#endif
}
