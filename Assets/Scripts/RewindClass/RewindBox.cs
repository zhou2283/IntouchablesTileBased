using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RewindBox : RewindObjectBase {

    public override void Rewind(float rewindTime)
    {
        GetComponent<BoxBase>().KillTweening();
        GetComponent<BoxBase>().RewindingDisable(rewindTime + 0.01f);
        base.Rewind(rewindTime);
    }
}
