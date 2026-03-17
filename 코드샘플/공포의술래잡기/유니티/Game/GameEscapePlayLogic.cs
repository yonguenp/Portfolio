public partial class Game
{
    public void PlayOpenDoor(bool isPlay, string playerId)
    {
        PlayerController.PlayOpenDoor(isPlay);
    }

    public void SetEscapeOpenDoor(string objectId, int gauge)
    {
        float value = (float)gauge * 0.001f;
        var obj = Managers.Object.FindById(objectId);
        if (obj != null)
        {
            var escapeKey = obj.GetComponent<EscapeKey>();
            if (escapeKey)
            {
                escapeKey.Gauge = value;
            }
        }
    }

    public void SetEscapeOpenDoor(string objectId, long openTime)
    {
        var go = Managers.Object.FindById(objectId);
        if (go == null) return;
        var key = go.GetComponent<EscapeKey>();
        if (key)
        {
            key.OpenDoor();
        }

        UIGame.ShowEscapeIcon();
        UIGame.ShowBatteryGeneratorProgress(false);
        UIGame.SetEscapeUI(0);
        GameRoom.SetOpenDoor(openTime);
    }

    public void EscapeRoom(string objectId)
    {
        var character = Managers.Object.FindCharacterById(objectId);
        if (character == null) return;

        if (objectId == PlayerController.Character.Id)
        {
            Game.Instance.UIGame.HideSkillGuide();
        }

        SBDebug.Log($"EscapeRoom {objectId}");

        character.OnEscape();
        GameRoom.OnEscape(objectId);

        UIGame.SetEscapeUI(GameRoom.CurrentEscapedSurvivorCount, character.PortraitUI);
    }

    public void PlayEscapeKey(bool isPlay, string playerId)
    {
        if (PlayerController.Character.Id.Equals(playerId))
            PlayerController.PlayEscapeKey(isPlay);
    }

    public void SetEscapeKey(string id, int gauge)
    {
        float value = (float)gauge * 0.001f;
        var obj = Managers.Object.FindById(id);
        if (obj != null)
        {
            var elBox = obj.GetComponent<ElectricBox>();
            if (elBox)
            {
                elBox.Gauge = value;
            }
        }
    }

    public void SetEscapeKey(int score, byte state, string id)
    {
        //GameRoom.SetEscapeScore(score);
        //UIGame.SetEscapeScore(score);
        if (state == 2)
        {
            var prop = Managers.Object.FindById(id);
            if (prop)
                prop.gameObject.SetActive(false);

            var on = Managers.Object.CreateEscapeOnObject();
            if (on)
            {
                on.transform.position = prop.transform.position;
            }
        }
    }

    public void OnBatteryCreate()
    {

    }

    public void OnBatteryDrop()
    {

    }

    public void OnBatteryPickUp()
    {

    }
}
