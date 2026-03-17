using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SandboxNetwork
{
    #region OBJECTS
    public abstract class SBObject
    {
        public Vector3 Position { get; protected set; } = Vector3.zero;
        public eDirectionBit Direction { get; protected set; } = eDirectionBit.None;
        public SBObject()
        {
            SetPosition(Vector3.zero);
        }
        public SBObject(Vector3 position)
        {
            SetPosition(position);
        }

        public virtual void SetPosition(Vector3 position) {
            Position = position;
        }
        public virtual void SetDirection(eDirectionBit dir)
        {
            Direction = dir;
        }
        public virtual bool IsContain(Vector3 target)
        {
            switch (Direction)
            {
                case eDirectionBit.Right:
                    return Position.x <= target.x;
                case eDirectionBit.Left:
                    return Position.x >= target.x;
                case eDirectionBit.Up:
                    return Position.y <= target.y;
                case eDirectionBit.Down:
                    return Position.y >= target.y;
                default:
                    return true;
            }
        }

        public virtual bool IsContain(Circle target)
        {
            return IsContain(target.Position, target.Radius);
        }

        public virtual bool IsContain(Vector3 targetPos, float radius)
        {
            var distance = Vector2.Distance(Position, targetPos);
            if (distance <= radius)
            {
                return true;
            }

            return false;
        }

        public abstract Vector3 ContactPosition(Vector3 unitVector);
        public abstract float ContactDistance(Vector3 unitVector);
        public virtual void Refresh() { }
    }
    public class Circle : SBObject
    {
        public eCircleType RadiusType { get; protected set; } = eCircleType.Circle;
        public float Radius { get; protected set; } = 0f;
        public float RadiusX { get; protected set; } = 0f;
        public float RadiusY { get; protected set; } = 0f;
        private Vector3 f1 = Vector3.zero;
        public Vector3 F1
        {
            get { return f1; }
            set { f1 = value; }
        }
        private Vector3 f2 = Vector3.zero;
        public Vector3 F2
        {
            get { return f2; }
            set { f2 = value; }
        }

        public Circle(Vector3 position, float radiusX, float radiusY)
        {
            SetPosition(position);
            SetEllipse(radiusX, radiusY);
        }

        public void SetEllipse(float radiusX, float radiusY) {
            RadiusX = radiusX;
            RadiusY = radiusY;
            Refresh();
        }

        public void AddRadius(float radius)
        {
            AddRadius(radius, radius);
        }

        public void AddRadius(float rx, float ry)
        {
            SetEllipse(RadiusX + rx, RadiusY + ry);
            Refresh();
        }
        public override void Refresh()
        {
            f1 = new Vector3(Position.x, Position.y, Position.z);
            f2 = new Vector3(Position.x, Position.y, Position.z);
            if (RadiusX == RadiusY)
            {
                Radius = RadiusX;
                RadiusType = eCircleType.Circle;
            }
            else if (RadiusX > RadiusY)
            {
                Radius = RadiusX;
                RadiusType = eCircleType.XEllipse;
                var f = Mathf.Sqrt(RadiusX * RadiusX - RadiusY * RadiusY);
                f1.x -= f;
                f2.x += f;
            }
            else
            {
                Radius = RadiusY;
                RadiusType = eCircleType.YEllipse;
                var f = Mathf.Sqrt(RadiusY * RadiusY - RadiusX * RadiusX);
                f1.y -= f;
                f2.y += f;
            }
        }

        public override bool IsContain(Vector3 target)
        {
            if (base.IsContain(target))
            {
                if (Position == target)
                    return true;

                switch (RadiusType)
                {
                    case eCircleType.Circle:
                    {
                        var distanceF1 = Vector2.Distance(Position, target);
                        if (distanceF1 <= Radius)
                        {
                            return true;
                        }
                    }
                    break;
                    case eCircleType.XEllipse:
                    case eCircleType.YEllipse:
                    {
                        var distanceF1 = Vector2.Distance(target, f1);
                        var distanceF2 = Vector2.Distance(target, f2);
                        if ((distanceF1 + distanceF2) <= (2 * Radius))
                        {
                            return true;
                        }
                    }
                    break;
                }
            }
            return false;
        }

        public override bool IsContain(Vector3 targetPos, float radius)
        {
            var distance = Vector2.Distance(Position, targetPos);
            if (distance <= (radius + Radius))
            {
                return true;
            }

            return false;
        }

        public override Vector3 ContactPosition(Vector3 unitVector)
        {
            return Position + new Vector3(unitVector.x * RadiusX, unitVector.y * RadiusY, 0f);
        }

        public override float ContactDistance(Vector3 unitVector)
        {
            return RadiusType switch
            {
                eCircleType.Circle => Radius,
                _ => Vector3.Distance(Position, ContactPosition(unitVector))
            };
        }
    }

    public class SBRect : SBObject
    {
        public float RadiusX { get; protected set; } = 0f;
        public float RadiusY { get; protected set; } = 0f;

        public SBRect(Vector3 position, float radiusX, float radiusY)
        {
            SetPosition(position);
            SetEllipse(radiusX, radiusY);
        }

        public SBRect(BoxCollider2D collider)
        {
            SetPosition(collider.transform.position + new Vector3(collider.offset.x, collider.offset.y, 0));
            SetEllipse(collider.size.x * 0.5f, collider.size.y * 0.5f);
        }

        public void SetEllipse(float radiusX, float radiusY)
        {
            RadiusX = radiusX;
            RadiusY = radiusY;
        }

        public override bool IsContain(Vector3 target)
        {
            if (base.IsContain(target))
            {
                if (Position == target)
                    return true;

                var diffPos = target - Position;
                if (Mathf.Abs(diffPos.x) > RadiusX || RadiusY < Mathf.Abs(diffPos.y))
                    return false;

                return true;
            }

            return false;
        }

        public override Vector3 ContactPosition(Vector3 unitVector)
        {
            if(unitVector.x > unitVector.y)
            {
                return Position + new Vector3(0 > unitVector.x ? RadiusX : -RadiusX, unitVector.y * RadiusY, 0f);
            }
            else if(unitVector.x < unitVector.y)
            {
                return Position + new Vector3(unitVector.x * RadiusX, 0 > unitVector.y ? RadiusY : -RadiusY, 0f);
            }
            else
            {
                return Position + new Vector3(0 > unitVector.x ? RadiusX : -RadiusX, 0 > unitVector.y ? RadiusY : -RadiusY, 0f);
            }
        }

        public override float ContactDistance(Vector3 unitVector)
        {
            return Vector3.Distance(Position, ContactPosition(unitVector));
        }
    }

    public class Cone : SBObject
    {
        public Cone(Vector3 position, eDirectionBit dir, float radius, float angle)
        {
            SetPosition(position);
            SetDirection(dir);
            SetCone(radius, angle);
        }
        public float Radius { get; protected set; } = 0f;
        public float Angle { get; protected set; } = 1f;

        protected float dirAngle = 0f;
        protected Vector3 ab, ac;
        protected Vector3 p2, p3 = default;
        public virtual void SetCone(float radius, float angle)
        {
            Radius = radius;
            Angle = angle * 0.5f;
            Refresh();
        }
        public override void Refresh()
        {
            switch (Direction)
            {
                case eDirectionBit.Left:
                {
                    ab = new Vector3(Mathf.Sin((Angle - 90f) * Mathf.Deg2Rad), Mathf.Cos((Angle - 90f) * Mathf.Deg2Rad), 0);
                    ac = new Vector3(Mathf.Sin((-Angle - 90f) * Mathf.Deg2Rad), Mathf.Cos((-Angle - 90f) * Mathf.Deg2Rad), 0);
                    p2 = Position + ab * Radius;
                    p3 = Position + ac * Radius;
                }
                break;
                case eDirectionBit.Right:
                case eDirectionBit.None:
                default:
                {
                    ab = new Vector3(Mathf.Sin((Angle + 90f) * Mathf.Deg2Rad), Mathf.Cos((Angle + 90f) * Mathf.Deg2Rad), 0);
                    ac = new Vector3(Mathf.Sin((-Angle + 90f) * Mathf.Deg2Rad), Mathf.Cos((-Angle + 90f) * Mathf.Deg2Rad), 0);
                    p2 = Position + ab * Radius;
                    p3 = Position + ac * Radius;
                }
                break;
            }
        }

        public override bool IsContain(Vector3 target)
        {
            if (Position == target)
                return true;

            float alpha = ((p2.y - p3.y) * (target.x - p3.x) + (p3.x - p2.x) * (target.y - p3.y)) /
               ((p2.y - p3.y) * (Position.x - p3.x) + (p3.x - p2.x) * (Position.y - p3.y));
            float beta = ((p3.y - Position.y) * (target.x - p3.x) + (Position.x - p3.x) * (target.y - p3.y)) /
                            ((p2.y - p3.y) * (Position.x - p3.x) + (p3.x - p2.x) * (Position.y - p3.y));
            float gamma = 1.0f - alpha - beta;

            bool contain = alpha > 0f && beta > 0f && gamma > 0f;
            if(contain)
                return true;

            return false;
        }

        public override Vector3 ContactPosition(Vector3 unitVector)
        {
            return Position + new Vector3(unitVector.x * Radius, unitVector.y * Radius, 0f);
        }

        public override float ContactDistance(Vector3 unitVector)
        {
            return Vector3.Distance(Position, ContactPosition(unitVector));
        }
    }
    #endregion
    #region OBJECT_EXTENSION
    public class Explosion
    {
        public IBattleCharacterData Caster { get; protected set; } = null;
        public Vector3 TargetPosition { get; protected set; } = default;
        public SBObject MinObject { get; protected set; } = null;
        public SBObject MaxObject { get; protected set; } = null;
        public float MinValue { get; protected set; } = 0f;
        public float MaxValue { get; protected set; } = 1f;
        public void SetData(IBattleCharacterData caster, Vector3 worldPosition, SkillEffectData data)
        {
            if (caster == null || data == null)
                return;

            Caster = caster;
            TargetPosition = worldPosition;
            MinValue = data.EX_GROUND_MIN * SBDefine.CONVERT_FLOAT;
            MaxValue = data.EX_GROUND_MAX * SBDefine.CONVERT_FLOAT;
            if (MaxValue < MinValue)
                MaxValue = 1f;
            switch (data.EX_RANGE_TYPE)
            {
                case eSkillRangeType.CIRCLE_C:
                    MinObject = new Circle(TargetPosition, data.EXPLOSION_GROUND_X, data.EXPLOSION_GROUND_Y);
                    MaxObject = new Circle(TargetPosition, data.EXPLOSION_X, data.EXPLOSION_Y);
                    break;
                case eSkillRangeType.CIRCLE_F:
                    MinObject = new Circle(TargetPosition, data.EXPLOSION_GROUND_X, data.EXPLOSION_GROUND_Y);
                    MinObject.SetDirection(Caster.IsEnemy ? eDirectionBit.Left : eDirectionBit.Right);
                    MaxObject = new Circle(TargetPosition, data.EXPLOSION_X, data.EXPLOSION_Y);
                    MaxObject.SetDirection(Caster.IsEnemy ? eDirectionBit.Left : eDirectionBit.Right);
                    break;
                case eSkillRangeType.SQUARE_C:
                    MinObject = new SBRect(TargetPosition, data.EXPLOSION_GROUND_X, data.EXPLOSION_GROUND_Y);
                    MaxObject = new SBRect(TargetPosition, data.EXPLOSION_X, data.EXPLOSION_Y);
                    break;
                case eSkillRangeType.SQUARE_F:
                    MinObject = new SBRect(TargetPosition, data.EXPLOSION_GROUND_X, data.EXPLOSION_GROUND_Y);
                    MinObject.SetDirection(Caster.IsEnemy ? eDirectionBit.Left : eDirectionBit.Right);
                    MaxObject = new SBRect(TargetPosition, data.EXPLOSION_X, data.EXPLOSION_Y);
                    MaxObject.SetDirection(Caster.IsEnemy ? eDirectionBit.Left : eDirectionBit.Right);
                    break;
                default:
                    break;
            }
        }
        public void SetData(IBattleCharacterData caster, Vector3 worldPosition, SkillSummonData data)
        {
            if (worldPosition == null || data == null)
                return;


            Caster = caster;
            TargetPosition = worldPosition;
            MinValue = data.GROUND_MIN;
            switch (data.RANGE_TYPE)
            {
                case eSkillRangeType.CIRCLE_C:
                    MinObject = new Circle(TargetPosition, data.GROUND_X, data.GROUND_Y);
                    MaxObject = new Circle(TargetPosition, data.RANGE_X, data.RANGE_Y);
                    break;
                case eSkillRangeType.CIRCLE_F:
                    MinObject = new Circle(TargetPosition, data.GROUND_X, data.GROUND_Y);
                    MinObject.SetDirection(Caster.IsEnemy ? eDirectionBit.Left : eDirectionBit.Right);
                    MaxObject = new Circle(TargetPosition, data.RANGE_X, data.RANGE_Y);
                    MaxObject.SetDirection(Caster.IsEnemy ? eDirectionBit.Left : eDirectionBit.Right);
                    break;
                case eSkillRangeType.SQUARE_C:
                    MinObject = new SBRect(TargetPosition, data.GROUND_X, data.GROUND_Y);
                    MaxObject = new SBRect(TargetPosition, data.RANGE_X, data.RANGE_Y);
                    break;
                case eSkillRangeType.SQUARE_F:
                    MinObject = new SBRect(TargetPosition, data.GROUND_X, data.GROUND_Y);
                    MinObject.SetDirection(Caster.IsEnemy ? eDirectionBit.Left : eDirectionBit.Right);
                    MaxObject = new SBRect(TargetPosition, data.RANGE_X, data.RANGE_Y);
                    MaxObject.SetDirection(Caster.IsEnemy ? eDirectionBit.Left : eDirectionBit.Right);
                    break;
                default:
                    break;
            }
        }
        public float ExplosionValue(Vector3 target)
        {
            var unitVector3 = (target - TargetPosition).normalized;
            var targetDistance = Vector3.Distance(TargetPosition, target);
            var minDistance = MinObject.ContactDistance(unitVector3);
            var maxDistance = MaxObject.ContactDistance(unitVector3);
            if (unitVector3 == Vector3.zero || targetDistance < minDistance)
                return MaxValue;
            else if (targetDistance > maxDistance)
                return 0f;
            else
                return MinValue + (MaxValue - MinValue) * (maxDistance - targetDistance) / (maxDistance - minDistance);
        }
    }
    #endregion
}