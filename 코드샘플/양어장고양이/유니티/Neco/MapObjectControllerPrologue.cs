using DG.Tweening;
using Newtonsoft.Json.Linq;
using Spine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MapObjectControllerPrologue : MapObjectController 
{
    private Dictionary<neco_data.PrologueSeq, List<string>> ScriptText = new Dictionary<neco_data.PrologueSeq, List<string>>();

    private List<string> GetCurScripts()
    {
        if(ScriptText.ContainsKey(neco_data.GetPrologueSeq()))
            return ScriptText[neco_data.GetPrologueSeq()];

        return new List<string>();
    }

    protected override void Awake()
    {
        base.Awake();

        ScriptText[neco_data.PrologueSeq.챕터1시작] = new List<string>();
        ScriptText[neco_data.PrologueSeq.챕터1시작].Add(LocalizeData.GetText("챕터1시작"));

        ScriptText[neco_data.PrologueSeq.길막이뒷쪽에표시] = new List<string>();
        ScriptText[neco_data.PrologueSeq.길막이뒷쪽에표시].Add(LocalizeData.GetText("길막이뒷쪽에표시"));
        
        ScriptText[neco_data.PrologueSeq.배고픈길막이대사] = new List<string>();
        ScriptText[neco_data.PrologueSeq.배고픈길막이대사].Add(LocalizeData.GetText("배고픈길막이대사"));

        ScriptText[neco_data.PrologueSeq.통발수급대사] = new List<string>();
        ScriptText[neco_data.PrologueSeq.통발수급대사].Add(LocalizeData.GetText("통발수급대사"));

        ScriptText[neco_data.PrologueSeq.조리대UI닫히고대사] = new List<string>();
        ScriptText[neco_data.PrologueSeq.조리대UI닫히고대사].Add(LocalizeData.GetText("조리대UI닫히고대사"));

        ScriptText[neco_data.PrologueSeq.첫밥주기완료대사] = new List<string>();
        ScriptText[neco_data.PrologueSeq.첫밥주기완료대사].Add(LocalizeData.GetText("첫밥주기완료대사"));

        ScriptText[neco_data.PrologueSeq.길막밥먹는거대사] = new List<string>();
        ScriptText[neco_data.PrologueSeq.길막밥먹는거대사].Add(LocalizeData.GetText("길막밥먹는거대사"));


        ScriptText[neco_data.PrologueSeq.챕터2시작대사] = new List<string>();
        ScriptText[neco_data.PrologueSeq.챕터2시작대사].Add(LocalizeData.GetText("챕터2시작대사1"));
        ScriptText[neco_data.PrologueSeq.챕터2시작대사].Add(LocalizeData.GetText("챕터2시작대사2"));

        ScriptText[neco_data.PrologueSeq.양어장닫고대사] = new List<string>();
        ScriptText[neco_data.PrologueSeq.양어장닫고대사].Add(LocalizeData.GetText("양어장닫고대사1"));
        ScriptText[neco_data.PrologueSeq.양어장닫고대사].Add(LocalizeData.GetText("양어장닫고대사2"));

        ScriptText[neco_data.PrologueSeq.철판제작가이드퀘스트완료대사] = new List<string>();
        ScriptText[neco_data.PrologueSeq.철판제작가이드퀘스트완료대사].Add(LocalizeData.GetText("철판제작가이드퀘스트완료대사"));
        
        ScriptText[neco_data.PrologueSeq.조리대레벨업완료후대사] = new List<string>();
        ScriptText[neco_data.PrologueSeq.조리대레벨업완료후대사].Add(LocalizeData.GetText("조리대레벨업완료후대사1"));
        ScriptText[neco_data.PrologueSeq.조리대레벨업완료후대사].Add(LocalizeData.GetText("조리대레벨업완료후대사2"));

        ScriptText[neco_data.PrologueSeq.상점배스구매완료대사] = new List<string>();
        ScriptText[neco_data.PrologueSeq.상점배스구매완료대사].Add(LocalizeData.GetText("상점배스구매완료대사"));

        ScriptText[neco_data.PrologueSeq.길막이배스구이먹음대사] = new List<string>();
        ScriptText[neco_data.PrologueSeq.길막이배스구이먹음대사].Add(LocalizeData.GetText("길막이배스구이먹음대사1"));
        ScriptText[neco_data.PrologueSeq.길막이배스구이먹음대사].Add(LocalizeData.GetText("길막이배스구이먹음대사2"));

        ScriptText[neco_data.PrologueSeq.뒷길막이선물생성] = new List<string>();
        ScriptText[neco_data.PrologueSeq.뒷길막이선물생성].Add(LocalizeData.GetText("뒷길막이선물생성"));

        ScriptText[neco_data.PrologueSeq.뒷길막이선물획득대사] = new List<string>();
        ScriptText[neco_data.PrologueSeq.뒷길막이선물획득대사].Add(LocalizeData.GetText("뒷길막이선물획득대사"));

        ScriptText[neco_data.PrologueSeq.길막이심심해대사] = new List<string>();
        ScriptText[neco_data.PrologueSeq.길막이심심해대사].Add(LocalizeData.GetText("길막이심심해대사"));

        //ScriptText[neco_data.PrologueSeq.길막이낚시돌발대사] = new List<string>();
        //ScriptText[neco_data.PrologueSeq.길막이낚시돌발대사].Add(LocalizeData.GetText("길막이낚시돌발대사1"));
        //ScriptText[neco_data.PrologueSeq.길막이낚시돌발대사].Add(LocalizeData.GetText("길막이낚시돌발대사2"));
        
        ScriptText[neco_data.PrologueSeq.길막이만지기돌발대사] = new List<string>();
        ScriptText[neco_data.PrologueSeq.길막이만지기돌발대사].Add(LocalizeData.GetText("길막이낚시돌발대사1"));
        ScriptText[neco_data.PrologueSeq.길막이만지기돌발대사].Add(LocalizeData.GetText("길막이낚시돌발대사2"));

        //ScriptText[neco_data.PrologueSeq.챕터4시작] = new List<string>();
        //ScriptText[neco_data.PrologueSeq.챕터4시작].Add(LocalizeData.GetText("챕터4시작1"));
        //ScriptText[neco_data.PrologueSeq.챕터4시작].Add(LocalizeData.GetText("챕터4시작2"));

        ScriptText[neco_data.PrologueSeq.사진찍기돌발대사] = new List<string>();
        ScriptText[neco_data.PrologueSeq.사진찍기돌발대사].Add(LocalizeData.GetText("사진찍기돌발대사"));

        ScriptText[neco_data.PrologueSeq.챕터5시작] = new List<string>();
        ScriptText[neco_data.PrologueSeq.챕터5시작].Add(LocalizeData.GetText("챕터5시작1"));
        ScriptText[neco_data.PrologueSeq.챕터5시작].Add(LocalizeData.GetText("챕터5시작2"));

        ScriptText[neco_data.PrologueSeq.챕터5끝] = new List<string>();
        ScriptText[neco_data.PrologueSeq.챕터5끝].Add(LocalizeData.GetText("챕터5끝1"));
        ScriptText[neco_data.PrologueSeq.챕터5끝].Add(LocalizeData.GetText("챕터5끝2"));
        ScriptText[neco_data.PrologueSeq.챕터5끝].Add(LocalizeData.GetText("챕터5끝3"));


        ScriptText[neco_data.PrologueSeq.방문고양이터치] = new List<string>();
        ScriptText[neco_data.PrologueSeq.방문고양이터치].Add(LocalizeData.GetText("방문고양이터치"));

        ScriptText[neco_data.PrologueSeq.말풍선아이템받기] = new List<string>();
        ScriptText[neco_data.PrologueSeq.말풍선아이템받기].Add(LocalizeData.GetText("말풍선아이템받기"));

        ScriptText[neco_data.PrologueSeq.첫보은받기성공] = new List<string>();
        ScriptText[neco_data.PrologueSeq.첫보은받기성공].Add(LocalizeData.GetText("첫보은받기성공"));
        ScriptText[neco_data.PrologueSeq.첫보은받기성공].Add(LocalizeData.GetText("첫보은받기성공_대사"));

        ScriptText[neco_data.PrologueSeq.길막이노잼대사] = new List<string>();
        ScriptText[neco_data.PrologueSeq.길막이노잼대사].Add(LocalizeData.GetText("길막이노잼대사1"));
        ScriptText[neco_data.PrologueSeq.길막이노잼대사].Add(LocalizeData.GetText("길막이노잼대사2"));

        ScriptText[neco_data.PrologueSeq.배틀패스강조및대사] = new List<string>();
        ScriptText[neco_data.PrologueSeq.배틀패스강조및대사].Add(LocalizeData.GetText("배틀패스강조및대사1"));
        ScriptText[neco_data.PrologueSeq.배틀패스강조및대사].Add(LocalizeData.GetText("배틀패스강조및대사2"));

        ScriptText[neco_data.PrologueSeq.배틀패스종료대사] = new List<string>();
        //ScriptText[neco_data.PrologueSeq.배틀패스종료대사].Add(LocalizeData.GetText("배틀패스종료대사1")); 내용 변경으로 인해 미사용
        ScriptText[neco_data.PrologueSeq.배틀패스종료대사].Add(LocalizeData.GetText("배틀패스종료대사2"));
        ScriptText[neco_data.PrologueSeq.배틀패스종료대사].Add(LocalizeData.GetText("배틀패스종료대사3"));

        ScriptText[neco_data.PrologueSeq.사진찍기완료대사] = new List<string>();
        ScriptText[neco_data.PrologueSeq.사진찍기완료대사].Add(LocalizeData.GetText("사진찍기완료대사1"));
        ScriptText[neco_data.PrologueSeq.사진찍기완료대사].Add(LocalizeData.GetText("사진찍기완료대사2"));
    }

    public override void OnInitMap(neco_map mapData)
    {
        base.OnInitMap(mapData);

        NecoCanvas.GetUICanvas().SetMapMoveButtons(true);         
    }

    public void CheckPrologueSeq()
    {
        neco_data.PrologueSeq seq = (neco_data.PrologueSeq)((uint)GameDataManager.Instance.GetUserData().data["contents"]);

        if (neco_data.PrologueSeq.프리플레이 == seq)
        {
            seq = neco_data.PrologueSeq.프리플레이 + 1;
        }
        else if(seq < neco_data.PrologueSeq.프리플레이)
        {
            //if (seq < neco_data.PrologueSeq.고양이10번터치가이드퀘스트)
            //    seq = neco_data.PrologueSeq.챕터1시작;
            //else if (seq < neco_data.PrologueSeq.챕터2시작)
            //    seq = neco_data.PrologueSeq.챕터2시작;
            //else if (seq < neco_data.PrologueSeq.챕터3시작)
            //    seq = neco_data.PrologueSeq.챕터3시작;
            //else if (seq < neco_data.PrologueSeq.챕터4시작)
            //    seq = neco_data.PrologueSeq.챕터4시작;
            //else if (seq < neco_data.PrologueSeq.챕터5시작)
            //    seq = neco_data.PrologueSeq.챕터5시작;
        }

        neco_data.SetPrologueSeq(seq);

        InitMapPrologueSeq();
        //NecoCanvas.GetPopupCanvas().OnShowCatPhotoPopup(neco_cat.GetNecoCat(1), neco_cat_memory.GetNecoMemory(201001));
    }

    public void InitMapPrologueSeq()
    {
        neco_data.PrologueSeq seq = neco_data.GetPrologueSeq();
        
        if(neco_data.PrologueSeq.프리플레이 <= seq)
        {
            MainMenuPanel MainUIPanel = NecoCanvas.GetUICanvas().gameObject.GetComponentInChildren<MainMenuPanel>(true);
            MainUIPanel.gameObject.SetActive(true);

            MainUIPanel.TopUIPanel.SetActive(true);
            MainUIPanel.mainMenuToggle.gameObject.SetActive(true);
            MainUIPanel.mainMenuGroup.SetActive(MainUIPanel.mainMenuToggle.isOn);

            MainUIPanel.topMenuToggle.gameObject.SetActive(true);
            MainUIPanel.topMenuGroup.SetActive(MainUIPanel.topMenuToggle.isOn);

            MainUIPanel.giftButton.gameObject.SetActive(true);
            MainUIPanel.trapButton.gameObject.SetActive(true);
            MainUIPanel.farmButton.gameObject.SetActive(true);
            MainUIPanel.passButton.gameObject.SetActive(true);
            MainUIPanel.shopButton.gameObject.SetActive(true);
            MainUIPanel.fishingButton.gameObject.SetActive(true);

            NecoCanvas.GetUICanvas().SetMapMoveButtons(true);

            if (!neco_cat.GetNecoCat(2).IsGainCat())
                NecoCanvas.GetGameCanvas().CallCat(2, 0, 0, 0);
            if (!neco_cat.GetNecoCat(3).IsGainCat() && !neco_cat.GetNecoCat(3).IsReadyCat())
                OnNewCat(3);


            //for (uint i = 1; i <= 13; i++)
            //{
            //    if (!neco_cat.GetNecoCat(i).IsGainCat())
            //        NecoCanvas.GetGameCanvas().CallCat(i, 0, 0, 0);
            //}

            //List<uint> itemList = new List<uint>();
            //itemList.Add(106);
            //itemList.Add(107);
            //itemList.Add(108);
            //itemList.Add(109);
            //itemList.Add(110);
            //itemList.Add(111);
            //itemList.Add(112);
            //itemList.Add(113);
            //itemList.Add(114);
            //itemList.Add(115);
            //itemList.Add(116);
            //itemList.Add(117);
            //itemList.Add(118);
            //itemList.Add(119);
            //itemList.Add(120);
            ////itemList.Add(121);
            //itemList.Add(122);
            //itemList.Add(123);
            //itemList.Add(124);

            //foreach (uint itemID in itemList)
            //{
            //    WWWForm param = new WWWForm();
            //    param.AddField("api", "chore");
            //    param.AddField("op", 1);

            //    param.AddField("item", itemID.ToString());
            //    param.AddField("cnt", 1);

            //    NetworkManager.GetInstance().SendApiRequest("chore", 1, param, (response) =>
            //    {

            //    });
            //}

            Silhouette[1].GetComponent<NecoSilhouetteButton>().enabled = true;
            Silhouette[2].GetComponent<NecoSilhouetteButton>().enabled = true;

            int guideQuestCount = PlayerPrefs.GetInt("GUIDE_QUEST_COUNT", 0);
            if (guideQuestCount <= 0)
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
                };

                if (seq - neco_data.PrologueSeq.배틀패스보상받기 < guideTry.Length)
                    PlayerPrefs.SetInt("GUIDE_QUEST_COUNT", guideTry[seq - neco_data.PrologueSeq.배틀패스보상받기]);
                else
                    PlayerPrefs.SetInt("GUIDE_QUEST_COUNT", 1);
            }

            NecoCanvas.GetUICanvas().SetGuideUI();

            NecoCanvas.GetGameCanvas().catUpdater.StartNecoCatUpdate();
            CheckGuideQuestCleared();

            return;
        }

        if (seq > neco_data.PrologueSeq.챕터1시작)
        {
            seq = seq - 1;
        }

        NecoCanvas.GetUICanvas().ResumeTutorialUI();
        neco_data.SetPrologueSeq(seq);

        switch (seq)
        {
            case neco_data.PrologueSeq.챕터1시작:
            case neco_data.PrologueSeq.길막이꼬리얼쩡댐:
            case neco_data.PrologueSeq.길막이등장영상:
            case neco_data.PrologueSeq.길막이뒷쪽에표시:
            case neco_data.PrologueSeq.길막이획득프로필팝업확인:
            case neco_data.PrologueSeq.길막이배고파연출:
            case neco_data.PrologueSeq.배고픈길막이대사:
            case neco_data.PrologueSeq.배고픈길막이가이드퀘스트:
            case neco_data.PrologueSeq.통발UI등장:
            case neco_data.PrologueSeq.통발UI닫힘:
            case neco_data.PrologueSeq.통발수급대사:
            case neco_data.PrologueSeq.조리대UI등장:
            case neco_data.PrologueSeq.조리대UI닫힘:
            case neco_data.PrologueSeq.조리대UI닫히고대사:
            case neco_data.PrologueSeq.첫밥그릇등장:
            case neco_data.PrologueSeq.첫밥주기완료:
            case neco_data.PrologueSeq.첫밥주기완료대사:
            case neco_data.PrologueSeq.길막배고픔연출사라짐:
            case neco_data.PrologueSeq.길막밥그릇돌진:
            case neco_data.PrologueSeq.길막밥먹는거대사:
            case neco_data.PrologueSeq.고양이10번터치가이드퀘스트:
            case neco_data.PrologueSeq.고양이10번터치가이드퀘스트완료:
                neco_data.SetPrologueSeq(neco_data.PrologueSeq.챕터1시작);
                OnStartPrologue();
                break;

            case neco_data.PrologueSeq.챕터1끝:
            case neco_data.PrologueSeq.챕터2시작:
            case neco_data.PrologueSeq.챕터2시작대사:
            case neco_data.PrologueSeq.양어장UI등장:
            case neco_data.PrologueSeq.양어장골드연출:
            case neco_data.PrologueSeq.양어장닫고대사:
                neco_data.SetPrologueSeq(neco_data.PrologueSeq.챕터1끝);
                챕터2시작();
                break;

            case neco_data.PrologueSeq.철판제작가이드퀘스트:
                철판제작UI();
                철판제작가이드퀘스트완료();
                break;

            case neco_data.PrologueSeq.철판제작가이드퀘스트완료:
            case neco_data.PrologueSeq.철판제작가이드퀘스트완료대사:
                if (neco_data.Instance.GetCookRecipeLevel() < 2)
                {
                    neco_data.SetPrologueSeq(neco_data.PrologueSeq.철판제작가이드퀘스트완료);
                    철판제작가이드퀘스트완료대사();
                }
                else
                {
                    neco_data.SetPrologueSeq(neco_data.PrologueSeq.조리대레벨업);
                    조리대레벨업완료();
                }
                break;

            case neco_data.PrologueSeq.조리대레벨업:
                조리대레벨업완료();
                break;

            case neco_data.PrologueSeq.조리대레벨업완료:
            case neco_data.PrologueSeq.조리대레벨업완료후대사:
                neco_data.SetPrologueSeq(neco_data.PrologueSeq.조리대레벨업완료);
                조리대레벨업완료후대사();
                break;

            case neco_data.PrologueSeq.상점배스구매가이드퀘스트:
                배스구매UI();
                상점배스구매완료();
                break;

            case neco_data.PrologueSeq.상점배스구매완료:
            case neco_data.PrologueSeq.상점배스구매완료대사:
                neco_data.SetPrologueSeq(neco_data.PrologueSeq.상점배스구매완료);
                상점배스구매완료대사();
                break;

            case neco_data.PrologueSeq.배스구이강조:
                배스구이만들기강조UI();
                배스구이완료();
                break;

            case neco_data.PrologueSeq.배스구이완료후밥그릇강조:
                //서버 오류로 유저들 정체구간이라 그냥 뒷스텝으로 넘겨줌
                //배스구이밥그릇에담음();
                //break;

            case neco_data.PrologueSeq.배스구이밥그릇에담음:
            case neco_data.PrologueSeq.뒷길막이선물생성:
            case neco_data.PrologueSeq.뒷길막이선물획득대사:
            case neco_data.PrologueSeq.길막이배스구이먹음대사:            
                neco_data.SetPrologueSeq(neco_data.PrologueSeq.배스구이밥그릇에담음);
                뒷길막이선물생성();
                break;

            case neco_data.PrologueSeq.보은바구니UI등장:
                보은바구니강조UI();
                break;

            case neco_data.PrologueSeq.첫보은받기성공:
                첫보은받기퀘스트성공();
                break;

            case neco_data.PrologueSeq.챕터2끝:            
                neco_data.SetPrologueSeq(neco_data.PrologueSeq.챕터2끝);
                배틀패스소개();
                break;

            case neco_data.PrologueSeq.배틀패스강조및대사:
            case neco_data.PrologueSeq.배틀패스보상획득:
            case neco_data.PrologueSeq.배틀패스종료대사:
                neco_data.SetPrologueSeq(neco_data.PrologueSeq.배틀패스보상획득);
                배틀패스완료();
                break;

            case neco_data.PrologueSeq.챕터3시작:
            case neco_data.PrologueSeq.길막이퇴장:
            case neco_data.PrologueSeq.길막이심심해대사:
                챕터3시작();
                break;

            case neco_data.PrologueSeq.낚시장난감만들기:
                if (user_items.GetUserItemAmount(106) > 0)
                {
                    챕터3시작();
                }
                else
                {
                    낚시장난감만들기완료();
                }
                break;

            case neco_data.PrologueSeq.낚시장난감만들기완료:
            case neco_data.PrologueSeq.길막이낚시장난감배치:
            case neco_data.PrologueSeq.길막이낚시돌발등장:
            case neco_data.PrologueSeq.길막이노잼대사:
            case neco_data.PrologueSeq.낚시장난감오브젝트레벨업가이드:
                neco_data.SetPrologueSeq(neco_data.PrologueSeq.낚시장난감만들기완료);
                길막이낚시장난감배치();
                break;

            case neco_data.PrologueSeq.길막이2레벨낚시장난감연출:
            case neco_data.PrologueSeq.길막이만지기돌발대사:
                neco_data.SetPrologueSeq(neco_data.PrologueSeq.낚시장난감오브젝트레벨업가이드);
                길막이2레벨낚시장난감연출();
                break;

            case neco_data.PrologueSeq.길막이만지기돌발발생:
            case neco_data.PrologueSeq.길막이낚시돌발완료:
                neco_user_cat userCatInfo = neco_user_cat.GetUserCatInfo(2);
                if (userCatInfo.GetPhotoMemoryCount() > 0)
                {
                    neco_data.SetPrologueSeq(neco_data.PrologueSeq.사진찍기돌발대사);
                    페이크사진찍기스텝();
                }
                else
                {
                    neco_data.SetPrologueSeq(neco_data.PrologueSeq.길막이낚시돌발완료);
                    길막이낚시돌발완료();
                }
                break;

            case neco_data.PrologueSeq.사진찍기돌발대사:
                사진찍기완료();
                break;

            //chapter5
            case neco_data.PrologueSeq.사진찍기완료대사:
            case neco_data.PrologueSeq.챕터5시작:
            case neco_data.PrologueSeq.스와이프가이드:
            case neco_data.PrologueSeq.삼색이등장:
            case neco_data.PrologueSeq.챕터5끝:
                neco_data.SetPrologueSeq(neco_data.PrologueSeq.챕터5시작);
                챕터5시작();
                break;
        }
    }

    public void OnStartPrologue()
    {
        foreach (MapObjectSpot spot in MapObjectSpots)
        {
            spot.gameObject.SetActive(false);
        }

        MapObjectSpots[0].gameObject.SetActive(true);

        foreach (GameObject silhouette in Silhouette)
        {
            if (silhouette != null)
                silhouette.SetActive(false);
        }

        NecoCanvas.GetUICanvas().OnRefreshGuideUI();
        NecoCanvas.GetUICanvas().SetMapMoveButtons(false);
        NecoCanvas.GetUICanvas().gameObject.SetActive(false);

        NecoCanvas.GetPopupCanvas().OnTouchDefend(3.0f, OnFirstScriptOn);
    }

    public void OnFirstScriptOn()
    {
        NecoCanvas.GetPopupCanvas().OnShowScriptsPopup(GetCurScripts(), 길막이꼬리등장);
    }

    public void 길막이꼬리등장()
    {
        if (!neco_data.CheckAndNextPrologueSeq(neco_data.PrologueSeq.챕터1시작))
            return;

        //길막이꼬리얼쩡댐

        GameObject gilmakTail = MapBackgroundImage.Find("GilmakTail").gameObject;
        gilmakTail.SetActive(true);

        Spine.Unity.SkeletonGraphic graphic = gilmakTail.GetComponent<Spine.Unity.SkeletonGraphic>();
        graphic.color = new Color(1, 1, 1, 0);
        graphic.DOColor(Color.white, 0.8f);

        foreach (Transform child in gilmakTail.transform)
        {
            child.gameObject.SetActive(false);
        }

        Spine.Unity.SkeletonGraphic spine = gilmakTail.GetComponent<Spine.Unity.SkeletonGraphic>();
        spine.AnimationState.SetAnimation(0, "tail", false);

        Spine.Animation animationObject = spine.skeletonDataAsset.GetSkeletonData(false).FindAnimation("tail");
        Invoke("길막이꼬리애니변경", animationObject.Duration);
        Invoke("길막이꼬리버튼등장", 4.5f);

        Image image = Silhouette[2].GetComponent<Image>();
        image.color = new Color(0f, 0f, 0f, 0f);
        image.DOColor(Color.white, 1.0f).OnComplete(() => {
            foreach (Transform child in Silhouette[2].transform)
            {
                child.gameObject.SetActive(true);
            }

            //NecoCanvas.GetPopupCanvas().OnShowScriptsPopup(GetCurScripts());
        });

        //transform.localScale = Vector3.one * 1.1f;
        //transform.DOShakePosition(3.0f, 5.0f);
        //transform.DOScale(1.0f, 3.0f);
    }

    public void 길막이꼬리버튼등장()
    {
        //길막이꼬리얼쩡댐
        GameObject gilmakTail = MapBackgroundImage.Find("GilmakTail").gameObject;
        foreach (Transform child in gilmakTail.transform)
        {
            child.gameObject.SetActive(true);
        }
    }

    public void 길막이꼬리애니변경()
    {
        //transform.DOKill();
        //transform.localPosition = Vector3.zero;
        //transform.localScale = Vector3.one;

        GameObject gilmakTail = MapBackgroundImage.Find("GilmakTail").gameObject;
        Spine.Unity.SkeletonGraphic spine = gilmakTail.GetComponent<Spine.Unity.SkeletonGraphic>();
        spine.AnimationState.SetAnimation(0, "tail_replay", true);
    }

    public void 길막이꼬리터치()
    {
        if (!neco_data.CheckAndNextPrologueSeq(neco_data.PrologueSeq.길막이꼬리얼쩡댐))
            return;

        //transform.DOKill();
        //transform.localPosition = Vector3.zero;
        //transform.localScale = Vector3.one;

        //길막이등장영상
        NecoCanvas.GetPopupCanvas().BlurActionForMapChange();

        Invoke("길막이영상재생", 0.5f);
    }

    public void 길막이영상재생()
    {
        NecoCanvas.GetVideoCanvas().OnVideoPlay(2000, 길막이등장영상완료);
    }

    public void 길막이등장영상완료()
    {
        if (!neco_data.CheckAndNextPrologueSeq(neco_data.PrologueSeq.길막이등장영상))
            return;

        //길막이뒷쪽에표시

        NecoCanvas.GetGameCanvas().CallCat(2, 0, 0, 0);
        
        GameObject gilmakTail = MapBackgroundImage.Find("GilmakTail").gameObject;
        gilmakTail.SetActive(false);

        Silhouette[2].SetActive(true);
        foreach (Transform child in Silhouette[2].transform)
        {
            child.gameObject.SetActive(false);
        }

        Image image = Silhouette[2].GetComponent<Image>();
        image.color = new Color(0f, 0f, 0f, 0f);
        image.DOColor(Color.white, 1.0f).OnComplete(() => {
            NecoCanvas.GetPopupCanvas().OnShowScriptsPopup(GetCurScripts(), 프로필보기);
        });

        Silhouette[2].GetComponent<NecoSilhouetteButton>().enabled = false;
    }

    public void 프로필보기()
    {
        if (!neco_data.CheckAndNextPrologueSeq(neco_data.PrologueSeq.길막이뒷쪽에표시))
            return;

        //길막이획득프로필팝업확인

        NecoCanvas.GetPopupCanvas().OnPopupShow(NecoPopupCanvas.POPUP_TYPE.CAT_LIST_POPUP);
    }

    public void 길막이배고파연출()
    {
        if (!neco_data.CheckAndNextPrologueSeq(neco_data.PrologueSeq.길막이획득프로필팝업확인))
            return;

        NecoCanvas.GetPopupCanvas().OnPopupClose(NecoPopupCanvas.POPUP_TYPE.CAT_LIST_POPUP);
        //길막이배고파연출
        Silhouette[2].transform.Find("hungry").gameObject.SetActive(true);

        Invoke("배고픈길막이대기2초", 0.5f);
    }

    public void 배고픈길막이대기2초()
    {
        NecoCanvas.GetPopupCanvas().OnTouchDefend(2.0f, 배고픈길막이대사);
    }

    public void 배고픈길막이대사()
    {
        if (!neco_data.CheckAndNextPrologueSeq(neco_data.PrologueSeq.길막이배고파연출))
            return;

        //배고픈길막이대사
        NecoCanvas.GetPopupCanvas().OnShowScriptsPopup(GetCurScripts(), 배고픈길막이가이드퀘스트);
    }

    public void 배고픈길막이가이드퀘스트()
    {
        if (!neco_data.CheckAndNextPrologueSeq(neco_data.PrologueSeq.배고픈길막이대사))
            return;

        NecoCanvas.GetUICanvas().OnTutorialGuideUI(LocalizeData.GetText("GQ1"), "", "", false);
        통발UI등장();
    }

    public void 통발UI등장()
    {
        if (!neco_data.CheckAndNextPrologueSeq(neco_data.PrologueSeq.배고픈길막이가이드퀘스트))
            return;

        //통발UI등장
        NecoCanvas.GetUICanvas().gameObject.SetActive(true);
        MainMenuPanel MainUIPanel = NecoCanvas.GetUICanvas().gameObject.GetComponentInChildren<MainMenuPanel>(true);
        MainUIPanel.gameObject.SetActive(true);
        
        MainUIPanel.TopUIPanel.SetActive(true);
        MainUIPanel.mainMenuToggle.gameObject.SetActive(true);        
        MainUIPanel.ToggleOnObject.gameObject.SetActive(false);
        MainUIPanel.ToggleOffObject.gameObject.SetActive(true);

        MainUIPanel.mainMenuGroup.SetActive(true);
        foreach (Transform mainButton in MainUIPanel.mainMenuGroup.transform)
        {
            mainButton.gameObject.SetActive(false);
        }

        MainUIPanel.mainMenuGroup.transform.Find("CatInfoButton").gameObject.SetActive(true);
        
        MainUIPanel.giftButton.gameObject.SetActive(false);
        MainUIPanel.trapButton.gameObject.SetActive(true);
        MainUIPanel.farmButton.gameObject.SetActive(false);
        MainUIPanel.passButton.gameObject.SetActive(false);
        MainUIPanel.shopButton.gameObject.SetActive(false);
        MainUIPanel.fishingButton.gameObject.SetActive(false);

        MainUIPanel.UpdatePrologueNotifyEffect();

        //골드 획득 버튼 강조 연출 추가
        NecoCanvas.GetPopupCanvas().UpdatePrologueSetting(NecoPopupCanvas.POPUP_TYPE.CAT_FISH_TRAP_POPUP);
    }

    public void 통발수급완료()
    {
        if (!neco_data.CheckAndNextPrologueSeq(neco_data.PrologueSeq.통발UI등장))
            return;

        NecoCanvas.GetPopupCanvas().OnPopupClose(NecoPopupCanvas.POPUP_TYPE.CAT_FISH_TRAP_POPUP);

        NecoCanvas.GetUICanvas().OnTutorialGuideUI("", LocalizeData.GetText("GQ2"), LocalizeData.GetText("GQ3"));
    }

    public void 통발수급완료대사()
    {
        if (!neco_data.CheckAndNextPrologueSeq(neco_data.PrologueSeq.통발UI닫힘))
            return;

        //통발수급대사
        NecoCanvas.GetPopupCanvas().OnShowScriptsPopup(GetCurScripts(), 첫요리퀘스트);
    }


    public void 첫요리퀘스트()
    {
        if (!neco_data.CheckAndNextPrologueSeq(neco_data.PrologueSeq.통발수급대사))
            return;

        //조리대UI등장
        NecoCanvas.GetUICanvas().OnTutorialGuideUI(LocalizeData.GetText("GQ4"), "", "", false);
        
        NecoCanvas.GetUICanvas().gameObject.SetActive(true);
        MainMenuPanel MainUIPanel = NecoCanvas.GetUICanvas().gameObject.GetComponentInChildren<MainMenuPanel>(true);
        MainUIPanel.gameObject.SetActive(true);

        MainUIPanel.TopUIPanel.SetActive(true);
        MainUIPanel.mainMenuToggle.gameObject.SetActive(true);
        MainUIPanel.ToggleOnObject.gameObject.SetActive(false);
        MainUIPanel.ToggleOffObject.gameObject.SetActive(true);

        MainUIPanel.mainMenuGroup.SetActive(false);
        foreach (Transform mainButton in MainUIPanel.mainMenuGroup.transform)
        {
            mainButton.gameObject.SetActive(false);
        }
        MainUIPanel.mainMenuGroup.transform.Find("CatInfoButton").gameObject.SetActive(true);
        MainUIPanel.mainMenuGroup.transform.Find("CookingButton").gameObject.SetActive(true);
        MainUIPanel.mainMenuGroup.transform.Find("CookingButton").Find("Background_2").Find("Noti_ani").gameObject.SetActive(true);

        MainUIPanel.giftButton.gameObject.SetActive(false);
        MainUIPanel.trapButton.gameObject.SetActive(false);
        MainUIPanel.farmButton.gameObject.SetActive(false);
        MainUIPanel.passButton.gameObject.SetActive(false);
        MainUIPanel.shopButton.gameObject.SetActive(false);
        MainUIPanel.fishingButton.gameObject.SetActive(false);

        MainUIPanel.UpdatePrologueNotifyEffect();
    }

    public void 첫요리완료()
    {
        if (!neco_data.CheckAndNextPrologueSeq(neco_data.PrologueSeq.조리대UI등장))
            return;

        NecoCanvas.GetPopupCanvas().OnPopupClose(NecoPopupCanvas.POPUP_TYPE.CAT_COOK_POPUP);
        NecoCanvas.GetPopupCanvas().OnPopupClose(NecoPopupCanvas.POPUP_TYPE.CAT_COOK_LIST_POPUP);

        // 강조 연출 off
        MainMenuPanel MainUIPanel = NecoCanvas.GetUICanvas().gameObject.GetComponentInChildren<MainMenuPanel>(true);
        MainUIPanel.mainMenuGroup.transform.Find("CookingButton").Find("Background_2").Find("Noti_ani").gameObject.SetActive(false);

        //조리대UI닫힘
        NecoCanvas.GetUICanvas().OnTutorialGuideUI(LocalizeData.GetText("GQ5"), LocalizeData.GetText("GQ2"), LocalizeData.GetText("GQ6"));
    }

    public void 첫요리완료대사()
    {
        if (!neco_data.CheckAndNextPrologueSeq(neco_data.PrologueSeq.조리대UI닫힘))
            return;

        //조리대UI닫히고대사
        NecoCanvas.GetPopupCanvas().OnShowScriptsPopup(GetCurScripts(), 첫밥그릇등장);
    }

    public void 첫밥그릇등장()
    {
        if (!neco_data.CheckAndNextPrologueSeq(neco_data.PrologueSeq.조리대UI닫히고대사))
            return;

        //첫밥그릇등장
        MapObjectSpots[1].gameObject.SetActive(true);
        MapObjectSpots[1].transform.Find("Arrow_ani").gameObject.SetActive(true);
    }

    public void 첫밥주기완료()
    {
        if (!neco_data.CheckAndNextPrologueSeq(neco_data.PrologueSeq.첫밥그릇등장))
            return;

        MapObjectSpots[1].transform.Find("Arrow_ani").gameObject.SetActive(false);

        //첫밥주기완료후대사
        NecoCanvas.GetUICanvas().OnTutorialGuideUI("", LocalizeData.GetText("GQ2"), LocalizeData.GetText("GQ7"));
    }

    public void 첫밥주기대사()
    {
        if (!neco_data.CheckAndNextPrologueSeq(neco_data.PrologueSeq.첫밥주기완료))
            return;
        //첫밥주기완료후대사
        NecoCanvas.GetPopupCanvas().OnShowScriptsPopup(GetCurScripts(), 길막이밥에반응);

        Debug.Log("Stop Coroutine : RunCatSystem - Prologure");
        NecoGameCanvas gameCanvas = NecoCanvas.GetGameCanvas();
        gameCanvas.StopAllCoroutines();

        MapObjectSpot foodSpot = MapObjectSpots[1];
        foodSpot.CatSpot[2].StopAllCoroutines();

        foodSpot.StopFoodTimerForPrologue();
    }

    public void 길막이밥에반응()
    {
        if (!neco_data.CheckAndNextPrologueSeq(neco_data.PrologueSeq.첫밥주기완료대사))
            return;

        //길막배고픔연출사라짐
        Silhouette[2].transform.Find("hungry").gameObject.SetActive(false);
        
        Image image = Silhouette[2].GetComponent<Image>();
        
        image.DOColor(new Color(1,1,1,0), 1.0f).OnComplete(() => {
            밥그릇으로길막이소환();
        });

    }


    public void 밥그릇으로길막이소환()
    {
        if (!neco_data.CheckAndNextPrologueSeq(neco_data.PrologueSeq.길막배고픔연출사라짐))
            return;

        //길막밥그릇돌진
        NecoCanvas.GetGameCanvas().CallCat(2, 101, 3, 0);
        NecoCanvas.GetPopupCanvas().OnTouchDefend(2.0f, 길막이밥먹음영상);

        Debug.Log("Stop Coroutine : RunCatSystem - Prologure");
        NecoGameCanvas gameCanvas = NecoCanvas.GetGameCanvas();
        gameCanvas.StopAllCoroutines();

        MapObjectSpot foodSpot = MapObjectSpots[1];
        foodSpot.CatSpot[2].StopAllCoroutines();

        foodSpot.StopFoodTimerForPrologue();

        MapObjectSpots[1].transform.Find("Arrow_ani").gameObject.SetActive(false);
    }

    public void 길막이밥먹음영상()
    {
        NecoCanvas.GetVideoCanvas().OnVideoPlay(203901, 길막이밥먹음대사);
    }

    public void 길막이밥먹음대사()
    {
        if (!neco_data.CheckAndNextPrologueSeq(neco_data.PrologueSeq.길막밥그릇돌진))
            return;

        //길막밥먹는거대사
        Debug.Log("Stop Coroutine : RunCatSystem - Prologure");
        NecoGameCanvas gameCanvas = NecoCanvas.GetGameCanvas();
        gameCanvas.StopAllCoroutines();

        MapObjectSpot foodSpot = MapObjectSpots[1];
        foodSpot.CatSpot[2].StopAllCoroutines();

        foodSpot.StopFoodTimerForPrologue();

        //길막밥먹는거대사
        NecoCanvas.GetPopupCanvas().OnShowScriptsPopup(GetCurScripts(), 열번만지기시키기);
    }

    public void 열번만지기시키기()
    {
        if (!neco_data.CheckAndNextPrologueSeq(neco_data.PrologueSeq.길막밥먹는거대사))
            return;

        //고양이10번터치가이드퀘스트
        NecoCanvas.GetUICanvas().OnTutorialGuideUI(LocalizeData.GetText("GQ8"), "", "", false);

        PlayerPrefs.SetInt("GUIDE_QUEST_COUNT", 10);

        GameObject touchGuide = Instantiate(Resources.Load<GameObject>("Prefabs/Neco/CatTouch_ani"));
        touchGuide.name = "touchGuide";
        touchGuide.transform.SetParent(MapObjectSpots[1].CatSpot[2].CatObjects[2].transform);

        touchGuide.transform.localPosition = new Vector3(30, -20, 0);
        touchGuide.transform.localScale = new Vector3(2, 2, 2);
    }

    public void 열번만지기시키기완료()
    {
        int GUIDE_QUEST_COUNT = PlayerPrefs.GetInt("GUIDE_QUEST_COUNT", 10) - 1;
        
        if (GUIDE_QUEST_COUNT > 0)
        {
            //NecoCanvas.GetUICanvas().OnTutorialGuideUI(LocalizeData.GetText("GQ9") + GUIDE_QUEST_COUNT.ToString() + LocalizeData.GetText("GQ10"), "", "", false);
            NecoCanvas.GetUICanvas().OnTutorialGuideUI(LocalizeData.GetText("GQ9") + string.Format(LocalizeData.GetText("GQ10"), GUIDE_QUEST_COUNT), "", "", false);
            PlayerPrefs.SetInt("GUIDE_QUEST_COUNT", GUIDE_QUEST_COUNT);
            return;
        }

        if (!neco_data.CheckAndNextPrologueSeq(neco_data.PrologueSeq.고양이10번터치가이드퀘스트))
            return;

        NecoCanvas.GetPopupCanvas().OnTouchDefend(1.0f, null);

        //고양이10번터치가이드퀘스트완료
        NecoCanvas.GetUICanvas().OnTutorialGuideUI("", LocalizeData.GetText("GQ2"), LocalizeData.GetText("GQ11"));
        Transform touchGuide = MapObjectSpots[1].CatSpot[2].CatObjects[2].transform.Find("touchGuide");
        if(touchGuide)
        {
            Destroy(touchGuide.gameObject);
        }
    }

    public void 고양이만지기보상완료()
    {
        if (!neco_data.CheckAndNextPrologueSeq(neco_data.PrologueSeq.고양이10번터치가이드퀘스트완료))
            return;

        //챕터1끝
        NecoCanvas.GetGameCanvas().CallCat(2, 0, 0, 0);
        neco_spot.GetNecoSpot(101).SetItem(null);
        neco_spot.GetNecoSpot(101).SetItemRemainTick(0);
        neco_spot.GetNecoSpot(101).Refresh();

        Invoke("챕터2시작", 1.0f);
    }

    public void 챕터2시작()
    {
        //if (!neco_data.CheckAndNextPrologueSeq(neco_data.PrologueSeq.챕터1끝))
        //    return;
        NecoCanvas.GetUICanvas().SetTutorialGuideUIPopupCanvas("");

        if (items.GetItem(64) == null)
        {
            WWWForm data = new WWWForm();
            data.AddField("api", "chore");
            data.AddField("op", 1);

            data.AddField("item", 64);
            data.AddField("cnt", 2);

            NetworkManager.GetInstance().SendApiRequest("chore", 1, data);
        }

        if(!neco_cat.GetNecoCat(2).IsGainCat())
            NecoCanvas.GetGameCanvas().CallCat(2, 0, 0, 0);

        
        neco_data.SetPrologueSeq(neco_data.PrologueSeq.챕터2시작, true);
        //챕터2시작
        if (neco_spot.GetNecoSpot(101).GetCurItem() != null)
        {
            neco_spot.GetNecoSpot(101).SetItem(null);
            neco_spot.GetNecoSpot(101).SetItemRemainTick(0);
            neco_spot.GetNecoSpot(101).Refresh();
        }

        foreach (Transform child in Silhouette[2].transform)
        {
            child.gameObject.SetActive(false);
        }

        Image image = Silhouette[2].GetComponent<Image>();        
        image.DOColor(Color.white, 1.0f).OnComplete(() => {
            Silhouette[2].transform.Find("hungry").gameObject.SetActive(true);
            Silhouette[2].transform.Find("hungry").Find("Busy_bt").Find("Text").GetComponent<Text>().text = LocalizeData.GetText("GQ12");
            Silhouette[2].transform.Find("hungry").Find("Busy_bt").Find("icon").gameObject.SetActive(true);
            Silhouette[2].transform.Find("hungry").Find("Busy_bt").Find("icon").GetComponent<Image>().color = new Color(0.0f, 0.0f, 0.0f, 0.0f);
            Silhouette[2].transform.Find("hungry").Find("Busy_bt").Find("icon").GetComponent<Image>().DOColor(Color.white, 1.0f).OnComplete(챕터2시작대사);            
        });
    }

    public void 챕터2시작대사()
    {
        if (!neco_data.CheckAndNextPrologueSeq(neco_data.PrologueSeq.챕터2시작))
            return;

        NecoCanvas.GetPopupCanvas().OnShowScriptsPopup(GetCurScripts(), 양어장UI등장);
    }

    public void 양어장UI등장()
    {
        if (!neco_data.CheckAndNextPrologueSeq(neco_data.PrologueSeq.챕터2시작대사))
            return;

        //양어장UI등장
        NecoCanvas.GetUICanvas().gameObject.SetActive(true);
        MainMenuPanel MainUIPanel = NecoCanvas.GetUICanvas().gameObject.GetComponentInChildren<MainMenuPanel>(true);
        MainUIPanel.gameObject.SetActive(true);

        MainUIPanel.TopUIPanel.SetActive(true);
        MainUIPanel.mainMenuToggle.gameObject.SetActive(true);
        MainUIPanel.ToggleOnObject.gameObject.SetActive(false);
        MainUIPanel.ToggleOffObject.gameObject.SetActive(true);

        MainUIPanel.mainMenuGroup.SetActive(false);
        foreach (Transform mainButton in MainUIPanel.mainMenuGroup.transform)
        {
            mainButton.gameObject.SetActive(false);
        }
        MainUIPanel.mainMenuGroup.transform.Find("CatInfoButton").gameObject.SetActive(true);
        MainUIPanel.mainMenuGroup.transform.Find("CookingButton").gameObject.SetActive(true);

        MainUIPanel.giftButton.gameObject.SetActive(false);
        MainUIPanel.trapButton.gameObject.SetActive(true);
        MainUIPanel.farmButton.gameObject.SetActive(true);
        MainUIPanel.passButton.gameObject.SetActive(false);
        MainUIPanel.shopButton.gameObject.SetActive(false);
        MainUIPanel.fishingButton.gameObject.SetActive(false);

        MainUIPanel.UpdatePrologueNotifyEffect();
    }

    public void 양어장골드연출()
    {
        if (!neco_data.CheckAndNextPrologueSeq(neco_data.PrologueSeq.양어장UI등장))
            return;
    }

    public void 양어장골드수급완료()
    {
        if (!neco_data.CheckAndNextPrologueSeq(neco_data.PrologueSeq.양어장골드연출))
            return;

        //양어장닫고대사
        NecoCanvas.GetPopupCanvas().OnPopupClose(NecoPopupCanvas.POPUP_TYPE.CAT_FISH_FARM_POPUP);
        NecoCanvas.GetPopupCanvas().OnShowScriptsPopup(GetCurScripts(), 철판제작가이드퀘스트);
    }

    public void 철판제작가이드퀘스트()
    {
        if (!neco_data.CheckAndNextPrologueSeq(neco_data.PrologueSeq.양어장닫고대사))
            return;

        철판제작UI();
    }

    public void 철판제작UI()
    {
        //철판제작가이드퀘스트
        NecoCanvas.GetUICanvas().gameObject.SetActive(true);
        MainMenuPanel MainUIPanel = NecoCanvas.GetUICanvas().gameObject.GetComponentInChildren<MainMenuPanel>(true);
        MainUIPanel.gameObject.SetActive(true);

        MainUIPanel.TopUIPanel.SetActive(true);
        MainUIPanel.mainMenuToggle.gameObject.SetActive(true);
        MainUIPanel.ToggleOnObject.gameObject.SetActive(false);
        MainUIPanel.ToggleOffObject.gameObject.SetActive(true);

        MainUIPanel.mainMenuGroup.SetActive(false);
        foreach (Transform mainButton in MainUIPanel.mainMenuGroup.transform)
        {
            mainButton.gameObject.SetActive(false);
        }
        MainUIPanel.mainMenuGroup.transform.Find("CatInfoButton").gameObject.SetActive(true);
        MainUIPanel.mainMenuGroup.transform.Find("CookingButton").gameObject.SetActive(true);
        MainUIPanel.mainMenuGroup.transform.Find("MakeToolButton").gameObject.SetActive(true);
        MainUIPanel.mainMenuGroup.transform.Find("MakeToolButton").Find("Background_2").Find("Noti_ani").gameObject.SetActive(true);

        MainUIPanel.giftButton.gameObject.SetActive(false);
        MainUIPanel.trapButton.gameObject.SetActive(true);
        MainUIPanel.farmButton.gameObject.SetActive(true);
        MainUIPanel.passButton.gameObject.SetActive(false);
        MainUIPanel.shopButton.gameObject.SetActive(false);
        MainUIPanel.fishingButton.gameObject.SetActive(false);

        MainUIPanel.UpdatePrologueNotifyEffect();

        NecoCanvas.GetUICanvas().OnTutorialGuideUI(LocalizeData.GetText("GQ13"), "", "", false);
        PlayerPrefs.SetInt("GUIDE_QUEST_COUNT", 10);
    }

    public void 철판제작가이드퀘스트완료()
    {
        int GUIDE_QUEST_COUNT = 10 - (int)user_items.GetUserItemAmount(99);
            //PlayerPrefs.GetInt("GUIDE_QUEST_COUNT", 3) - 1;

        if (GUIDE_QUEST_COUNT > 0)
        {
            //NecoCanvas.GetUICanvas().OnTutorialGuideUI(LocalizeData.GetText("GQ14") + GUIDE_QUEST_COUNT.ToString() + LocalizeData.GetText("GQ15"), "", "", false);
            NecoCanvas.GetUICanvas().OnTutorialGuideUI(LocalizeData.GetText("GQ14") + string.Format(LocalizeData.GetText("GQ15"), GUIDE_QUEST_COUNT), "", "", false);
            PlayerPrefs.SetInt("GUIDE_QUEST_COUNT", GUIDE_QUEST_COUNT);
            return;
        }

        if (!neco_data.CheckAndNextPrologueSeq(neco_data.PrologueSeq.철판제작가이드퀘스트))
            return;

        //철판제작가이드퀘스트완료
        NecoCanvas.GetPopupCanvas().OnPopupClose();
        
        NecoCanvas.GetUICanvas().OnTutorialGuideUI("", LocalizeData.GetText("GQ2"), LocalizeData.GetText("GQ16"));
    }

    public void 철판제작가이드퀘스트완료대사()
    {
        if (!neco_data.CheckAndNextPrologueSeq(neco_data.PrologueSeq.철판제작가이드퀘스트완료))
            return;

        //철판제작가이드퀘스트완료대사
        NecoCanvas.GetPopupCanvas().OnShowScriptsPopup(GetCurScripts(), 조리대레벨업강조);
    }

    public void 조리대레벨업강조()
    {
        if (!neco_data.CheckAndNextPrologueSeq(neco_data.PrologueSeq.철판제작가이드퀘스트완료대사))
            return;

        //조리대레벨업
        NecoCanvas.GetUICanvas().gameObject.SetActive(true);
        MainMenuPanel MainUIPanel = NecoCanvas.GetUICanvas().gameObject.GetComponentInChildren<MainMenuPanel>(true);
        MainUIPanel.gameObject.SetActive(true);

        MainUIPanel.TopUIPanel.SetActive(true);
        MainUIPanel.mainMenuToggle.gameObject.SetActive(true);
        MainUIPanel.ToggleOnObject.gameObject.SetActive(false);
        MainUIPanel.ToggleOffObject.gameObject.SetActive(true);

        MainUIPanel.mainMenuGroup.SetActive(false);
        foreach (Transform mainButton in MainUIPanel.mainMenuGroup.transform)
        {
            mainButton.gameObject.SetActive(false);
        }
        MainUIPanel.mainMenuGroup.transform.Find("CatInfoButton").gameObject.SetActive(true);
        MainUIPanel.mainMenuGroup.transform.Find("CookingButton").gameObject.SetActive(true);
        MainUIPanel.mainMenuGroup.transform.Find("CookingButton").Find("Background_2").Find("Noti_ani").gameObject.SetActive(true);
        MainUIPanel.mainMenuGroup.transform.Find("MakeToolButton").gameObject.SetActive(true);
        MainUIPanel.mainMenuGroup.transform.Find("MakeToolButton").Find("Background_2").Find("Noti_ani").gameObject.SetActive(false);

        MainUIPanel.giftButton.gameObject.SetActive(false);
        MainUIPanel.trapButton.gameObject.SetActive(true);
        MainUIPanel.farmButton.gameObject.SetActive(true);
        MainUIPanel.passButton.gameObject.SetActive(false);
        MainUIPanel.shopButton.gameObject.SetActive(false);
        MainUIPanel.fishingButton.gameObject.SetActive(false);

        MainUIPanel.UpdatePrologueNotifyEffect();

        NecoCanvas.GetUICanvas().OnTutorialGuideUI(LocalizeData.GetText("GQ17"), "", "", false);
    }

    public void 조리대레벨업완료()
    {
        if (neco_data.Instance.GetCookRecipeLevel() < 2)
            return;

        if (!neco_data.CheckAndNextPrologueSeq(neco_data.PrologueSeq.조리대레벨업))
            return;

        // 강조 연출 off
        MainMenuPanel MainUIPanel = NecoCanvas.GetUICanvas().gameObject.GetComponentInChildren<MainMenuPanel>(true);
        MainUIPanel.mainMenuGroup.transform.Find("CookingButton").Find("Background_2").Find("Noti_ani").gameObject.SetActive(false);

        NecoCanvas.GetPopupCanvas().OnPopupClose();
        NecoCanvas.GetUICanvas().OnTutorialGuideUI("", LocalizeData.GetText("GQ2"), LocalizeData.GetText("GQ18"));
    }

    public void 조리대레벨업완료후대사()
    {
        if (!neco_data.CheckAndNextPrologueSeq(neco_data.PrologueSeq.조리대레벨업완료))
            return;

        //조리대레벨업완료후대사
        NecoCanvas.GetPopupCanvas().OnShowScriptsPopup(GetCurScripts(), 상점배스구매가이드퀘스트);
    }

    public void 상점배스구매가이드퀘스트()
    {
        if (!neco_data.CheckAndNextPrologueSeq(neco_data.PrologueSeq.조리대레벨업완료후대사))
            return;

        //상점배스구매가이드퀘스트
        배스구매UI();
    }

    public void 배스구매UI()
    {
        NecoCanvas.GetUICanvas().gameObject.SetActive(true);
        MainMenuPanel MainUIPanel = NecoCanvas.GetUICanvas().gameObject.GetComponentInChildren<MainMenuPanel>(true);
        MainUIPanel.gameObject.SetActive(true);

        MainUIPanel.TopUIPanel.SetActive(true);
        MainUIPanel.mainMenuToggle.gameObject.SetActive(true);
        MainUIPanel.ToggleOnObject.gameObject.SetActive(false);
        MainUIPanel.ToggleOffObject.gameObject.SetActive(true);

        MainUIPanel.mainMenuGroup.SetActive(false);
        foreach (Transform mainButton in MainUIPanel.mainMenuGroup.transform)
        {
            mainButton.gameObject.SetActive(false);
        }
        MainUIPanel.mainMenuGroup.transform.Find("CatInfoButton").gameObject.SetActive(true);
        MainUIPanel.mainMenuGroup.transform.Find("CookingButton").gameObject.SetActive(true);
        MainUIPanel.mainMenuGroup.transform.Find("MakeToolButton").gameObject.SetActive(true);

        MainUIPanel.giftButton.gameObject.SetActive(false);
        MainUIPanel.trapButton.gameObject.SetActive(true);
        MainUIPanel.farmButton.gameObject.SetActive(true);
        MainUIPanel.passButton.gameObject.SetActive(false);
        MainUIPanel.shopButton.gameObject.SetActive(true);
        MainUIPanel.fishingButton.gameObject.SetActive(false);

        MainUIPanel.shopButton.transform.Find("Background_2").Find("Noti_ani").gameObject.SetActive(true);

        MainUIPanel.UpdatePrologueNotifyEffect();

        NecoCanvas.GetUICanvas().OnTutorialGuideUI(LocalizeData.GetText("GQ19"), "", "", false);
        PlayerPrefs.SetInt("GUIDE_QUEST_COUNT", 3);
    }

    public void 상점배스구매완료()
    {
        int GUIDE_QUEST_COUNT = 3 - (int)user_items.GetUserItemAmount(64);

        if (GUIDE_QUEST_COUNT > 0)
        {
            //NecoCanvas.GetUICanvas().OnTutorialGuideUI(LocalizeData.GetText("GQ20") + GUIDE_QUEST_COUNT.ToString() + LocalizeData.GetText("GQ21"), "", "", false);
            NecoCanvas.GetUICanvas().OnTutorialGuideUI(LocalizeData.GetText("GQ20") + string.Format(LocalizeData.GetText("GQ21"), GUIDE_QUEST_COUNT), "", "", false);
            PlayerPrefs.SetInt("GUIDE_QUEST_COUNT", GUIDE_QUEST_COUNT);
            return;
        }

        if (!neco_data.CheckAndNextPrologueSeq(neco_data.PrologueSeq.상점배스구매가이드퀘스트))
            return;

        // 강조 연출 off
        MainMenuPanel MainUIPanel = NecoCanvas.GetUICanvas().gameObject.GetComponentInChildren<MainMenuPanel>(true);
        MainUIPanel.shopButton.transform.Find("Background_2").Find("Noti_ani").gameObject.SetActive(false);

        //상점배스구매완료
        NecoCanvas.GetPopupCanvas().OnPopupClose();
        NecoCanvas.GetUICanvas().OnTutorialGuideUI("", LocalizeData.GetText("GQ2"), LocalizeData.GetText("GQ22"));
    }
    public void 상점배스구매완료대사()
    {
        if (!neco_data.CheckAndNextPrologueSeq(neco_data.PrologueSeq.상점배스구매완료))
            return;

        //상점배스구매완료대사
        NecoCanvas.GetPopupCanvas().OnShowScriptsPopup(GetCurScripts(), 배스구이강조);
    }

    public void 배스구이강조()
    {
        if (!neco_data.CheckAndNextPrologueSeq(neco_data.PrologueSeq.상점배스구매완료대사))
            return;

        배스구이만들기강조UI();

        if (user_items.GetUserItemAmount(80) > 0)
            배스구이완료();
    }

    public void 배스구이만들기강조UI()
    {
        NecoCanvas.GetUICanvas().gameObject.SetActive(true);
        MainMenuPanel MainUIPanel = NecoCanvas.GetUICanvas().gameObject.GetComponentInChildren<MainMenuPanel>(true);
        MainUIPanel.gameObject.SetActive(true);

        MainUIPanel.TopUIPanel.SetActive(true);
        MainUIPanel.mainMenuToggle.gameObject.SetActive(true);
        MainUIPanel.ToggleOnObject.gameObject.SetActive(false);
        MainUIPanel.ToggleOffObject.gameObject.SetActive(true);

        MainUIPanel.mainMenuGroup.SetActive(false);
        foreach (Transform mainButton in MainUIPanel.mainMenuGroup.transform)
        {
            mainButton.gameObject.SetActive(false);
        }
        MainUIPanel.mainMenuGroup.transform.Find("CatInfoButton").gameObject.SetActive(true);
        MainUIPanel.mainMenuGroup.transform.Find("CookingButton").gameObject.SetActive(true);
        MainUIPanel.mainMenuGroup.transform.Find("CookingButton").Find("Background_2").Find("Noti_ani").gameObject.SetActive(true);
        MainUIPanel.mainMenuGroup.transform.Find("MakeToolButton").gameObject.SetActive(true);

        MainUIPanel.giftButton.gameObject.SetActive(false);
        MainUIPanel.trapButton.gameObject.SetActive(true);
        MainUIPanel.farmButton.gameObject.SetActive(true);
        MainUIPanel.passButton.gameObject.SetActive(false);
        MainUIPanel.shopButton.gameObject.SetActive(true);
        MainUIPanel.fishingButton.gameObject.SetActive(false);

        MainUIPanel.UpdatePrologueNotifyEffect();

        NecoCanvas.GetUICanvas().OnTutorialGuideUI(LocalizeData.GetText("GQ23"), "", "", false);
    }

    public void 배스구이완료()
    {
        if (!neco_data.CheckAndNextPrologueSeq(neco_data.PrologueSeq.배스구이강조))
            return;

        // 강조 연출 off
        MainMenuPanel MainUIPanel = NecoCanvas.GetUICanvas().gameObject.GetComponentInChildren<MainMenuPanel>(true);
        MainUIPanel.mainMenuGroup.transform.Find("CookingButton").Find("Background_2").Find("Noti_ani").gameObject.SetActive(false);

        NecoCanvas.GetPopupCanvas().OnPopupClose(NecoPopupCanvas.POPUP_TYPE.CAT_COOK_POPUP);
        NecoCanvas.GetPopupCanvas().OnPopupClose(NecoPopupCanvas.POPUP_TYPE.CAT_COOK_LIST_POPUP);

        //배스구이완료후밥그릇강조
        MapObjectSpots[1].gameObject.SetActive(true);
        MapObjectSpots[1].transform.Find("Arrow_ani").gameObject.SetActive(true);

        NecoCanvas.GetUICanvas().OnTutorialGuideUI("", LocalizeData.GetText("GQ2"), LocalizeData.GetText("GQ24"));
    }

    public void 배스구이밥그릇에담음()
    {
        if (!neco_data.CheckAndNextPrologueSeq(neco_data.PrologueSeq.배스구이완료후밥그릇강조))
            return;

        //배스구이밥그릇에담음
        MapObjectSpots[1].transform.Find("Arrow_ani").gameObject.SetActive(false);

        NecoCanvas.GetGameCanvas().CallCat(2, 0, 0, 0, ()=> {
            MapObjectSpot foodSpot = MapObjectSpots[1];
            foodSpot.CatSpot[2].StopAllCoroutines();
            foodSpot.StopFoodTimerForPrologue();

            뒷길막이선물생성();
        });

        NecoCanvas.GetGameCanvas().catUpdater.ForceMoveCat(2);
        NecoCanvas.GetPopupCanvas().OnPopupClose();

        Debug.Log("Stop Coroutine : RunCatSystem - Prologure");
        NecoGameCanvas gameCanvas = NecoCanvas.GetGameCanvas();
        gameCanvas.StopAllCoroutines();
    }

    public void 뒷길막이선물생성()
    {
        if (!neco_data.CheckAndNextPrologueSeq(neco_data.PrologueSeq.배스구이밥그릇에담음))
            return;

        NecoCanvas.GetGameCanvas().CallCat(2, 0, 0, 0, ()=> {
            Invoke("뒷길막이선물발동", 0.1f);
        });
    }

    public void 뒷길막이선물발동()
    {
        Silhouette[2].SetActive(true);
        foreach (Transform child in Silhouette[2].transform)
        {
            child.gameObject.SetActive(false);
        }

        Image image = Silhouette[2].GetComponent<Image>();
        image.DOColor(Color.white, 1.0f).OnComplete(() => {
            Silhouette[2].GetComponent<NecoSilhouetteButton>().SetGiftStateForPrologue();
        });

        Silhouette[2].GetComponent<NecoSilhouetteButton>().enabled = false;

        NecoCanvas.GetPopupCanvas().OnTouchDefend(3.0f, 뒷길막이선물대사);
    }

    public void 뒷길막이선물대사()
    {
        NecoCanvas.GetPopupCanvas().OnShowScriptsPopup(GetCurScripts());
    }

    public void 뒷길막이선물획득대사()
    {
        if (!neco_data.CheckAndNextPrologueSeq(neco_data.PrologueSeq.뒷길막이선물생성))
            return;

        Silhouette[2].SetActive(true);
        foreach (Transform child in Silhouette[2].transform)
        {
            child.gameObject.SetActive(false);
        }
        Silhouette[2].GetComponent<NecoSilhouetteButton>().enabled = false;

        Image image = Silhouette[2].GetComponent<Image>();
        image.color = new Color(0f, 0f, 0f, 0f);

        NecoCanvas.GetPopupCanvas().OnShowScriptsPopup(GetCurScripts(), 길막보은주러밥그릇소환);
    }

    public void 길막보은주러밥그릇소환()
    {
        NecoCanvas.GetGameCanvas().CallCat(2, 101, 3, 0);
        NecoCanvas.GetPopupCanvas().OnTouchDefend(1.0f, 배스구이먹으로옴);
    }

    public void 배스구이먹으로옴()
    {
        if (!neco_data.CheckAndNextPrologueSeq(neco_data.PrologueSeq.뒷길막이선물획득대사))
            return;

        Debug.Log("Stop Coroutine : RunCatSystem - Prologure");
        NecoGameCanvas gameCanvas = NecoCanvas.GetGameCanvas();
        gameCanvas.StopAllCoroutines();

        MapObjectSpot foodSpot = MapObjectSpots[1];
        foodSpot.CatSpot[2].StopAllCoroutines();

        foodSpot.StopFoodTimerForPrologue();

        //길막이배스구이먹음대사
        NecoCanvas.GetPopupCanvas().OnShowScriptsPopup(GetCurScripts(), 보은바구니UI등장);
    }

    public void 보은바구니UI등장()
    {
        if (!neco_data.CheckAndNextPrologueSeq(neco_data.PrologueSeq.길막이배스구이먹음대사))
            return;

        //neco_data.PrologueSeq.보은바구니UI등장
        보은바구니강조UI();
    }

    public void 보은바구니강조UI()
    {
        if (neco_data.PrologueSeq.보은바구니UI등장 != neco_data.GetPrologueSeq())
            return;
        
        NecoCanvas.GetGameCanvas().CallCat(2, 101, 3, 0);

        //보은바구니UI등장
        NecoCanvas.GetUICanvas().gameObject.SetActive(true);
        MainMenuPanel MainUIPanel = NecoCanvas.GetUICanvas().gameObject.GetComponentInChildren<MainMenuPanel>(true);
        MainUIPanel.gameObject.SetActive(true);

        MainUIPanel.TopUIPanel.SetActive(true);
        MainUIPanel.mainMenuToggle.gameObject.SetActive(true);
        MainUIPanel.ToggleOnObject.gameObject.SetActive(false);
        MainUIPanel.ToggleOffObject.gameObject.SetActive(true);

        MainUIPanel.mainMenuGroup.SetActive(false);
        foreach (Transform mainButton in MainUIPanel.mainMenuGroup.transform)
        {
            mainButton.gameObject.SetActive(false);
        }
        MainUIPanel.mainMenuGroup.transform.Find("CatInfoButton").gameObject.SetActive(true);
        MainUIPanel.mainMenuGroup.transform.Find("CookingButton").gameObject.SetActive(true);

        MainUIPanel.giftButton.gameObject.SetActive(true);
        MainUIPanel.trapButton.gameObject.SetActive(true);
        MainUIPanel.farmButton.gameObject.SetActive(true);
        MainUIPanel.passButton.gameObject.SetActive(false);
        MainUIPanel.shopButton.gameObject.SetActive(true);
        MainUIPanel.fishingButton.gameObject.SetActive(false);

        MainUIPanel.UpdatePrologueNotifyEffect();

        NecoCanvas.GetUICanvas().OnTutorialGuideUI(LocalizeData.GetText("GQ25"), "", "", false);

        WWWForm data = new WWWForm();
        data.AddField("api", "object");
        data.AddField("op", 3);

        NetworkManager.GetInstance().SendApiRequest("object", 3, data, (response) =>
        {
            JObject root = JObject.Parse(response);
            JToken apiToken = root["api"];
            if (null == apiToken || apiToken.Type != JTokenType.Array
                || !apiToken.HasValues)
            {
                return;
            }

            JArray apiArr = (JArray)apiToken;
            foreach (JObject row in apiArr)
            {
                string uri = row.GetValue("uri").ToString();
                if (uri == "object")
                {
                    JToken resultCode = row["rs"];
                    if (resultCode != null && resultCode.Type == JTokenType.Integer)
                    {
                        int rs = resultCode.Value<int>();
                        if (rs == 0)
                        {
                            Invoke("CheckGiftStatus", 0.1f);
                        }
                        else
                        {
                            string msg = rs.ToString();
                            switch (rs)
                            {
                                case 1: msg = LocalizeData.GetText("LOCALIZE_499"); break;
                                case 2: msg = LocalizeData.GetText("LOCALIZE_502"); break;
                                case 3: msg = LocalizeData.GetText("LOCALIZE_278"); break;
                                case 4: msg = LocalizeData.GetText("LOCALIZE_504"); break;
                            }

                            NecoCanvas.GetPopupCanvas().OnSystemMessagePopupShow(LocalizeData.GetText("LOCALIZE_278"), msg);
                        }
                    }
                }
            }
        });
    }

    public void CheckGiftStatus()
    {
        if (neco_data.PrologueSeq.보은바구니UI등장 == neco_data.GetPrologueSeq())
        {
            if (neco_data.Instance.GetGiftList().Count == 0)
            {
                첫보은받기성공();
            }
        }
    }

    public void 첫보은받기성공()
    {
        if (!neco_data.CheckAndNextPrologueSeq(neco_data.PrologueSeq.보은바구니UI등장))
            return;

        NecoCanvas.GetPopupCanvas().OnShowScriptsPopup(GetCurScripts(), ()=> {
            NecoCanvas.GetUICanvas().OnTutorialGuideUI("", LocalizeData.GetText("GQ2"), LocalizeData.GetText("GQ26"));
        });        
    }

    public void 첫보은받기퀘스트성공()
    {
        if (!neco_data.CheckAndNextPrologueSeq(neco_data.PrologueSeq.첫보은받기성공))
            return;

        //챕터2끝
        배틀패스소개();
    }

    public void 배틀패스소개()
    {
        if (!neco_data.CheckAndNextPrologueSeq(neco_data.PrologueSeq.챕터2끝))
            return;

        //배틀패스강조및대사
        NecoCanvas.GetPopupCanvas().OnShowScriptsPopup(GetCurScripts(), ()=> {
            배틀패스UI강조();
        });
    }

    public void 배틀패스UI강조()
    {
        //배틀패스 강조 및 대사
        NecoCanvas.GetUICanvas().gameObject.SetActive(true);
        MainMenuPanel MainUIPanel = NecoCanvas.GetUICanvas().gameObject.GetComponentInChildren<MainMenuPanel>(true);
        MainUIPanel.gameObject.SetActive(true);

        MainUIPanel.TopUIPanel.SetActive(true);
        MainUIPanel.mainMenuToggle.gameObject.SetActive(true);
        MainUIPanel.ToggleOnObject.gameObject.SetActive(false);
        MainUIPanel.ToggleOffObject.gameObject.SetActive(true);

        MainUIPanel.mainMenuGroup.SetActive(false);
        foreach (Transform mainButton in MainUIPanel.mainMenuGroup.transform)
        {
            mainButton.gameObject.SetActive(false);
        }
        MainUIPanel.mainMenuGroup.transform.Find("CatInfoButton").gameObject.SetActive(true);
        MainUIPanel.mainMenuGroup.transform.Find("CookingButton").gameObject.SetActive(true);

        MainUIPanel.giftButton.gameObject.SetActive(true);
        MainUIPanel.trapButton.gameObject.SetActive(true);
        MainUIPanel.farmButton.gameObject.SetActive(true);
        MainUIPanel.passButton.gameObject.SetActive(true);
        MainUIPanel.shopButton.gameObject.SetActive(true);
        MainUIPanel.fishingButton.gameObject.SetActive(false);

        MainUIPanel.passButton.transform.Find("Noti_ani").gameObject.SetActive(true);

        MainUIPanel.UpdatePrologueNotifyEffect();
    }

    public void 배틀패스보상획득()
    {
        if (!neco_data.CheckAndNextPrologueSeq(neco_data.PrologueSeq.배틀패스강조및대사))
            return;

        //배틀패스 보상 획득
    }
    public void 배틀패스완료()
    {
        if (!neco_data.CheckAndNextPrologueSeq(neco_data.PrologueSeq.배틀패스보상획득))
            return;

        NecoCanvas.GetPopupCanvas().OnPopupClose();

        NecoCanvas.GetUICanvas().gameObject.SetActive(true);
        MainMenuPanel MainUIPanel = NecoCanvas.GetUICanvas().gameObject.GetComponentInChildren<MainMenuPanel>(true);
        MainUIPanel.gameObject.SetActive(true);

        MainUIPanel.TopUIPanel.SetActive(true);
        MainUIPanel.mainMenuToggle.gameObject.SetActive(true);
        MainUIPanel.ToggleOnObject.gameObject.SetActive(false);
        MainUIPanel.ToggleOffObject.gameObject.SetActive(true);

        MainUIPanel.mainMenuGroup.SetActive(false);
        foreach (Transform mainButton in MainUIPanel.mainMenuGroup.transform)
        {
            mainButton.gameObject.SetActive(false);
        }
        MainUIPanel.mainMenuGroup.transform.Find("CatInfoButton").gameObject.SetActive(true);
        MainUIPanel.mainMenuGroup.transform.Find("CookingButton").gameObject.SetActive(true);

        MainUIPanel.giftButton.gameObject.SetActive(true);
        MainUIPanel.trapButton.gameObject.SetActive(true);
        MainUIPanel.farmButton.gameObject.SetActive(true);
        MainUIPanel.passButton.gameObject.SetActive(true);
        MainUIPanel.shopButton.gameObject.SetActive(true);
        MainUIPanel.fishingButton.gameObject.SetActive(false);

        MainUIPanel.passButton.transform.Find("Noti_ani").gameObject.SetActive(false);

        MainUIPanel.UpdatePrologueNotifyEffect();

        NecoCanvas.GetPopupCanvas().OnShowScriptsPopup(GetCurScripts(), () => {
            챕터3시작();
        });
    }

    public void 챕터3시작()
    {
        neco_data.SetPrologueSeq(neco_data.PrologueSeq.챕터3시작, true);
        //챕터3시작
        NecoCanvas.GetPopupCanvas().OnPopupClose();

        NecoCanvas.GetGameCanvas().CallCat(2, 0, 0, 0, ()=> {
            Silhouette[2].SetActive(true);
            foreach (Transform child in Silhouette[2].transform)
            {
                child.gameObject.SetActive(false);
            }
            Silhouette[2].GetComponent<NecoSilhouetteButton>().enabled = false;

            Image image = Silhouette[2].GetComponent<Image>();
            image.DOColor(Color.white, 1.0f).OnComplete(() => {
                길막이퇴장();
            });
        });

        Debug.Log("Stop Coroutine : RunCatSystem - Prologure");
        NecoGameCanvas gameCanvas = NecoCanvas.GetGameCanvas();
        gameCanvas.StopAllCoroutines();

        MapObjectSpot foodSpot = MapObjectSpots[1];
        foodSpot.CatSpot[2].StopAllCoroutines();
    }

    public void 길막이퇴장()
    {
        if (!neco_data.CheckAndNextPrologueSeq(neco_data.PrologueSeq.챕터3시작))
            return;

        Silhouette[2].SetActive(true);
        foreach (Transform child in Silhouette[2].transform)
        {
            child.gameObject.SetActive(false);
        }
        Silhouette[2].GetComponent<NecoSilhouetteButton>().enabled = false;
        Image image = Silhouette[2].GetComponent<Image>();
        image.color = Color.white;

        NecoSilhouetteButton sb = Silhouette[2].GetComponent<NecoSilhouetteButton>();
        sb.SetBoringStateForPrologue();

        NecoCanvas.GetPopupCanvas().OnTouchDefend(3.0f, 길막이심심해대사);

        Debug.Log("Stop Coroutine : RunCatSystem - Prologure");
        NecoGameCanvas gameCanvas = NecoCanvas.GetGameCanvas();
        gameCanvas.StopAllCoroutines();

        MapObjectSpot foodSpot = MapObjectSpots[1];
        foodSpot.CatSpot[2].StopAllCoroutines();
    }

    public void 길막이심심해대사()
    {
        if (!neco_data.CheckAndNextPrologueSeq(neco_data.PrologueSeq.길막이퇴장))
            return;

        Silhouette[2].SetActive(true);
        foreach (Transform child in Silhouette[2].transform)
        {
            child.gameObject.SetActive(false);
        }
        Silhouette[2].GetComponent<NecoSilhouetteButton>().enabled = false;
        Image image = Silhouette[2].GetComponent<Image>();
        image.color = Color.white;

        //길막이심심해대사
        NecoSilhouetteButton sb = Silhouette[2].GetComponent<NecoSilhouetteButton>();
        sb.SetBoringStateForPrologue();
        NecoCanvas.GetPopupCanvas().OnShowScriptsPopup(GetCurScripts(), 낚시장난감만들기);

        Debug.Log("Stop Coroutine : RunCatSystem - Prologure");
        NecoGameCanvas gameCanvas = NecoCanvas.GetGameCanvas();
        gameCanvas.StopAllCoroutines();

        MapObjectSpot foodSpot = MapObjectSpots[1];
        foodSpot.CatSpot[2].StopAllCoroutines();
    }

    public void 낚시장난감만들기()
    {
        if (!neco_data.CheckAndNextPrologueSeq(neco_data.PrologueSeq.길막이심심해대사))
            return;

        //낚시장난감만들기
        NecoCanvas.GetUICanvas().gameObject.SetActive(true);
        MainMenuPanel MainUIPanel = NecoCanvas.GetUICanvas().gameObject.GetComponentInChildren<MainMenuPanel>(true);
        MainUIPanel.gameObject.SetActive(true);

        MainUIPanel.TopUIPanel.SetActive(true);
        MainUIPanel.mainMenuToggle.gameObject.SetActive(true);
        MainUIPanel.ToggleOnObject.gameObject.SetActive(false);
        MainUIPanel.ToggleOffObject.gameObject.SetActive(true);

        MainUIPanel.mainMenuGroup.SetActive(false);
        foreach (Transform mainButton in MainUIPanel.mainMenuGroup.transform)
        {
            mainButton.gameObject.SetActive(false);
        }
        MainUIPanel.mainMenuGroup.transform.Find("CatInfoButton").gameObject.SetActive(true);
        MainUIPanel.mainMenuGroup.transform.Find("CookingButton").gameObject.SetActive(true);
        MainUIPanel.mainMenuGroup.transform.Find("CookingButton").Find("Background_2").Find("Noti_ani").gameObject.SetActive(false);
        MainUIPanel.mainMenuGroup.transform.Find("MakeToolButton").gameObject.SetActive(true);
        MainUIPanel.mainMenuGroup.transform.Find("MakeToolButton").Find("Background_2").Find("Noti_ani").gameObject.SetActive(true);

        MainUIPanel.giftButton.gameObject.SetActive(true);
        MainUIPanel.trapButton.gameObject.SetActive(true);
        MainUIPanel.farmButton.gameObject.SetActive(true);
        MainUIPanel.passButton.gameObject.SetActive(false);
        MainUIPanel.shopButton.gameObject.SetActive(true);
        MainUIPanel.fishingButton.gameObject.SetActive(false);

        MainUIPanel.UpdatePrologueNotifyEffect();

        NecoCanvas.GetUICanvas().OnTutorialGuideUI(LocalizeData.GetText("GQ27"), "", "", false);

        if(user_items.GetUserItemAmount(106) > 0)
            낚시장난감만들기완료();
    }

    public void 낚시장난감만들기완료()
    {
        if (!neco_data.CheckAndNextPrologueSeq(neco_data.PrologueSeq.낚시장난감만들기))
            return;

        // 강조 연출 off
        MainMenuPanel MainUIPanel = NecoCanvas.GetUICanvas().gameObject.GetComponentInChildren<MainMenuPanel>(true);
        MainUIPanel.mainMenuGroup.transform.Find("MakeToolButton").Find("Background_2").Find("Noti_ani").gameObject.SetActive(false);

        NecoSilhouetteButton sb = Silhouette[2].GetComponent<NecoSilhouetteButton>();
        sb.SetState();

        NecoCanvas.GetUICanvas().OnTutorialGuideUI("", LocalizeData.GetText("GQ2"), LocalizeData.GetText("GQ28"));
    }

    public void 길막이낚시장난감배치()
    {
        if (!neco_data.CheckAndNextPrologueSeq(neco_data.PrologueSeq.낚시장난감만들기완료))
            return;
        //길막이낚시장난감배치
        NecoCanvas.GetGameCanvas().CallCat(2, 2, 3, 0);
    }

    public void 길막이낚시돌발등장()
    {
        if (!neco_data.CheckAndNextPrologueSeq(neco_data.PrologueSeq.길막이낚시장난감배치))
            return;

        //길막이낚시돌발등장
        NecoCanvas.GetVideoCanvas().OnVideoPlay(203004, 길막이노잼대사);
        NecoCanvas.GetGameCanvas().CallCat(2, 0, 0, 0);
    }

    public void 길막이노잼대사()
    {
        if (!neco_data.CheckAndNextPrologueSeq(neco_data.PrologueSeq.길막이낚시돌발등장))
            return;

        //길막이노잼대사
        Silhouette[2].transform.Find("hungry").gameObject.SetActive(true);
        Silhouette[2].transform.Find("hungry").Find("Busy_bt").Find("Text").GetComponent<Text>().text = LocalizeData.GetText("GQ47");
        Silhouette[2].transform.Find("hungry").Find("Busy_bt").Find("icon").gameObject.SetActive(false);

        Invoke("길막이노잼대사2", 1.0f);
    }

    public void 길막이노잼대사2()
    {
        //길막이노잼대사
        NecoCanvas.GetPopupCanvas().OnShowScriptsPopup(GetCurScripts(), 낚시장난감오브젝트레벨업가이드);
    }
    public void 낚시장난감오브젝트레벨업가이드()
    {
        if (!neco_data.CheckAndNextPrologueSeq(neco_data.PrologueSeq.길막이노잼대사))
            return;

        NecoCanvas.GetUICanvas().gameObject.SetActive(true);
        MainMenuPanel MainUIPanel = NecoCanvas.GetUICanvas().gameObject.GetComponentInChildren<MainMenuPanel>(true);
        MainUIPanel.gameObject.SetActive(true);

        MainUIPanel.TopUIPanel.SetActive(true);
        MainUIPanel.mainMenuToggle.gameObject.SetActive(true);
        MainUIPanel.ToggleOnObject.gameObject.SetActive(false);
        MainUIPanel.ToggleOffObject.gameObject.SetActive(true);

        MainUIPanel.mainMenuGroup.SetActive(false);
        foreach (Transform mainButton in MainUIPanel.mainMenuGroup.transform)
        {
            mainButton.gameObject.SetActive(false);
        }
        MainUIPanel.mainMenuGroup.transform.Find("CatInfoButton").gameObject.SetActive(true);
        MainUIPanel.mainMenuGroup.transform.Find("CookingButton").gameObject.SetActive(true);
        MainUIPanel.mainMenuGroup.transform.Find("MakeToolButton").gameObject.SetActive(true);
        MainUIPanel.mainMenuGroup.transform.Find("MakeToolButton").Find("Background_2").Find("Noti_ani").gameObject.SetActive(true);

        MainUIPanel.giftButton.gameObject.SetActive(true);
        MainUIPanel.trapButton.gameObject.SetActive(true);
        MainUIPanel.farmButton.gameObject.SetActive(true);
        MainUIPanel.passButton.gameObject.SetActive(false);
        MainUIPanel.shopButton.gameObject.SetActive(true);
        MainUIPanel.fishingButton.gameObject.SetActive(false);

        MainUIPanel.UpdatePrologueNotifyEffect();

        if (neco_spot.GetNecoSpot(2).GetSpotLevel() >= 2)
            길막이2레벨낚시장난감연출();
    }

    public void 길막이2레벨낚시장난감연출()
    {
        if (!neco_data.CheckAndNextPrologueSeq(neco_data.PrologueSeq.낚시장난감오브젝트레벨업가이드))
            return;

        // 강조 연출 off
        MainMenuPanel MainUIPanel = NecoCanvas.GetUICanvas().gameObject.GetComponentInChildren<MainMenuPanel>(true);
        MainUIPanel.mainMenuGroup.transform.Find("MakeToolButton").Find("Background_2").Find("Noti_ani").gameObject.SetActive(false);

        // 노잼 대사 off
        Silhouette[2].transform.Find("hungry")?.gameObject.SetActive(false);

        //길막이2레벨낚시장난감연출
        NecoCanvas.GetPopupCanvas().OnPopupClose();

        Invoke("길막이부르기", 0.5f);
    }

    public void 길막이부르기()
    {
        //길막이만지기돌발발생
        NecoCanvas.GetGameCanvas().CallCat(2, 2, 3, 1, () => {
            NecoCanvas.GetPopupCanvas().OnTouchDefend(2.0f, 길막이만지기돌발대사);
        });
    }

    public void 길막이만지기돌발대사()
    {
        if (!neco_data.CheckAndNextPrologueSeq(neco_data.PrologueSeq.길막이2레벨낚시장난감연출))
            return;

        //길막이만지기돌발대사
        NecoCanvas.GetPopupCanvas().OnShowScriptsPopup(GetCurScripts(), 길막이만지기돌발발생);
    }

    public void 길막이만지기돌발발생()
    {
        if (!neco_data.CheckAndNextPrologueSeq(neco_data.PrologueSeq.길막이만지기돌발대사))
            return;

        NecoCanvas.GetUICanvas().OnTutorialGuideUI(LocalizeData.GetText("GQ29"), "", "", false);
    }

    public void 길막이낚시만지기돌발체크()
    {
        if (!neco_data.CheckAndNextPrologueSeq(neco_data.PrologueSeq.길막이만지기돌발발생))
            return;
    }

    public void 길막이낚시돌발완료()
    {
        if (neco_data.PrologueSeq.길막이낚시돌발완료 != neco_data.GetPrologueSeq())
            return;

        //길막이낚시돌발완료
        NecoCanvas.GetUICanvas().OnTutorialGuideUI("", LocalizeData.GetText("GQ2"), LocalizeData.GetText("GQ30"));
    }

    public void 첫돌발체험성공()
    {
        if (!neco_data.CheckAndNextPrologueSeq(neco_data.PrologueSeq.길막이낚시돌발완료))
            return;

        //사진찍기돌발대사

        NecoCanvas.GetPopupCanvas().OnPopupClose();
        Invoke("길막이사진돌발부르기", 1.0f);
    }

    public void 길막이사진돌발부르기()
    {
        //사진찍기돌발대사
        NecoCanvas.GetGameCanvas().CallCat(2, 2, 3, 3);
        NecoCanvas.GetPopupCanvas().OnTouchDefend(2.0f, 사진찍기돌발대사);
    }

    public void 사진찍기돌발대사()
    {
        //사진찍기돌발대사
        NecoCanvas.GetPopupCanvas().OnShowScriptsPopup(GetCurScripts());
    }

    public void 페이크사진찍기스텝()
    {
        if (neco_data.GetPrologueSeq() != neco_data.PrologueSeq.사진찍기돌발대사)
            return;

        neco_user_cat userCatInfo = neco_user_cat.GetUserCatInfo(2);
        neco_cat_memory memory = null;
        for (int i = 0; i < userCatInfo.GetPhotoMemoryCount(); i++)
        {
            memory = neco_cat_memory.GetNecoMemory(userCatInfo.GetMemories()[0]);
            if (memory != null)
            {
                string memoryType = memory.GetNecoMemoryType();
                if (memoryType == "photo" || memoryType == "ani")
                {
                    break;
                }
            }
        }

        NecoCanvas.GetPopupCanvas().OnShowGetCatPhotoPopup(neco_cat.GetNecoCat(2), memory, true, null);
    }

    public void 사진찍기완료()
    {
        if (!neco_data.CheckAndNextPrologueSeq(neco_data.PrologueSeq.사진찍기돌발대사))
            return;

        //사진찍기완료대사
        NecoCanvas.GetPopupCanvas().OnShowScriptsPopup(GetCurScripts(), 챕터5시작);
    }

    public void 챕터5시작()
    {
        neco_data.SetPrologueSeq(neco_data.PrologueSeq.챕터5시작, true);

        //챕터5시작
        NecoCanvas.GetPopupCanvas().OnPopupClose();

        OnNewCat(3);
        NecoCanvas.GetUICanvas().Invoke("RefreshNewCatAlarm", 0.5f);

        NecoCanvas.GetPopupCanvas().OnTouchDefend(1.5f, ()=> {
            NecoCanvas.GetPopupCanvas().OnShowScriptsPopup(GetCurScripts(), 스와이프가이드);
        });
    }

    public void 스와이프가이드()
    {
        if (!neco_data.CheckAndNextPrologueSeq(neco_data.PrologueSeq.챕터5시작))
            return;

        NecoCanvas.GetUICanvas().gameObject.SetActive(true);
        MainMenuPanel MainUIPanel = NecoCanvas.GetUICanvas().gameObject.GetComponentInChildren<MainMenuPanel>(true);
        MainUIPanel.gameObject.SetActive(true);

        MainUIPanel.TopUIPanel.SetActive(true);
        MainUIPanel.mainMenuToggle.gameObject.SetActive(true);
        MainUIPanel.ToggleOnObject.gameObject.SetActive(false);
        MainUIPanel.ToggleOffObject.gameObject.SetActive(true);

        MainUIPanel.mainMenuGroup.SetActive(false);
        foreach (Transform mainButton in MainUIPanel.mainMenuGroup.transform)
        {
            mainButton.gameObject.SetActive(false);
        }
        MainUIPanel.mainMenuGroup.transform.Find("CatInfoButton").gameObject.SetActive(true);
        MainUIPanel.mainMenuGroup.transform.Find("CookingButton").gameObject.SetActive(true);
        MainUIPanel.mainMenuGroup.transform.Find("MakeToolButton").gameObject.SetActive(true);

        MainUIPanel.giftButton.gameObject.SetActive(true);
        MainUIPanel.trapButton.gameObject.SetActive(true);
        MainUIPanel.farmButton.gameObject.SetActive(true);
        MainUIPanel.passButton.gameObject.SetActive(false);
        MainUIPanel.shopButton.gameObject.SetActive(true);
        MainUIPanel.fishingButton.gameObject.SetActive(false);

        MainUIPanel.UpdatePrologueNotifyEffect();

        NecoCanvas.GetUICanvas().SetMapMoveButtons(true);
        
        Instantiate(Resources.Load<GameObject>("Prefabs/Neco/Swipe_popup"), transform);        
    }

    public void 삼색이등장()
    {
        if (!neco_data.CheckAndNextPrologueSeq(neco_data.PrologueSeq.스와이프가이드))
            return;

        NecoCanvas.GetPopupCanvas().OnTouchDefend(2.0f, 챕터5끝);
    }

    public void 챕터5끝()
    {        
        if (!neco_data.CheckAndNextPrologueSeq(neco_data.PrologueSeq.삼색이등장))
            return;

        NecoCanvas.GetPopupCanvas().OnShowScriptsPopup(GetCurScripts(), () => {
            NecoCanvas.GetPopupCanvas().OnPopupShow(NecoPopupCanvas.POPUP_TYPE.TUTO_SUCCESS_POPUP);
            프리플레이();
            
            Firebase.Analytics.FirebaseAnalytics.LogEvent("tutorial_done");
        });
    }

    public void 프리플레이()
    {
        neco_data.SetPrologueSeq(neco_data.PrologueSeq.프리플레이, true);
        //프리플레이

        MainMenuPanel MainUIPanel = NecoCanvas.GetUICanvas().gameObject.GetComponentInChildren<MainMenuPanel>(true);
        MainUIPanel.gameObject.SetActive(true);

        MainUIPanel.TopUIPanel.SetActive(true);
        MainUIPanel.mainMenuToggle.gameObject.SetActive(true);
        MainUIPanel.mainMenuGroup.SetActive(MainUIPanel.mainMenuToggle.isOn);

        MainUIPanel.topMenuToggle.gameObject.SetActive(true);
        MainUIPanel.topMenuGroup.SetActive(MainUIPanel.topMenuToggle.isOn);

        MainUIPanel.giftButton.gameObject.SetActive(true);
        MainUIPanel.trapButton.gameObject.SetActive(true);
        MainUIPanel.farmButton.gameObject.SetActive(true);
        MainUIPanel.passButton.gameObject.SetActive(true);
        MainUIPanel.shopButton.gameObject.SetActive(true);
        MainUIPanel.fishingButton.gameObject.SetActive(true);

        MainUIPanel.mainMenuGroup.transform.Find("CatInfoButton").gameObject.SetActive(true);
        MainUIPanel.mainMenuGroup.transform.Find("CookingButton").gameObject.SetActive(true);
        MainUIPanel.mainMenuGroup.transform.Find("MakeToolButton").gameObject.SetActive(true);
        MainUIPanel.mainMenuGroup.transform.Find("AlbumButton").gameObject.SetActive(true);

        NecoCanvas.GetUICanvas().SetGuideUI();

        NecoCanvas.GetUICanvas().SetMapMoveButtons(true);

        if (!neco_cat.GetNecoCat(2).IsGainCat())
            NecoCanvas.GetGameCanvas().CallCat(2, 0, 0, 0);
        if (!neco_cat.GetNecoCat(3).IsGainCat() && !neco_data.Instance.IsCatReady(3))
            OnNewCat(3);

        NecoCanvas.GetGameCanvas().catUpdater.StartNecoCatUpdate();

        foreach (MapObjectSpot spot in MapObjectSpots)
        {
            spot.gameObject.SetActive(true);
        }

        Silhouette[1].GetComponent<NecoSilhouetteButton>().enabled = true;
        Silhouette[2].GetComponent<NecoSilhouetteButton>().enabled = true;

        OnGuideQuestCheckOut(neco_data.PrologueSeq.프리플레이);

        CheckGuideQuestCleared();
    }


    public void OnFishfarmButtonShow()
    {
        if (!neco_data.CheckAndNextPrologueSeq(neco_data.PrologueSeq.챕터1시작))
            return;

        //NecoCanvas.GetUICanvas().OnTutorialGuideUI("양어장에서 골드를 회수해보자.", "", "", false);

        NecoCanvas.GetUICanvas().gameObject.SetActive(true);

        MainMenuPanel MainUIPanel = NecoCanvas.GetUICanvas().gameObject.GetComponentInChildren<MainMenuPanel>(true);
        
        MainUIPanel.gameObject.SetActive(true);

        MainUIPanel.TopUIPanel.SetActive(false);
        MainUIPanel.mainMenuToggle.gameObject.SetActive(false);
        MainUIPanel.mainMenuGroup.SetActive(false);
        
        MainUIPanel.giftButton.gameObject.SetActive(false);
        MainUIPanel.trapButton.gameObject.SetActive(false);
        MainUIPanel.farmButton.gameObject.SetActive(true);
        MainUIPanel.passButton.gameObject.SetActive(false);
        MainUIPanel.shopButton.gameObject.SetActive(false);
        MainUIPanel.fishingButton.gameObject.SetActive(false);
    }

    public void OnMapMoveButtonShow()
    {
        MainMenuPanel MainUIPanel = NecoCanvas.GetUICanvas().gameObject.GetComponentInChildren<MainMenuPanel>(true);
        MainUIPanel.gameObject.SetActive(true);

        MainUIPanel.TopUIPanel.SetActive(true);
        MainUIPanel.mainMenuToggle.gameObject.SetActive(true);
        MainUIPanel.mainMenuGroup.SetActive(true);

        MainUIPanel.topMenuToggle.gameObject.SetActive(true);
        MainUIPanel.topMenuGroup.SetActive(MainUIPanel.topMenuToggle.isOn);

        MainUIPanel.giftButton.gameObject.SetActive(true);
        MainUIPanel.trapButton.gameObject.SetActive(true);
        MainUIPanel.farmButton.gameObject.SetActive(true);

        MainUIPanel.RefreshGiftButton();
        MainUIPanel.RefreshGoldButton();
        MainUIPanel.RefreshFishButton();
        MainUIPanel.UpdatePrologueNotifyEffect();

        MainUIPanel.mainMenuGroup.transform.Find("CatInfoButton").gameObject.SetActive(true);
        MainUIPanel.mainMenuGroup.transform.Find("CookingButton").gameObject.SetActive(true);
        MainUIPanel.mainMenuGroup.transform.Find("MakeToolButton").gameObject.SetActive(true);
        NecoCanvas.GetUICanvas().SetMapMoveButtons(true);
        OnNewCat(3);

        NecoCanvas.GetUICanvas().SetGuideUI();
    }

    public void OnGuideQuestCheckOut(neco_data.PrologueSeq seq)
    {
        // 모든 가이드 퀘스트 완료 시 예외처리
        if (neco_data.GetPrologueSeq() >= neco_data.PrologueSeq.PROLOGUE_SEQ_DONE) { return; }

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
        };

        if (neco_data.GetPrologueSeq() == seq)
        {
            int guideQuestCount = PlayerPrefs.GetInt("GUIDE_QUEST_COUNT", 1);
            bool countPass = true;

            if (neco_data.PrologueSeq.배틀패스보상받기 < seq)
            {
                if (guideTry.Length <= seq - neco_data.PrologueSeq.배틀패스보상받기)
                {
                    countPass = true;
                }
                else
                {
                    if (guideTry[seq - neco_data.PrologueSeq.배틀패스보상받기] > 1)
                    {
                        countPass = guideQuestCount <= 1;
                    }
                }
            }

            if (!countPass)
            {
                PlayerPrefs.SetInt("GUIDE_QUEST_COUNT", guideQuestCount - 1);

                NecoCanvas.GetUICanvas().SetGuideUI();
                return;
            }
        }
        
        if (!neco_data.CheckAndNextPrologueSeq(seq))
            return;

        seq = neco_data.GetPrologueSeq();        

        if(seq - neco_data.PrologueSeq.배틀패스보상받기 < guideTry.Length)
            PlayerPrefs.SetInt("GUIDE_QUEST_COUNT", guideTry[seq - neco_data.PrologueSeq.배틀패스보상받기]);
        else
            PlayerPrefs.SetInt("GUIDE_QUEST_COUNT", 1);

        NecoCanvas.GetUICanvas().OnRefreshGuideUI(CheckGuideQuestCleared);

        //CheckGuideQuestCleared();
    }

    public void CheckGuideQuestCleared()
    {
        neco_data.PrologueSeq seq = neco_data.GetPrologueSeq();
        if (NecoCanvas.GetPopupCanvas().IsPopupOpen(NecoPopupCanvas.POPUP_TYPE.IMAGE_TOAST_POPUP)) { return; }

        switch (seq)
        {
            case neco_data.PrologueSeq.제작대2레벨:
                if(neco_data.Instance.GetCraftRecipeLevel() >= 2)
                {        
                    OnGuideQuestCheckOut(neco_data.PrologueSeq.제작대2레벨);
                }
                break;
            case neco_data.PrologueSeq.제작대3레벨:
                if (neco_data.Instance.GetCraftRecipeLevel() >= 3)
                {
                    OnGuideQuestCheckOut(neco_data.PrologueSeq.제작대3레벨);
                }
                break;
            case neco_data.PrologueSeq.제작대4레벨:
                if (neco_data.Instance.GetCraftRecipeLevel() >= 4)
                {
                    OnGuideQuestCheckOut(neco_data.PrologueSeq.제작대4레벨);
                }
                break;
            case neco_data.PrologueSeq.제작대5레벨:
                if (neco_data.Instance.GetCraftRecipeLevel() >= 5)
                {
                    OnGuideQuestCheckOut(neco_data.PrologueSeq.제작대5레벨);
                }
                break;
            case neco_data.PrologueSeq.제작대6레벨:
                if (neco_data.Instance.GetCraftRecipeLevel() >= 6)
                {
                    OnGuideQuestCheckOut(neco_data.PrologueSeq.제작대6레벨);
                }
                break;
            case neco_data.PrologueSeq.제작대7레벨:
                if (neco_data.Instance.GetCraftRecipeLevel() >= 7)
                {
                    OnGuideQuestCheckOut(neco_data.PrologueSeq.제작대7레벨);
                }
                break;
            case neco_data.PrologueSeq.제작대8레벨:
                if (neco_data.Instance.GetCraftRecipeLevel() >= 8)
                {
                    OnGuideQuestCheckOut(neco_data.PrologueSeq.제작대8레벨);
                }
                break;
            case neco_data.PrologueSeq.제작대9레벨:
                if (neco_data.Instance.GetCraftRecipeLevel() >= 9)
                {
                    OnGuideQuestCheckOut(neco_data.PrologueSeq.제작대9레벨);
                }
                break;
            case neco_data.PrologueSeq.제작대10레벨:
                if (neco_data.Instance.GetCraftRecipeLevel() >= 10)
                {
                    OnGuideQuestCheckOut(neco_data.PrologueSeq.제작대10레벨);
                }
                break;
            case neco_data.PrologueSeq.제작대11레벨:
                if (neco_data.Instance.GetCraftRecipeLevel() >= 11)
                {
                    OnGuideQuestCheckOut(neco_data.PrologueSeq.제작대11레벨);
                }
                break;
            case neco_data.PrologueSeq.제작대12레벨:
                if (neco_data.Instance.GetCraftRecipeLevel() >= 12)
                {
                    OnGuideQuestCheckOut(neco_data.PrologueSeq.제작대12레벨);
                }
                break;

            case neco_data.PrologueSeq.양어장2레벨:
                if (neco_data.Instance.GetFishfarmLevel() >= 2)
                {
                    OnGuideQuestCheckOut(neco_data.PrologueSeq.양어장2레벨);
                }
                break;
            case neco_data.PrologueSeq.양어장3레벨:
                if (neco_data.Instance.GetFishfarmLevel() >= 3)
                {
                    OnGuideQuestCheckOut(neco_data.PrologueSeq.양어장3레벨);
                }
                break;
            case neco_data.PrologueSeq.양어장4레벨:
                if (neco_data.Instance.GetFishfarmLevel() >= 4)
                {
                    OnGuideQuestCheckOut(neco_data.PrologueSeq.양어장4레벨);
                }
                break;
            case neco_data.PrologueSeq.양어장5레벨:
                if (neco_data.Instance.GetFishfarmLevel() >= 5)
                {
                    OnGuideQuestCheckOut(neco_data.PrologueSeq.양어장5레벨);
                }
                break;
            case neco_data.PrologueSeq.양어장6레벨:
                if (neco_data.Instance.GetFishfarmLevel() >= 6)
                {
                    OnGuideQuestCheckOut(neco_data.PrologueSeq.양어장6레벨);
                }
                break;
            case neco_data.PrologueSeq.양어장7레벨:
                if (neco_data.Instance.GetFishfarmLevel() >= 7)
                {
                    OnGuideQuestCheckOut(neco_data.PrologueSeq.양어장7레벨);
                }
                break;
            case neco_data.PrologueSeq.양어장8레벨:
                if (neco_data.Instance.GetFishfarmLevel() >= 8)
                {
                    OnGuideQuestCheckOut(neco_data.PrologueSeq.양어장8레벨);
                }
                break;
            case neco_data.PrologueSeq.양어장9레벨:
                if (neco_data.Instance.GetFishfarmLevel() >= 9)
                {
                    OnGuideQuestCheckOut(neco_data.PrologueSeq.양어장9레벨);
                }
                break;
            case neco_data.PrologueSeq.양어장10레벨:
                if (neco_data.Instance.GetFishfarmLevel() >= 10)
                {
                    OnGuideQuestCheckOut(neco_data.PrologueSeq.양어장10레벨);
                }
                break;

            case neco_data.PrologueSeq.바구니2레벨:
                if (neco_data.Instance.GetGiftBasketLevel() >= 2)
                {
                    OnGuideQuestCheckOut(neco_data.PrologueSeq.바구니2레벨);
                }
                break;
            case neco_data.PrologueSeq.바구니3레벨:
                if (neco_data.Instance.GetGiftBasketLevel() >= 3)
                {
                    OnGuideQuestCheckOut(neco_data.PrologueSeq.바구니3레벨);
                }
                break;
            case neco_data.PrologueSeq.바구니4레벨:
                if (neco_data.Instance.GetGiftBasketLevel() >= 4)
                {
                    OnGuideQuestCheckOut(neco_data.PrologueSeq.바구니4레벨);
                }
                break;
            case neco_data.PrologueSeq.바구니5레벨:
                if (neco_data.Instance.GetGiftBasketLevel() >= 5)
                {
                    OnGuideQuestCheckOut(neco_data.PrologueSeq.바구니5레벨);
                }
                break;
            case neco_data.PrologueSeq.바구니6레벨:
                if (neco_data.Instance.GetGiftBasketLevel() >= 6)
                {
                    OnGuideQuestCheckOut(neco_data.PrologueSeq.바구니6레벨);
                }
                break;
            case neco_data.PrologueSeq.바구니7레벨:
                if (neco_data.Instance.GetGiftBasketLevel() >= 7)
                {
                    OnGuideQuestCheckOut(neco_data.PrologueSeq.바구니7레벨);
                }
                break;
            case neco_data.PrologueSeq.바구니8레벨:
                if (neco_data.Instance.GetGiftBasketLevel() >= 8)
                {
                    OnGuideQuestCheckOut(neco_data.PrologueSeq.바구니8레벨);
                }
                break;
            case neco_data.PrologueSeq.바구니9레벨:
                if (neco_data.Instance.GetGiftBasketLevel() >= 9)
                {
                    OnGuideQuestCheckOut(neco_data.PrologueSeq.바구니9레벨);
                }
                break;
            case neco_data.PrologueSeq.바구니10레벨:
                if (neco_data.Instance.GetGiftBasketLevel() >= 10)
                {
                    OnGuideQuestCheckOut(neco_data.PrologueSeq.바구니10레벨);
                }
                break;

            case neco_data.PrologueSeq.통발3레벨:
                if (neco_data.Instance.GetFishtrapLevel() >= 3)
                {
                    OnGuideQuestCheckOut(neco_data.PrologueSeq.통발3레벨);
                }
                break;
            case neco_data.PrologueSeq.통발4레벨:
                if (neco_data.Instance.GetFishtrapLevel() >= 4)
                {
                    OnGuideQuestCheckOut(neco_data.PrologueSeq.통발4레벨);
                }
                break;
            case neco_data.PrologueSeq.통발5레벨:
                if (neco_data.Instance.GetFishtrapLevel() >= 5)
                {
                    OnGuideQuestCheckOut(neco_data.PrologueSeq.통발5레벨);
                }
                break;
            case neco_data.PrologueSeq.통발6레벨:
                if (neco_data.Instance.GetFishtrapLevel() >= 6)
                {
                    OnGuideQuestCheckOut(neco_data.PrologueSeq.통발6레벨);
                }
                break;
            case neco_data.PrologueSeq.통발7레벨:
                if (neco_data.Instance.GetFishtrapLevel() >= 7)
                {
                    OnGuideQuestCheckOut(neco_data.PrologueSeq.통발7레벨);
                }
                break;
            case neco_data.PrologueSeq.통발8레벨:
                if (neco_data.Instance.GetFishtrapLevel() >= 8)
                {
                    OnGuideQuestCheckOut(neco_data.PrologueSeq.통발8레벨);
                }
                break;
            case neco_data.PrologueSeq.통발9레벨:
                if (neco_data.Instance.GetFishtrapLevel() >= 9)
                {
                    OnGuideQuestCheckOut(neco_data.PrologueSeq.통발9레벨);
                }
                break;
            case neco_data.PrologueSeq.통발10레벨:
                if (neco_data.Instance.GetFishtrapLevel() >= 10)
                {
                    OnGuideQuestCheckOut(neco_data.PrologueSeq.통발10레벨);
                }
                break;

            case neco_data.PrologueSeq.조리대3레벨:
                if (neco_data.Instance.GetCookRecipeLevel() >= 3)
                {
                    OnGuideQuestCheckOut(neco_data.PrologueSeq.조리대3레벨);
                }
                break;
            case neco_data.PrologueSeq.조리대4레벨:
                if (neco_data.Instance.GetCookRecipeLevel() >= 4)
                {
                    OnGuideQuestCheckOut(neco_data.PrologueSeq.조리대4레벨);
                }
                break;
            case neco_data.PrologueSeq.조리대5레벨:
                if (neco_data.Instance.GetCookRecipeLevel() >= 5)
                {
                    OnGuideQuestCheckOut(neco_data.PrologueSeq.조리대5레벨);
                }
                break;
            case neco_data.PrologueSeq.조리대6레벨:
                if (neco_data.Instance.GetCookRecipeLevel() >= 6)
                {
                    OnGuideQuestCheckOut(neco_data.PrologueSeq.조리대6레벨);
                }
                break;
            case neco_data.PrologueSeq.조리대7레벨:
                if (neco_data.Instance.GetCookRecipeLevel() >= 7)
                {
                    OnGuideQuestCheckOut(neco_data.PrologueSeq.조리대7레벨);
                }
                break;
            case neco_data.PrologueSeq.조리대8레벨:
                if (neco_data.Instance.GetCookRecipeLevel() >= 8)
                {
                    OnGuideQuestCheckOut(neco_data.PrologueSeq.조리대8레벨);
                }
                break;
            case neco_data.PrologueSeq.조리대9레벨:
                if (neco_data.Instance.GetCookRecipeLevel() >= 9)
                {
                    OnGuideQuestCheckOut(neco_data.PrologueSeq.조리대9레벨);
                }
                break;
            case neco_data.PrologueSeq.조리대10레벨:
                if (neco_data.Instance.GetCookRecipeLevel() >= 10)
                {
                    OnGuideQuestCheckOut(neco_data.PrologueSeq.조리대10레벨);
                }
                break;


            case neco_data.PrologueSeq.화이트캣하우스제작:
                if (user_items.GetUserItemAmount(109) > 0)
                {
                    OnGuideQuestCheckOut(neco_data.PrologueSeq.화이트캣하우스제작);
                }
                break;
            case neco_data.PrologueSeq.화장실제작:
                if (user_items.GetUserItemAmount(107) > 0)
                {
                    OnGuideQuestCheckOut(neco_data.PrologueSeq.화장실제작);
                }
                break;
            case neco_data.PrologueSeq.보온캣하우스제작:
                if (user_items.GetUserItemAmount(112) > 0)
                {
                    OnGuideQuestCheckOut(neco_data.PrologueSeq.보온캣하우스제작);
                }
                break;
            case neco_data.PrologueSeq.무스크래쳐제작:
                if (user_items.GetUserItemAmount(113) > 0)
                {
                    OnGuideQuestCheckOut(neco_data.PrologueSeq.무스크래쳐제작);
                }
                break;
            case neco_data.PrologueSeq.원목캣타워제작:
                if (user_items.GetUserItemAmount(115) > 0)
                {
                    OnGuideQuestCheckOut(neco_data.PrologueSeq.원목캣타워제작);
                }
                break;
            case neco_data.PrologueSeq.반자동화장실제작:
                if (user_items.GetUserItemAmount(116) > 0)
                {
                    OnGuideQuestCheckOut(neco_data.PrologueSeq.반자동화장실제작);
                }
                break;
            case neco_data.PrologueSeq.파이프캣타워제작:
                if (user_items.GetUserItemAmount(118) > 0)
                {
                    OnGuideQuestCheckOut(neco_data.PrologueSeq.파이프캣타워제작);
                }
                break;
            case neco_data.PrologueSeq.나무위캣하우스제작:
                if (user_items.GetUserItemAmount(120) > 0)
                {
                    OnGuideQuestCheckOut(neco_data.PrologueSeq.나무위캣하우스제작);
                }
                break;
            case neco_data.PrologueSeq.크리스마스캣타워제작:
                if (user_items.GetUserItemAmount(122) > 0)
                {
                    OnGuideQuestCheckOut(neco_data.PrologueSeq.크리스마스캣타워제작);
                }
                break;
            case neco_data.PrologueSeq.캣닢급식소제작:
                if (user_items.GetUserItemAmount(147) > 0)
                {
                    OnGuideQuestCheckOut(neco_data.PrologueSeq.캣닢급식소제작);
                }
                break;
            case neco_data.PrologueSeq.캣닢급식소3레벨:
                if (user_items.GetUserItemAmount(147) > 0 && neco_spot.GetNecoSpotObjectByItemID(147).GetSpotLevel() >= 3)
                {
                    OnGuideQuestCheckOut(neco_data.PrologueSeq.캣닢급식소3레벨);
                }
                break;
            case neco_data.PrologueSeq.캣닢급식소4레벨:
                if (user_items.GetUserItemAmount(147) > 0 && neco_spot.GetNecoSpotObjectByItemID(147).GetSpotLevel() >= 4)
                {
                    OnGuideQuestCheckOut(neco_data.PrologueSeq.캣닢급식소4레벨);
                }
                break;
            case neco_data.PrologueSeq.캣닢급식소5레벨:
                if (user_items.GetUserItemAmount(147) > 0 && neco_spot.GetNecoSpotObjectByItemID(147).GetSpotLevel() >= 5)
                {
                    OnGuideQuestCheckOut(neco_data.PrologueSeq.캣닢급식소5레벨);
                }
                break;
            case neco_data.PrologueSeq.화이트캣하우스5레벨:
                if (user_items.GetUserItemAmount(109) > 0 && neco_spot.GetNecoSpotObjectByItemID(109).GetSpotLevel() >= 5 || user_items.GetUserItemAmount(157) > 0)
                {
                    OnGuideQuestCheckOut(neco_data.PrologueSeq.화이트캣하우스5레벨);
                }
                break;
            case neco_data.PrologueSeq.화이트캣하우스6레벨:
                if (user_items.GetUserItemAmount(109) > 0 && neco_spot.GetNecoSpotObjectByItemID(109).GetSpotLevel() >= 6 || user_items.GetUserItemAmount(157) > 0)
                {
                    OnGuideQuestCheckOut(neco_data.PrologueSeq.화이트캣하우스6레벨);
                }
                break;
            case neco_data.PrologueSeq.화이트캣하우스7레벨:
                if (user_items.GetUserItemAmount(109) > 0 && neco_spot.GetNecoSpotObjectByItemID(109).GetSpotLevel() >= 7 || user_items.GetUserItemAmount(157) > 0)
                {
                    OnGuideQuestCheckOut(neco_data.PrologueSeq.화이트캣하우스7레벨);
                }
                break;
            case neco_data.PrologueSeq.화이트캣하우스8레벨:
                if (user_items.GetUserItemAmount(109) > 0 && neco_spot.GetNecoSpotObjectByItemID(109).GetSpotLevel() >= 8 || user_items.GetUserItemAmount(157) > 0)
                {
                    OnGuideQuestCheckOut(neco_data.PrologueSeq.화이트캣하우스8레벨);
                }
                break;
            case neco_data.PrologueSeq.화이트캣하우스9레벨:
                if (user_items.GetUserItemAmount(109) > 0 && neco_spot.GetNecoSpotObjectByItemID(109).GetSpotLevel() >= 9 || user_items.GetUserItemAmount(157) > 0)
                {
                    OnGuideQuestCheckOut(neco_data.PrologueSeq.화이트캣하우스9레벨);
                }
                break;
            case neco_data.PrologueSeq.화이트캣하우스10레벨:
                if (user_items.GetUserItemAmount(109) > 0 && neco_spot.GetNecoSpotObjectByItemID(109).GetSpotLevel() >= 10 || user_items.GetUserItemAmount(157) > 0)
                {
                    OnGuideQuestCheckOut(neco_data.PrologueSeq.화이트캣하우스10레벨);
                }
                break;
            case neco_data.PrologueSeq.플로랄캣하우스제작:
                if (user_items.GetUserItemAmount(157) > 0)
                {
                    OnGuideQuestCheckOut(neco_data.PrologueSeq.플로랄캣하우스제작);
                }
                break;
            case neco_data.PrologueSeq.플로랄캣하우스3레벨:
                if (user_items.GetUserItemAmount(157) > 0 && neco_spot.GetNecoSpotObjectByItemID(157).GetSpotLevel() >= 3)
                {
                    OnGuideQuestCheckOut(neco_data.PrologueSeq.플로랄캣하우스3레벨);
                }
                break;
            case neco_data.PrologueSeq.플로랄캣하우스4레벨:
                if (user_items.GetUserItemAmount(157) > 0 && neco_spot.GetNecoSpotObjectByItemID(157).GetSpotLevel() >= 4)
                {
                    OnGuideQuestCheckOut(neco_data.PrologueSeq.플로랄캣하우스4레벨);
                }
                break;
            case neco_data.PrologueSeq.플로랄캣하우스5레벨:
                if (user_items.GetUserItemAmount(157) > 0 && neco_spot.GetNecoSpotObjectByItemID(157).GetSpotLevel() >= 5)
                {
                    OnGuideQuestCheckOut(neco_data.PrologueSeq.플로랄캣하우스5레벨);
                }
                break;
            case neco_data.PrologueSeq.알록달록이동식캣하우스제작:
                if (user_items.GetUserItemAmount(154) > 0)
                {
                    OnGuideQuestCheckOut(neco_data.PrologueSeq.알록달록이동식캣하우스제작);
                }
                break;
            case neco_data.PrologueSeq.알록달록이동식캣하우스3레벨:
                if (user_items.GetUserItemAmount(154) > 0 && neco_spot.GetNecoSpotObjectByItemID(154).GetSpotLevel() >= 3)
                {
                    OnGuideQuestCheckOut(neco_data.PrologueSeq.알록달록이동식캣하우스3레벨);
                }
                break;
            case neco_data.PrologueSeq.알록달록이동식캣하우스4레벨:
                if (user_items.GetUserItemAmount(154) > 0 && neco_spot.GetNecoSpotObjectByItemID(154).GetSpotLevel() >= 4)
                {
                    OnGuideQuestCheckOut(neco_data.PrologueSeq.알록달록이동식캣하우스4레벨);
                }
                break;
            case neco_data.PrologueSeq.알록달록이동식캣하우스5레벨:
                if (user_items.GetUserItemAmount(154) > 0 && neco_spot.GetNecoSpotObjectByItemID(154).GetSpotLevel() >= 5)
                {
                    OnGuideQuestCheckOut(neco_data.PrologueSeq.알록달록이동식캣하우스5레벨);
                }
                break;
        }
    }

    public void CheckGuideQuestDone()
    {
        neco_data.PrologueSeq seq = neco_data.GetPrologueSeq();

        switch (seq)
        {
            case neco_data.PrologueSeq.방문고양이터치:
            case neco_data.PrologueSeq.말풍선아이템받기:
                NecoCanvas.GetPopupCanvas().OnShowScriptsPopup(GetCurScripts());
                break;
        }

        CheckGuideQuestCleared();
    }

    public void OnSilhouetteTouch(int id)
    {
        NecoCanvas.GetVideoCanvas().OnVideoPlay((uint)id * 1000, () => {
            foreach (Transform child in Silhouette[id].transform)
            {
                child.gameObject.SetActive(false);
            }

            NecoCanvas.GetGameCanvas().CallCat((uint)id, 0, 0, 0);

            neco_data.Instance.CatAppear((uint)id);

            Silhouette[id].GetComponent<NecoSilhouetteButton>().enabled = true;

            NecoCanvas.GetUICanvas().RefreshNewCatAlarm();
        });
    }


    public void OnNewCat(int id)
    {
        //uint catID = (uint)id;
        //neco_cat cat = neco_cat.GetNecoCat(catID);
        //if(cat != null)
        //{
        //    if(!cat.IsGainCat())
        //    {
        //        neco_data.Instance.ReadyCat(catID);
        //        NecoCanvas.GetUICanvas().RefreshNewCatAlarm();
        //    }
        //}
    }
    public override void OnCatSilhouetteIn()
    {
        //??
    }

    public override void OnCatSilhouetteOut()
    {
        //??
    }

    [ContextMenu("CHECK PIVOT")]
    public void CheckPivot()
    {

        foreach (Spine.Unity.SkeletonGraphic sk in GetComponentsInChildren<Spine.Unity.SkeletonGraphic>(true))
        {
            Bone bone = sk.Skeleton.FindBone("pivot");
            if (bone == null)
            {
                string path = "";
                Transform parent = sk.transform;
                while(parent != null && parent != transform)
                {
                    path = parent.name + "/" + path;
                    parent = parent.parent;
                }
                Debug.LogError("not found pivot : " + path);
            }
        }
    }
}
