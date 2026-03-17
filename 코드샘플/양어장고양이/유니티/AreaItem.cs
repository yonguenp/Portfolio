using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AreaItem : MonoBehaviour
{
    public uint world_type;
    public GameObject smallCursor;
    public GameObject cursor;
    public Transform respawnPos; 

    private AreaUI areaUI;
    
    public void SetAreaUI(AreaUI ui)
    {
        areaUI = ui;

        GetComponent<Button>().onClick.AddListener(OnSelected);

        CancelInvoke("CreateCat");
        Invoke("CreateCat", Random.Range(0.0f, 5.0f));
    }

    void CheckAreaOpen()
    {
        users userData = GameDataManager.Instance.GetUserData();
        if (userData != null)
        {
            List<uint> unlocked_area = ((List<uint>)userData.data["unlocked_area"]);
            foreach (uint area in unlocked_area)
            {
                if (area == world_type)
                {
                    return;
                }
            }
        }
        smallCursor.GetComponent<Image>().sprite = GetComponent<Button>().spriteState.disabledSprite;
    }

    private void OnDisable()
    {
        CancelInvoke("CreateCat");
    }

    public void CreateCat()
    {
        areaUI.CreateWorldCat(respawnPos);
        Invoke("CreateCat", Random.Range(0.0f, 10.0f));
    }

    public void OnSelected()
    {
        users userData = GameDataManager.Instance.GetUserData();
        List<uint> unlocked_area = ((List<uint>)userData.data["unlocked_area"]);
        foreach (uint area in unlocked_area)
        {
            if (area == world_type)
            {
                areaUI.OnAreaItemSelected(this);
                return;
            }
        }

        string msg = "조건 텍스트를 찾을 수 없습니다.";
        List<game_data> locationDatas = GameDataManager.Instance.GetGameData(GameDataManager.DATA_TYPE.LOCATION);
        if (locationDatas != null)
        {
            object obj;
            foreach (game_data locationData in locationDatas)
            {
                location locData = (location)locationData;                
                if(locData.data.TryGetValue("location_id", out obj))
                {
                    if(world_type == (uint)obj)
                    {
                        if (locData.data.TryGetValue("open_condition", out obj))
                        {
                            JObject jobject = JObject.Parse((string)obj);                            
                            msg = (string)jobject["dsc"];
                        }
                    }
                }
            }
        }

        areaUI.worldCanvas.GameManager.PopupControl.OnPopupToast(msg);
    }

    public void SetCursor(bool enable)
    {
        CheckAreaOpen();

        cursor.SetActive(enable);
        smallCursor.SetActive(!enable);
    }
}
