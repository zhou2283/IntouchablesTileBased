using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonBase : MonoBehaviour {

    public Transform connectedLight;
    int count = 0;
    int countLastFrame = 0;

    public bool isRewinding = false;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        if (!isRewinding)
        {
            if (countLastFrame == 0 && count > 0)
            {
                connectedLight.GetComponent<Light2DBaseControl>().LightSwitch();
            }
            else if (countLastFrame > 0 && count == 0)
            {
                connectedLight.GetComponent<Light2DBaseControl>().LightSwitch();
            }
        }
        countLastFrame = count;
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        count++;
    }

    private void OnTriggerExit2D(Collider2D col)
    {
        count--;
    }

    //rewind part
    public void RewindingDisable(float delaySeconds)
    {
        isRewinding = true;
        StartCoroutine(DelayToDisactiveIsRewinding(delaySeconds));
    }

    public IEnumerator DelayToDisactiveIsRewinding(float delaySeconds)
    {
        yield return new WaitForSeconds(delaySeconds);
        isRewinding = false;
    }
}
