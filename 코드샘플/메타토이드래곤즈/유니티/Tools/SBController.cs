using SandboxNetwork;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SandboxNetwork
{
    public class SBController : MonoBehaviour
    {
        public CircleCollider2D myCollider { get; private set; }

        [SerializeField]
        protected bool isRight = false;
        public bool IsRight
        {
            get { return isRight; }
            set { isRight = value; }
        }
        [SerializeField]
        protected float speed = 1f;
        public float Speed
        {
            get { return speed; }
            set { speed = value * 0.01f; }
        }
        public bool IsMove { get; protected set; } = false;
        private eDirectionBit typeBit = eDirectionBit.None;
        private IEnumerator curCO = null;

        private void OnEnable()
        {
            myCollider = GetComponent<CircleCollider2D>();
        }

        private void OnDisable()
        {
            myCollider = null;
        }

        public void MoveEnter(eDirectionBit directionBit)
        {
            typeBit |= directionBit;
        }
        public void MoveExit(eDirectionBit directionBit)
        {
            typeBit &= ~directionBit;
        }
        public void MoveAllExit()
        {
            typeBit = eDirectionBit.None;
        }
        public virtual void OnController(float dt, bool direction = true, bool isWalk = false, float speed = -1)
        {
            IsMove = false;
            if (speed >= 0)
                Speed = speed;

            if (typeBit.HasFlag(eDirectionBit.Up))
            {
                transform.localPosition += (Vector3.up * (Speed * dt));
                IsMove = true;
            }
            if (typeBit.HasFlag(eDirectionBit.Down))
            {
                transform.localPosition += (Vector3.down * (Speed * dt));
                IsMove = true;
            }
            if (typeBit.HasFlag(eDirectionBit.Right))
            {
                transform.localPosition += (Vector3.right * (Speed * dt));
                IsMove = true;
                if (direction)
                    transform.localScale = new Vector3(isRight ? Mathf.Abs(transform.localScale.x) : -Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
            }
            if (typeBit.HasFlag(eDirectionBit.Left))
            {
                transform.localPosition += (Vector3.left * (Speed * dt));
                IsMove = true;
                if (direction)
                    transform.localScale = new Vector3(isRight ? -Mathf.Abs(transform.localScale.x) : Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
            }
        }

        Vector3 Get8WayNormal(Vector3 normal)
        {
            Vector3 ret = normal;
            //Vector3[] dir = {
            //                    new Vector3(0.0f, 1.0f, normal.z).normalized,
            //                    new Vector3(1.0f, 1.0f, normal.z).normalized,
            //                    new Vector3(1.0f, 0.0f, normal.z).normalized,
            //                    new Vector3(1.0f, -1.0f, normal.z).normalized,
            //                    new Vector3(0.0f, -1.0f, normal.z).normalized,
            //                    new Vector3(-1.0f, -1.0f, normal.z).normalized,
            //                    new Vector3(-1.0f, 0.0f, normal.z).normalized,
            //                    new Vector3(-1.0f, 1.0f, normal.z).normalized,
            //                };

            //float[] dis = {
            //                    Vector3.Distance(normal,dir[0]),
            //                    Vector3.Distance(normal,dir[1]),
            //                    Vector3.Distance(normal,dir[2]),
            //                    Vector3.Distance(normal,dir[3]),
            //                    Vector3.Distance(normal,dir[4]),
            //                    Vector3.Distance(normal,dir[5]),
            //                    Vector3.Distance(normal,dir[6]),
            //                    Vector3.Distance(normal,dir[7]),
            //                };

            //float min = Mathf.Min(dis);

            //for (int i = 0; i < dis.Length; i++)
            //{
            //    if (min == dis[i])
            //    {
            //        ret = dir[i];
            //        break;
            //    }
            //}

            return ret;
        }
        public virtual void MoveWorldTargetUpdate(float dt, Vector3 worldPos, Vector3 lookupPos, float speed = -1)
        {
            var normal = (transform.position - lookupPos).normalized;
            if (normal.x > 0)
                transform.localScale = new Vector3(isRight ? Mathf.Abs(transform.localScale.x) : -Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
            else if (normal.x < 0)
                transform.localScale = new Vector3(isRight ? -Mathf.Abs(transform.localScale.x) : Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);

            MoveWorldTargetUpdate(dt, worldPos, false, speed);
        }

        public virtual void MoveWorldTargetUpdate(float dt, Vector3 worldPos, bool direction = true, float speed = -1)
        {
            if (speed >= 0)
                Speed = speed;

            Vector3 normal = (worldPos - transform.position).normalized;
            float magnitude = (worldPos - transform.position).magnitude;

            if (direction)
            {
                if (normal.x < 0)
                    transform.localScale = new Vector3(isRight ? Mathf.Abs(transform.localScale.x) : -Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
                else if (normal.x > 0)
                    transform.localScale = new Vector3(isRight ? -Mathf.Abs(transform.localScale.x) : Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
            }

            if(magnitude > 1.0f)
                normal = Get8WayNormal(normal);

            transform.position = Vector3.MoveTowards(transform.position, transform.position + (normal * magnitude), dt * Speed);
        }
        public virtual void MoveLocalTargetUpdate(float dt, Vector3 localPos, Vector3 lookupPos, float speed = -1)
        {
            var normal = (transform.localPosition - lookupPos).normalized;
            if (normal.x > 0)
                transform.localScale = new Vector3(isRight ? Mathf.Abs(transform.localScale.x) : -Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
            else if (normal.x < 0)
                transform.localScale = new Vector3(isRight ? -Mathf.Abs(transform.localScale.x) : Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);

            MoveWorldTargetUpdate(dt, localPos, false, speed);
        }

        public virtual void MoveLocalTargetUpdate(float dt, Vector3 localPos, bool direction = true, float speed = -1)
        {
            if (speed >= 0)
                Speed = speed;

            Vector3 normal = (localPos - transform.localPosition).normalized;
            float magnitude = (localPos - transform.localPosition).magnitude;

            if (direction)
            {
                if (normal.x < 0)
                    transform.localScale = new Vector3(isRight ? Mathf.Abs(transform.localScale.x) : -Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
                else if (normal.x > 0)                                                                                                                           
                    transform.localScale = new Vector3(isRight ? -Mathf.Abs(transform.localScale.x) : Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
            }

            if (magnitude > 1.0f)
                normal = Get8WayNormal(normal);

            transform.localPosition = Vector3.MoveTowards(transform.localPosition, transform.localPosition + (normal * magnitude), dt * Speed);
        }

        protected virtual IEnumerator MoveLocalTargetCO(Vector3 localPos, float distance = 0f, bool direction = true, float speed = -1, Action cb = null)
        {
            if (speed >= 0)
                Speed = speed;

            if (direction)
            {
                var normal = (transform.localPosition - localPos).normalized;
                if (normal.x > 0)
                    transform.localScale = new Vector3(isRight ? Mathf.Abs(transform.localScale.x) : -Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
                else if (normal.x < 0)
                    transform.localScale = new Vector3(isRight ? -Mathf.Abs(transform.localScale.x) : Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
            }

            if (distance <= 0f)
                distance = 0.005f;

            IsMove = true;

            while (Vector2.Distance(transform.localPosition, localPos) > distance)
            {
                if (!IsMove)
                {
                    cb?.Invoke();
                    yield break;
                }

                yield return null;

                MoveLocalTargetUpdate(SBGameManager.Instance.DTime, localPos, false, speed);
            }

            IsMove = false;
            
            cb?.Invoke();
            yield break;
        }

        protected virtual IEnumerator MoveWorldTargetCO(Vector3 worldPos, float distance = 0f, bool direction = true, float speed = -1)
        {
            if (speed >= 0)
                Speed = speed;

            if (direction)
            {
                var normal = (transform.position - worldPos).normalized;
                if (normal.x > 0)
                    transform.localScale = new Vector3(isRight ? Mathf.Abs(transform.localScale.x) : -Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
                else if (normal.x < 0)
                    transform.localScale = new Vector3(isRight ? -Mathf.Abs(transform.localScale.x) : Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
            }

            if (distance <= 0f)
                distance = 0.005f;

            IsMove = true;

            while (Vector2.Distance(transform.position, worldPos) > distance)
            {
                if (!IsMove)
                    yield break;

                yield return null;

                MoveWorldTargetUpdate(SBGameManager.Instance.DTime, worldPos, false, speed);
            }

            IsMove = false;
            yield break;
        }

        public virtual void MoveLocalTarget(Vector3 localPos, Action cb)
        {
            MoveLocalTarget(localPos, 0, true, -1, cb);
        }
        public virtual void MoveLocalTarget(Vector3 localPos, float distance = 0f, bool direction = true, float speed = -1, Action cb = null)
        {
            StopCO();

            if (!gameObject.activeInHierarchy)
                return;
            curCO = MoveLocalTargetCO(localPos, distance, direction, speed, cb);
            StartCoroutine(curCO);
        }

        public virtual void MoveWorldTarget(Vector3 worldPos, float distance = 0f, bool direction = true, float speed = -1)
        {
            StopCO();

            if (!gameObject.activeInHierarchy)
                return;
            curCO = MoveWorldTargetCO(worldPos, distance, direction, speed);
            StartCoroutine(curCO);
        }

        public virtual bool UpdateLocalTarget(float dt, Vector3 localPos, float distance = 0f, bool direction = false, float speed = -1)
        {
            StopCO();
            if (speed >= 0)
                Speed = speed;

            if (distance <= 0f)
                distance = 0.005f;

            if (Vector2.Distance(transform.localPosition, localPos) > distance)
            {
                MoveLocalTargetUpdate(dt, localPos, direction, speed);
                return true;
            }

            return false;
        }

        public virtual bool UpdateWorldTarget(float dt, Vector3 worldPos, float distance = 0f, bool direction = true, float speed = -1)
        {
            StopCO();
            if (speed >= 0)
                Speed = speed;

            if (distance <= 0f)
                distance = 0.005f;

            if (Vector2.Distance(transform.position, worldPos) > distance)
            {
                MoveWorldTargetUpdate(dt, worldPos, direction, speed);
                return true;
            }

            return false;
        }

        public virtual bool UpdateWorldTargetTile(float dt, Vector3 worldPos, float distance = 0f, bool direction = true, float speed = -1)
        {
            StopCO();
            if (speed >= 0)
                Speed = speed;

            if (distance <= 0f)
                distance = 0.005f;

            if(direction)
            {
                var vecDir = transform.position - worldPos;
                vecDir = vecDir.normalized;
                var lotate = Mathf.Rad2Deg * (Mathf.Atan2(vecDir.y, vecDir.x));
                transform.localRotation = Quaternion.Euler(0f, 0f, lotate + 180f);
            }

            if (Vector2.Distance(transform.position, worldPos) > distance)
            {
                MoveTileWorldTargetUpdate(dt, worldPos, false, speed);
                return true;
            }

            return false;
        }

        public virtual void MoveTileWorldTargetUpdate(float dt, Vector3 worldPos, bool direction = true, float speed = -1)
        {
            if (speed >= 0)
                Speed = speed;

            Vector3 normal = (worldPos - transform.position).normalized;
            float magnitude = (worldPos - transform.position).magnitude;

            if (direction)
            {
                if (normal.x < 0)
                    transform.localScale = new Vector3(isRight ? Mathf.Abs(transform.localScale.x) : -Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
                else if (normal.x > 0)
                    transform.localScale = new Vector3(isRight ? -Mathf.Abs(transform.localScale.x) : Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
            }

            transform.position = Vector3.MoveTowards(transform.position, transform.position + (normal * magnitude), dt * Speed);
        }

        public virtual void StopCO()
        {
            if (curCO != null)
            {
                IsMove = false;
                StopCoroutine(curCO);
                curCO = null;
            }
        }
    }
}