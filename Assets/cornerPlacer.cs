using System;
using System.Collections;
using System.Linq;
using NaughtyAttributes.Test;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;
using TerrainEngine.Tools;
namespace TerrainEngine{
    public class cornerPlacer : MonoBehaviour
    {

        [SerializeField] public Transform topleft;
        [SerializeField] public Transform topright;
        [SerializeField] public Transform botleft;
        [SerializeField] public Transform botright;
        [SerializeField] public Transform center; 
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
            JMARSScene scene = SceneMaterializer.singleton.selectedScene;
            center.position = getSpherePostion(Convert.ToSingle(scene.scene_center_lon) * -1f, Convert.ToSingle(scene.scene_center_lat));
            var tllatlon = scene.top_left.Split(", ");
            topleft.position = getSpherePostion(Convert.ToSingle(tllatlon[0]), Convert.ToSingle(tllatlon[1]));
            var trlatlon = scene.top_right.Split(", ");
            topright.position = getSpherePostion(Convert.ToSingle(trlatlon[0]), Convert.ToSingle(trlatlon[1]));
            var bllatlon = scene.bottom_left.Split(", ");
            botleft.position = getSpherePostion(Convert.ToSingle(bllatlon[0]), Convert.ToSingle(bllatlon[1]));
            var brlatlon = scene.bottom_right.Split(", ");
            botright.position = getSpherePostion(Convert.ToSingle(brlatlon[0]), Convert.ToSingle(brlatlon[1]));
        }
    }
}