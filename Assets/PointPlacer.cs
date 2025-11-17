using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointPlacer : MonoBehaviour
{
    public Transform obj;

    // These angles, in degrees
    [Range(-180, 180)]
    public float inlat = 0;
    [Range(-90, 90)]
    public float inlon = 0;
    [SerializeField] public float sphereRadius;
    [SerializeField] public Vector3 spherePostion;
    [SerializeField] public float scale;
    // Start is called before the first frame update
    void Start()
    {

    }
    Vector3 getSpherePostion(float lon, float lat)
        { 
            float theta = Mathf.Deg2Rad * lon;
            float phi = Mathf.Deg2Rad * lat;
            return (spherePostion + sphereRadius * new Vector3(Mathf.Sin(theta) * Mathf.Cos(phi), Mathf.Cos(theta), Mathf.Sin(theta) * Mathf.Sin(phi))) * scale;
        }

    // Update is called once per frame
    void Update()
    {

        // Set the position of the obj transform 
        obj.position = getSpherePostion(inlon, inlat); 
    }
}
