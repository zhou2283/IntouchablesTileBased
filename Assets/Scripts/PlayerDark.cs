using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDark : PlayerBase {

    Ray ray;
    RaycastHit rayhit;
    float rayDistance = 10f;
    LayerMask lightLayer = 1 << 20;

    public override void UpdateDeathDetect()
    {
        ray = new Ray(transform.position + new Vector3(0, 0, -rayDistance / 2f), new Vector3(0, 0, 1f));
        if (Physics.Raycast(ray, out rayhit, rayDistance, lightLayer))
        {
            PlayerDead();
        }
        else
        {

        }

    }

}
