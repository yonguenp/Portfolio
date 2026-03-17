using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MapPlant : MonoBehaviour
{
    public GameMain GameManager;
    public GameObject[] PlantObject;

    public Sprite[] rewardIcon;

    public struct Reward
    {
        public string type;
        public uint amount;
    };

    private void OnEnable()
    {
        foreach(GameObject plant in PlantObject)
        {
            if(plant != null)
                TooltipInit(plant.transform.Find("ToolTip"));
        }
    }

    private uint GetPlantIndex(Transform tooltip)
    {
        GameObject parent = tooltip.parent.gameObject;
        for(uint i = 0; i < PlantObject.Length; i++)
        {
            if (PlantObject[i] == parent)
                return i;
        }

        return 0;
    }
    private void TooltipInit(Transform tooltip)
    {
        Button button = tooltip.GetComponent<Button>();
        if(button)
        {
            button.onClick.AddListener(() => {
                OnTooltipButton(tooltip);
            });
        }
        tooltip.gameObject.SetActive(false);

        uint index = GetPlantIndex(tooltip);
        plants plant = plants.GetPlant(index);
        plants.PLANT_STATE state = plant.GetPlantState();
        switch(state)
        {
            case plants.PLANT_STATE.WAIT:
                StartCoroutine(ActiveWait(tooltip));
                break;
            case plants.PLANT_STATE.SOME:
                SetSomeState(tooltip);
                break;
            case plants.PLANT_STATE.MAX:
                SetTooltipMax(tooltip);
                break;
        }
    }

    public void OnTooltipButton(Transform tooltip)
    {
        WWWForm data = new WWWForm();
        data.AddField("api", "plant");
        data.AddField("op", 1);
        uint index = GetPlantIndex(tooltip);
        data.AddField("id", index.ToString());

        NetworkManager.GetInstance().SendApiRequest("plant", 1, data, (response) =>
        {
            JObject root = JObject.Parse(response);
            JToken apiToken = root["api"];
            if (null == apiToken || apiToken.Type != JTokenType.Array
                || !apiToken.HasValues)
            {
                return;
            }

            JArray apiArr = (JArray)apiToken;
            foreach (JObject row in apiArr)
            {
                string uri = row.GetValue("uri").ToString();
                if (uri == "plant")
                {
                    JToken opCode = row["op"];
                    if (opCode != null && opCode.Type == JTokenType.Integer)
                    {
                        int op = opCode.Value<int>();
                        switch (op)
                        {
                            case 1: //OpPlant::HARVEST  
                                {
                                    Reward reward = new Reward();
                                    if (row.ContainsKey("rew"))
                                    {
                                        JObject income = (JObject)row["rew"];
                                        if (income.ContainsKey("gold"))
                                        {
                                            reward.type = "gold";
                                            reward.amount = income["gold"].Value<uint>();
                                        }
                                        if (income.ContainsKey("exp"))
                                        {
                                            reward.type = "exp";
                                            reward.amount = income["exp"].Value<uint>();
                                        }
                                        if (income.ContainsKey("item"))
                                        {
                                            reward.type = "item";
                                            JArray item = (JArray)income["item"];
                                            foreach(JObject rw in item)
                                            {
                                                reward.amount = rw["amount"].Value<uint>();
                                            }
                                        }

                                        StartCoroutine(TooltipAction(tooltip, reward));
                                    }
                                }
                                break;
                        }
                    }
                }
            }

            GameManager.FarmCanvas.FarmUIPanel.Invoke("Refresh", 0.1f);
        });        
    }

    IEnumerator ActiveWait(Transform tooltip)
    {
        tooltip.gameObject.SetActive(false);

        uint index = GetPlantIndex(tooltip);
        plants plant = plants.GetPlant(index);
        if (plant.GetNextIncrement() > System.Convert.ToUInt32(GameManager.GetCurTime()))
        {
            float time = System.Convert.ToSingle(plant.GetNextIncrement() - System.Convert.ToUInt32(GameManager.GetCurTime()));
            yield return new WaitForSecondsRealtime(time);
        }
        

        SetSomeState(tooltip);
    }

    public void SetSomeState(Transform tooltip)
    {
        tooltip.gameObject.SetActive(true);

        Transform BackGround_In = tooltip.Find("BackGround_In");
        if (BackGround_In)
        {
            BackGround_In.gameObject.SetActive(false);

            //Transform Normal = BackGround_In.Find("Normal");
            //Transform Full = BackGround_In.Find("Full");

            //BackGround_In.gameObject.SetActive(true);
            //if (Normal)
            //    Normal.gameObject.SetActive(true);
            //if (Full)
            //    Full.gameObject.SetActive(false);
        }

        Transform BackGround_Line = tooltip.Find("BackGround_Line");
        if (BackGround_Line)
        {
            BackGround_Line.gameObject.SetActive(false);

            //BackGround_Line.gameObject.SetActive(true);
        }

        Transform Resource_Icon = tooltip.Find("Resource_Icon");
        if (Resource_Icon)
        {
            Resource_Icon.gameObject.SetActive(true);
            Resource_Icon.GetComponent<Image>().StopAllCoroutines();
            
            Resource_Icon.GetComponent<Image>().StartCoroutine(TooltipMaxWait(tooltip));
        }

        Transform Get_Ani = tooltip.Find("Get_Ani");
        if (Get_Ani)
        {
            Get_Ani.gameObject.SetActive(false);
        }

        Transform Max_Ani = tooltip.Find("Max_Ani");
        if (Max_Ani)
        {
            Max_Ani.gameObject.SetActive(false);
        }
    }

    IEnumerator TooltipMaxWait(Transform tooltip)
    {
        uint index = GetPlantIndex(tooltip);
        plants plant = plants.GetPlant(index);

        if (plant.GetMaxStack() > plant.GetCurVal())
        {
            float time = (System.Convert.ToSingle(plant.GetMaxStack() - plant.GetCurVal()) / System.Convert.ToSingle(plant.GetTickPeriod())) * 60.0f;
            yield return new WaitForSeconds(time);
        }

        SetTooltipMax(tooltip);
    }

    public void SetTooltipMax(Transform tooltip)
    {
        tooltip.gameObject.SetActive(true);

        Transform BackGround_In = tooltip.Find("BackGround_In");
        if (BackGround_In)
        {
            BackGround_In.gameObject.SetActive(false);

            //Transform Normal = BackGround_In.Find("Normal");
            //Transform Full = BackGround_In.Find("Full");

            //BackGround_In.gameObject.SetActive(true);
            //if (Normal)
            //    Normal.gameObject.SetActive(true);
            //if (Full)
            //    Full.gameObject.SetActive(true);
        }

        Transform BackGround_Line = tooltip.Find("BackGround_Line");
        if (BackGround_Line)
        {
            BackGround_Line.gameObject.SetActive(false);
            
            //BackGround_Line.gameObject.SetActive(true);
        }

        Transform Resource_Icon = tooltip.Find("Resource_Icon");
        if (Resource_Icon)
        {
            Resource_Icon.gameObject.SetActive(false);
            Resource_Icon.GetComponent<Image>().StopAllCoroutines();
        }

        Transform Get_Ani = tooltip.Find("Get_Ani");
        if (Get_Ani)
        {
            Get_Ani.gameObject.SetActive(false);
        }

        Transform Max_Ani = tooltip.Find("Max_Ani");
        if (Max_Ani)
        {
            Max_Ani.gameObject.SetActive(true);
            Max_Ani.GetComponent<Animation>().Play("Resource_max_ani");
        }
    }

    IEnumerator TooltipAction(Transform tooltip, Reward reward)
    {
        Transform BackGround_In = tooltip.Find("BackGround_In");
        if (BackGround_In)
        {
            BackGround_In.gameObject.SetActive(false);

            Transform Normal = BackGround_In.Find("Normal");
            Transform Full = BackGround_In.Find("Full");

            BackGround_In.gameObject.SetActive(false);
            if (Normal)
                Normal.gameObject.SetActive(false);
            if (Full)
                Full.gameObject.SetActive(false);
        }

        Transform BackGround_Line = tooltip.Find("BackGround_Line");
        if (BackGround_Line)
        {
            BackGround_Line.gameObject.SetActive(false);
        }

        Transform Resource_Icon = tooltip.Find("Resource_Icon");
        if (Resource_Icon)
        {
            Resource_Icon.gameObject.SetActive(false);
            Resource_Icon.GetComponent<Image>().StopAllCoroutines();
        }

        bool isEnableAction = false;
        Transform Get_Ani = tooltip.Find("Get_Ani");
        if (Get_Ani)
        {
            Get_Ani.gameObject.SetActive(true);
            Image rewardIconTarget = Get_Ani.Find("Get_Icon").GetComponent<Image>();
            Text rewardAmountTarget = Get_Ani.Find("Get_Count").GetComponent<Text>();

            
            switch (reward.type)
            {
                case "gold":
                    rewardIconTarget.sprite = rewardIcon[0];
                    isEnableAction = true;
                    break;
                case "exp":
                    rewardIconTarget.sprite = rewardIcon[1];
                    isEnableAction = true;
                    break;
                case "item":
                    rewardIconTarget.sprite = rewardIcon[2];
                    isEnableAction = true;
                    break;
                default:
                    break;
            }

            rewardAmountTarget.text = "+ " + reward.amount.ToString("n0");


            Get_Ani.GetComponent<Animation>().Play("resource_get_ani");
        }

        Transform Max_Ani = tooltip.Find("Max_Ani");
        if (Max_Ani)
        {
            Max_Ani.gameObject.SetActive(false);
        }

        if(isEnableAction)
        {
            yield return new WaitForSeconds(2.0f);

            tooltip.gameObject.SetActive(false);
        }
        
        StartCoroutine(ActiveWait(tooltip));
    }
}
