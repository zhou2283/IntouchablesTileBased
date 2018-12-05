using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using FMODUnity;

[RequireComponent(typeof(Light2D))]

public class Light2DBaseControl : MonoBehaviour {

    
    Light2D light2DScript;
    public bool lightIsOn = false;
    [SerializeField]
    float maxRange = 20f;
    [SerializeField]
    float minRange = 1f;

    public bool isTweening = false;

    public float duration = 0.5f;
    public float normalDuration = 0.5f;
    public float rewindDuration = 0.1f;

    MeshCollider _meshCollider;
    MeshFilter _meshFilter;

    Tweener lightExpandTweener;
    
    
    //FMOD
    public string lightOnSound = "event:/Light/LightOnSound";
    public string lightOffSound = "event:/Light/LightOffSound";

    // Use this for initialization
    void Start () {
        light2DScript = GetComponent<Light2D>();

        if (lightIsOn)
        {
            light2DScript.Range = maxRange;
        }
        else
        {
            light2DScript.Range = minRange;
        }

        if(GetComponent<MeshCollider>() == null)
        {
            _meshCollider = gameObject.AddComponent<MeshCollider>();
        }
        _meshFilter = GetComponent<MeshFilter>();
        UpdateMeshCollider();
    }
	
	// Update is called once per frame
	void Update () {
        UpdateMeshCollider();
    }

    public void LightOff()
    {
        if (lightIsOn)
        {
            
            lightExpandTweener.Kill();
            //lightIsOn = false;
            isTweening = true;
            lightExpandTweener = DOTween.To(() => light2DScript.Range, x => light2DScript.Range = x, minRange, duration).OnComplete(TweeningComplete);
            
            //FMOD
            GameControlSingleton.Instance.PlayOneShotSound(lightOffSound);
        }
    }

    public void LightOn()
    {
        if (!lightIsOn)
        {
            
            lightExpandTweener.Kill();
            //lightIsOn = true;
            isTweening = true;
            lightExpandTweener = DOTween.To(() => light2DScript.Range, x => light2DScript.Range = x, maxRange, duration).OnComplete(TweeningComplete);
            
            //FMOD
            GameControlSingleton.Instance.PlayOneShotSound(lightOnSound);
        }
    }

    public bool LightSwitch()
    {
        if (!isTweening)
        {
            if (lightIsOn)
            {
                LightOff();
            }
            else
            {
                LightOn();
            }

            return true;
        }

        return false;
    }

    void TweeningComplete()
    {
        isTweening = false;
        lightIsOn = !lightIsOn;
    }

    void UpdateMeshCollider()
    {
        _meshCollider.sharedMesh = _meshFilter.sharedMesh;
    }

    //rewind part
    public void LightOffRewind(float _duration)
    {
        if (lightIsOn)
        {
            lightExpandTweener.Kill();
            //isTweening = true;
            lightIsOn = false;
            lightExpandTweener = DOTween.To(() => light2DScript.Range, x => light2DScript.Range = x, minRange, _duration);
        }
    }

    public void LightOnRewind(float _duration)
    {
        if (!lightIsOn)
        {
            lightExpandTweener.Kill();
            //isTweening = true;
            lightIsOn = true;
            lightExpandTweener = DOTween.To(() => light2DScript.Range, x => light2DScript.Range = x, maxRange, _duration);
        }
    }
    
    
}
