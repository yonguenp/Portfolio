using UnityEngine;
using System.Collections;
using DG.Tweening;

public class Battery : PropController
{
    [SerializeField] SpriteRenderer batteryImage = null;
    [SerializeField] GameObject batteryCntObj = null;
    [SerializeField] TextMesh batteryCnt = null;

    public void Init(int cnt)
    {
        base.Init();

        if (cnt > 1)
            batteryCnt.text = cnt.ToString();
        else
            batteryCntObj.SetActive(false);

        if(GameConfig.Instance.DROPPED_BATTERY_LIFETIME - 3.0f > 0.0f)
            Invoke("DestroyAction", GameConfig.Instance.DROPPED_BATTERY_LIFETIME - 3.0f);
    }
    //--------------------
    //jump trigger
    //차후 이벤트트리거로 컨트롤 예정
    //--------------------
    public void PlayDrop(float playTime, Vector2 dir, float distance)
    {
        StartCoroutine(DropActionCO(playTime, dir, distance));
    }

    IEnumerator DropActionCO(float playTime, Vector2 dir, float distance)
    {
        var norVec = dir.normalized;
        var limitTime = playTime;
        var curPlayTime = 0.0f;
        var tickvec = norVec * distance / playTime;
        var movePos = transform.position;

        while (true)
        {
            var moveVec = new Vector3(tickvec.x * Time.deltaTime, tickvec.y * Time.deltaTime);
            var nextPos = movePos + moveVec;

            var timeRate = curPlayTime / limitTime;
            float r = Mathf.PI / 180.0f;
            float radian = 180.0f * timeRate * r;
            float value = Mathf.Sin(radian);
            if (float.IsNaN(value)) value = 0;
            movePos = nextPos;
            transform.position = movePos + new Vector3(0, value);
            curPlayTime += Time.deltaTime;

            if (timeRate >= 1) yield break;
            yield return null;
        }
    }

    void DestroyAction()
    {
        CancelInvoke("DestroyAction");

        StartCoroutine(DestroyActionCoroutine(3.0f));
    }

    IEnumerator DestroyActionCoroutine(float playTime)
    {
        float perAnimTime = (playTime - 0.375f) / 3.0f;
        if (perAnimTime > 0.0f)
        {
            batteryImage.gameObject.SetActive(false);
            yield return new WaitForSeconds(0.1f);
            batteryImage.gameObject.SetActive(true);

            yield return new WaitForSeconds(perAnimTime);

            batteryImage.gameObject.SetActive(false);
            yield return new WaitForSeconds(0.05f);
            batteryImage.gameObject.SetActive(true);
            yield return new WaitForSeconds(0.05f);
            batteryImage.gameObject.SetActive(false);
            yield return new WaitForSeconds(0.05f);
            batteryImage.gameObject.SetActive(true);

            yield return new WaitForSeconds(perAnimTime);

            batteryImage.gameObject.SetActive(false);
            yield return new WaitForSeconds(0.025f);
            batteryImage.gameObject.SetActive(true);
            yield return new WaitForSeconds(0.025f);
            batteryImage.gameObject.SetActive(false);
            yield return new WaitForSeconds(0.025f);
            batteryImage.gameObject.SetActive(true);
            yield return new WaitForSeconds(0.025f);
            batteryImage.gameObject.SetActive(false);
            yield return new WaitForSeconds(0.025f);
            batteryImage.gameObject.SetActive(true);

            Color fadeColor = batteryImage.color;
            fadeColor.a = 0.0f;
            batteryImage.DOColor(fadeColor, perAnimTime);
        }
    }
}
