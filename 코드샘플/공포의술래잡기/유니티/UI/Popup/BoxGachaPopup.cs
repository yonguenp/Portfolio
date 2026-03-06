using Coffee.UIExtensions;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BoxGachaPopup : Popup
{
    [SerializeField] Image head;
    [SerializeField] Image body;
    [SerializeField] Animator animator;
    [SerializeField] Transform main;
    [SerializeField] List<UIParticle> uIParticles = new List<UIParticle>();

    private AnimationEvent particleEvent;
    private int touchCnt = 0;
    private string effectName = string.Empty;
    public override void RefreshUI()
    {
        base.RefreshUI();
        touchCnt = 0;

        if (string.IsNullOrEmpty(effectName))
            effectName = uIParticles[0].name;

        particleEvent = new AnimationEvent();
        particleEvent.functionName = "PlayEffect";
        particleEvent.stringParameter = effectName;
        particleEvent.time = 0.5f;

        animator.runtimeAnimatorController.animationClips[0].events = null;

        animator.runtimeAnimatorController.animationClips[0].AddEvent(particleEvent);
        animator.Play("box_gacha_popup_open", -1, 0);
    }
    public void SkipBtn()
    {
        main.localScale = Vector3.one;
        main.localRotation = Quaternion.identity;

        Close();
    }

    public void PlayEffect(string p_name)
    {
        uIParticles.Find(_ => _.name == p_name).Clear();
        uIParticles.Find(_ => _.name == p_name).Play();
    }

    public void SetUI(int item_no)
    {
        switch (item_no)
        {
            case 33:
                head.sprite = Managers.Resource.Load<Sprite>("Texture/UI/Christmas/result_box_christmas_01");
                body.sprite = Managers.Resource.Load<Sprite>("Texture/UI/Christmas/result_box_christmas_02");
                effectName = uIParticles[0].name;
                break;
            case 34:
                head.sprite = Managers.Resource.Load<Sprite>("Texture/UI/Christmas/result_box_christmas_03");
                body.sprite = Managers.Resource.Load<Sprite>("Texture/UI/Christmas/result_box_christmas_04");
                effectName = uIParticles[1].name;
                break;
            case 35:
                head.sprite = Managers.Resource.Load<Sprite>("Texture/UI/Christmas/result_box_christmas_05");
                body.sprite = Managers.Resource.Load<Sprite>("Texture/UI/Christmas/result_box_christmas_06");
                effectName = uIParticles[2].name;
                break;
        }
    }
}
