using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RewindPlayer : RewindObjectBase{

    public override void Rewind(float time)
    {
        GetComponent<PlayerBase>().KillTweening();
        GetComponent<PlayerBase>().RewindingDisable(time + 0.01f);
        base.Rewind(time);
    }
}
