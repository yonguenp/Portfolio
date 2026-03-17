using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SandboxNetwork
{
    public class BattleState : StateBase
    {
        protected static List<IBattleEventObject> events = new();

        public BattleStage Stage { get; protected set; } = null;
        public IBattleData Data { get; protected set; } = null;
        protected Dictionary<IBattleCharacterData, float> sinusoidalDic = null;
        public void Set(BattleStage stage, IBattleData data)
        {
            Stage = stage;
            Data = data;

        }
        public virtual void Clear()
        {
            if (events == null)
                events = new List<IBattleEventObject>();
            else
                events.Clear();
        }
        public override bool Destroy()
        {
            if(base.Destroy())
            {
                Clear();
                Stage = null;
                Data = null;
                return true;
            }
            return false;
        }
        protected bool IsData
        {
            get
            {
                return Stage != null && Data != null;
            }
        }
        public override bool OnEnter()
        {
            if (base.OnEnter() && IsData)
            {
                if (sinusoidalDic == null)
                    sinusoidalDic = new();
                else
                    sinusoidalDic.Clear();

                return true;
            }
            return false;
        }
        public override bool OnExit()
        {
            if (base.OnExit() && IsData)
            {
                return true;
            }
            return false;
        }
        public override bool OnPause()
        {
            if (base.OnPause() && IsData)
            {
                return true;
            }
            return false;
        }
        public override bool OnResume()
        {
            if (base.OnResume() && IsData)
            {
                return true;
            }
            return false;
        }
        public override bool Update(float dt)
        {
            if (base.Update(dt) && IsData)
            {
                Data.Update(dt);
                UpdateEvent(dt);
                return true;
            }
            return false;
        }
        protected virtual void ShowErrorSystemPopup()
        {
            SystemPopup.OnSystemPopup(StringData.GetStringByIndex(100000248), StringData.GetStringByIndex(100000636), StringData.GetStringByStrKey("확인"), "",
                    () => {
                        UIManager.Instance.InitUI(eUIType.None);
                        LoadingManager.Instance.LoadStartScene();
                        PopupManager.AllClosePopup();
                    }, null,
                    () =>
                    {
                        UIManager.Instance.InitUI(eUIType.None);
                        LoadingManager.Instance.LoadStartScene();
                        PopupManager.AllClosePopup();
                    });
        }
        #region Position
        protected virtual Vector3 GetOffenseStartFieldPosition(float offset)
        {
            return Stage.GetOffenseStartFieldPosition(offset);
        }
        protected virtual Vector3 GetOffenseEndFieldPosition(float offset)
        {
            return Stage.GetOffenseEndFieldPosition(offset);
        }
        protected virtual Vector3 GetMonsterStartFieldPosition(float xOffset, float extraOffset = 0)
        {
            return Stage.GetMonsterStartFieldPosition(xOffset, extraOffset);
        }
        protected virtual Vector3 GetMonsterEndFieldPosition(float offset)
        {
            return Stage.GetMonsterEndFieldPosition(offset);
        }
        protected float GetDragonPositionY(int x, int y, int maxY)
        {
            return Stage.GetDragonPositionY(x, y, maxY);
        }
        protected virtual Vector3 GetDragonStartPosition(int x, int y, int maxY)
        {
            return Stage.GetDragonStartPosition(x, y, maxY);
        }
        protected virtual Vector3 GetDragonEndPosition(int x, int y, int maxY)
        {
            return Stage.GetDragonEndPosition(x, y, maxY);
        }
        public float GetBattlePosY(int y, int maxY)
        {
            return Stage.GetBattlePosY(y, maxY);
        }
        protected virtual Vector3 GetMonsterStartPosition(int x, int y, int maxY, float xOffset, float extraOffset = 0)
        {
            return Stage.GetMonsterStartPosition(x, y, maxY, xOffset, extraOffset);
        }
        protected Vector3 GetMonsterEndPosition(int x, int y, int maxY)
        {
            return Stage.GetMonsterEndPosition(x, y, maxY);
        }
        protected Vector3 GetBattleMonsterPosition(int x, int y, int maxY)
        {
            return Stage.GetBattleMonsterPosition(x, y, maxY);
        }
        protected Vector3 GetBattleDragonPosition(int x, int y, int maxY)
        {
            return Stage.GetBattleDragonPosition(x, y, maxY);
        }
        protected void UpdateSinusoidalCharacter(float dt, IBattleCharacterData cData)
        {
            if (cData == null)
                return;

            var spine = cData.GetSpine();
            if (spine == null)
                return;

            var spineTransform = spine.SpineTransform;
            if (spineTransform == null)
                return;

            float time = 0f;
            if (!sinusoidalDic.TryAdd(cData, 0f))
                time = sinusoidalDic[cData];
            
            var curTime = time + dt;
            sinusoidalDic[cData] = curTime;
            var Sinusoidal = SBFunc.Sinusoidal(curTime, 4f, 0.05f * cData.MOVE_SPEED);

            var pos = spineTransform.localPosition;
            pos.x = Sinusoidal;
            spineTransform.localPosition = pos;

            var shadowTransform = spine.ShadowTransform;
            if (shadowTransform == null)
                return;
            var sPos = shadowTransform.localPosition;
            sPos.x = Sinusoidal;
            shadowTransform.localPosition = sPos;
        }
        protected void ResetSinusoidalCharacter(IBattleCharacterData cData)
        {
            if (cData == null)
                return;

            var spine = cData.GetSpine();
            if (spine == null)
                return;

            var spineTransform = spine.SpineTransform;
            if (spineTransform == null)
                return;

            var pos = spineTransform.localPosition;
            pos.x = 0f;
            spineTransform.localPosition = pos;

            var shadowTransform = spine.ShadowTransform;
            if (shadowTransform == null)
                return;

            var sPos = shadowTransform.localPosition;
            sPos.x = 0f;
            shadowTransform.localPosition = sPos;
        }
        #endregion
        #region Event
        protected void AddEvent(IBattleEventObject obj)
        {
            if (events == null)
                return;

            events.Add(obj);
        }
        protected void UpdateEvent(float dt)
        {
            if (events == null)
                return;

            for (int i = 0, count = events.Count; i < count; ++i)
            {
                events[i]?.Update(dt);
            }

            events.RemoveAll((eventObject) =>
            {
                return eventObject.IsEnd;
            });
        }
        #endregion
    }
}