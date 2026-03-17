using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterFilterUI : MonoBehaviour
{
    public enum FILTER_TYPE
    {
        GAIN_DATE = 1,
        LEVEL = 2,
        GRADE = 3
    };

    [SerializeField]
    GameObject filterListUI;
    [SerializeField]
    ScrollUIController targetScroll;
    [SerializeField]
    GameObject descIcon;
    [SerializeField]
    GameObject ascIcon;
    [SerializeField]
    Text curFilterText;
    public bool desc { get; private set; } = true;
    FILTER_TYPE curFilter = FILTER_TYPE.GAIN_DATE;

    private void Awake()
    {
        descIcon.SetActive(true);
        ascIcon.SetActive(false);
    }

    public void OnFilterButton()
    {
        filterListUI.SetActive(!filterListUI.activeSelf);
    }

    private void OnEnable()
    {
        filterListUI.SetActive(false);
        OnFiltering(curFilter);
    }

    private void OnDisable()
    {
        filterListUI.SetActive(false);
    }

    public void ToggleDesc()
    {
        desc = !desc;

        descIcon.SetActive(desc);
        ascIcon.SetActive(!desc);

        OnFiltering(curFilter);
    }
    public void OnFiltering(int type)
    {
        OnFiltering((FILTER_TYPE)type);
        filterListUI.SetActive(false);
    }


    public void OnFiltering(FILTER_TYPE type)
    {
        curFilter = type;
        switch (curFilter)
        {
            case FILTER_TYPE.GAIN_DATE:
                if(desc)
                    targetScroll.OnSorting(SortGainDateDesc);
                else
                    targetScroll.OnSorting(SortGainDateAsc);
                break;
            case FILTER_TYPE.LEVEL:
                if (desc)
                    targetScroll.OnSorting(SortLevelDesc);
                else
                    targetScroll.OnSorting(SortLevelAsc);
                break;
            case FILTER_TYPE.GRADE:
                if (desc)
                    targetScroll.OnSorting(SortGradeDesc);
                else
                    targetScroll.OnSorting(SortGradeAsc);
                break;
        }
        SetCurFilteringUI();
    }

    void SetCurFilteringUI()
    {
        if(curFilterText != null)
        {
            switch (curFilter)
            {
                case FILTER_TYPE.GAIN_DATE:
                    curFilterText.text = StringManager.GetString("char_sorting_timeline");;
                    break;
                case FILTER_TYPE.LEVEL:
                    curFilterText.text = StringManager.GetString("char_sorting_level"); 
                    break;
                case FILTER_TYPE.GRADE:
                    curFilterText.text = StringManager.GetString("char_sorting_rare"); 
                    break;
            }
        }
    }

    public static int SortGainDateDesc(ScrollUIControllerItem x, ScrollUIControllerItem y)
    {
        UserCharacterData yc = (y as CharacterListItem).userData;
        if (CheckDefaultCharacter(yc))
            return 1;

        UserCharacterData xc = (x as CharacterListItem).userData;
        if (CheckDefaultCharacter(xc))
            return -1;

        if(xc.gainDate == yc.gainDate)
        {
            return xc.characterData.GetID() > yc.characterData.GetID() ? -1 : 1;
        }

        return xc.gainDate > yc.gainDate ? -1 : 1;
    }

    public static int SortGainDateAsc(ScrollUIControllerItem x, ScrollUIControllerItem y)
    {
        UserCharacterData yc = (y as CharacterListItem).userData;
        if (CheckDefaultCharacter(yc))
            return 1;

        UserCharacterData xc = (x as CharacterListItem).userData;
        if (CheckDefaultCharacter(xc))
            return -1;

        if (xc.gainDate == yc.gainDate)
        {
            return xc.characterData.GetID() > yc.characterData.GetID() ? 1 : -1;
        }

        return xc.gainDate > yc.gainDate ? 1 : -1;
    }


    public static int SortLevelDesc(ScrollUIControllerItem x, ScrollUIControllerItem y)
    {
        UserCharacterData yc = (y as CharacterListItem).userData;
        if (CheckDefaultCharacter(yc))
            return 1;

        UserCharacterData xc = (x as CharacterListItem).userData;
        if (CheckDefaultCharacter(xc))
            return -1;

        if (xc.lv == yc.lv)
        {
            return xc.characterData.GetID() > yc.characterData.GetID() ? -1 : 1;
        }

        return xc.lv > yc.lv ? -1 : 1;
    }

    public static int SortLevelAsc(ScrollUIControllerItem x, ScrollUIControllerItem y)
    {
        UserCharacterData yc = (y as CharacterListItem).userData;
        if (CheckDefaultCharacter(yc))
            return 1;

        UserCharacterData xc = (x as CharacterListItem).userData;
        if (CheckDefaultCharacter(xc))
            return -1;

        if (xc.lv == yc.lv)
        {
            return xc.characterData.GetID() > yc.characterData.GetID() ? 1 : -1;
        }

        return xc.lv > yc.lv ? 1 : -1;
    }
    public static int SortGradeDesc(ScrollUIControllerItem x, ScrollUIControllerItem y)
    {
        UserCharacterData yc = (y as CharacterListItem).userData;
        if (CheckDefaultCharacter(yc))
            return 1;

        UserCharacterData xc = (x as CharacterListItem).userData;
        if (CheckDefaultCharacter(xc))
            return -1;

        if (xc.characterData.char_grade == yc.characterData.char_grade)
        {
            return xc.characterData.GetID() > yc.characterData.GetID() ? -1 : 1;
        }

        return xc.characterData.char_grade > yc.characterData.char_grade ? -1 : 1;
    }

    public static int SortGradeAsc(ScrollUIControllerItem x, ScrollUIControllerItem y)
    {
        UserCharacterData yc = (y as CharacterListItem).userData;
        if (CheckDefaultCharacter(yc))
            return 1;

        UserCharacterData xc = (x as CharacterListItem).userData;
        if (CheckDefaultCharacter(xc))
            return -1;

        if (xc.characterData.char_grade == yc.characterData.char_grade)
        {
            return xc.characterData.GetID() > yc.characterData.GetID() ? 1 : -1;
        }

        return xc.characterData.char_grade > yc.characterData.char_grade ? 1 : -1;
    }

    static bool CheckDefaultCharacter(UserCharacterData data)
    {
        if (Managers.UserData.MyDefaultChaserCharacter == data.characterData.GetID())
            return true;

        if (Managers.UserData.MyDefaultSurvivorCharacter == data.characterData.GetID())
            return true;

        return false;
    }

    public static int SortGradeNotMineDesc(ScrollUIControllerItem x, ScrollUIControllerItem y)
    {
        CharacterGameData xg = (x as CharacterListItem).charData;
        CharacterGameData yg = (y as CharacterListItem).charData;

        if (xg.is_limited != yg.is_limited)
        {
            return xg.is_limited - yg.is_limited;
        }

        if (xg.char_grade == yg.char_grade)
        {
            return xg.GetID() > yg.GetID() ? -1 : 1;
        }
        return xg.char_grade > yg.char_grade ? -1 : 1;
    }

    public static int SortGradeNotMineAsc(ScrollUIControllerItem x, ScrollUIControllerItem y)
    {
        CharacterGameData xg = (x as CharacterListItem).charData;
        CharacterGameData yg = (y as CharacterListItem).charData;

        if (xg.is_limited != yg.is_limited)
        {
            return xg.is_limited - yg.is_limited;
        }

        if (xg.char_grade == yg.char_grade)
        {
            return xg.GetID() > yg.GetID() ? 1 : -1;
        }
        return (x as CharacterListItem).charData.char_grade > (y as CharacterListItem).charData.char_grade ? 1 : -1;
    }
}
