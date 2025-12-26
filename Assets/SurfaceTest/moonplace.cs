using CesiumForUnity;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class moonplace : MonoBehaviour
{
    [SerializeField] public CesiumGeoreference GeoRef;
    private CesiumEllipsoid ellipsoid;
    [Range(0, 180)]
    public float lat = 0;
    [Range(0, 360)]
    public float lon = 0;
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
    // Start is called before the first frame update
    void Start()
    {
        ellipsoid = GeoRef.ellipsoid;
    }

    // Update is called once per frame
    void Update()
    {
        this.transform.position = getSpherePostion(lon, lat);
    }
}
