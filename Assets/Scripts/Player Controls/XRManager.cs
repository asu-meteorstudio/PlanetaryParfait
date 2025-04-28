using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace XRControls{

    public class XRManager : MonoBehaviour
    {
        public static XRManager Instance;
        private void Awake()
        {

            if (Instance)
            {
                Destroy(gameObject);
                return;
            }
            DontDestroyOnLoad(gameObject);
            Instance = this;
        }
    }

}