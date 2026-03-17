using Spine.Unity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace SandboxNetwork
{
	public abstract class BattleSpine : SBSpine<eSpineAnimation>
	{
		protected IBattleMap Map { get; set; } = null;
		ParticleSystem dust = null;
		
		Vector2 collisionVector = Vector2.zero;
		protected FollowSpine pet = null;
		protected MeshRenderer mesh = null;
		protected float defaultScale = 1f;

		//KnockBack
		protected bool isKnockBack = false;
		protected float knockBackMinTime = 0.02f;
		protected float knockBackTime = 0f;
		protected float knockBackMaxTime = 0.1375f;//0.275f; 용준님 피드백
		protected readonly float knockBackPower = 0.3f;
		//

		public statusEffect StatusEffect { get; protected set; } = null;
		public IBattleCharacterData Data { get; protected set; } = null;
		[SerializeField]
		private SBController controller = null;
		public virtual SBController Controller
		{
			get { return controller; }
			private set { controller = value; }
		}
		protected bool isDeath = false;
		public bool IsDeath { get { return Data.Death && isDeath; } }
		public bool IsBoss => Data != null && Data.IsBoss;

		protected EffectCustomSender effectSender = null;
		public Vector3 spinePivotPos { get; protected set; } = Vector3.zero;

		public override void InitializeComponent()
        {
            base.InitializeComponent();
			Controller = gameObject.GetComponent<SBController>();
			if (Controller == null)
				Controller = gameObject.AddComponent<SBController>();
		}

        public override void Init()
		{
			base.Init();
			
			if( mesh == null )
				mesh = skeletonAni.GetComponent<MeshRenderer>();

			if (skeletonAni != null)
				skeletonAni.timeScale = defaultScale * SBGameManager.Instance.TimeScale;

			effectSender = GetComponent<EffectCustomSender>();
			if (effectSender != null)
				effectSender.SetCaster(this, Data);

			SetShadow(true);
		}

		public virtual void SetData(IBattleCharacterData data)
        {
			Data = data;
			if (Data != null)
				SetData(Data.BaseData as ISpineCharacterData);
        }

		public void SetMapData(IBattleMap map)
        {
			Map = map;
		}

		public virtual void SetDamage(int value, IBattleCharacterData caster)
		{
			if (Data == null)
			{
				Death();
				return;
            }

			if (Data.Stat.GetStatusInt(eStatusCategory.ADD_BUFF, eStatusType.SHIELD_POINT) > 0)
			{
				for (int i = 0, count = Data.Infos.Count; i < count; ++i)
				{
					if (Data.Infos[i] == null)
						continue;

					value = Data.Infos[i].SetDamage(value);
					if (value == 0)
						break;
				}
			}

			Data.HP -= value;
			if (Data.HP <= 0)
			{
				Data.HP = 0;
				Death();
			}
		}
		public abstract void Hit(); //타격 처리
		public virtual void KnockBackHit()
		{
			if (Data != null)
			{
				if (Data.Death || Data.Untouchable) 
				{
					return;
				}

				if (knockBackTime <= knockBackMinTime || knockBackTime >= (knockBackMaxTime - knockBackMinTime))
				{
					knockBackTime = knockBackMaxTime;
				}
			}
		}

		public void SetDefaultScale()
		{
			if (skeletonAni != null)
				skeletonAni.timeScale = defaultScale * SBGameManager.Instance.TimeScale;
		}
		public float GetAnimScale(eSpineAnimation anim = eSpineAnimation.NONE)
        {
			if (anim == eSpineAnimation.NONE)
				anim = Animation;

            switch (anim)
			{
				case eSpineAnimation.A_CASTING:
				{
					if (Data.NormalSkill != null)
					{
						var animTime = GetAnimaionTime(eSpineAnimation.A_CASTING);
						if (animTime > 0f && Data.CastingDelay > 0f)
							return animTime / Data.CastingDelay;
					}
					return SBGameManager.Instance.TimeScale * Data.Stat.GetAttackSpeed();
				}
				case eSpineAnimation.ATTACK:
				{
					if (Data.NormalSkill != null)
					{
						var animTime = GetAnimaionTime(eSpineAnimation.ATTACK);
						if (animTime > 0f && Data.AfterDelay > 0f && animTime > Data.AfterDelay)
							return animTime / Data.AfterDelay * Data.Stat.GetAttackSpeed();
					}
					return SBGameManager.Instance.TimeScale * Data.Stat.GetAttackSpeed();
				}
				case eSpineAnimation.CASTING:
				{
					if (Data.Skill1 != null)
					{
						var animTime = GetAnimaionTime(eSpineAnimation.CASTING);
						if (animTime > 0f && Data.CastingDelay > 0f)
							return animTime / Data.CastingDelay;
					}
					return defaultScale * SBGameManager.Instance.TimeScale;
				}
                default:
                    return defaultScale * SBGameManager.Instance.TimeScale;
            }
        }

		protected void InitAnimation(eSpineAnimation anim)
        {
			SetAnimation(anim);

			if (GetTypeToLoop(Animation))
			{
				RandomAnimationFrame();
			}
		}

		public void SetRigidbodySimulated(bool simulated)
        {
			if(Controller != null && Controller.myCollider != null && Controller.myCollider.attachedRigidbody != null)			
				Controller.myCollider.attachedRigidbody.simulated = false;
		}
		public override Spine.TrackEntry SetAnimation(eSpineAnimation anim)
		{
			if (Animation == anim)
				return null;

			if (GetTypeToSkip(Animation, anim))
				return null;

			Animation = anim;

			switch (Animation)
			{
				case eSpineAnimation.WALK:
				case eSpineAnimation.IDLE:
					if(AdventureStage.Instance != null || WorldBossStage.Instance != null)
						SetRigidbodySimulated(false);
					break;
				
				case eSpineAnimation.WIN:
				case eSpineAnimation.LOSE:
				case eSpineAnimation.DEATH:
					SetRigidbodySimulated(false);
                    break;

				case eSpineAnimation.CASTING:
					if(Data.Skill1 != null && Data.Skill1.CASTING_EFFECT_RSC_KEY > 0f)
                    {
						StartCoroutine(nameof(StartCastingEffect));
					}
					break;
				case eSpineAnimation.SKILL:
					skeletonAni.ClearState();//스킬은 클리어?
					break;
			}

            var animName = GetTypeToName(anim);
			if (skeletonAni == null || skeletonAni.AnimationName == animName || string.IsNullOrEmpty(animName))
				return null;

			bool loop = GetTypeToLoop(Animation);
			var ret = base.SetAnimation(0, animName, loop);
			if (effectSender != null && !Data.IsEnemy)
				effectSender.Send(Animation);

			if (loop)
			{
				RandomAnimationFrame();
			}

			return ret;
		}

		public void SetMoveSpeed(float speed)
		{
			if (Controller != null)
				Controller.Speed = speed;
		}

		public void SetSpeed(int speed)
		{
			if (speed <= 0)
				return;

			SetSpeed((float)speed / SBDefine.TownDefaultSpeed);
		}

		public void SetSpeed(float scale)
		{
			if (skeletonAni == null)
				return;

			skeletonAni.timeScale = scale;
		}

		public void SetDefaultSpeed(float speed)
        {
			defaultScale = speed;
			if(skeletonAni != null)
				skeletonAni.timeScale = defaultScale * SBGameManager.Instance.TimeScale;
		}

		protected virtual void RandomAnimationFrame()
		{
			if (skeletonAni != null)
				skeletonAni.Update(SBFunc.RandomValue * 3.0f);
		}

		public void ClearAnimation()
        {
			if (Animation == eSpineAnimation.DEATH)
				return;

			Animation = eSpineAnimation.NONE;
			SetAnimation(eSpineAnimation.IDLE);
        }

		Dictionary<Collision2D, Vector2> collisions = new Dictionary<Collision2D, Vector2>();
        private void OnCollisionEnter2D(Collision2D collision)
        {
			if (collision.rigidbody == null)
				return;

			Vector2 my = Controller.myCollider.bounds.center;
			Vector2 other = collision.collider.bounds.center;

			Vector2 vector = my - other;
			vector = vector.normalized;
			if (collisions.ContainsKey(collision) == false)
            {
				collisions.Add(collision, vector);
			}
			else
            {
				collisionVector -= collisions[collision];
				collisions[collision] = vector;
			}

			collisionVector += vector;
		}
        private void OnCollisionExit2D(Collision2D collision)
        {
			RemoveCollision(collision);
			
			if (collisions.Count <= 0)
				collisionVector = Vector2.zero;
		}
		void RemoveCollision(Collision2D collision)
        {
			if (collisions.ContainsKey(collision))
			{
				collisionVector -= collisions[collision];
				collisions.Remove(collision);
			}			
		}
		public FollowSpine GetPet()
		{
			return pet;
		}

		public void SetPet(FollowSpine p)
		{
			pet = p;
		}

		public void OnDust()
        {
            if (dust == null)
            {
                Vector3 pos = transform.position;
                if (Controller.myCollider != null)
                {
                    pos.x = Controller.myCollider.bounds.center.x;
                    pos.y = Controller.myCollider.bounds.center.y - (Controller.myCollider.radius * transform.localScale.y);
                }

				//pos.x += data.IsEnemy ? 0.5f : -0.5f;

                GameObject dust_resource = ResourceManager.GetResource<GameObject>(eResourcePath.EffectPrefabPath, "fx_common_skill_006");
                if (dust_resource == null)
                    return;

                GameObject dustObject = Instantiate(dust_resource, transform.parent);
				dustObject.transform.SetAsFirstSibling();

#if UNITY_EDITOR
				dustObject.name = "DUST_EFFECT";
#endif
				dust = dustObject.GetComponent<ParticleSystem>();
				dustObject.transform.position = pos;
            }

            if (dust != null)
            {
				if (!dust.gameObject.activeSelf)
					dust.gameObject.SetActive(true);

				dust.Play();
            }
		}


		public void MoveDust(Vector3 vector)
        {
			if(dust == null) OnDust();
            dust.gameObject.SetActive(true);
            dust.transform.Translate(vector);
        }
		public void SetDustDragonPos(bool left)
		{
			if (dust == null) 
				OnDust();

			dust.transform.position = transform.position + (left ? Vector3.left : Vector3.right) * 0.1f;

			foreach (Transform child in dust.transform)
			{
				Vector3 s = child.localScale;
				s.x = Mathf.Abs(s.x) * (left ? 1.0f : -1.0f);
				child.localScale = s;
			}

			Vector3 scale = dust.transform.localScale;
			scale.x = Mathf.Abs(scale.x) * (left ? 1.0f : -1.0f);
			dust.transform.localScale = scale;
		}
		public void SetDustDragonPos()
		{
			SetDustDragonPos(Data.IsLeft);
		}

		public void StopDust()
		{
			if (dust != null)
			{
				dust.Stop();
			}
		}
		public void OffDust()
        {
			if (dust != null)
            {
                dust.gameObject.SetActive(false);
            }
        }

		public abstract void UpdateStatus(float dt);
		public abstract void ClearEffectSpine();
		public void ClearBuffStat()
        {
			if (Data == null)
				return;

			Data.ClearBuffStat();
        }
		public virtual void SkillActionCancle()
		{
			if (false == Data.IsWeak())
				return;

			SkillCancle();
		}
		public virtual void SkillCancle()
		{
			StopCastingEffect();
			ClearEffectSpine();
			ClearAnimation();
			Data.SetActiveSkilType(eBattleSkillType.None);
			Data.SetActionCoroutine(null);
			EffectReceiverClearEvent.Send();
		}

		//KnockBack
		public void KnockbackEffect()
		{
			if (Data == null || spineObj == null || Data.Untouchable)
				return;

			if (Data.Death)
			{
				knockBackTime = 0f;
				spineObj.transform.localPosition = spinePivotPos;
				return;
			}

			if (Data.IsEffectInfo(eSkillEffectType.AIRBORNE,
				eSkillEffectType.KNOCK_BACK,
				eSkillEffectType.STUN,
				eSkillEffectType.FROZEN))
			{
				var pos = spineObj.transform.localPosition;
				pos.x = 0f;
				pos.z = 0f;
				spineObj.transform.localPosition = pos;
				knockBackTime = 0f;
				return;
			}

			knockBackTime -= SBGameManager.Instance.DTime;
			if (!isKnockBack && knockBackTime > 0)
			{
				isKnockBack = true;
			}
			else if (isKnockBack && knockBackTime <= 0)
			{
				isKnockBack = false;
				spineObj.transform.localPosition = spinePivotPos;
			}
			else if (knockBackTime > 0)
			{
				var bezierCurveX = SBFunc.BezierCurve2Speed(0, !Data.IsLeft ? knockBackPower : -knockBackPower, 0, knockBackMaxTime - knockBackTime, knockBackMaxTime, new Vector4(0f, 0.625f, 1f, 0.75f));
				var pos = spineObj.transform.localPosition;
				pos.x = bezierCurveX;
				spineObj.transform.localPosition = pos;
			}
		}
		public virtual void Death()
		{
			StopCoroutine("DeathCO");
			StartCoroutine("DeathCO");
		}
		public virtual Vector3 GetAddDeathPos(Vector3 lastPos, float pos)
		{
			Vector3 ret = new Vector3(lastPos.x, lastPos.y + SBDefine.DeathY1, lastPos.z);
			if (transform.localScale.x < 0)
			{
				ret.x += Data.ConvertPos(pos);
			}
			else
			{
				ret.x -= Data.ConvertPos(pos);
			}

			return ret;
		}
		protected virtual IEnumerator DeathCO()
		{
			SetShadow(false);

			var curDeathTime = 0f;
			var maxDeathTime = 0.7f;
			var lastPos = spineObj.transform.localPosition;
			Vector3 vec1 = GetAddDeathPos(lastPos, SBDefine.DeathX1);
			Vector3 vec2 = GetAddDeathPos(lastPos, SBDefine.DeathX2);
			
			SetAnimation(eSpineAnimation.DEATH);
			skeletonAni.timeScale = 3f;
			while (curDeathTime < maxDeathTime)
			{
				curDeathTime += SBGameManager.Instance.DTime;
				spineObj.transform.localPosition = SBFunc.BezierCurve2Vec3(lastPos, vec1, vec2, curDeathTime, maxDeathTime);
				yield return null;
			}
			spineObj.transform.localPosition = SBFunc.BezierCurve2Vec3(lastPos, vec1, vec2, maxDeathTime, maxDeathTime);
			isDeath = true;

			ClearEffectSpine();
			yield break;
		}
		protected GameObject castingEffect = null;
		protected void StopCastingEffect()
        {
			StopCoroutine(nameof(StartCastingEffect));
			if (castingEffect != null)
			{
				Destroy(castingEffect);
				castingEffect = null;
			}
		}
		protected IEnumerator StartCastingEffect()
        {
			StopCastingEffect();
			if (Data.Skill1.CASTING_EFFECT_RSC_DELAY > 0f)
				yield return SBDefine.GetWaitForSeconds(Data.Skill1.CASTING_EFFECT_RSC_DELAY);

			var castingResourceData = SkillResourceData.Get(Data.Skill1.CASTING_EFFECT_RSC_KEY);
			if (castingResourceData == null)
				yield break;

			var castingEffectPrefab = ResourceManager.GetResource<GameObject>(eResourcePath.EffectPrefabPath, castingResourceData.FILE);
			if (castingEffectPrefab == null)
				yield break;

			Transform parentTransform;
			Transform targetTransform;
			Vector3 targetPos = Vector3.zero;
			switch (castingResourceData.ORDER_TYPE)
			{
				case eSkillResourceOrderType.CHAR:
					parentTransform = transform;
					break;
				case eSkillResourceOrderType.WORLD:
				case eSkillResourceOrderType.NONE:
				default:
					if (Map == null)
						parentTransform = transform.parent;
					else
						parentTransform = Map.EffectBeacon;
					break;
			}
			switch (castingResourceData.LOCATION)
			{
				case eSkillResourceLocation.COLLIDER:
					targetTransform = transform;
					targetPos = Controller.myCollider.bounds.center;
					break;
				case eSkillResourceLocation.TOP:
					targetTransform = StatusEffect.transform;
					break;
				case eSkillResourceLocation.BOTTOM:
				default:
					targetTransform = transform;
					break;
			}

			var castingEffectObject = Instantiate(castingEffectPrefab, parentTransform);
			if(castingResourceData.FOLLOW == eSkillResourceFollow.FOLLOW)
			{
				var follow = castingEffectObject.GetComponent<FollowPosition>();
				if (follow == null)
					follow = castingEffectObject.AddComponent<FollowPosition>();
				follow.Set(targetTransform, parentTransform, targetPos, Data.IsLeft);
			}
			else
            {
				castingEffectObject.transform.position = targetTransform.position;
			}

			var sort = castingEffectObject.GetComponent<SortingGroup>();
			if (sort == null)
				sort = castingEffectObject.AddComponent<SortingGroup>();
			if (sort != null)
			{
				switch (castingResourceData.ORDER)
				{
					case eSkillResourceOrder.BACK:
						sort.sortingOrder = -1;
						break;
					case eSkillResourceOrder.FRONT:
						sort.sortingOrder = 99;
						break;
					case eSkillResourceOrder.AUTO:
					default:
						sort.sortingOrder = 0;
						break;
				}
			}

            yield break;
        }
        private void OnDestroy()
        {
			ClearBuffStat();
        }
        //

        //deprecated
        //protected override void LateUpdate()
        //      {
        //	base.LateUpdate();
        //          if (Controller.myCollider != null)
        //          {
        //		if (Controller.myCollider.attachedRigidbody.simulated)
        //		{
        //			curTime += SBGameManager.Instance.DTime;
        //			if (positionRefreshTime > curTime)
        //			{
        //				return;
        //			}
        //			curTime = 0.0f;

        //			switch (Animation)
        //			{
        //				case eSpineAnimation.ATTACK:
        //				case eSpineAnimation.SKILL:
        //					return;
        //			}

        //			if (collisionVector != Vector2.zero)
        //			{
        //				Vector2 normal = collisionVector;

        //				Vector2[] dir = {
        //						new Vector2(0.0f, 1.0f).normalized,
        //						new Vector2(1.0f, 1.0f).normalized,
        //						new Vector2(1.0f, 0.0f).normalized,
        //						new Vector2(1.0f, -1.0f).normalized,
        //						new Vector2(0.0f, -1.0f).normalized,
        //						new Vector2(-1.0f, -1.0f).normalized,
        //						new Vector2(-1.0f, 0.0f).normalized,
        //						new Vector2(-1.0f, 1.0f).normalized,
        //					};

        //				float[] dis =
        //				{
        //						Vector2.Distance(normal,dir[0]),
        //						Vector2.Distance(normal,dir[1]),
        //						Vector2.Distance(normal,dir[2]),
        //						Vector2.Distance(normal,dir[3]),
        //						Vector2.Distance(normal,dir[4]),
        //						Vector2.Distance(normal,dir[5]),
        //						Vector2.Distance(normal,dir[6]),
        //						Vector2.Distance(normal,dir[7]),
        //					};

        //				float min = Mathf.Min(dis);

        //				for (int i = 0; i < dis.Length; i++)
        //				{
        //					if (min == dis[i])
        //					{
        //						normal = dir[i];
        //						break;
        //					}
        //				}

        //				normal *= 0.1f;

        //				Vector3 pos = Controller.transform.localPosition;
        //				pos.x += normal.x;
        //				pos.y += normal.y;

        //				Controller.MoveLocalTargetUpdate(positionRefreshTime, pos, false);
        //				collisionVector = Vector2.zero;
        //				collisions.Clear();

        //				switch (Animation)
        //				{
        //					case eSpineAnimation.IDLE:
        //						string animName = GetTypeToName(eSpineAnimation.WALK);
        //						if (skeletonAni.AnimationName != animName)
        //						{
        //							skeletonAni.AnimationState.SetAnimation(0, animName, true);
        //						}
        //						CancelInvoke("ReturnToIdleAnimation");
        //						Invoke("ReturnToIdleAnimation", positionRefreshTime);
        //						break;
        //				}
        //			}
        //			else
        //                  {
        //				collisions.Clear();
        //			}
        //		}
        //          }
        //      }

        //void ReturnToIdleAnimation()
        //      {
        //	CancelInvoke("ReturnToIdleAnimation");

        //	switch (Animation)
        //	{
        //		case eSpineAnimation.IDLE:
        //			string animName = GetTypeToName(eSpineAnimation.IDLE);
        //			if (skeletonAni.AnimationName != animName)
        //				skeletonAni.AnimationState.SetAnimation(0, animName, true);
        //			break;
        //	}
        //}
    }
}
