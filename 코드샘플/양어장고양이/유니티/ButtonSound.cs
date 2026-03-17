using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ButtonSound : MonoBehaviour
{
    public Button targetButton;  
    
    public List<AudioClip> audioClipPool;

    public AudioClip clip;
    private bool ButtonInteractableOrigin = false;
    public void Awake()
    {        
        if (targetButton)
        {
            targetButton.onClick.AddListener(() =>
            {
                AudioManager.GetInstance().PlayEffectAudio(clip);

                if (audioClipPool.Count > 0)
                {
                    int index = Random.Range(0, audioClipPool.Count);
                    clip = audioClipPool[index];
                }
                
                ButtonInteractableOrigin = targetButton.interactable;
                targetButton.interactable = false;
                Invoke("ResetButtonInteractable", 1.0f);
            });
        }
    }

    //public void OnPointerEnter(PointerEventData data)
    //{
    //    PlaySound();
    //}

    //public void OnPointerDown(PointerEventData data)
    //{
    //    PlaySound();
    //}

    public void PlaySound()
    {
        
    }

    public void ResetButtonInteractable()
    {
        targetButton.interactable = ButtonInteractableOrigin;
    }


    [ContextMenu("auto apply buttonSound")]
    public void AutoApply()
    {
        GameObject[] roots = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();
        foreach (GameObject root in roots)
        {
            if (root.GetComponent<Canvas>() != null)
            {
                Button[] buttons = root.GetComponentsInChildren<Button>(true);
                foreach (Button button in buttons)
                {
                    ButtonSound btnSound = button.gameObject.GetComponent<ButtonSound>();
                    if (btnSound != null)
                    {
                        AudioSource audioSource = btnSound.GetComponent<AudioSource>();
                        if(audioSource != null)
                        {
                            clip = audioSource.clip;
                            DestroyImmediate(audioSource);
                        }
                    }
                }
            }
        }
    }
}
