using Coffee.UIExtensions;
using DG.Tweening;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static SBWeb;

public class HalloweenEventPopup : Popup
{
    [SerializeField] GameObject type_bingo;
    [SerializeField] GameObject type_notiy;
    [SerializeField] GameObject ClearDim;

    [SerializeField] Button bingoBtn;
    [SerializeField] HalloweenRewardNotiSample getRewardSample_all;
    [SerializeField] HalloweenRewardNotiSample getRewardSample;
    [SerializeField] HalloweenBingoItem[] bingoItems;

    [Header("[UI 구성 리소스]")]
    [SerializeField] Text text_popupTitle;
    [SerializeField] Text text_gradeBingoReward;
    [SerializeField] Text text_gradeBingoName;
    [SerializeField] Text text_correctionCount;
    [SerializeField] Text text_totalCandyCnt;
    [SerializeField] Text text_btn_candycnt;

    [Header("[UI 파티클]")]
    [SerializeField] List<UIParticle> uIParticles = new List<UIParticle>();


    List<int> bingoValue = new List<int>();
    int currentPageType = 1;                                    // 1 => 빙고 페이지 2 => 규칙 3 => 연출 중
    List<GameData> datas = new List<GameData>();
    int cur_stage = 0;                                          //현재 빙고 스테이지
    int stage_cnt = 0;                                          //현재 스테이지 시도 횟수
    int line_clear = 0;
    const int item_Candy = 30;                                  //캔디 아이템 번호

    int pick_row = 0;
    int pick_col = 0;
    public override void Open(CloseCallback cb = null)
    {
        SetPage();
        TryBingo();

        datas = Managers.Data.GetData(GameDataManager.DATA_TYPE.event_bingo_reward);
        base.Open(cb);
    }

    public override void RefreshUI()
    {
        text_gradeBingoReward.text = StringManager.GetString("ui_bingo_stage_reward", cur_stage);
        text_gradeBingoName.text = StringManager.GetString("ui_bingo_stage", cur_stage);
        text_correctionCount.text = StringManager.GetString("ui_bingo_start_count", stage_cnt);
        text_totalCandyCnt.text = Managers.UserData.GetMyItemCount(item_Candy).ToString();
        text_btn_candycnt.text = (Managers.Data.GetData(GameDataManager.DATA_TYPE.event_bingo_info, 1) as EventBingoInfoData).event_item_value.ToString();
        ClearItem();

        Dictionary<EventBingoRewardData, bool> dataDic = new Dictionary<EventBingoRewardData, bool>();
        foreach (EventBingoRewardData data in datas)
        {
            if (data.bingo_num == cur_stage && data.reward_num < 8 && data.type == 2)
            {
                dataDic.Add(data, data.reward_num <= line_clear);

            }
            if (data.bingo_num == cur_stage && data.reward_num == 8 && data.type == 2)
            {
                getRewardSample_all.SetItem(StringManager.GetString("ui_bingo_allclear", data.bingo_num), StringManager.GetString(getRewardSample_all.SetReward(ShopPackageGameData.GetRewardDataList(data.goods_id)).GetDesc()), data.reward_num == line_clear);
                getRewardSample_all.SetReward(ShopPackageGameData.GetRewardDataList(data.goods_id));
            }

            if (data.bingo_num == cur_stage && data.type == 1)
            {
                bingoItems[data.reward_num - 1].Init(ShopPackageGameData.GetRewardDataList(data.goods_id));
            }
        }

        getRewardSample.gameObject.SetActive(true);
        foreach (var data in dataDic)
        {
            if (!data.Value)
            {
                var item = GameObject.Instantiate(getRewardSample, getRewardSample.transform.parent);
                item.SetItem(StringManager.GetString("ui_bingo_line", data.Key.reward_num), StringManager.GetString(item.SetReward(ShopPackageGameData.GetRewardDataList(data.Key.goods_id)).GetDesc()), data.Key.reward_num <= line_clear); ; ;
                item.SetReward(ShopPackageGameData.GetRewardDataList(data.Key.goods_id));
            }
        }
        foreach (var data in dataDic)
        {
            if (data.Value)
            {
                var item = GameObject.Instantiate(getRewardSample, getRewardSample.transform.parent);
                item.SetItem(StringManager.GetString("ui_bingo_line", data.Key.reward_num), StringManager.GetString(item.SetReward(ShopPackageGameData.GetRewardDataList(data.Key.goods_id)).GetDesc()), data.Key.reward_num <= line_clear); ; ;
                item.SetReward(ShopPackageGameData.GetRewardDataList(data.Key.goods_id));
            }
        }
        getRewardSample.gameObject.SetActive(false);
        dataDic.Clear();

        //단계 = 5 , 빙고 전부 클리어 시 버튼 비활성화 
        if (AllClear())
            bingoBtn.interactable = false;
        else
            bingoBtn.interactable = true;

        ClearDim.SetActive(!bingoBtn.interactable);
        if (bingoBtn.interactable)
        {
            foreach (Transform item in bingoBtn.transform)
            {
                if (item.GetComponent<Graphic>() != null && item.GetComponent<Text>() != null)
                {
                    item.GetComponent<Graphic>().color = bingoBtn.colors.normalColor;
                    item.GetComponent<Text>().color = Color.black;
                }
                else if (item.GetComponent<Graphic>() != null)
                    item.GetComponent<Graphic>().color = bingoBtn.colors.normalColor;

            }
        }
        else
        {
            foreach (Transform item in bingoBtn.transform)
            {
                if (item.GetComponent<Graphic>() != null && item.GetComponent<Text>() != null)
                {
                    item.GetComponent<Graphic>().color = bingoBtn.colors.disabledColor;
                    item.GetComponent<Text>().color = Color.black;
                }
                else if (item.GetComponent<Graphic>() != null)
                    item.GetComponent<Graphic>().color = bingoBtn.colors.disabledColor;
            }
        }

        RefreshUIBingoItems();
    }

