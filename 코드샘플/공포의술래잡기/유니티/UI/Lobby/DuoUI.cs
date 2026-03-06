using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DuoUI : MonoBehaviour
{
    [Header("Host")]
    [SerializeField] DuoPlayerUI Host;

    [Header("Guest")]
    [SerializeField] DuoPlayerUI Guest;

    [SerializeField] GameObject OtherUserCharactersObject;
    [SerializeField] SelectedCharacter OtherSurvivorCharacter;
    [SerializeField] SelectedCharacter OtherChaserCharacter;
    public void OnEnable()
    {
        OnRefresh();    
    }

    public void OnRefresh()
    {
        OtherUserCharactersObject.SetActive(false);

        if (Managers.FriendData.DUO.Host == null && Managers.FriendData.DUO.Guest == null)
        {
            gameObject.SetActive(false);
            return;
        }

        gameObject.SetActive(true);

        if (Managers.FriendData.DUO.Host != null)
        {
            Host.SetUI(Managers.FriendData.DUO.Host.NickName, Managers.FriendData.DUO.Host.RankPoint, true);
        }
        else
        {
            Host.Clear();
        }

        if (Managers.FriendData.DUO.Guest != null)
        {
            Guest.SetUI(Managers.FriendData.DUO.Guest.NickName, Managers.FriendData.DUO.Guest.RankPoint, Managers.FriendData.DUO.GuestReady);
        }
        else
        {
            Guest.Clear();
        }

        if (Managers.FriendData.DUO.IsDuoPlaying())
        {
            OtherUserCharactersObject.SetActive(true);

            if (Managers.FriendData.DUO.Host.SurvivorUID <= 0 || Managers.FriendData.DUO.Host.ChaserUID <= 0 ||
                Managers.FriendData.DUO.Guest.SurvivorUID <= 0 || Managers.FriendData.DUO.Guest.ChaserUID <= 0)
            {
                OnDestroyDuo();
            }

            if (Managers.FriendData.DUO.IsHost())
            {
                OtherSurvivorCharacter.SetCharacter(Managers.FriendData.DUO.Guest.SurvivorUID);
                OtherChaserCharacter.SetCharacter(Managers.FriendData.DUO.Guest.ChaserUID);
            }
            else
            {
                OtherSurvivorCharacter.SetCharacter(Managers.FriendData.DUO.Host.SurvivorUID);
                OtherChaserCharacter.SetCharacter(Managers.FriendData.DUO.Host.ChaserUID);
            }
        }
    }

    public void OnDestroyDuo()
    {
        gameObject.SetActive(false);
        Managers.FriendData.DUO.ClearDuo();
    }
}
