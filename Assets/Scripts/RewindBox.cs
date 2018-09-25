using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RewindBox : RewindObjectBase {

    public override void Rewind(float time)
    {
        GetComponent<BoxBase>().KillTweening();
        GetComponent<BoxBase>().RewindingDisable(time + 0.01f);
        base.Rewind(time);
    }
}
