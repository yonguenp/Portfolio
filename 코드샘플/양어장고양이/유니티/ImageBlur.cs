using DG.Tweening;
using Coffee.UIEffects;
using SuperBlur;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

[ExecuteInEditMode]
public class ImageBlur : MonoBehaviour
{
	protected static class Uniforms
	{
		public static readonly int _Radius = Shader.PropertyToID("_Radius");
		public static readonly int _BackgroundTexture = Shader.PropertyToID("_SuperBlurTexture");
	}

	public SuperBlur.RenderMode renderMode = SuperBlur.RenderMode.Screen;

	public BlurKernelSize kernelSize = BlurKernelSize.Small;

	[Range(0f, 1f)]
	public float interpolation = 1f;

	[Range(0, 4)]
	public int downsample = 1;

	[Range(1, 8)]
	public int iterations = 1;

	public bool gammaCorrection = true;

	public Material blurMaterial;

	public Material UIMaterial;

	public RawImage target;
	public Texture texture;
	public Animation Iris;
	public UIEffect uiEffect;
	public GameObject frame;
	public Text cardName;
	public GameObject resultEffect;
	public RawImage resultPicture;
	public VideoPlayer videoPlayer;

	private Vector2 size;
	private RenderTexture source = null;
	private RenderTexture dest = null;
	private Coroutine coroutine = null;

	public delegate void Callback();
	Callback doneCallback = null;

	public GameObject MissEffect;
	public GameObject PerfectEffect;
	public GameObject EpicEffect;

	void OnEnable()
    {
		
	}

	void OnDisable()
	{
		if (coroutine != null)
			StopCoroutine(coroutine);

		coroutine = null;

		if(source != null)
			source.Release();
		source = null;
		if (dest != null)
			dest.Release();
		dest = null;

		target.texture = texture;
		doneCallback = null;
	}

	public IEnumerator UpdateResultBlurTexture()
	{
		frame.SetActive(false);
		Iris.gameObject.SetActive(false);
		uiEffect.enabled = false;
		resultEffect.SetActive(false);

		downsample = 4;//i
		interpolation = 1;//f
		iterations = 8;//i
		RefreshBlur();

		Rect targetUV = target.uvRect;
		target.uvRect = new Rect(Vector2.zero, Vector2.one);

		while (target.color.r < 1.0f)
		{
			target.color = target.color + new Color(0.2f, 0.2f, 0.2f);
			yield return new WaitForSeconds(0.1f);
		}

		frame.SetActive(true);

		foreach (Image image in frame.GetComponentsInChildren<Image>())
		{
			image.color = Color.white;
			image.DOKill();
		}

		int iterRatio = -1;
		float interRatio = -0.05f;
		int downRatio = -1;

		float waitForSec = 0.1f;

		Callback downSampleControl = () => {
			downsample += downRatio;

			if (downsample <= 1)
			{
				downRatio = 1;
			}
			if (downsample >= 2)
			{
				downRatio = -1;
			}
		};

		Callback iterControl = () => {
			iterations += iterRatio;

			if (iterations <= 2)
			{
				iterRatio = 1;
				downSampleControl();
			}
			if (iterations >= 5)
			{
				iterRatio = -1;
				downSampleControl();
			}
		};

		while (!Input.anyKey)
		{
			interpolation += interRatio;			

			if(interpolation <= 0.3f)
            {
				foreach (Image image in frame.GetComponentsInChildren<Image>())
				{
					image.color = new Color(255, 152, 0);
					image.DOColor(Color.white, 1.0f);
				}

				interRatio = 0.05f;
				iterControl();
			}
			if (interpolation >= 1.0f)
			{
				interRatio = -0.05f;
				iterControl();
			}

			RefreshBlur();
			yield return new WaitForSeconds(waitForSec);
		}

		downsample = 0;//i
		interpolation = 0.0f;//f
		iterations = 1;//i
		RefreshBlur();
		yield return new WaitForSeconds(0.05f);

		yield return StartCoroutine(OnDartMode(targetUV));

		//while (iterations < 8)
		//{
		//	interpolation -= 0.125f;
		//	iterations += 1;
			
		//	RefreshBlur();
		//	yield return new WaitForSeconds(0.1f);
		//}

		target.uvRect = targetUV;

		Iris.gameObject.SetActive(true);
		Iris.Play();
		frame.SetActive(false);

		yield return new WaitForSeconds(Iris.clip.length);

		Iris.gameObject.SetActive(false);

		Animation anim = resultEffect.GetComponent<Animation>();
		anim.Stop();

		resultEffect.SetActive(true);

		anim.Play();
		yield return new WaitForSeconds(anim.clip.length);
		GetComponent<RectTransform>().sizeDelta = size;

		resultEffect.SetActive(false);

		downsample = 0;//i
		interpolation = 0.0f;//f
		iterations = 1;//i

		RefreshBlur();
		
		yield return new WaitForSeconds(0.1f);

		target.texture = texture;

		uiEffect.enabled = true;
		uiEffect.colorFactor = 0;		
		while (uiEffect.colorFactor < 1.0f)
		{
			uiEffect.colorFactor += 0.1f;
			yield return new WaitForSeconds(0.01f);
		}

		while (uiEffect.colorFactor > 0.0f)
		{
			uiEffect.colorFactor -= 0.1f;
			yield return new WaitForSeconds(0.01f);
		}

		uiEffect.enabled = false;

		coroutine = null;

		cardName.gameObject.SetActive(true);
		string name = cardName.text;
		cardName.text = "";

		cardName.DOText(name, 1.0f).OnComplete(()=> {
			if (doneCallback != null)
				doneCallback.Invoke();
		});
	}

