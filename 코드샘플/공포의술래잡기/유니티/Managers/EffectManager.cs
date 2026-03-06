using SBSocketSharedLib;
using System.Collections.Generic;
using UnityEngine;

public class EffectManager
{
    GameObject preloadedObj;
    Dictionary<int, GameObject> pool = new Dictionary<int, GameObject>();
    bool isInitialized;
    const int particleEffectID = 10000;

    public void Initialize()
    {
        if (isInitialized) return;

        preloadedObj = new GameObject { name = "@PreloadedEffect" };
        Object.DontDestroyOnLoad(preloadedObj);

        var effectDatas = Managers.Data.GetData(GameDataManager.DATA_TYPE.effect_resource_data);
        foreach (EffectResourceGameData data in effectDatas)
        {
            var obj = Managers.Resource.Instantiate(data.ResourcePath);
            if (obj == null) continue;
            var id = data.GetID();
            if (id > particleEffectID) continue;   // 파티클 이펙트는 미리 로드하지 않는다.
            obj.transform.SetParent(preloadedObj.transform);
            pool.Add(id, obj);
            obj.gameObject.SetActive(false);
        }
        isInitialized = true;
    }

    public void OnGameOver()
    {
        GameObject.Destroy(preloadedObj);
        pool.Clear();
        isInitialized = false;
    }

    private EffectResourceGameData GetEffectResourceData(int index)
    {
        var gameData = GameDataManager.Instance.GetData(GameDataManager.DATA_TYPE.effect_resource_data, index);
        if (gameData == null)
        {
            SBDebug.LogError("GetEffectSpineData gameData is NULL");
            return null;
        }

        EffectResourceGameData effectData = gameData as EffectResourceGameData;
        if (effectData == null)
        {
            SBDebug.LogError("GetEffectSpineData effectData is NULL");
            return null;
        }

        return effectData;
    }

    public GameObject PlayEffect(int index, Transform target, float playTime = 0, bool flip = false, float dirX = 0f, float dirY = 0f)
    {
        SBDebug.Log($"PlayEffect [{index}]");
        var effectData = GetEffectResourceData(index);
        if (effectData == null)
        {
            SBDebug.LogError($"[{index}]GetEffectResourceData is NULL");
            return null;
        }

        if (target == null)
        {
            SBDebug.LogError("PlayEffect parent is NULL");
            return null;
        }

        pool.TryGetValue(index, out GameObject go);
        if (go == null)
        {
            go = Managers.Resource.Instantiate(effectData.ResourcePath);
            if (go == null)
            {
                SBDebug.LogError($"PlayEffect {effectData.ResourcePath} is NULL");
                return null;
            }
            else if (index <= particleEffectID)
            {
                pool.Add(index, go);
            }
        }

        // 스파인 오브젝트 이펙트라면 풀에 있는거든, 새로 만든거든 그것의 복제를 한다
        if (index <= particleEffectID)
        {
            go = GameObject.Instantiate(go);
            go.SetActive(true);
        }
        var effectSP = go.GetComponent<EffectSP>();

        GameObject result = null;
        if (effectSP != null)
        {
            result = PlayEffectSpine(effectSP, effectData, target, playTime, flip);
        }
        else
        {
            float lifeTime = playTime;
            if(effectData.PlayTime > 0)
                lifeTime = effectData.PlayTime;
            
            result = PlayEffectParticle(go, effectData, target, lifeTime, dirX, dirY);

            if(lifeTime != 0)
                GameObject.Destroy(result, lifeTime);
        }

        // 만약 이 이펙트가 시야에 가려 보이지 않는다 해도 소리는 재생되어야 한다.
        // 현재 이펙트 소리 재생이 Start에서 되고 있으므로, 강제로 이펙트 소리 재생 후 이펙트를 파괴한다.
        if (!target.gameObject.activeInHierarchy && effectData.AlwaysShow == false)
        {
            var sc = go.GetComponent<SoundController>();
            if (sc != null)
                sc.PlaySound();
            // 파티클인 경우에는 소리를 재생 후 destroy 해주어야 한다. 자동으로 destroy 되지 않기 때문
            if (effectSP == null)
                DestoryEffect(go, effectData.PlayTime);
            return null;
        }

        return result;
    }

    private GameObject PlayEffectParticle(GameObject go, EffectResourceGameData effectData, Transform target, float playTime, float dirX, float dirY)
    {
        if (effectData.IsParentTarget)
        {
            go.transform.SetParent(target);
            go.transform.localPosition = new Vector3(effectData.OffsetX, effectData.OffsetY, 0);
        }
        else
        {
            go.transform.position = target.position + new Vector3(effectData.OffsetX, effectData.OffsetY, 0);
        }
        go.transform.localScale = new Vector3(effectData.ScaleX, effectData.ScaleY, 1);
        float angle = 0;

        if (effectData.IsParentDirection == true)
        {
            angle = Mathf.Atan2(dirY, dirX) * Mathf.Rad2Deg;
        }
        go.transform.eulerAngles = new Vector3(0, 0, effectData.Rotation + angle);

        return go;
    }

    //parent null이면 이펙트의 위치를 상위 스택에서 잡아주세요.
    private GameObject PlayEffectSpine(EffectSP effectSP, EffectResourceGameData effectData, Transform target, float playTime, bool flip)
    {
        var go = effectSP.gameObject;

        MoveDir animDir = MoveDir.Down; // 기본 방향은 아래를 본다

        if (effectData.IsParentDirection == true)
        {
            var parent = target.parent;
            if (parent != null)
            {
                var co = parent.gameObject.GetComponent<CharacterObject>();
                if (co)
                {
                    var charMoveDir = co.PosInfo.MoveDir;
                    animDir = CharacterAnimationController.GetDirForAnimation(charMoveDir.X, charMoveDir.Y);
                }
            }
        }

        effectSP.Init(effectData, target, animDir, effectData.IsParentTarget);
        if (!effectData.IsParentDirection)
            effectSP.SetFlip(flip);

        if (playTime > 0)
        {
            Object.Destroy(go, playTime);
        }
        else
        {
            if (effectData.PlayTime != 0)
                Object.Destroy(go, effectData.PlayTime);
        }

        return go;
    }
    public void DestoryEffect(GameObject target, float playTime = 0)
    {
        Object.Destroy(target, playTime);
    }
}
