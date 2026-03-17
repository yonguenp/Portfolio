using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SamandaCapture : MonoBehaviour
{
    public RawImage rawImage = null;
    public SamandaLauncher launcher = null;

    Texture2D screenShot = null;
    Texture2D thumbnail = null;

    private void OnDestroy()
    {
        Destroy(screenShot);
        Destroy(thumbnail);
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnCaptureCheck(SamandaLauncher lc)
    {
        launcher = lc;
        // main only
        Camera camera = Camera.main;

        // calc size shorter side will be fixed to 720 / 2
        int screenWidth = Screen.width;
        int screenHeight = Screen.height;
        if (null != camera.targetTexture)
        {
            screenWidth = camera.targetTexture.width;
            screenHeight = camera.targetTexture.height;
        }
        int outWidth = 720 / 2;
        int outHeight = 720 / 2;
        int thumbWidth = 64;
        int thumbHeight = 64;
        if (Screen.width > Screen.height)
        {
            outWidth = outHeight * screenWidth / screenHeight;
            thumbWidth = thumbWidth * screenWidth / screenHeight;

            GetComponent<RectTransform>().sizeDelta = new Vector2(outWidth * ((float)screenHeight / outHeight) * (1.0f / transform.parent.localScale.x), outHeight * ((float)screenHeight / outHeight) * (1.0f / transform.parent.localScale.y));            
        }
        else
        {
            outHeight = outWidth * screenHeight / screenWidth;
            thumbHeight = thumbHeight * screenHeight / screenWidth;

            GetComponent<RectTransform>().sizeDelta = new Vector2(outWidth * ((float)screenWidth / outWidth) * (1.0f / transform.parent.localScale.x), outHeight * ((float)screenWidth / outWidth) * (1.0f / transform.parent.localScale.y));
        }

        // remember current active rendertexture
        RenderTexture origRenderTexture = RenderTexture.active;
        RenderTexture origCameraTexture = camera.targetTexture;

        // swap render target
        RenderTexture rt1 = new RenderTexture(outWidth, outHeight, 24);
        camera.targetTexture = rt1;
        RenderTexture.active = rt1;

        // draw
        camera.Render();

        // capture main image
        screenShot = new Texture2D(outWidth, outHeight, TextureFormat.RGB24, false);
        screenShot.ReadPixels(new Rect(0, 0, outWidth, outHeight), 0, 0);
        screenShot.Apply();

        // second rt
        RenderTexture rt2 = new RenderTexture(thumbWidth, thumbHeight, 24);
        camera.targetTexture = rt2;
        RenderTexture.active = rt2;

        // draw
        camera.Render();

        // capture thumb
        thumbnail = new Texture2D(thumbWidth, thumbHeight, TextureFormat.RGB24, false);
        thumbnail.ReadPixels(new Rect(0, 0, thumbWidth, thumbHeight), 0, 0);
        thumbnail.Apply();

        // restore active rendertexture
        RenderTexture.active = origRenderTexture;
        camera.targetTexture = origCameraTexture;

        rawImage.texture = screenShot;
    }

    public void OnSend()
    {
        string b64 = System.Convert.ToBase64String(screenShot.EncodeToPNG());
        string b64Thumb = System.Convert.ToBase64String(thumbnail.EncodeToPNG());

        if(launcher != null)
            launcher.OnSendScreenCapture(b64, b64Thumb);

        Destroy(this.gameObject);
    }

    public void OnCancel()
    {
        Destroy(this.gameObject);
    }
}
