using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RewindPlayer : RewindObjectBase{

    public override void Rewind(float rewindTime)
    {
        GetComponent<PlayerBase>().KillTweening();
        //GetComponent<PlayerBase>().RewindingDisable(rewindTime + 0.01f);
        base.Rewind(rewindTime);
    }
}
