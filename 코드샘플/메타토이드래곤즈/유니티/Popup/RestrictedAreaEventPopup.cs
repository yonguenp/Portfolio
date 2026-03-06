using Newtonsoft.Json.Linq;
using SandboxNetwork;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class RestrictedAreaEventPopup : OpenEventRankingPopup
{
    public override void InitUI()
    {
        List<eOpenEventRankingPage> pages = new List<eOpenEventRankingPage>();
        List<EventScheduleData> events = User.Instance.EventData.GetActiveEvents(false);
        foreach (var data in events)
        {
            int remain = User.Instance.EventData.GetRemainTime(data, false);
            if (remain > 0)
            {
                switch (data.TYPE)
                {
                    case eActionType.RESTRICTED_AREA_EVENT:
                        switch ((eOpenEventRankingPage)int.Parse(data.KEY))
                        {
                            case eOpenEventRankingPage.ARENA_XMAS1:
                                pages.Add(eOpenEventRankingPage.ARENA_XMAS1);
                                break;
                            case eOpenEventRankingPage.ARENA_XMAS2:
                                pages.Add(eOpenEventRankingPage.ARENA_XMAS2);
                                break;
                            case eOpenEventRankingPage.RAID_XMAS1:
                                pages.Add(eOpenEventRankingPage.RAID_XMAS1);
                                break;
                            case eOpenEventRankingPage.RAID_XMAS2:
                                pages.Add(eOpenEventRankingPage.RAID_XMAS2);
                                break;
                            case eOpenEventRankingPage.RESTRICTED_XMAS1:
                                pages.Add(eOpenEventRankingPage.RESTRICTED_XMAS1);
                                break;
                            case eOpenEventRankingPage.RESTRICTED_XMAS2:
                                pages.Add(eOpenEventRankingPage.RESTRICTED_XMAS2);
                                break;
                        }
                        break;
                }
            }
        }


        if (controller != null)
        {
            controller.SetTabList(pages);
            if (pages.Count > 0)
            {
                eOpenEventRankingPage page = pages[0];
                controller.Init(page);
            }
        }
    }
}