    public void SetPage(int setPage = 1)
    {
        currentPageType = setPage;

        type_bingo.SetActive(currentPageType == 1 ? true : false);
        type_notiy.SetActive(currentPageType == 2 ? true : false);
    }

    public override void Close()
    {
        if (currentPageType == 3)
            return;

        base.Close();
    }

    public void ClearItem()
    {
        foreach (Transform tr in getRewardSample.transform.parent)
        {
            if (tr == getRewardSample.transform)
                continue;
            Destroy(tr.gameObject);
        }
    }
    public void RefreshUIBingoItems()
    {
        if (bingoValue.Count > 0)
        {
            int idx = 0;
            foreach (var item in bingoItems)
            {
                item.Clear(bingoValue[idx]);
                idx++;
            }
        }
    }

    public void PlayAnim(List<ResponseReward> rewards, bool lineAnim, bool staage, int row, int col)
    {
        pick_row = row;
        pick_col = col;
        Debug.Log($"현재 픽 보상 ::: {pick_row} -> {pick_col}");
        StartCoroutine(RewardAnim(lineAnim, staage, rewards));
    }

    //연출 코루틴
    public IEnumerator RewardAnim(bool lineAnim, bool stage, List<ResponseReward> rewards)
    {
        var prevPage = currentPageType;
        currentPageType = 3;

        int preidx = -1;
        float time = 0.05f;
        //PopupCanvas.Instance.ShowFadeText("연출 시작합니다");
        for (int i = 0; i < 15; i++)
        {
            int random = Random.Range(0, bingoItems.Length);
            if (random == preidx)
            {
                time += 0.02f;
                continue;
            }
            preidx = random;
            bingoItems[random].ParticlePlay(bingoItems[random].particle_grid, time);

            time += 0.02f;
            yield return new WaitForSeconds(time);
        }

        int pick = (pick_col - 1) + ((pick_row - 1) * 3);
        bingoItems[pick].ParticlePlay(bingoItems[pick].particle_grid);
        yield return new WaitForSeconds(0.05f);

        if (!bingoItems[pick].stamp.activeSelf)
        {
            uIParticles.Find(_ => _.name == "fx_halown_stamp00").transform.position = bingoItems[pick].stamp.transform.position;
            uIParticles.Find(_ => _.name == "fx_halown_stamp00").gameObject.SetActive(true);
            uIParticles.Find(_ => _.name == "fx_halown_stamp00").Play();
            bingoItems[pick].Clear(1);
            yield return new WaitForSeconds(1f);
            uIParticles.Find(_ => _.name == "fx_halown_stamp00").Stop();
            uIParticles.Find(_ => _.name == "fx_halown_stamp00").gameObject.SetActive(false);

            //라인 빙고 연출
            if (lineAnim)
            {
                yield return new WaitForSeconds(0.5f);
                //PopupCanvas.Instance.ShowFadeText("라인 빙고 성공!!!!");
                uIParticles.Find(_ => _.name == "fx_halown_bingo00").gameObject.SetActive(true);
                uIParticles.Find(_ => _.name == "fx_halown_bingo00").Play();
            }

            //PopupCanvas.Instance.ShowFadeText("연출 끝났습니다.");
            yield return new WaitForSeconds(0.7f);
            uIParticles.Find(_ => _.name == "fx_halown_bingo00").Stop();
            uIParticles.Find(_ => _.name == "fx_halown_bingo00").gameObject.SetActive(false);
        }
        else
            yield return new WaitForSeconds(0.3f);
        foreach (HalloweenBingoItem item in bingoItems)
        {
            item.ParticleStop();
        }
        PopupCanvas.Instance.ShowRewardResult(rewards, () =>
        {
            //스테이지 변경 연출
            if (stage)
            {
                Sequence sequence = DOTween.Sequence();
                sequence.AppendInterval(0.5f);
                sequence.AppendCallback(() =>
                {
                    uIParticles.Find(_ => _.name == "fx_halown_change00").gameObject.SetActive(true);
                    uIParticles.Find(_ => _.name == "fx_halown_change00").Play();
                    text_gradeBingoName.transform.DOScale(Vector3.one * 2f, 0.3f).SetEase(Ease.OutCubic).OnComplete(() =>
                    {
                        text_gradeBingoName.transform.DOScale(Vector3.one, 0.05f);
                    });

                });
                sequence.AppendCallback(() => { RefreshUI(); });
                sequence.AppendInterval(1f);
                sequence.AppendCallback(() =>
                {
                    uIParticles.Find(_ => _.name == "fx_halown_change00").Stop();
                    uIParticles.Find(_ => _.name == "fx_halown_change00").gameObject.SetActive(false);
                }).OnComplete(() =>
                {
                    currentPageType = 1;
                });
            }
        });
        if (!stage)
        {
            RefreshUI();
            currentPageType = 1;
        }
    }