	public IEnumerator OnDartMode(Rect rect)
    {
		Rect from = target.uvRect;
		Rect to = rect;

		Vector2 diffPos = from.position - to.position;
		Vector2 diffSize = from.size - to.size;

		float actionTime = 0.05f;
		float actionRatio = 1 / actionTime;


		while (actionTime > 0.0f)
		{
			actionTime -= Time.deltaTime;

			float delta = actionRatio * Time.deltaTime;
			from.position -= diffPos * delta;
			from.size -= diffSize * delta;
			target.uvRect = from;

			yield return new WaitForEndOfFrame();
		}
	}

	[ContextMenu("RefreshBlur")]
	public void RefreshBlur()
    {
        if (texture == null || target == null)
            return;

		int tw = (int)size.x >> downsample;
		int th = (int)size.y >> downsample;

		if(source != null)
        {
			source.Release();
		}

		source = new RenderTexture(tw, th, 24);
		
		Graphics.Blit(texture, source);
		
		Blur(source, dest);

		target.texture = dest;
	}

	private void Update()
    {
		
	}

    protected void Blur(RenderTexture source, RenderTexture destination)
	{
		if (gammaCorrection)
		{
			Shader.EnableKeyword("GAMMA_CORRECTION");
		}
		else
		{
			Shader.DisableKeyword("GAMMA_CORRECTION");
		}

		int kernel = 0;

		switch (kernelSize)
		{
			case BlurKernelSize.Small:
				kernel = 0;
				break;
			case BlurKernelSize.Medium:
				kernel = 2;
				break;
			case BlurKernelSize.Big:
				kernel = 4;
				break;
		}

		var rt2 = RenderTexture.GetTemporary(source.width, source.height, 0, source.format);

		for (int i = 0; i < iterations; i++)
		{
			// helps to achieve a larger blur
			float radius = (float)i * interpolation + interpolation;
			blurMaterial.SetFloat(Uniforms._Radius, radius);

			Graphics.Blit(source, rt2, blurMaterial, 1 + kernel);
			source.DiscardContents();

			// is it a last iteration? If so, then blit to destination
			if (i == iterations - 1)
			{
				Graphics.Blit(rt2, destination, blurMaterial, 2 + kernel);
			}
			else
			{
				Graphics.Blit(rt2, source, blurMaterial, 2 + kernel);
				rt2.DiscardContents();
			}
		}

		RenderTexture.ReleaseTemporary(rt2);
	}

