using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameLogNotifyMessage : MonoBehaviour
{
    public bool PassForceShowTime { get; set; } = true;
    public delegate void DestroyCallback(GameLogNotifyMessage msg);

    private MaskableGraphic[] graphics;
    private float lifeTime;
    private float fadeoutTime;
    private DestroyCallback destroyCallback = null;

    private void Start()
    {
        graphics = GetComponentsInChildren<MaskableGraphic>();
    }

    public void SetTime(float lifeTime = 0f, float fadeoutTime = 0f)
    {
        if (lifeTime > 0)
            this.lifeTime = lifeTime;
        else
            this.lifeTime = GameConfig.Instance.GAMELOG_MESSAGE_LIFETIME;

        if (fadeoutTime > 0)
            this.fadeoutTime = fadeoutTime;
        else
            this.fadeoutTime = GameConfig.Instance.GAMELOG_MESSAGE_FADEOUT_TIME;
    }

    public IEnumerator FadeOutCoroutine()
    {
        yield return new WaitForSeconds(lifeTime);

        float time = fadeoutTime;
        float rate = 1.0f / fadeoutTime;

        while (time > 0.0f)
        {
            float delta = Time.deltaTime;
            time -= delta;

            foreach (var graphic in graphics)
            {
                graphic.color = graphic.color - new Color(0, 0, 0, delta * rate);
            }

            yield return new WaitForEndOfFrame();
        }

        Destroy(gameObject);
    }

    public void SetDestroyCallback(DestroyCallback cb)
    {
        destroyCallback = cb;
    }

    private void OnDestroy()
    {
        (transform as RectTransform).DOKill();
        destroyCallback?.Invoke(this);
    }
}
