using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenShot : MonoBehaviour
{

    [SerializeField]
    private string path;
    [SerializeField]
    [Range(1,5)]
    private int size = 4;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.C)){
            path += "screenshot";
            path += System.Guid.NewGuid().ToString() + ".png";
            ScreenCapture.CaptureScreenshot(path, size);
        } 
    }
}
