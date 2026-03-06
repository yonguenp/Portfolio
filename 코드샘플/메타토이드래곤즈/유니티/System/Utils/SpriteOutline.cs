using UnityEngine;

[ExecuteInEditMode]
public class SpriteOutline : MonoBehaviour {
	public Color color = Color.white;

	[SerializeField]
	private SpriteRenderer _spriteRenderer;

	public SpriteRenderer spriteRenderer{
		get{
			if(_spriteRenderer == null){
				_spriteRenderer = GetComponent<SpriteRenderer>();
			}
			return _spriteRenderer;
		}
	}
	[SerializeField]
	private float _outlineSize = 1;

	private Material _preMat;

	void OnEnable() {
		_preMat = spriteRenderer.sharedMaterial;
		spriteRenderer.sharedMaterial = defaultMaterial;
		UpdateOutline(_outlineSize);
	}

	void OnDisable() {
		spriteRenderer.sharedMaterial = _preMat;
	}

	void UpdateOutline(float outline) {
		MaterialPropertyBlock mpb = new MaterialPropertyBlock();
		spriteRenderer.GetPropertyBlock(mpb);
		mpb.SetFloat("_Outline", outline > 0f ? 1.0f : 0f);
		mpb.SetColor("_OutlineColor", color);
		mpb.SetFloat("_OutlineSize", outline);
		spriteRenderer.SetPropertyBlock(mpb);
	}

	void OnValidate(){
		if(enabled){
			UpdateOutline(_outlineSize);
		}
	}


	private static Material _defaultMaterial = null;
	public static Material defaultMaterial{
		get{
			if(_defaultMaterial == null){
				_defaultMaterial = Resources.Load<Material>("Material/Sprite-Outline");
			}
			return _defaultMaterial;
		}
	}
}