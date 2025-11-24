using CesiumForUnity;
using System;
using System.Collections;
using System.Collections.Generic;
using TerrainEngine;
using Unity.Mathematics;
using UnityEngine;

public class CesiumCornerPlacer : MonoBehaviour
{
    [SerializeField] public CesiumGeoreference GeoRef;
    [SerializeField] public Transform topleft;
    [SerializeField] public Transform topright;
    [SerializeField] public Transform botleft;
    [SerializeField] public Transform botright;
    [SerializeField] public Transform center;
    private CesiumEllipsoid ellipsoid;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    Vector3 getSpherePostion(double lon, double lat)
    {
        //float theta = Mathf.Deg2Rad * lon;
        //float phi = Mathf.Deg2Rad * lat;
        double3 lonlath = new double3(lon, lat, 0d);
        double3 ecef = ellipsoid.LongitudeLatitudeHeightToCenteredFixed(lonlath);
        double3 d3pos = GeoRef.TransformEarthCenteredEarthFixedPositionToUnity(ecef);
        Vector3 v3pos = new Vector3((float)d3pos.x, (float)d3pos.y, (float)d3pos.z);
        return v3pos;
    }
    // Update is called once per frame
    void Update()
    {
        ellipsoid = GeoRef.ellipsoid;
        JMARSScene scene = SceneMaterializer.singleton.selectedScene;
        center.position = getSpherePostion(Convert.ToDouble(scene.scene_center_lon) * -1f, Convert.ToDouble(scene.scene_center_lat));
        var tllatlon = scene.top_left.Split(", ");
        topleft.position = getSpherePostion(Convert.ToDouble(tllatlon[0]), Convert.ToDouble(tllatlon[1]));
        var trlatlon = scene.top_right.Split(", ");
        topright.position = getSpherePostion(Convert.ToDouble(trlatlon[0]), Convert.ToDouble(trlatlon[1]));
        var bllatlon = scene.bottom_left.Split(", ");
        botleft.position = getSpherePostion(Convert.ToDouble(bllatlon[0]), Convert.ToDouble(bllatlon[1]));
        var brlatlon = scene.bottom_right.Split(", ");
        botright.position = getSpherePostion(Convert.ToDouble(brlatlon[0]), Convert.ToDouble(brlatlon[1]));

        
        //Debug.Log("Left Side: " + (topleft.position - botleft.position).ToString());
        //Debug.Log("Right Side: " + (topright.position - botright.position).ToString());
        //Debug.Log("Top Side: " + (topleft.position - topright.position).ToString());
        //Debug.Log("Bot Side: " + (botleft.position - botright.position).ToString());
    }
}
