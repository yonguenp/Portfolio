
public class DummyCharacterObject : CharacterObject
{
    public override void SetUserName(HudName userName)
    {
        userName.SetName(CharacterGameData.GetCharacterData(CharacterType).GetName());

        base.SetUserName(userName);
    }
}