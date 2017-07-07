using UnityEngine;
using System.Collections;

/* ####################################### */
//                                         //
//         ST_ = State Events              //
//         UI_ = UI Events                 //
//         CO_ = Controller Events         //
//         TB_ = TracktorBeam Events       //
//                                         //
/* ####################################### */

public class Events : MonoBehaviour
{
	public enum Event
    {
        ST_STATECHANGED,
        UI_UICHANGED,
        CO_CONTROLLER1CONNECTED,
        CO_CONTROLLER2CONNECTED,
        CO_CONTROLLER3CONNECTED,
        CO_CONTROLLER4CONNECTED, 
        CO_CONTROLLER1DISCONNECTED,
        CO_CONTROLLER2DISCONNECTED,
        CO_CONTROLLER3DISCONNECTED,
        CO_CONTROLLER4DISCONNECTED,
        CO_CONTROLLERADDED,
        TB_BEAMREADY,
        Count
    }
}
