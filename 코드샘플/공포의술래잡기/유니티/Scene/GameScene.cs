using UnityEngine;

public class GameScene : BaseScene
{
    public override void Clear() { }

    public override void StartBackgroundMusic(bool clearPopup = true)
    {
        base.StartBackgroundMusic(clearPopup);
        var data = MapTypeGameData.GetMapTypeData(Managers.PlayData.GameRoomInfo.MapId);

        if (data != null)
        {
            Managers.Sound.Play(data.map_sound, Sound.Bgm);
        }

    }

    public override void Update()
    {
        
    }
}
