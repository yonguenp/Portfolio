using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SandboxNetwork
{
    public struct LandmarkUpdateEvent
    {
        private static LandmarkUpdateEvent obj;

        public eLandmarkType eLandmark;
        public static void Send(eLandmarkType type)
        {
            obj.eLandmark = type;
            EventManager.TriggerEvent(obj);
        }
    }
    public class Landmark
    {
        public int Tag { get; private set; } = 0;
        public BuildInfo BuildInfo { get { return User.Instance.GetUserBuildingInfoByTag(Tag); } }
        public eLandmarkType Type { get; private set; } = eLandmarkType.UNKNOWN;

        public Landmark(eLandmarkType type)
        {
            Type = type;
        }

        public virtual void SetData(JToken jsonData)
        {
            if (SBFunc.IsJTokenCheck(jsonData) && SBFunc.IsJTokenCheck(jsonData["tag"]))
            {
                Tag = jsonData["tag"].Value<int>();
            }
        }
    }
}