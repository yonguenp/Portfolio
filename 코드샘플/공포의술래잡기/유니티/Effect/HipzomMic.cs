using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HipzomMic : MonoBehaviour
{
    [SerializeField] GameObject mic;
    [SerializeField] float rotValue = 30;

    float z = 0;
    // Update is called once per frame
    void Update()
    {
        z += Time.deltaTime * rotValue;
        mic.transform.rotation = Quaternion.Euler(0, 0, z);
    }
}
