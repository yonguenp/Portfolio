using DG.Tweening;
using SandboxNetwork;
using UnityEngine;
using UnityEngine.UI;

public class BossHpUIObject : UIObject
{
    [SerializeField]
    protected Slider advBossHpbar = null;
    [SerializeField]
    protected Slider shieldBar = null;
    [SerializeField]
    protected RectTransform advLostBossHp = null;
    [SerializeField]
    protected Image profileImage = null;
    [SerializeField]
    protected Image profileBackground = null;
    [SerializeField]
    protected Text nameText = null;
    [SerializeField]
    protected Text hpText = null;
    [SerializeField]
    protected Color bgColor = Color.white;

    protected float lastHpBoss = 1;
    protected IBattleCharacterData boss = null;

    protected Sequence bossSequence;
    public MonsterBaseData TargetBaseData { get; private set; } = null;

    protected float lastShieldBoss = 0.0f;
    public bool IsBoss()
    {
        return gameObject.activeInHierarchy;
    }
    public virtual void Clear()
    {
        if(bossSequence != null)
            bossSequence.Kill();
        bossSequence = null;

        boss = null;
        lastHpBoss = 0.0f;
        lastShieldBoss = 0.0f;
        advLostBossHp.sizeDelta = Vector2.zero;
        gameObject.SetActive(false);

        if (profileImage != null)
        {
            profileImage.sprite = null;
            profileImage.color = Color.white;
            profileImage.transform.localPosition = Vector3.zero;
        }

        if (profileBackground != null)
            profileBackground.color = bgColor;

        TargetBaseData = null;
    }

    public virtual void SetTargetBoss(IBattleCharacterData bossData)
    {
        Clear();

        boss = bossData;
        if (boss == null)
            return;

        gameObject.SetActive(true);
        bossSequence = DOTween.Sequence();
        
        Sprite sprite = null;
        MonsterSpawnData spawnData = MonsterSpawnData.GetKey(bossData.ID);
        if (spawnData != null)
        {
            TargetBaseData = MonsterBaseData.Get(spawnData.MONSTER.ToString());            
            if(TargetBaseData != null)
            {
                sprite = ResourceManager.GetResource<Sprite>(eResourcePath.CharIconPath, TargetBaseData.THUMBNAIL);
                if(nameText != null)
                {
                    nameText.text = StringData.GetStringByStrKey(TargetBaseData._NAME);
                    nameText.gameObject.SetActive(nameText.text != "");
                }
            }
        }

        if (profileImage != null)
        {
            profileImage.transform.parent.gameObject.SetActive(sprite != null);
            profileImage.sprite = sprite;
            profileImage.transform.localPosition = Vector3.zero;
        }

        if(profileBackground != null)
            profileBackground.gameObject.SetActive(sprite != null);
        

        if (hpText != null)
            hpText.text = "";

        advBossHpbar.value = 1.0f;
        shieldBar.value = 0f;
    }

    public virtual void RefreshBossHpBar()
    {
        if (advBossHpbar == null || boss == null || !advBossHpbar.IsActive())
            return;

        if (boss.HP > 0)
            advBossHpbar.value = Mathf.Max(0.0f, (float)boss.HP / boss.MaxHP);
        else
            advBossHpbar.value = 0.0f;
        var shield = boss.Stat.GetStatusInt(eStatusCategory.ADD_BUFF, eStatusType.SHIELD_POINT);
        if (shield > 0f)
        {
            var shieldValue = (float)shield / boss.MaxShield;
            if (shieldValue > 1f)
                shieldValue = 1f;
            shieldBar.value = shieldValue;
        }
        else
        {
            shieldBar.value = 0f;
        }

        DeffBossHpBarChange(0);
    }

    public virtual void DeffBossHpBarChange(int shieldPoint)
    {
        if (boss == null)
            return;

        bossSequence.Kill();
        advLostBossHp.sizeDelta += new Vector2(540 * Mathf.Min(lastHpBoss - advBossHpbar.value, 1.0f), 0);
        bossSequence.Append(advLostBossHp.DOSizeDelta(Vector2.zero, 1.0f));
        
        var shieldPt = boss.Stat.GetStatusInt(eStatusCategory.ADD_BUFF, eStatusType.SHIELD_POINT);
        if (hpText != null)
        {
            string curText = SBFunc.CommaFromNumber(boss.HP) + "/" + SBFunc.CommaFromNumber(boss.MaxHP);
            
            if (shieldPt > 0)
            {
                curText = curText + "(" + shieldPt.ToString()+")";
            }
            hpText.text = curText;
        }
        shieldBar.gameObject.SetActive(shieldPt > 0);
        if (shieldPt > 0)
        {
            (shieldBar.image.transform as RectTransform).sizeDelta += new Vector2(1080 * Mathf.Min(lastShieldBoss - shieldBar.value, 1.0f), 0);
            bossSequence.Append((shieldBar.image.transform as RectTransform).DOSizeDelta(Vector2.zero, 1.0f));
        }

        if (lastHpBoss == advBossHpbar.value && lastShieldBoss == shieldBar.value)
            return;

        lastShieldBoss = shieldBar.value;
        lastHpBoss = advBossHpbar.value;

        if (lastHpBoss == advBossHpbar.maxValue && lastShieldBoss == shieldBar.minValue)
            return;

        if (advBossHpbar.value <= 0.0f)
        {
            advBossHpbar.value = 0f;

            if (profileBackground != null)
            {
                profileBackground.color = bgColor;
            }

            if (profileImage != null)
            {
                profileImage.transform.localPosition = Vector3.zero;
            }
        }
        else
        {

            if (profileBackground != null)
            {
                profileBackground.DOKill();
                profileBackground.DOColor(new Color(0.8392157f, 0.3882353f, 0.3882353f), 0.1f).OnComplete(() => {
                    profileBackground.DOColor(bgColor, 0.5f);
                }); 
            }
            
            if(profileImage != null)
            {
                profileImage.transform.DOKill();
                profileImage.transform.localPosition = Vector3.zero;
                profileImage.transform.DOShakePosition(1.0f);
            }
        }
    }

    
}
