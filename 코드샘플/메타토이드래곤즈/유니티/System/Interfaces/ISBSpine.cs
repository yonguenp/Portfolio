using Spine;

namespace SandboxNetwork
{
	public enum eSpineType
	{
		UI = 0,
		FIELD,
		ADVENTURE,
		PVP,
		DAILY,
	}
	public interface ISBSpine
	{
		public void Init();
		public Spine.TrackEntry SetAnimation(int trackIndex, string animnation, bool loop);
		public void SetSkin(string skinName);
		public void SetShadow(bool show);
		//public bool Update(float dt);
	}
}