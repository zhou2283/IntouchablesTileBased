using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class RewindPlayer : RewindObjectBase{
    
    public override void Record()
    {
        if(isTeleportingHistroy.Count >= MaxLength)
        {
            isTeleportingHistroy.RemoveAt(MaxLength - 1);
        }
        isTeleportingHistroy.Insert(0, GetComponent<PlayerBase>().isTeleporting);
        base.Record();
    }

    public override void Rewind(float rewindTime)
    {
        GetComponent<PlayerBase>().KillTweening();
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
            isTeleportingHistroy.RemoveAt(0);
            return;
        }
        //if it is moved
        if (isTeleportingHistroy[0])
        {
            rewindSequence.Append(transform.DOScale(0.1f, rewindTime/2f))//if the player size is 0, OnTriggerExit will not be called
                .Append(transform.DOMove(positionHistory[0], 0))
                .Append(transform.DOScale(1, rewindTime/2f))
                .AppendCallback(DisableIsRewinding);
            //remove history
            positionHistory.RemoveAt(0);
            isTeleportingHistroy.RemoveAt(0);
        }
        else
        {
            var yTimeFactor = yDist / (xDist + yDist);
            var xTimeFactor = xDist / (xDist + yDist);
            //tween the transform and delay to disable isRewinding
            rewindSequence.Append(transform.DOMoveY(positionHistory[0].y, yTimeFactor * rewindTime).SetEase(Ease.Linear))
                .Append(transform.DOMoveX(positionHistory[0].x, xTimeFactor * rewindTime).SetEase(Ease.Linear))
                .AppendCallback(DisableIsRewinding);
            //remove history
            positionHistory.RemoveAt(0);
            isTeleportingHistroy.RemoveAt(0);
        }
        
    }
}
