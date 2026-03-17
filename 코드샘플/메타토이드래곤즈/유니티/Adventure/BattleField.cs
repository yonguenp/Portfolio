using System.Collections.Generic;
using UnityEngine;

namespace SandboxNetwork
{
    [System.Serializable]
    public class BattleField
    {
        [SerializeField]
        protected List<GameObject> targets = null;
        [SerializeField]
        protected float speed = 1f;
        [SerializeField]
        protected bool loop = true;
        [SerializeField]
        protected bool isTop = false;

        private List<SpriteRenderer> renderers = null;
        List<Sprite> spriteList = null;
#if false//UNITY_EDITOR
        public void Init(List<GameObject> t, float s, bool l)
        {
            targets = t;
            speed = s;
            loop = l;
        }
#endif//해당 Init은 맵 리뉴얼로 들어감.(평소엔 안씀)
        void LoadSpriteArray()
        {
            spriteList = new List<Sprite>();
            for (int i = 0, count = targets.Count; i < count; ++i)
            {
                if (targets[i] == null)
                    continue;

                foreach (SpriteRenderer spr in targets[i].GetComponentsInChildren<SpriteRenderer>())
                {
                    if (spriteList.Contains(spr.sprite))
                        break;

                    spriteList.Add(spr.sprite);
                }
            }
        }
        public void UpdateLeft(float dt)
        {
            if (targets == null)
                return;

            UpdateMove(dt);
        }

        public void UpdateRight(float dt)
        {
            if (targets == null)
                return;

            UpdateMove(dt * -1.0f);
        }

        public void UpdateMove(float dt)
        {
            if (targets == null)
                return;

            for (int i = 0, count = targets.Count; i < count; ++i)
            {
                if (targets[i] == null)
                    continue;

                var pos = targets[i].transform.localPosition;
                pos.x -= speed * dt;

                targets[i].transform.localPosition = pos;
            }
        }

        public void OffsetCheck()
        {
            for (int i = 0, count = targets.Count; i < count; ++i)
            {
                if (targets[i] == null)
                    continue;

                if (loop)
                {
                    var pos = targets[i].transform.position;
                    if (pos.x <= BattleMap.LimitLeft)
                    {
                        pos.x += BattleMap.SpencingX;
                        MoveRightPosition(i);
                        targets[i].transform.position = pos;
                    }

                    if (pos.x >= BattleMap.LimitRight)
                    {
                        pos.x -= BattleMap.SpencingX;
                        MoveLeftPosition(i);
                        targets[i].transform.position = pos;
                    }
                }
            }
        }

        void MoveRightPosition(int index)
        {
            if (spriteList == null)
                LoadSpriteArray();
            if (spriteList == null)
                return;
            if (!IsTop())
                return;

            Transform cur = targets[index].transform;
            Transform prev = targets[(targets.Count + (index - 1)) % targets.Count].transform;

            Transform copy_target = prev.GetChild(prev.childCount - 1);
            if (copy_target == null)
                return;
            SpriteRenderer sp = copy_target.GetComponentInChildren<SpriteRenderer>();
            if (sp == null)
                return;

            Sprite sprite = sp.sprite;
            int lastIndex = spriteList.IndexOf(sprite);
            if (lastIndex < 0 || lastIndex >= spriteList.Count)
                return;

            int addIndex = 1;

            foreach (SpriteRenderer render in cur.GetComponentsInChildren<SpriteRenderer>())
            {
                render.sprite = spriteList[(spriteList.Count + lastIndex + addIndex) % spriteList.Count];
                addIndex++;
            }
        }

        void MoveLeftPosition(int index)
        {
            if (spriteList == null)
                LoadSpriteArray();
            if (spriteList == null)
                return;
            if (!IsTop())
                return;

            Transform cur = targets[index].transform;
            Transform next = targets[(targets.Count + (index + 1)) % targets.Count].transform;

            Transform copy_target = next.GetChild(0);
            if (copy_target == null)
                return;
            SpriteRenderer sp = copy_target.GetComponentInChildren<SpriteRenderer>();
            if (sp == null)
                return;

            Sprite sprite = sp.sprite;
            int lastIndex = spriteList.IndexOf(sprite);
            if (lastIndex < 0 || lastIndex >= spriteList.Count)
                return;

            int addIndex = -1;

            foreach (Transform child in cur)
            {

                while (addIndex < 0)
                {
                    addIndex += spriteList.Count;
                }
                SpriteRenderer render = child.GetComponentInChildren<SpriteRenderer>();
                render.sprite = spriteList[(spriteList.Count + lastIndex + addIndex) % spriteList.Count];
                addIndex--;
            }
        }

        public bool IsTop()
        {
            return isTop;
        }

        public void AddOpacity(float v)
        {
            if (renderers == null)
            {
                renderers = new List<SpriteRenderer>();
                for (int i = 0, count = targets.Count; i < count; ++i)
                {
                    renderers.AddRange(targets[i].GetComponentsInChildren<SpriteRenderer>());
                }
            }

            foreach (SpriteRenderer render in renderers)
            {
                Color color = render.color;
                if (color.a >= 1.0f)
                    continue;
                color.a += v;
                render.color = color;
            }
        }

        public void SubOpacity(float v)
        {
            if (renderers == null)
            {
                renderers = new List<SpriteRenderer>();
                for (int i = 0, count = targets.Count; i < count; ++i)
                {
                    renderers.AddRange(targets[i].GetComponentsInChildren<SpriteRenderer>());
                }
            }

            foreach (SpriteRenderer render in renderers)
            {
                Color color = render.color;
                if (color.a <= 0.0f)
                    continue;
                color.a -= v;
                render.color = color;
            }
        }

        public void SetTopPosition(float posY)
        {
            if (targets == null)
                return;

            for (int i = 0, count = targets.Count; i < count; ++i)
            {
                if (targets[i] == null)
                    continue;

                var pos = targets[i].transform.localPosition;
                pos.y = posY;

                targets[i].transform.localPosition = pos;
            }
        }
    }
}