    /// <param name="try_bingo">
    /// 조회 = 0 , 0 < 빙고 시도
    /// </param>
    public void TryBingo(int try_bingo = 0)
    {
        if (try_bingo > 0)
        {
            if (Managers.UserData.GetMyItemCount(item_Candy) <= 0 ||
                (Managers.Data.GetData(GameDataManager.DATA_TYPE.event_bingo_info, 1) as EventBingoInfoData).event_item_value > Managers.UserData.GetMyItemCount(item_Candy))
            {
                PopupCanvas.Instance.ShowFadeText(StringManager.GetString("msg_candy_lack"));
                var _audio = Managers.Resource.LoadAssetsBundle<AudioClip>("AssetsBundle/Sounds/effect/BUTTON_NEGATIVE");
                Managers.Sound.Play(_audio);
                return;
            }
        }
        if (currentPageType == 3)
            return;

        uIParticles.Find(_ => _.name == "fx_halown_candy").Play();
        if (try_bingo != 0)
        {
            var audio = Managers.Resource.LoadAssetsBundle<AudioClip>("AssetsBundle/Sounds/effect/BUTTON_START_BINGO");
            Managers.Sound.Play(audio);
        }

        bingoValue.Clear();
        pick_row = 0;
        pick_col = 0;
        SBWeb.TryBingo(try_bingo, (res) =>
        {
            cur_stage = res["cur_stage"].Value<int>();
            stage_cnt = res["stage_cnt"].Value<int>();
            line_clear = res["line_clear"].Value<int>();

            bingoValue.Add(res["box_1_1"].Value<int>());
            bingoValue.Add(res["box_1_2"].Value<int>());
            bingoValue.Add(res["box_1_3"].Value<int>());
            bingoValue.Add(res["box_2_1"].Value<int>());
            bingoValue.Add(res["box_2_2"].Value<int>());
            bingoValue.Add(res["box_2_3"].Value<int>());
            bingoValue.Add(res["box_3_1"].Value<int>());
            bingoValue.Add(res["box_3_2"].Value<int>());
            bingoValue.Add(res["box_3_3"].Value<int>());

            if (currentPageType != 3)
                RefreshUI();
        });
    }

    public bool CheckLine(int line)
    {
        if (line_clear != line)
            return true;
        return false;
    }

    public bool CheckStage(int stage)
    {
        if (cur_stage != stage)
            return true;
        return false;
    }

    public bool AllClear()
    {
        if (cur_stage == 5)
        {
            foreach (var item in bingoValue)
            {
                if (item == 0)
                    return false;
            }

            return true;
        }

        return false;
    }
}
