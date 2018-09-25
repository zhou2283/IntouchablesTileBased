using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RewindLight : RewindObjectBase {

    public override void Record()
    {
        if (lightIsOnHistory.Count >= maxLength)
        {
            lightIsOnHistory.RemoveAt(maxLength - 1);
        }
        print(transform.name + " " + transform.GetComponent<Light2DBaseControl>().lightIsOn.ToString());
        lightIsOnHistory.Insert(0, transform.GetComponent<Light2DBaseControl>().lightIsOn);
    }

    public override void Rewind(float time)
    {
        if (lightIsOnHistory.Count > 0)
        {
            if (lightIsOnHistory[0])
            {
                transform.GetComponent<Light2DBaseControl>().LightOnRewind(time);
            }
            else
            {
                transform.GetComponent<Light2DBaseControl>().LightOffRewind(time);
            }
            lightIsOnHistory.RemoveAt(0);
        }
    }
}
