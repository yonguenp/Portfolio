using UnityEngine;
using UnityEngine.UI;

public class HudNode : MonoBehaviour
{
    [SerializeField]
    Camera renderCamera = null;
    [SerializeField]
    RectTransform canvas = null;
    [SerializeField]
    Transform hudRoot;
    [SerializeField]
    CharacterHud characterUIRoot;

    public HudHp CreateHudHP(CharacterObject character, int hp)
    {
        var p = Resources.Load("Prefabs/hud/hp") as GameObject;
        if (p == null)
            return null;

        var hudHp = GameObject.Instantiate(p) as GameObject;

        var hud = hudHp.GetComponent<HudHp>();
        if (hud != null)
        {
            hud.InitHp(character, renderCamera, canvas, characterUIRoot, hp);
        }

        return hud;
    }

    public HudBattery CreateHudBattery(CharacterObject character, int maxBattery)
    {
        var p = Resources.Load("Prefabs/hud/battery") as GameObject;
        if (p == null)
            return null;

        var hudBattery = GameObject.Instantiate(p) as GameObject;

        var hud = hudBattery.GetComponent<HudBattery>();
        if (hud != null)
        {
            hud.InitBattery(character, renderCamera, canvas, characterUIRoot, maxBattery);
        }

        return hud;
    }
    public HudPosText CreateHudPos(GameObject character, Vector2 pos)
    {
        var p = Resources.Load("Prefabs/hud/pos") as GameObject;
        if (p == null)
        {
            SBDebug.LogError("load fail hudpos");
            return null;
        }


        var hudHp = GameObject.Instantiate(p) as GameObject;

        var hud = hudHp.GetComponent<HudPosText>();
        if (hud != null)
        {
            hud.Init(character, renderCamera, canvas, hudRoot);
            hud.InitPos(pos);
            hud.SetAddPosY(-0.8f);
        }

        return hud;
    }

    public HudPos CreateHudPosEscapeDoor(GameObject target, Vector2 pos)
    {
        var p = Resources.Load("Prefabs/hud/GuideEscape") as GameObject;
        if (p == null)
        {
            SBDebug.LogError("load fail GuideEscape");
            return null;
        }


        var obj = GameObject.Instantiate(p) as GameObject;

        var hud = obj.GetComponent<HudEscapeGuide>();
        if (hud != null)
        {
            hud.Init(target, renderCamera, canvas, hudRoot);
        }

        return hud;
    }

    public HudPos CreateHudPosCharge(GameObject target, Vector2 pos)
    {
        var p = Resources.Load("Prefabs/hud/GuideCharge") as GameObject;
        if (p == null)
        {
            SBDebug.LogError("load fail GuideCharge");
            return null;
        }


        var obj = GameObject.Instantiate(p) as GameObject;

        var hud = obj.GetComponent<HudChargeGuide>();
        if (hud != null)
        {
            hud.Init(target, renderCamera, canvas, hudRoot);
        }

        return hud;
    }

    public HudPos CreateHudPosElectricBox(GameObject target, Vector2 pos, float value, float playTIme)
    {
        ObjectKeyGameData keyData = GameDataManager.Instance.GetData(GameDataManager.DATA_TYPE.object_key, 20001) as ObjectKeyGameData;
        if (keyData == null)
            return null;

        if (value <= keyData.Alarm1)
            return null;

        string resource = keyData.alarm1_resource;
        if (value >= keyData.Alarm2)
            resource = keyData.alarm2_resource;

        if (string.IsNullOrEmpty(resource))
            return null;

        return CreateHudPosElectricBox(target, pos, resource, playTIme);
    }

    public HudPos CreateHudPosElectricBox(GameObject target, Vector2 pos, string resourcePath, float playTIme)
    {
        GameObject p = Resources.Load(resourcePath) as GameObject;

        if (p == null)
        {
            SBDebug.LogError("load fail CreateHudPosElectricBox");
            return null;
        }


        var obj = GameObject.Instantiate(p) as GameObject;

        var hud = obj.AddComponent<HudPos>();
        if (hud != null)
        {
            hud.Init(target, renderCamera, canvas, hudRoot, playTIme);
            hud.SetAddPosY(4.0f);
        }

        return hud;
    }

    public HudPos CreateHudPosEscapeDoor(GameObject target, Vector2 pos, float value, float playTIme)
    {
        ObjectKeyGameData keyData = GameDataManager.Instance.GetData(GameDataManager.DATA_TYPE.object_key, 20002) as ObjectKeyGameData;
        if (keyData == null)
            return null;

        if (value <= keyData.Alarm1)
            return null;

        string resource = keyData.alarm1_resource;
        if (value >= keyData.Alarm2)
            resource = keyData.alarm2_resource;

        if (string.IsNullOrEmpty(resource))
            return null;

        return CreateHudPosEscapeDoor(target, pos, resource, playTIme);
    }

