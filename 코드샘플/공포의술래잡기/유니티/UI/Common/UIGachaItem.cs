using Coffee.UIExtensions;
using DG.Tweening;
using Spine.Unity;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class UIGachaItem : MonoBehaviour
{
    [SerializeField] BoneFollowerGraphic boneFollowerGraphic;
    [SerializeField] SkeletonGraphic card_Grade;

    [SerializeField] GameObject normalCharacter;
    [SerializeField] GameObject normalItem;
    [SerializeField] Text itemCount;

    [SerializeField] UIParticle particle_garde_S;
    [SerializeField] UIParticle particle_garde_A;
    [SerializeField] UIParticle particle_spatula_normal;
    [SerializeField] UIParticle particle_spatula_special;
    [SerializeField] UIParticle card_open_normal01; // 한개오픈일때 나오도록

    [SerializeField] GameObject chaserbg;
    [SerializeField] GameObject surviverbg;


    Sequence _sq;
    GameObject mainObj = null;
    SkeletonGraphic bgSpine = null;
    CharacterGameData character_d;
    ItemGameData item_d;

    SBWeb.ResponseReward originData = null;

    bool isPush = false;

    public SkeletonGraphic GetSkeleton()
    {
        return card_Grade;
    }
    public void OnDisable()
    {
        _sq.Kill();
        _sq = null;
        card_Grade.color = new Color(1, 1, 1, 1);
        character_d = null;
        item_d = null;

        if (this.gameObject.GetComponent<Button>() != null)
            this.gameObject.GetComponent<Button>().enabled = true;
        (this.transform as RectTransform).localScale = Vector3.one;
        isPush = false;

    }

    public void CharacterInit(CharacterGameData data, SBWeb.ResponseReward origin = null)
    {
        character_d = data;
        originData = origin;
        SetActiveClear();

        mainObj = normalCharacter;

        BGSetActive();
        SetCardImage(character_d.char_grade, "idle");
    }

    public void BGSetActive()
    {
        chaserbg.SetActive(character_d.IsChaserCharacter());
        surviverbg.SetActive(character_d.IsSuvivorCharacter());

        if (chaserbg.activeSelf)
            bgSpine = chaserbg.GetComponent<SkeletonGraphic>();
        if(surviverbg.activeSelf)
            bgSpine = surviverbg.GetComponent<SkeletonGraphic>();
    }

    public void ItemInit(ItemGameData data, int count, SBWeb.ResponseReward origin = null)
    {
        originData = origin;
        item_d = data;
        SetActiveClear();
        mainObj = null;
        mainObj = normalItem;
        itemCount.text = count > 1 ? "×" + count.ToString() : "";

        int char_grade = 0;
        if (origin != null && origin.Type == SBWeb.ResponseReward.RandomRewardType.CHARACTER)
        {
            CharacterGameData charData = CharacterGameData.GetCharacterData(origin.Id);
            if (charData != null)
                char_grade = charData.char_grade;
        }

        SetCardImage(char_grade, "idle");
    }
    public void SetCardImage(int grade, string state = null, bool loop = true)
    {
        particle_garde_S.gameObject.SetActive(false);
        particle_garde_A.gameObject.SetActive(false);

        string start_ani = string.Empty;
        if (state == "idle")
        {
            switch (grade)
            {
                case 3:
                    start_ani = "idle_gradeA";
                    particle_garde_A.gameObject.SetActive(true);
                    particle_garde_A.Play();
                    card_Grade.timeScale = 2f;
                    break;
                case 4:
                    start_ani = "idle_gradeS";
                    particle_garde_S.gameObject.SetActive(true);
                    particle_garde_S.Play();
                    card_Grade.timeScale = 3f;
                    break;
                default:
                    start_ani = "idle_gradeB";
                    card_Grade.timeScale = Random.Range(0.5f, 1f);
                    break;
            }
            card_Grade.color = Color.white;
        }
        else
        {
            switch (grade)
            {
                case 3:
                    start_ani = "open_gradeA";
                    break;
                case 4:
                    start_ani = "open_gradeS";
                    break;
                default:
                    start_ani = "open_gradeB";
                    break;
            }
            card_Grade.timeScale = 1f;
        }
        card_Grade.startingLoop = loop;
        card_Grade.startingAnimation = start_ani;
        card_Grade.Initialize(true);
    }
    public void SetActive(bool enable)
    {
        gameObject.SetActive(enable);
    }
    public void SetActiveClear(bool isOn = false)
    {
        normalCharacter.SetActive(isOn);
        normalItem.SetActive(isOn);
    }


    public void OnButton()
    {
        var popup = PopupCanvas.Instance.GetPopup(PopupCanvas.POPUP_TYPE.GACHARESULT_POPUP) as GachaResultPopup;
        if (mainObj == null || mainObj.activeSelf)
            return;

        //카드의 등급
        int g = 0;
        if (item_d != null && item_d.type == ItemGameData.ITEM_TYPE.CHAR_PIECE)
            g = CharacterGameData.GetCharacterData(item_d.GetID()).char_grade;
        else if (character_d != null)
            g = character_d.char_grade;

        //캐릭터 아이템 같이 획득 시 캐릭터 먼저 보여주기
        if (item_d != null && item_d.type == ItemGameData.ITEM_TYPE.CHAR_PIECE && popup.FindCharacterData(item_d.GetID()) && !popup.changeToCharacter.Contains(item_d.GetID()))
        {
            CharacterGameData temp = CharacterGameData.GetCharacterData(item_d.GetID());
            character_d = temp;
            Debug.Log("캐릭터 아이템 스왑함");
            popup.changeToCharacter.Add(item_d.GetID());
            mainObj = normalCharacter;
            BGSetActive();

            item_d = null;
        }
        else if (character_d != null)
        {
            if (popup.changeToCharacter.Contains(character_d.GetID()))
            {
                Debug.Log("캐릭터였는데 아이템으로 변경되었음");
                ItemGameData tempItem = ItemGameData.GetItemData(character_d.GetID());
                item_d = tempItem;
                itemCount.text = "x5";
                mainObj = normalItem;
                character_d = null;
            }
        }

        if (this.gameObject.GetComponent<Button>() != null)
            this.gameObject.GetComponent<Button>().enabled = false;


        particle_spatula_normal.gameObject.SetActive(false);
        particle_spatula_special.gameObject.SetActive(false);
        card_open_normal01.gameObject.SetActive(false);
        if (g < 4)
        {
            if (popup.resultDatas.Count == 1)
                card_open_normal01.gameObject.SetActive(true);
            else
                particle_spatula_normal.gameObject.SetActive(true);
            particle_spatula_normal.Play();
            card_open_normal01.Play();
        }
        else if (g == 4)
        {
            particle_spatula_special.gameObject.SetActive(true);
            particle_spatula_special.Play();
        }

        //아이템에 다른 ui 조절
        if (item_d != null)
        {
            Managers.Sound.Play("effect/EF_GACHA_OPEN", Sound.Effect);
            int tempid = 0;
            if (originData == null)
                tempid = item_d.GetID();
            else
                tempid = originData.Id;

            CharacterGameData cdata = null;
            
            int grade = 0;
            if (item_d.type == ItemGameData.ITEM_TYPE.CHAR_PIECE)
            {
                cdata = CharacterGameData.GetCharacterData(tempid);
            }

            if(cdata != null)
                grade = cdata.char_grade;
            SetCardImage(grade, "open", false);

            card_Grade.AnimationState.Complete += delegate
            {
                card_Grade.DOColor(new Color(1, 1, 1, 0), 0.1f).OnComplete(() =>
                {
                    if (popup.OnGachaItemCnt() == 1)
                    {
                        (this.transform as RectTransform).localScale *= 1.8f;
                    }

                    //particle_spatula_normal.gameObject.SetActive(false);
                    //particle_spatula_special.gameObject.SetActive(false);
                    popup.openCardCnt++;


                    //카드를 다 열면 버튼 종류 변경
                    if (popup.OnGachaItemCnt() == 1 && popup.openCardCnt == 1)
                    {
                        if (popup.openCardCnt == 1)
                            popup.ButtonGroupOn();
                    }
                    else
                    {
                        if (popup.openCardCnt == 11)
                            popup.ButtonGroupOn();
                    }
                });

                mainObj.SetActive(true);
                if (cdata != null)
                    mainObj.transform.Find("NameText").GetComponent<Text>().text = cdata.GetName();
                else
                    mainObj.transform.Find("NameText").GetComponent<Text>().text = item_d.GetName();
                mainObj.transform.Find("ItemImage").GetComponent<Image>().sprite = item_d.sprite;

                itemCount.gameObject.SetActive(false);
                mainObj.transform.DOScale(Vector3.zero, 0.2f).SetEase(Ease.OutQuad).From().OnComplete(() =>
                {
                    itemCount.gameObject.SetActive(true);
                    itemCount.transform.DOScale(Vector3.zero, 0.05f).From().SetEase(Ease.OutQuad);
                });
            };

            popup.Invoke("SetSkip", 0.1f);
        }

        else if (character_d != null)
        {
            popup.changeToCharacter.Add(character_d.GetID());

            SetCardImage(character_d.char_grade, "open", false);
            card_Grade.AnimationState.Complete += delegate
            {
                VibrateManager.OnVibrate(0.5f + (0.1f * character_d.char_grade), 50 * character_d.char_grade);
                card_Grade.DOColor(new Color(1, 1, 1, 0), 0.1f).OnComplete(() =>
                {
                    if (popup.OnGachaItemCnt() == 1)
                    {
                        (this.transform as RectTransform).localScale *= 1.8f;
                    }

                    mainObj.SetActive(true);
                    mainObj.transform.Find("NameText").GetComponent<Text>().text = character_d.GetName();
                    mainObj.transform.Find("ItemImage").GetComponent<SkeletonGraphic>().skeletonDataAsset = character_d.spine_resource;
                    mainObj.transform.Find("ItemImage").GetComponent<SkeletonGraphic>().startingAnimation = "f_idle_0";
                    mainObj.transform.Find("ItemImage").GetComponent<SkeletonGraphic>().startingLoop = true;
                    mainObj.transform.Find("ItemImage").GetComponent<SkeletonGraphic>().Initialize(true);

                    bgSpine.GetComponent<SkeletonGraphic>().startingAnimation = "f_play_0";
                    bgSpine.GetComponent<SkeletonGraphic>().startingLoop = true;
                    bgSpine.GetComponent<SkeletonGraphic>().Initialize(true);

                    Sequence sq = DOTween.Sequence();
                    sq.AppendInterval(0.65f);
                    sq.AppendCallback(() =>
                    {
                        PopupCanvas.Instance.ShowNewCharacter(character_d, () =>
                        {
                            popup.isSkip = true;
                        });
                    });

                    //particle_spatula_normal.gameObject.SetActive(false);
                    //particle_spatula_special.gameObject.SetActive(false);
                    popup.openCardCnt++;


                    //카드를 다 열면 버튼 종류 변경
                    if (popup.OnGachaItemCnt() == 1 && popup.openCardCnt == 1)
                    {
                        if (popup.openCardCnt == 1)
                            popup.ButtonGroupOn();
                    }
                    else
                    {
                        if (popup.openCardCnt == 11)
                            popup.ButtonGroupOn();
                    }
                });
            };
        }
    }


    public void PressButton()
    {
        if (isPush || (mainObj != null && mainObj.activeSelf))
            return;
        StartCoroutine(OnOpenButton());
    }
    public void DePressButton()
    {
        card_Grade.timeScale = 1f;
        StopAllCoroutines();
    }
    public IEnumerator OnOpenButton()
    {
        particle_spatula_normal.gameObject.SetActive(false);
        particle_spatula_special.gameObject.SetActive(false);
        card_open_normal01.gameObject.SetActive(false);

        var popup = PopupCanvas.Instance.GetPopup(PopupCanvas.POPUP_TYPE.GACHARESULT_POPUP) as GachaResultPopup;
        if (mainObj.activeSelf)
            yield return null;

        //카드의 등급
        int g = 0;
        if (item_d != null && item_d.type == ItemGameData.ITEM_TYPE.CHAR_PIECE)
        {
            CharacterGameData char_data = CharacterGameData.GetCharacterData(item_d.GetID());
            if (char_data != null)
            {
                g = char_data.char_grade;
            }
        }
        else if (character_d != null)
            g = character_d.char_grade;

        //등급별 시간초 설정
        float duration = 0;
        if (g == 3)
            duration = 0.05f;
        else if (g == 4)
            duration = 0.1f;

        if (g > 2)
        {
            isPush = true;
            float time = 0f;
            while (time < duration)
            {
                time += Time.deltaTime;
                Debug.Log(time);
                card_Grade.timeScale = 10f;
                yield return new WaitForEndOfFrame();

                if (time < duration)
                {
                    isPush = false;
                    continue;
                }

                else
                {
                    isPush = true;
                    card_Grade.timeScale = 1f;
                }
            }
        }
        else
        {
            isPush = true;
        }

        //캐릭터 아이템 같이 획득 시 캐릭터 먼저 보여주기
        if (item_d != null && item_d.type == ItemGameData.ITEM_TYPE.CHAR_PIECE && popup.FindCharacterData(item_d.GetID()) && !popup.changeToCharacter.Contains(item_d.GetID()))
        {
            CharacterGameData temp = CharacterGameData.GetCharacterData(item_d.GetID());
            character_d = temp;
            Debug.Log("캐릭터 아이템 스왑함");
            popup.changeToCharacter.Add(item_d.GetID());
            mainObj = normalCharacter;
            BGSetActive();

            item_d = null;
        }
        else if (character_d != null)
        {
            if (popup.changeToCharacter.Contains(character_d.GetID()))
            {
                Debug.Log("캐릭터였는데 아이템으로 변경되었음");
                ItemGameData tempItem = ItemGameData.GetItemData(character_d.GetID());
                item_d = tempItem;
                itemCount.text = "x5";
                mainObj = normalItem;
                character_d = null;
            }
        }

        if (this.gameObject.GetComponent<Button>() != null)
            this.gameObject.GetComponent<Button>().enabled = false;

        if (g < 4)
        {
            if (popup.resultDatas.Count == 1)
                card_open_normal01.gameObject.SetActive(true);
            else
                particle_spatula_normal.gameObject.SetActive(true);
            particle_spatula_normal.Play();
            card_open_normal01.Play();
        }
        else if (g == 4)
        {
            particle_spatula_special.gameObject.SetActive(true);
            particle_spatula_special.Play();
        }

        //아이템에 다른 ui 조절
        if (item_d != null)
        {
            Managers.Sound.Play("effect/EF_GACHA_OPEN", Sound.Effect);
            int tempid = 0;
            if (originData == null)
                tempid = item_d.GetID();
            else
                tempid = originData.Id;

            int char_grade = 0;

            CharacterGameData char_data = null;
            if (item_d.type == ItemGameData.ITEM_TYPE.CHAR_PIECE)
            {
                char_data = CharacterGameData.GetCharacterData(tempid);
                if (char_data != null)
                {
                    char_grade = char_data.char_grade;
                }
            }
            SetCardImage(char_grade, "open", false);

            card_Grade.AnimationState.Complete += delegate
            {
                card_Grade.DOColor(new Color(1, 1, 1, 0), 0.1f).OnComplete(() =>
                {
                    if (popup.OnGachaItemCnt() == 1)
                    {
                        (this.transform as RectTransform).localScale *= 1.8f;
                    }

                    //particle_spatula_normal.gameObject.SetActive(false);
                    //particle_spatula_special.gameObject.SetActive(false);
                    popup.openCardCnt++;


                    //카드를 다 열면 버튼 종류 변경
                    if (popup.OnGachaItemCnt() == 1 && popup.openCardCnt == 1)
                    {
                        if (popup.openCardCnt == 1)
                            popup.ButtonGroupOn();
                    }
                    else
                    {
                        if (popup.openCardCnt == 11)
                            popup.ButtonGroupOn();
                    }
                });

                mainObj.SetActive(true);

                mainObj.transform.Find("NameText").GetComponent<Text>().text = char_data != null ? char_data.GetName() : item_d.GetName();
                mainObj.transform.Find("ItemImage").GetComponent<Image>().sprite = item_d.sprite;

                itemCount.gameObject.SetActive(false);
                mainObj.transform.DOScale(Vector3.zero, 0.2f).SetEase(Ease.OutQuad).From().OnComplete(() =>
                {
                    itemCount.gameObject.SetActive(true);
                    itemCount.transform.DOScale(Vector3.zero, 0.05f).From().SetEase(Ease.OutQuad);
                });
            };

            popup.Invoke("SetSkip", 0.1f);
        }

        else if (character_d != null)
        {
            popup.changeToCharacter.Add(character_d.GetID());

            SetCardImage(character_d.char_grade, "open", false);
            card_Grade.AnimationState.Complete += delegate
            {
                VibrateManager.OnVibrate(0.5f + (0.1f * character_d.char_grade), 50 * character_d.char_grade);
                card_Grade.DOColor(new Color(1, 1, 1, 0), 0.1f).OnComplete(() =>
                {
                    if (popup.OnGachaItemCnt() == 1)
                    {
                        (this.transform as RectTransform).localScale *= 1.8f;
                    }

                    mainObj.SetActive(true);
                    mainObj.transform.Find("NameText").GetComponent<Text>().text = character_d.GetName();
                    mainObj.transform.Find("ItemImage").GetComponent<SkeletonGraphic>().skeletonDataAsset = character_d.spine_resource;
                    mainObj.transform.Find("ItemImage").GetComponent<SkeletonGraphic>().startingAnimation = "f_idle_0";
                    mainObj.transform.Find("ItemImage").GetComponent<SkeletonGraphic>().startingLoop = true;
                    mainObj.transform.Find("ItemImage").GetComponent<SkeletonGraphic>().Initialize(true);

                    bgSpine.GetComponent<SkeletonGraphic>().startingAnimation = "f_play_0";
                    bgSpine.GetComponent<SkeletonGraphic>().startingLoop = true;
                    bgSpine.GetComponent<SkeletonGraphic>().Initialize(true);


                    Sequence sq = DOTween.Sequence();
                    sq.AppendInterval(0.65f);
                    sq.AppendCallback(() =>
                    {
                        PopupCanvas.Instance.ShowNewCharacter(character_d, () =>
                        {
                            popup.isSkip = true;
                        });
                    });

                    //particle_spatula_normal.gameObject.SetActive(false);
                    //particle_spatula_special.gameObject.SetActive(false);
                    popup.openCardCnt++;


                    //카드를 다 열면 버튼 종류 변경
                    if (popup.OnGachaItemCnt() == 1 && popup.openCardCnt == 1)
                    {
                        if (popup.openCardCnt == 1)
                            popup.ButtonGroupOn();
                    }
                    else
                    {
                        if (popup.openCardCnt == 11)
                            popup.ButtonGroupOn();
                    }
                });
            };
        }
    }
    public void CharacterDetailOn()
    {
        PopupCanvas.Instance.ShowNewCharacter(character_d, null, false);
    }

    public void ItemDetailOn()
    {
        if (item_d == null)
        {
            ItemDetailOff();
            return;
        }

        PopupCanvas.Instance.ShowTooltip(item_d.GetName(), item_d.GetDesc(), transform.position);
    }

    public void ItemDetailOff()
    {
        PopupCanvas.Instance.ClosePopup(PopupCanvas.POPUP_TYPE.TOOLTIP_POPUP);
    }
}
