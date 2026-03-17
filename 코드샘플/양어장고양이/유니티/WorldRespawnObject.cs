using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WorldRespawnObject : MonoBehaviour
{
    public struct Reward {
        public string type;
        public uint amount;
    };
    public WorldRespawnObjects curController;
    public Button curButton;
    public GameObject[] RespawnObjects;
    public Animation GetAni;

    public Image rewardIconTarget;
    public Text rewardAmountTarget;
    public Sprite[] rewardIcon;

    private uint curIndex = 0;
    private Reward rewardInfo = new Reward();
    
    private void OnEnable()
    {
        curButton.onClick.AddListener(OnSelectObject);
    }

    public void InitRespawnObject(uint index, WorldRespawnObjects controller)
    {
        curIndex = index;
        curController = controller;

        foreach (GameObject tmp in RespawnObjects)
        {
            tmp.SetActive(false);
        }
        int type = Random.Range(0, RespawnObjects.Length);
        RespawnObjects[type].SetActive(true);
        GetAni.gameObject.SetActive(false);
    }

    public void OnSelectObject()
    {
        curController.OnSelectObject(this);
    }

    public void OnClearObject(Reward reward)
    {
        switch(reward.type)
        {
            case "gold":
                rewardIconTarget.sprite = rewardIcon[0];                
                break;
            case "exp":
                rewardIconTarget.sprite = rewardIcon[1];
                break;
            case "item":
                rewardIconTarget.sprite = rewardIcon[2];
                break;
            default:
                Destroy(gameObject);
                return;
        }

        rewardAmountTarget.text = "+ " + reward.amount.ToString("n0");

        GetAni.gameObject.SetActive(true);
        AnimationClip clip = GetAni.GetClip("resource_get_ani");
        GetAni.Play("resource_get_ani");

        Destroy(gameObject, clip.length);
    }
}
