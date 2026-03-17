using UnityEngine;

namespace SuperBlur
{
    public class CompactBlur : SuperBlur
    {
        float before_interpolation = 0f;
        int before_downsample = 0;
        int before_iterations = 0;

        float refreshCycle = 0.0f;
        float curTick = 0.0f;
        public float RefreshCycleTime
        {
            set
            {
                refreshCycle = value;
            }
            get
            {
                return refreshCycle;
            }
        }

        private void Awake()
        {
            NecoSettingPanel.GRAPHIC_STATE state = (NecoSettingPanel.GRAPHIC_STATE)PlayerPrefs.GetInt("Setting_GraphicSet", (int)NecoSettingPanel.GRAPHIC_STATE.MEDIUM);
            switch (state)
            {
                case NecoSettingPanel.GRAPHIC_STATE.LOW:
                    RefreshCycleTime = 10.0f;
                    break;
                case NecoSettingPanel.GRAPHIC_STATE.MEDIUM:
                    RefreshCycleTime = 1.0f;
                    break;
                case NecoSettingPanel.GRAPHIC_STATE.HIGH:
                    RefreshCycleTime = 0.0f;
                    break;
            }
        }

        void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            if (blurMaterial == null || UIMaterial == null) return;
            if (before_interpolation == interpolation && before_downsample == downsample && before_iterations == iterations)
            {
                if (curTick > 0.0f)
                {
                    Graphics.Blit(source, null as RenderTexture);
                    return;
                }
            }

            int tw = source.width >> downsample;
            int th = source.height >> downsample;

            var rt = RenderTexture.GetTemporary(tw, th, 0, source.format);

            Graphics.Blit(source, rt);

            Blur(rt, destination);

            RenderTexture.ReleaseTemporary(rt);

            before_interpolation = interpolation;
            before_downsample = downsample;
            before_iterations = iterations;

            curTick = RefreshCycleTime;
        }


        private void Update()
        {
            if (curTick > 0.0f)
            {
                curTick -= Time.deltaTime;
            }
        }

        void OnApplicationFocus(bool hasFocus)
        {
            if(hasFocus)
                curTick = 0.0f;
        }
    }
}