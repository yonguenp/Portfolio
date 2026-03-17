using SandboxNetwork;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIHamburger : UIObject
{
    public void OnClickHamburger()
    {
        UIObjectEvent.Event(UIObjectEvent.eEvent.EVENT_SHOW, UIObjectEvent.eUITarget.HAMBURGER);
    }
}
