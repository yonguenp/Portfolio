
namespace SandboxNetwork
{
	public struct UserStatusEvent
	{
		public enum eUserStatusEventEnum
		{
			EXP,    //LEVEL 포함
			LEVEL,	//레벨 패스 용도로 씀 (레벨이 변경되는 시점에 사용)
			ENERGY,
			ENERGY_TICK,
			GOLD,
			GEMSTONE,
			MILEAGE,
			ARENA_POINT,
			MAGNET,
			MAGNITE,
			FRIEND_POINT,
			PORTRAIT,
			GUILD_POINT,
			ORACLE,
		}

		public eUserStatusEventEnum Event;
		static UserStatusEvent e;

		// 기타 추가 정보가 있다면
		public int exp;
		public int level;
		public int energy;
		public int energytick;
		public int amount;
		public int gemstone;

		public UserStatusEvent(eUserStatusEventEnum _Event, int _exp, int _energy, int _gold, int _gemstone, int _energytick, int _level)
        {
			Event = _Event;
			exp = _exp;
			energy = _energy;
			amount = _gold;
			gemstone = _gemstone;
			energytick = _energytick;
			level = _level;
		}

		public static void RefreshExp(int _exp, int _level)
        {
			e.exp = _exp;
			e.level = _level;
			e.Event = eUserStatusEventEnum.EXP;
			EventManager.TriggerEvent(e);
        }
		public static void RefreshLevel()
		{
			e.Event = eUserStatusEventEnum.LEVEL;
			EventManager.TriggerEvent(e);
		}
		public static void RefreshEnergy(int _energy,int _energytick)
		{
			e.energy = _energy;
			e.energytick = _energytick;
			e.Event = eUserStatusEventEnum.ENERGY;
			EventManager.TriggerEvent(e);
		}

		public static void RefreshGemStone(int _gemstone)
		{
			e.gemstone = _gemstone;
			e.Event = eUserStatusEventEnum.GEMSTONE;
			EventManager.TriggerEvent(e);
		}

		public static void RefreshGold(int _gold)
		{
			e.amount = _gold;
			e.Event = eUserStatusEventEnum.GOLD;
			EventManager.TriggerEvent(e);
		}

		public static void RefreshMileage()
		{
			e.Event = eUserStatusEventEnum.MILEAGE;
			EventManager.TriggerEvent(e);
		}

		public static void RefreshArenaPoint()
		{
			e.Event = eUserStatusEventEnum.ARENA_POINT;
			EventManager.TriggerEvent(e);
		}
		public static void RefreshMagnet()
		{
			e.Event = eUserStatusEventEnum.MAGNET;
			EventManager.TriggerEvent(e);
		}
		public static void RefreshMagnite()
		{
			e.Event = eUserStatusEventEnum.MAGNITE;
			EventManager.TriggerEvent(e);
		}
		public static void RefreshOracle()
		{
			e.Event = eUserStatusEventEnum.ORACLE;
			EventManager.TriggerEvent(e);
		}
		public static void RefreshFriendPoint()
		{
			e.Event = eUserStatusEventEnum.FRIEND_POINT;
			EventManager.TriggerEvent(e);
		}
		public static void RefreshPortrait()
		{
			e.Event = eUserStatusEventEnum.PORTRAIT;
			EventManager.TriggerEvent(e);
		}
        public static void RefreshGuildPoint()
        {
            e.Event = eUserStatusEventEnum.GUILD_POINT;
            EventManager.TriggerEvent(e);
        }
    }
}
