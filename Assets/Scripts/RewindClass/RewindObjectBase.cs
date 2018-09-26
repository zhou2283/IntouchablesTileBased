using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class RewindObjectBase : MonoBehaviour {
    
    //max length of the history sequence
    protected const int MaxLength = 1000;
    //basic history list
    protected List<Vector3> positionHistory = new List<Vector3>();//used for box, player, 
    protected List<bool> lightIsOnHistory = new List<bool>();//used for light
    //rewind state indicator
    public bool isRewinding = false;

    public virtual void Record()
    {
        if(positionHistory.Count >= MaxLength)
        {
            positionHistory.RemoveAt(MaxLength - 1);
        }
        positionHistory.Insert(0, transform.position);
        //print("Record: "gameObject.name + " " + transform.position.ToString());
    }

    public virtual void Rewind(float rewindTime)
    {
        //if it is rewinding
        if (isRewinding)
        {
            //ignore all
            return;
        }
        //if there is nothing to rewind
        if (positionHistory.Count == 0) 
        {
            //nothing to rewind, return
            return;
        }
        //calculate the distance difference
        var yDist = Mathf.Abs(transform.position.y - positionHistory[0].y);
        var xDist = Mathf.Abs(transform.position.x - positionHistory[0].x);
        //enable isRewinding
        EnableIsRewinding();
        //initialize sequence
        var rewindSequence = DOTween.Sequence();
        //if it is not moved
        if (yDist+xDist <= 0.001f)
        {
            //tween the transform and delay to disable isRewinding
            rewindSequence.AppendInterval(rewindTime)
                .AppendCallback(DisableIsRewinding);
            //remove history
            positionHistory.RemoveAt(0);
            return;
        }
        //if it is moved
        var yTimeFactor = yDist / (xDist + yDist);
        var xTimeFactor = xDist / (xDist + yDist);
        //tween the transform and delay to disable isRewinding
        rewindSequence.Append(transform.DOMoveY(positionHistory[0].y, yTimeFactor * rewindTime).SetEase(Ease.Linear))
            .Append(transform.DOMoveX(positionHistory[0].x, xTimeFactor * rewindTime).SetEase(Ease.Linear))
            .AppendCallback(DisableIsRewinding);
        //remove history
        positionHistory.RemoveAt(0);
    }

    //set isRewinding to true
    protected void EnableIsRewinding()
    {
        isRewinding = true;
    }
    //set isRewinding to false
    protected void DisableIsRewinding()
    {
        isRewinding = false;
    }
}
