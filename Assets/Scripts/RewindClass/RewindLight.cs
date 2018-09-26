using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RewindLight : RewindObjectBase {

    public override void Record()
    {
        if (lightIsOnHistory.Count >= MaxLength)
        {
            lightIsOnHistory.RemoveAt(MaxLength - 1);
        }
        //print(transform.name + " " + transform.GetComponent<Light2DBaseControl>().lightIsOn.ToString());
        lightIsOnHistory.Insert(0, transform.GetComponent<Light2DBaseControl>().lightIsOn);
    }

    public override void Rewind(float rewindTime)
    {
        if (lightIsOnHistory.Count > 0)
        {
            if (lightIsOnHistory[0])
            {
                transform.GetComponent<Light2DBaseControl>().LightOnRewind(rewindTime);
            }
            else
            {
                transform.GetComponent<Light2DBaseControl>().LightOffRewind(rewindTime);
            }
            lightIsOnHistory.RemoveAt(0);
        }
    }
}
