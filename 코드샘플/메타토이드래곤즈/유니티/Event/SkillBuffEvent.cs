namespace SandboxNetwork
{
	public struct SkillBuffEvent
	{
		public enum eSkillBuffEventEnum
		{
			regist,
			delete,
			REGIST_PASSIVE,
			DELETE_PASSIVE,
		}
		public eSkillBuffEventEnum Event;
		static SkillBuffEvent e;

		// 기타 추가 정보가 있다면
		public int Tag;
		public EffectInfo Info;
		public bool IsMonster;

		public SkillBuffEvent(eSkillBuffEventEnum _Event, int _tag, EffectInfo _info, bool _isMonster)
		{
			Event = _Event;
			Tag = _tag;
			Info = _info;
			IsMonster = _isMonster;
		}

		public static void RegistBuff(int _tag, EffectInfo _info, bool _isMonster, bool _isPassive = false)
		{
			e.Tag = _tag;
			e.Info = _info;
			e.IsMonster = _isMonster;
			e.Event = _isPassive ? eSkillBuffEventEnum.REGIST_PASSIVE : eSkillBuffEventEnum.regist;
			EventManager.TriggerEvent(e);
		}

		public static void DeleteBuff(int _tag, EffectInfo _info, bool _isMonster, bool _isPassive = false)
		{
			e.Tag = _tag;
			e.Info = _info;
			e.IsMonster = _isMonster;
			e.Event = _isPassive ? eSkillBuffEventEnum.DELETE_PASSIVE : eSkillBuffEventEnum.delete;
			EventManager.TriggerEvent(e);
		}
	}
}
