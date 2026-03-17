using Coffee.UIExtensions;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HalloweenBingoItem : MonoBehaviour
{
    [SerializeField] UIBundleItem item;
    [SerializeField] GameObject select;
    public GameObject stamp;
    public UIParticle particle_grid;

    public void Init(List<ShopPackageGameData> rewards)
    {
        item.SetReward(rewards[0]);
        select.gameObject.SetActive(false);
        stamp.gameObject.SetActive(false);
    }
    public void Clear(int value = 0)
    {
        if (value == 0)
            return;
        select.gameObject.SetActive(true);
        stamp.gameObject.SetActive(true);
    }

    public void ParticlePlay(UIParticle _particle, float duration = 0)
    {
        _particle.gameObject.SetActive(true);
        _particle.Play();

        if (duration != 0)
        {
            CancelInvoke("ParticleStop");
            Invoke("ParticleStop", duration);
        }
    }

    public void ParticleStop()
    {
        particle_grid.Stop();

        particle_grid.gameObject.SetActive(false);
    }
}
