using UnityEngine;


// This Class exists to place things on the spherical ellipse of the moon. Mostly for testing reasons. 
public class SphereLocation : MonoBehaviour
{
    public Transform obj;

    [SerializeField] float radius = 10;
    // These angles, in degrees
    [Range(0, 360)]
    public float lat = 0;
    [Range(0, 180)]
    public float lon = 0;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        // Convert the angles to Radians (doing this in update lets the latlon change in realtime)
        float theta = Mathf.Deg2Rad * lon;
        float phi = Mathf.Deg2Rad * lat;

        // Set the position of the obj transform 
        obj.position = radius * new Vector3(Mathf.Sin(theta) * Mathf.Cos(phi), Mathf.Cos(theta), Mathf.Sin(theta) * Mathf.Sin(phi));
    }
}