    public HudPos CreateHudPosEscapeDoor(GameObject target, Vector2 pos, string resourcePath, float playTIme)
    {
        GameObject p = Resources.Load(resourcePath) as GameObject;

        if (p == null)
        {
            SBDebug.LogError("load fail CreateHudPosEscapeDoor");
            return null;
        }


        var obj = GameObject.Instantiate(p) as GameObject;

        var hud = obj.AddComponent<HudPos>();
        if (hud != null)
        {
            hud.Init(target, renderCamera, canvas, hudRoot, playTIme);
            hud.SetAddPosY(4.0f);
        }

        return hud;
    }

    public HudPosText CreateHudPosTopBattery(GameObject target, Vector2 pos, float playTIme)
    {
        GameObject p = Resources.Load("Prefabs/hud/topBattery") as GameObject;

        if (p == null)
        {
            SBDebug.LogError("load fail Top Battary");
            return null;
        }

        var obj = GameObject.Instantiate(p) as GameObject;

        var hud = obj.AddComponent<HudPosText>();
        if (hud != null)
        {
            hud.Init(target, renderCamera, canvas, hudRoot, playTIme);
            hud.SetAddPosY(2.5f);
        }

        return hud;
    }


    public HudName CreateHudUserName(CharacterObject character, int slotIndex)
    {
        var p = Resources.Load("Prefabs/hud/name") as GameObject;
        if (p == null)
        {
            SBDebug.LogError("load fail hudpos");
            return null;
        }


        var hudObj = GameObject.Instantiate(p) as GameObject;

        var hud = hudObj.GetComponent<HudName>();
        if (hud != null)
        {
            hud.InitName(character, renderCamera, canvas, characterUIRoot, character.UserName, character.IsChaser, slotIndex);
        }

        return hud;
    }
    public HudEmotion CreateHudEmotion(CharacterObject character)
    {
        var p = Resources.Load("Prefabs/hud/Emotion") as GameObject;
        if (p == null)
            return null;

        var hudEmotion = GameObject.Instantiate(p) as GameObject;

        var hud = hudEmotion.GetComponent<HudEmotion>();
        if (hud != null)
        {
            hud.InitEmotion(character, renderCamera, canvas, characterUIRoot);
        }
        return hud;
    }


    public HudSkillPopup CreateHudSkillPopup(CharacterObject character, bool isChaser)
    {
        GameObject go = null;

        if (isChaser)
        {
            go = Managers.Resource.Instantiate("hud/skillPopup_cs");
        }
        else
        {
            go = Managers.Resource.Instantiate("hud/skillPopup_sv");
        }

        var hSkillPopup = go.AddComponent<HudSkillPopup>();
        if (hSkillPopup != null)
        {
            go.transform.position = character.transform.position;

            hSkillPopup.Init(character.gameObject, renderCamera, canvas, hudRoot);
            hSkillPopup.AddX = 1.1f;
            hSkillPopup.AddY = 2.2f;
        }

        return hSkillPopup;
    }

    public HudVehicleBar CreateVehicleBar(CharacterObject character)
    {
        var p = Resources.Load("Prefabs/hud/vehiclebar") as GameObject;
        if (p == null)
            return null;

        var hudVehicleBar = GameObject.Instantiate(p) as GameObject;

        var hud = hudVehicleBar.GetComponent<HudVehicleBar>();
        if (hud != null)
        {
            hud.Init(character, renderCamera, canvas, characterUIRoot);
        }

        return hud;
    }

    public void OnRemoveCharacterHud(CharacterObject charObj)
    {
        characterUIRoot.OnRemoveCharacterHud(charObj);
    }

    public void OnPoint(CharacterObject character, int amount)
    {
        var p = Resources.Load("Prefabs/hud/point") as GameObject;
        if (p == null)
        {
            SBDebug.LogError("load fail hudpos");
            return;
        }


        var hudObj = GameObject.Instantiate(p) as GameObject;

        var hud = hudObj.GetComponent<HudPoint>();
        if (hud != null)
        {
            hud.InitPoint(character, canvas, characterUIRoot, amount);
        }
    }

    public HudPos CreateHudDetect(CharacterObject target, float playTIme)
    {
        GameObject p = Resources.Load("Prefabs/hud/detect01") as GameObject;

        if (p == null)
        {
            SBDebug.LogError("load fail CreateHudDetect");
            return null;
        }

        var obj = GameObject.Instantiate(p) as GameObject;

        var hud = obj.GetComponent<HudDetect>();
        if (hud != null)
        {
            hud.Init(target, renderCamera, canvas, hudRoot, playTIme);
            hud.SetAddPosY(3.0f);
        }

        return hud;
    }

    public void ClearHudDetect()
    {
        foreach(HudDetect child in hudRoot.GetComponentsInChildren<HudDetect>())
        {
            Destroy(child.gameObject);
        }
    }
}
