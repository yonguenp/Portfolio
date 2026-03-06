using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapObjectDecorationController : MonoBehaviour
{
    public GameObject target;
    private void Awake()
    {
        target.SetActive(gameObject.activeSelf);
    }

    private void OnEnable()
    {
        target.SetActive(true);
    }

    private void OnDisable()
    {
        target.SetActive(false);
    }
}
