using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Coffee.UIParticleExtensions;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Serialization;
using UnityEngine.UI;

[assembly: InternalsVisibleTo("Coffee.UIParticle.Editor")]

namespace Coffee.UIExtensions
{
    [ExecuteAlways]
    [RequireComponent(typeof(RectTransform))]
    [RequireComponent(typeof(CanvasRenderer))]

    public class SoundUIParticle : UIParticle
    {
        [SerializeField] string audio_key;
        [SerializeField] AudioClip audioClip = null;
        [SerializeField] bool loop = false;

        private AudioSource audioSource;
        public override void Play()
        {
            base.Play();
            if (audio_key != null && audio_key.Length > 0)
                audioClip = SoundResourceData.GetAudioClip(audio_key);
            if (audioClip != null)
                audioSource = Managers.Sound.Play(audioClip, Sound.Effect, loop: loop);
        }
        public override void Stop()
        {
            base.Stop();
            if (audioSource != null)
                Managers.Sound.Stop(audioSource);
        }
    }
}
