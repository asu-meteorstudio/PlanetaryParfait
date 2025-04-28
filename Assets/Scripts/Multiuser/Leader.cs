using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Multiuser
{
    /// <summary>
    /// This script is attached to the local player, which contains the camera and all movement controls for the user.
    /// </summary>
    public class Leader : MonoBehaviour
    {
        /// <summary>
        /// contains the location and string identifier for each Leader object
        /// </summary>
        public static Dictionary<string, Transform> pairs;
        public string localTag;

        private Transform spawnPoint;

        private void Awake()
        {
            pairs = new Dictionary<string, Transform>(); 
        }

        void Start()
        {
            if (localTag != "")
            {
                pairs.Add(localTag, transform);
            }

            /*// Spawn in correct position
            spawnPoint = GameObject.FindGameObjectWithTag("UserSpawnpoint").transform;
            transform.position = spawnPoint.position;*/
        }
    }
}