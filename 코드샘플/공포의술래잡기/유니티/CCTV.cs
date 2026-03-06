using UnityEngine;

public class CCTV : MonoBehaviour
{
    UIGame ui = null;
    [SerializeField]
    Camera cctvCamera = null;
    private void Start()
    {
        ui = GameObject.FindObjectOfType<UIGame>();
    }

    public void Show(bool isShow)
    {
        if (ui == null)
            ui = GameObject.FindObjectOfType<UIGame>();

        if (cctvCamera) cctvCamera.gameObject.SetActive(isShow);
        if (ui) ui.ShowCCTV(isShow);
    }
}
