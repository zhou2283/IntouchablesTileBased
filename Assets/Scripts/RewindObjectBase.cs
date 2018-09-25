using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class RewindObjectBase : MonoBehaviour {


    protected List<Vector3> positionHistory = new List<Vector3>();
    protected List<bool> lightIsOnHistory = new List<bool>();


    protected int maxLength = 1000;

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}


    public virtual void Record()
    {
        if(positionHistory.Count >= maxLength)
        {
            positionHistory.RemoveAt(maxLength - 1);
        }
        positionHistory.Insert(0, transform.position);
        //print(gameObject.name + " " + transform.position.ToString());
    }

    public virtual void Rewind(float time)
    {

        if (positionHistory.Count > 0)
        {
            float yDist = Mathf.Abs(transform.position.y - positionHistory[0].y);
            float xDist = Mathf.Abs(transform.position.x - positionHistory[0].x);

            //print(gameObject.name + " " + positionHistory[0].ToString());

            if (yDist+xDist <= 0.001f)
            {
                positionHistory.RemoveAt(0);
                return;
            }

            float yTimeFactor = yDist / (xDist + yDist);
            float xTimeFactor = xDist / (xDist + yDist);

            transform.DOMoveY(positionHistory[0].y, yTimeFactor * time).SetEase(Ease.Linear);
            transform.DOMoveX(positionHistory[0].x, xTimeFactor * time).SetEase(Ease.Linear).SetDelay(yTimeFactor * time);

            positionHistory.RemoveAt(0);
        }

    }
}
