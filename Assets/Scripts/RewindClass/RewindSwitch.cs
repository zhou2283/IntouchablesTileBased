using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RewindSwitch : RewindObjectBase {

    List<bool> isChangedList = new List<bool>();

    public override void Record()
    {
        
    }

    public override void Rewind(float rewindTime)
    {
        if (isChangedList.Count >= MaxLength)
        {
            isChangedList.RemoveAt(MaxLength - 1);
        }
        //isChangedList.Insert(0, transform.position);
    }

}
