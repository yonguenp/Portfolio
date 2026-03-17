using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WorldMapItem : MonoBehaviour
{
    public uint world_type;
    private WorldMapUI parentUI;
    public GameObject smallCursor;
    public GameObject cursor;

    public void SetParentWorldMapPanel(WorldMapUI ui)
    {
        parentUI = ui;
    }

    void Start()
    {
        GetComponent<Button>().onClick.AddListener(OnSelected);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnSelected()
    {
        parentUI.OnSelect(world_type);
    }

    public void SetCursor(bool enable)
    {
        if (cursor != null)
            cursor.SetActive(enable);
        smallCursor.SetActive(!enable);
    }
}
