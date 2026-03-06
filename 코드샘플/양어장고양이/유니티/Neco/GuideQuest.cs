using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GuideQuest : MonoBehaviour
{
    public Text guideText;

    public Image guideGuage;
    public Text guideGuageText;

    //public GameObject RewardPanel;
    //public Image RewardItem;
    //public Text RewardText;

    public bool SetGuideUI()
    {
        neco_data.PrologueSeq seq = neco_data.GetPrologueSeq();
        if (seq >= neco_data.PrologueSeq.배틀패스보상받기)
        {
            string[] guidestring =
            {
                LocalizeData.GetText("LOCALIZE_462"),
                LocalizeData.GetText("LOCALIZE_213"),
                LocalizeData.GetText("LOCALIZE_214"),
                LocalizeData.GetText("LOCALIZE_215"),
                LocalizeData.GetText("LOCALIZE_216"),
                LocalizeData.GetText("LOCALIZE_217"),
                LocalizeData.GetText("LOCALIZE_218"),
                LocalizeData.GetText("LOCALIZE_219"),
                LocalizeData.GetText("LOCALIZE_220"),
                LocalizeData.GetText("LOCALIZE_221"),
                LocalizeData.GetText("LOCALIZE_222"),
                LocalizeData.GetText("LOCALIZE_223"),
                LocalizeData.GetText("LOCALIZE_224"),
                LocalizeData.GetText("LOCALIZE_225"),
                LocalizeData.GetText("LOCALIZE_226"),
                LocalizeData.GetText("LOCALIZE_227"),
                LocalizeData.GetText("LOCALIZE_228"),
                LocalizeData.GetText("LOCALIZE_407"),
                LocalizeData.GetText("LOCALIZE_408"),
                LocalizeData.GetText("LOCALIZE_409"),
                LocalizeData.GetText("LOCALIZE_410"),
                LocalizeData.GetText("LOCALIZE_411"),
                LocalizeData.GetText("LOCALIZE_412"),
                LocalizeData.GetText("LOCALIZE_413"),
                LocalizeData.GetText("LOCALIZE_414"),
                LocalizeData.GetText("LOCALIZE_415"),
                LocalizeData.GetText("LOCALIZE_416"),
                LocalizeData.GetText("LOCALIZE_417"),
                LocalizeData.GetText("LOCALIZE_418"),
                LocalizeData.GetText("LOCALIZE_419"),
                LocalizeData.GetText("LOCALIZE_420"),
                LocalizeData.GetText("LOCALIZE_421"),
                LocalizeData.GetText("LOCALIZE_422"),
                LocalizeData.GetText("LOCALIZE_423"),
                LocalizeData.GetText("LOCALIZE_424"),
                LocalizeData.GetText("LOCALIZE_425"),
                LocalizeData.GetText("LOCALIZE_426"),
                LocalizeData.GetText("LOCALIZE_427"),
                LocalizeData.GetText("LOCALIZE_428"),
                LocalizeData.GetText("LOCALIZE_429"),
                LocalizeData.GetText("LOCALIZE_430"),
                LocalizeData.GetText("LOCALIZE_431"),
                LocalizeData.GetText("LOCALIZE_432"),
                LocalizeData.GetText("LOCALIZE_433"),
                LocalizeData.GetText("LOCALIZE_434"),
                LocalizeData.GetText("LOCALIZE_435"),
                LocalizeData.GetText("LOCALIZE_436"),
                LocalizeData.GetText("LOCALIZE_437"),
                LocalizeData.GetText("LOCALIZE_438"),
                LocalizeData.GetText("LOCALIZE_439"),
                LocalizeData.GetText("LOCALIZE_440"),
                LocalizeData.GetText("LOCALIZE_441"),
                LocalizeData.GetText("LOCALIZE_442"),
                LocalizeData.GetText("LOCALIZE_443"),
                LocalizeData.GetText("LOCALIZE_444"),
                LocalizeData.GetText("LOCALIZE_445"),
                LocalizeData.GetText("LOCALIZE_446"),
                LocalizeData.GetText("LOCALIZE_447"),
                LocalizeData.GetText("LOCALIZE_448"),
                LocalizeData.GetText("LOCALIZE_449"),
                LocalizeData.GetText("LOCALIZE_450"),
                LocalizeData.GetText("LOCALIZE_451"),
                LocalizeData.GetText("LOCALIZE_452"),
                LocalizeData.GetText("LOCALIZE_453"),
                LocalizeData.GetText("LOCALIZE_454"),
                LocalizeData.GetText("LOCALIZE_455"),
                LocalizeData.GetText("LOCALIZE_456"),
                LocalizeData.GetText("LOCALIZE_457"),
                LocalizeData.GetText("LOCALIZE_514"),
                LocalizeData.GetText("LOCALIZE_515"),
                LocalizeData.GetText("LOCALIZE_516"),
                LocalizeData.GetText("LOCALIZE_517"),
                LocalizeData.GetText("LOCALIZE_518"),
                LocalizeData.GetText("LOCALIZE_519"),
                LocalizeData.GetText("LOCALIZE_520"),
                LocalizeData.GetText("LOCALIZE_521"),
                LocalizeData.GetText("LOCALIZE_522"),
                LocalizeData.GetText("LOCALIZE_523"),
                LocalizeData.GetText("LOCALIZE_524"),
                LocalizeData.GetText("LOCALIZE_525"),
                LocalizeData.GetText("LOCALIZE_526"),
                LocalizeData.GetText("LOCALIZE_527"),
                LocalizeData.GetText("LOCALIZE_528"),
                LocalizeData.GetText("LOCALIZE_529"),
                LocalizeData.GetText("LOCALIZE_530"),
                LocalizeData.GetText("LOCALIZE_531"),
                LocalizeData.GetText("LOCALIZE_532"),
                LocalizeData.GetText("LOCALIZE_533"),
            };

            if (guidestring.Length > (seq - neco_data.PrologueSeq.배틀패스보상받기))
            {
                int[] guideTry = {
                    1,
                    1,
                    10,
                    1,
                    5,
                    1,
                    20,
                    1,
                    1,
                    5,
                    1,
                    1,
                    1,
                    1,
                    10,
                    1,
                    1,
                    1,
                    1,
                    1,
                    1,
                    1,
                    1,
                    1,
                    1,
                    1,
                    1,
                    1,
                    1,
                    1,
                    1,
                    1,
                    1,
                    1,
                    1,
                    1,
                    1,
                    1,
                    1,
                    1,
                    1,
                    1,
                    1,
                    1,
                    1,
                    1,
                    1,
                    1,
                    1,
                    1,
                    1,
                    1,
                    1,
                    1,
                    1,
                    1,
                    1,
                    1,
                    1,
                    1,
                    1,
                    1,
                    1,
                    1,
                    1,
                    1,
                    1,
                    1,
                    1,
                    1,
                    1,
                    1,
                    1,
                    1,
                    1,
                    1,
                    1,
                    1,
                    1,
                    1,
                    1,
                    1,
                    1,
                    1,
                    1,
                    1,
                    1,
                    1,
                };

                int maxTry = guideTry[seq - neco_data.PrologueSeq.배틀패스보상받기];

                int curTry = maxTry - PlayerPrefs.GetInt("GUIDE_QUEST_COUNT", 0);

                //guideGuage.fillAmount = (float)curTry / maxTry;
                if (maxTry > 1)
                {
                    guideGuageText.text = string.Format("({0}/{1})", curTry, maxTry);
                    guideGuageText.gameObject.SetActive(true);
                }
                else
                {
                    guideGuageText.text = "";
                    guideGuageText.gameObject.SetActive(false);
                }

                guideText.text = guidestring[seq - neco_data.PrologueSeq.배틀패스보상받기];
                return true;
            }
        }

        return false;
    }

    public void SetTutorialGuideUI(string text)
    {
        guideGuageText.text = "";
        guideGuageText.gameObject.SetActive(false);
        guideText.text = text;
    }

    public void OnClickGuideButton()
    {
        // 팝업이 하나라도 열려있으면 동작하지 않음.
        if (NecoCanvas.GetPopupCanvas().IsPopupOpen()) { return; }

        // 튜토리얼 중 예외처리
        if (neco_data.GetPrologueSeq() < neco_data.PrologueSeq.프리플레이) { return; }

        switch (neco_data.GetPrologueSeq())
        {
            case neco_data.PrologueSeq.배틀패스보상받기:
                NecoCanvas.GetPopupCanvas().PopupObject[(int)NecoPopupCanvas.POPUP_TYPE.SEASON_PASS_POPUP]?.GetComponent<SeasonPassPanel>()?.OnClickOpenSeasonPassButton();
                break;
            case neco_data.PrologueSeq.제작대2레벨:
            case neco_data.PrologueSeq.제작대3레벨:
            case neco_data.PrologueSeq.제작대4레벨:
            case neco_data.PrologueSeq.제작대5레벨:
            case neco_data.PrologueSeq.제작대6레벨:
            case neco_data.PrologueSeq.제작대7레벨:
            case neco_data.PrologueSeq.제작대8레벨:
            case neco_data.PrologueSeq.제작대9레벨:
            case neco_data.PrologueSeq.제작대10레벨:
            case neco_data.PrologueSeq.우드락제작:
            case neco_data.PrologueSeq.화이트캣하우스제작:
            case neco_data.PrologueSeq.화장실제작:
            case neco_data.PrologueSeq.보온캣하우스제작:
            case neco_data.PrologueSeq.무스크래쳐제작:
            case neco_data.PrologueSeq.원목캣타워제작:
            case neco_data.PrologueSeq.반자동화장실제작:
            case neco_data.PrologueSeq.파이프캣타워제작:
            case neco_data.PrologueSeq.나무위캣하우스제작:
            case neco_data.PrologueSeq.크리스마스캣타워제작:

            case neco_data.PrologueSeq.제작대11레벨:
            case neco_data.PrologueSeq.제작대12레벨:
            case neco_data.PrologueSeq.캣닢급식소제작:
            case neco_data.PrologueSeq.캣닢급식소3레벨:
            case neco_data.PrologueSeq.캣닢급식소4레벨:
            case neco_data.PrologueSeq.캣닢급식소5레벨:

            case neco_data.PrologueSeq.화이트캣하우스5레벨:
            case neco_data.PrologueSeq.화이트캣하우스6레벨:
            case neco_data.PrologueSeq.화이트캣하우스7레벨:
            case neco_data.PrologueSeq.화이트캣하우스8레벨:
            case neco_data.PrologueSeq.화이트캣하우스9레벨:
            case neco_data.PrologueSeq.화이트캣하우스10레벨:

            case neco_data.PrologueSeq.플로랄캣하우스제작:
            case neco_data.PrologueSeq.플로랄캣하우스3레벨:
            case neco_data.PrologueSeq.플로랄캣하우스4레벨:
            case neco_data.PrologueSeq.플로랄캣하우스5레벨:

            case neco_data.PrologueSeq.알록달록이동식캣하우스제작:
            case neco_data.PrologueSeq.알록달록이동식캣하우스3레벨:
            case neco_data.PrologueSeq.알록달록이동식캣하우스4레벨:
            case neco_data.PrologueSeq.알록달록이동식캣하우스5레벨:
                NecoCanvas.GetPopupCanvas().OnPopupShow(NecoPopupCanvas.POPUP_TYPE.CAT_MAKING_LIST_POPUP);
                break;
            case neco_data.PrologueSeq.양어장2레벨:
            case neco_data.PrologueSeq.양어장3레벨:
            case neco_data.PrologueSeq.양어장4레벨:
            case neco_data.PrologueSeq.양어장5레벨:
            case neco_data.PrologueSeq.양어장6레벨:
            case neco_data.PrologueSeq.양어장7레벨:
            case neco_data.PrologueSeq.양어장8레벨:
            case neco_data.PrologueSeq.양어장9레벨:
            case neco_data.PrologueSeq.양어장10레벨:
                NecoCanvas.GetUICanvas().mainMenuPanel?.farmButton?.OnClickFishfarmButton();
                break;
            case neco_data.PrologueSeq.바구니2레벨:
            case neco_data.PrologueSeq.바구니3레벨:
            case neco_data.PrologueSeq.바구니4레벨:
            case neco_data.PrologueSeq.바구니5레벨:
            case neco_data.PrologueSeq.바구니6레벨:
            case neco_data.PrologueSeq.바구니7레벨:
            case neco_data.PrologueSeq.바구니8레벨:
            case neco_data.PrologueSeq.바구니9레벨:
            case neco_data.PrologueSeq.바구니10레벨:
                NecoCanvas.GetUICanvas().mainMenuPanel?.giftButton?.OnClickCatGiftButton();
                break;
            case neco_data.PrologueSeq.통발2레벨물고기10개획득:
            case neco_data.PrologueSeq.통발3레벨:
            case neco_data.PrologueSeq.통발4레벨:
            case neco_data.PrologueSeq.통발5레벨:
            case neco_data.PrologueSeq.통발6레벨:
            case neco_data.PrologueSeq.통발7레벨:
            case neco_data.PrologueSeq.통발8레벨:
            case neco_data.PrologueSeq.통발9레벨:
            case neco_data.PrologueSeq.통발10레벨:
                NecoCanvas.GetUICanvas().mainMenuPanel?.trapButton?.OnClickFishtrapButton();
                break;
            case neco_data.PrologueSeq.조리대3레벨:
            case neco_data.PrologueSeq.조리대4레벨:
            case neco_data.PrologueSeq.조리대5레벨:
            case neco_data.PrologueSeq.조리대6레벨:
            case neco_data.PrologueSeq.조리대7레벨:
            case neco_data.PrologueSeq.조리대8레벨:
            case neco_data.PrologueSeq.조리대9레벨:
            case neco_data.PrologueSeq.조리대10레벨:
            case neco_data.PrologueSeq.빙어튀김요리:
            case neco_data.PrologueSeq.잉어찜요리:
            case neco_data.PrologueSeq.민물고기찜요리:
            case neco_data.PrologueSeq.바다고기회요리:
            case neco_data.PrologueSeq.장어구이요리:
            case neco_data.PrologueSeq.참치구이요리:
            case neco_data.PrologueSeq.고급바다고기회요리:
            case neco_data.PrologueSeq.무지개배스찜요리:
                NecoCanvas.GetPopupCanvas().OnPopupShow(NecoPopupCanvas.POPUP_TYPE.CAT_COOK_LIST_POPUP);
                break;
        }
    }
}
