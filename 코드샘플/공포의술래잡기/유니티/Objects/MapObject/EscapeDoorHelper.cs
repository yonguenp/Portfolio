using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EscapeDoorHelper : MonoBehaviour
{
    public GameObject DoorSwitch = null;
    public void SetDoorId(int id)
    {
        var escapeDoor = GetComponent<EscapeDoor>();
        if(escapeDoor == null)
        {
            Debug.LogError("escapeDoor == null");
            return;
        }

        escapeDoor.EscapeDoorId = id;
    }
}