	public bool OnResultEffect(user_card card, Callback cb)
    {
		if (card.GetCardData().GetResourceType() == 1)
		{
			resultEffect = EpicEffect;
			PerfectEffect.SetActive(false);
			MissEffect.SetActive(false);

			Text text = resultEffect.transform.Find("randomText").GetComponent<Text>();
			if (text)
			{
				text.text = LocalizeData.GetText("gacha_epic" + Random.Range(1, 3));
			}
		}
		else if (card.GetUVRect().size == Vector2.one)
        {
			resultEffect = PerfectEffect;
			EpicEffect.SetActive(false);
			MissEffect.SetActive(false);

			Text text = resultEffect.transform.Find("Text").GetComponent<Text>();
			if (text)
			{
				text.text = LocalizeData.GetText("gacha_perfect" + Random.Range(1, 3));
			}
		}
		else
        {
			resultEffect = MissEffect;
			EpicEffect.SetActive(false);
			PerfectEffect.SetActive(false);

			AudioSource audio = resultEffect.GetComponent<AudioSource>();
			string path = "Sound/";
			switch(Random.Range(0,3))
            {
				case 1:
					path += "sfx_miss1";
					break;
				case 2:
					path += "sfx_miss2";
					break;
				case 0:
				default:
					path += "sfx_miss";
					break;
			}
			audio.clip = Resources.Load<AudioClip>(path);

			Text text = resultEffect.transform.Find("Text").GetComponent<Text>();
			if(text)
            {
				text.text = LocalizeData.GetText("gacha_miss" + Random.Range(1, 4));
			}
		}

		resultPicture = resultEffect.transform.Find("Picture").GetComponent<RawImage>();

		doneCallback = null;

		cardName.text = "";
		cardName.gameObject.SetActive(false);

		Texture TargetSprite = null;
		object obj;

		videoPlayer.Stop();
		VideoClip preClip = videoPlayer.clip;
		Resources.UnloadAsset(preClip);

		RenderTexture preTexture = videoPlayer.targetTexture;
		if (preTexture)
		{
			RenderTexture rt = RenderTexture.active;
			RenderTexture.active = preTexture;
			GL.Clear(true, true, Color.clear);
			RenderTexture.active = rt;
		}

		uint resourceType = card.GetCardData().GetResourceType();
		Rect uvRect = new Rect(Vector2.zero, Vector2.one);
		
		if (resourceType == 1)
		{
			if (card.GetCardData().data.TryGetValue("resource_path", out obj))
			{
				VideoClip clip = Resources.Load<VideoClip>((string)obj);
				videoPlayer.clip = clip;
				videoPlayer.isLooping = true;
				videoPlayer.Play();
				TargetSprite = videoPlayer.targetTexture;
			}
		}
		else
		{
			TargetSprite = card.GetSprite().texture;
			uvRect = card.GetUVRect();
		}



		if (card.data.TryGetValue("card_title_kr", out obj))
		{
			cardName.text = (string)obj;
		}

		if (TargetSprite == null)
		{
			return false;
		}

		resultPicture.texture = TargetSprite;
		resultPicture.uvRect = uvRect;

		frame.SetActive(false);
		Iris.gameObject.SetActive(false);
		uiEffect.enabled = false;
		texture = TargetSprite;
		size = new Vector2(TargetSprite.width, TargetSprite.height);
		if(resourceType != 1)
        {
			size = card.GetRect().size;			
        }
		target.uvRect = uvRect;
		RectTransform containerRT = transform.parent.GetComponent<RectTransform>();
		Vector2 MAX_SIZE = containerRT.rect.size;

		float minRatio = MAX_SIZE.x / size.x;
		size = size * minRatio;

		GetComponent<RectTransform>().sizeDelta = new Vector2(MAX_SIZE.x, MAX_SIZE.x * 0.75f);
		resultPicture.GetComponent<RectTransform>().sizeDelta = size * (1.0f / 0.18f) * 0.9f;
		source = new RenderTexture((int)size.x, (int)size.y, 24);
		dest = new RenderTexture((int)size.x, (int)size.y, 24);
		target.color = Color.black;
		if (coroutine != null)
			StopCoroutine(coroutine);

		coroutine = StartCoroutine(UpdateResultBlurTexture());

		doneCallback = cb;

		return true;
	}

	public void UseNormalTextureBlur(Sprite targetSprite)
    {
		size = targetSprite.rect.size;

		if (source == null)
			source = new RenderTexture((int)size.x, (int)size.y, 24);
		if(dest == null)
			dest = new RenderTexture((int)size.x, (int)size.y, 24);

		texture = targetSprite.texture;

		RefreshBlur();
	}

	public void RefreshNormalTextureBlur()
    {
		if (source == null)
			source = new RenderTexture((int)size.x, (int)size.y, 24);
		if (dest == null)
			dest = new RenderTexture((int)size.x, (int)size.y, 24);
	}

    private void OnApplicationFocus(bool focus)
    {
        if(focus)
        {
            if(enabled)
                RefreshBlur();
        }
    }
}

