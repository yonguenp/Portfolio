using SBSocketSharedLib;
using UnityEngine;

public partial class CharacterObject
{
    bool isClone = false;
    string sourceID = string.Empty;

    public void SetSource(string sourceID)
    {
        this.sourceID = sourceID;
        isClone = true;
    }

    private CharacterObject GetSourceCharcterObject()
    {
        return Managers.Object.FindCharacterById(sourceID);
    }

    void CheckOwnerIsInvisible()
    {
        if (GetSourceCharcterObject().IsInvisible != IsInvisible)
        {
            if (IsInvisible)
            {
                _buff.BuffStatus.ClearStatusFlag(ObjectBuffStatus.Invisible);
                StopInvisible();
            }
            else
            {
                _buff.BuffStatus.SetStatusFlag(ObjectBuffStatus.Invisible);
                PlayInvisible();
            }
        }
    }
}