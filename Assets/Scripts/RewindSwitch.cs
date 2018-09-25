using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RewindSwitch : RewindObjectBase {

    List<bool> isChangedList = new List<bool>();

    public override void Record()
    {
        
    }

    public override void Rewind(float time)
    {
        if (isChangedList.Count >= maxLength)
        {
            isChangedList.RemoveAt(maxLength - 1);
        }
        //isChangedList.Insert(0, transform.position);
    }

}
