using System.Collections;
using System.Collections.Generic;
using TMPro.SpriteAssetUtilities;
using UnityEngine;

public class CameraLerp : MonoBehaviour
{
    public Vector3 startPos;
    public Vector3 stopPos;

    public float lerpDuration = 0f;
    public float totalTime = 10;

    private bool activate = false;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            activate = true;
        }

        if (activate)
        {
            LerpCam();
        }
    }

    void LerpCam()
    {
        if (lerpDuration < totalTime)
        {
            transform.position = Vector3.Lerp(startPos, stopPos, lerpDuration / totalTime);
            lerpDuration += Time.deltaTime;
        }
        else
        {
            activate = false;
            lerpDuration = 0;
        }
    }
}
