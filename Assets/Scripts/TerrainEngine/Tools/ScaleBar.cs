using System;
using System.Collections;
using System.Collections.Generic;
using TerrainEngine;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UserInterface;
using XRControls;

namespace TerrainEngine.Tools
{
    /// <summary>
    /// This script deals with generating the scale values used for each terrain dynamically for the scalebar tool.
    /// It functions by using a ScalebarPrefab, which is spawned into the scene and is scaled dynamically depending on the terrain's dimensions.
    /// </summary>
    public class ScaleBar : MonoBehaviour
    {
        #region FIELDS

        public static ScaleBar singleton;

        [System.Serializable]
        public class
            ScaleBarPrefab // Describes a Scalebar object. Includes the prefab, the height value in real world (in meters), and the unit 
        {
            public GameObject prefab;
            public float realWorldHeight; //height of 1 Unity unit (of the prefab) in real world meters.
            public string label; // unit
        }

        [HideInInspector] public bool scalebarMode = false; // determines if user has scalebar tool enabled or not

        public List<ScaleBarPrefab>
            bars; // list containing all of the scalebar prefabs. When the game starts, this list gets populated with the needed data.

        GameObject obj; // the temporary scalebar GameObject that spawns into the scene
        private TextMeshPro text; // the Text component that is attached to the "obj" variable
        public TextMeshProUGUI menuText; //text on menu that displays scale
        public int valueToRoundTo = 100; // what value we round the scalebar places to 

        // Calculation for scalebar values
        private float min; // the minimum value the scalebar can be
        private float max; // the maximum value the scalebar can be 
        private int index; // used to cycle through the bars list

        private float
            scale; // calculation of the scale, determined by the terrain's UnityUnitPerMeter and the scalebar's height in real world

        [Header("Terrain")] public GameObject terrain; // terrain GameObject

        public SampleTerrainsMenu
            _SampleTerrainsMenu; // used to determine when a sample terrain has been loaded so we can refresh the scalebar values for the new terrain

        private Ray ray; // raycast we are making from the controller/camera to the terrain

        /// <summary>
        /// +1 if positive, -1 if negative
        /// </summary>
        private int lastSign = 0; // determines if we have made an intersection with the terrain or not

        public Material material;
        public Texture2D heightTexture;

        // VR-related Variables
        [SerializeField] private GameObject vrController;
        public XRController controlCheck; // determines if we are pressing the button in VR

        private bool
            pressedThisFrame; // determines if VR user pressed button or not for that frame. Used to make code happen once, and not every frame.

        private bool
            changeTextOnce =
                false; // ensures that the Scalebar's text gets changed once, and not every frame, for both desktop and VR 

        private bool
            placeOnce = false; // ensures that a Scalebar is spawned into a scene once. Prevents scalebars spawning every frame in VR

        #endregion

        #region MONO

        // Start is called before the first frame update
        void Start()
        {
            singleton = this;
            _SampleTerrainsMenu = GameObject.FindObjectOfType<SampleTerrainsMenu>();

            if (GameState.IsVR)
            {
                controlCheck = GameObject.FindGameObjectWithTag("Player").GetComponent<XRController>();
            }
        }

        // Update is called once per frame
        void Update()
        {
            //check which rig is used so we can raycast properly to the terrain
            DeterminePlayerRay();

            // If the user changes the terrain to a CUSTOM TERRAIN, update the Scalebar prefabs stored in the bars list.
            if (SceneDownloader.currentState == SceneDownloader.SceneSession.READY)
            {
                changeTextOnce = true; // updates the text
            }

            // If the user changes the terrain to a SAMPLE TERRAIN, update the Scalebar prefabs stored in the bars list.
            if (_SampleTerrainsMenu
                .loadedSampleTerrain) // loadedSampleTerrain tells us when a sample terrain is being loaded
            {
                changeTextOnce = true;
                _SampleTerrainsMenu.loadedSampleTerrain = false;
            }

            if (changeTextOnce)
            {
                // update text for currently spawned Scalebar
                if (obj != null) // another solution to this to make it more performant? This was added to ensure when a custom terrain is changed, the obj's text is updated accordingly
                {
                    UpdateLabelText(text);
                }

                changeTextOnce = false;
            }

            // If user has scalebar tool activated
            if (scalebarMode)
            {
                // do raycast from mouse/controller to the terrain
                CalculateRay();

                // If user right clicks or presses A button on VR, they will be able to cycle through different scale measurements
                if (Input.GetMouseButtonDown(1) || (controlCheck.aActive && !pressedThisFrame))
                {
                    // Go to the next scale length
                    pressedThisFrame =
                        true; // ensures the code only runs once so we correctly switch to the next scale for VR
                    index += 1;
                    if (index < 0 || index >= bars.Count)
                    {
                        index = 0;
                    } // makes sure index does not exceed array length

                    Destroy(obj);
                    placeOnce = false; // this will spawn the next scalebar. Make this more efficient?

                }

                // calculate scale --  **make a variable so this does not happen every frame
                scale = (bars[index].realWorldHeight * SceneMaterializer.singleton.UnityUnitPerMeter) *
                        terrain.transform.parent.localScale.x;

                if (controlCheck.aActive == false) pressedThisFrame = false;
            }
            else
            {
                // Destroy obj so the scalebar disappears when the user disables the scalebar tool
                Destroy(obj);
                placeOnce = false;
            }
        }

        #endregion

        #region METHODS

        /// <summary>
        /// Toggles scalebar mode when user clicks UI button via desktop/VR
        /// </summary>
        public void ToggleScalebarMode()
        {
            scalebarMode = (obj == null) ? true : false;
        }

        /// <summary>
        /// When a scalebar's label is changed, it updates the text on the Scalebar GameObject
        /// </summary>
        public void UpdateLabelText(TextMeshPro text)
        {
            menuText.text = "Current Scale: " + bars[index].realWorldHeight + " " +
                            SceneDownloader.singleton.scene.units;
            /*bars[index].label = bars[index].realWorldHeight + " " + SceneDownloader.singleton.scene.units;
            text.text = bars[index].label;*/
        }

        /// <summary>
        /// Determines what type of raycast to use depending on what platform they are on (VR, Desktop)
        /// </summary>
        public void DeterminePlayerRay()
        {
            if (!GameState.IsVR) //desktop
            {
                Vector3 mousePos = Input.mousePosition;
                ray.origin = Camera.main.ScreenToWorldPoint(mousePos);
                ray.direction = Camera.main.transform.forward;
            }
            else if (GameState.IsVR) //VR
            {
                ray.origin = vrController.transform.position;
                ray.direction = vrController.transform.forward;
            }
            else
            {
                Debug.LogError("rig error");
            }
        }

        /// <summary>
        /// Calculates the 5 scalebar prefabs by finding the min, max, and in-between values.
        /// Sets these values to the scalebar's "realWorldHeight", which is used to calculate the "scale" variable.
        /// </summary>
        public void CalculatePrefabs(JMARSScene scene)
        {
            // get terrain and calculate prefabs
            string xdim = scene.dimension.Split("x")[0];
            var xxdim = Convert.ToSingle(xdim);

            // Min calculation
            for (int i = 0; i < xxdim; i += 5)
            {
                if ((i * SceneMaterializer.singleton.UnityUnitPerMeter) * terrain.transform.parent.localScale.x > 4)
                {
                    // round down to value so we don't get messy numbers displayed on the scalebar (ex: 23,947 m)
                    if (i % valueToRoundTo == 0)
                    {
                        min = i;
                    }
                    else
                    {
                        var valueToSubtract = i % valueToRoundTo;
                        if ((i - valueToSubtract) > min)
                        {
                            min = i - valueToSubtract;

                        }
                        else
                        {
                            // Since the valueToSubtract is BIGGER than min, keep the original min value. Prevents the scalebar from being too small to appear in the scene
                            // For this edge-case, you can try a smaller value to round to (hundredth, tenth?)  
                            min = i;
                        }
                    }

                    bars[0].realWorldHeight = min;
                    break;
                }
            }

            // Max calculation
            for (int i = 0; i < xxdim; i += 5)
            {
                if ((i * SceneMaterializer.singleton.UnityUnitPerMeter) * terrain.transform.parent.localScale.x >
                    terrain.transform.parent.localScale.x
                    || (i * SceneMaterializer.singleton.UnityUnitPerMeter) * terrain.transform.parent.localScale.x >
                    100)
                {
                    // round down to value so we don't get messy numbers displayed on the scalebar (ex: 23,947 m)
                    if (i % valueToRoundTo == 0)
                    {
                        max = i;
                    }
                    else
                    {

                        var valueToSubtract = i % valueToRoundTo;

                        if ((i - valueToSubtract) > max)
                        {
                            max = i - valueToSubtract;
                        }
                        else
                        {
                            // Since the valueToSubtract is BIGGER than max, keep the original max value. Prevents the scalebar from being too big & covers player's screen
                            // For this edge-case, you can try a smaller value to round to (hundredth, tenth?)  
                            max = i;
                        }

                    }

                    var lastIndex = bars.Count - 1;
                    bars[lastIndex].realWorldHeight = max;
                    break;
                }
            }

            // Calculations for the other bars in the list (the ones in-between the min and max values) 
            int average = (int)max / 5; // the average value between each scalebar
            float meters = 0; // represents the number of meters between each index. Used for addition between indexes
            for (int i = 0; i < bars.Count; i++)
            {
                if (i == 0 || i == bars.Count - 1) // if first or last index
                {
                    // do nothing
                }
                else
                {
                    meters += average;

                    if (average % valueToRoundTo == 0)
                    {
                        // do nothing
                    }
                    else
                    {
                        var valueToSubtract = meters % valueToRoundTo;
                        meters = meters - valueToSubtract;
                    }

                    bars[i].realWorldHeight = meters;
                }
            }
        }

        /// <summary>
        /// Creates raycast from user's mouse/controller to the terrain.
        /// Depending if intersecting with the terrain or not, run code for functionality
        /// </summary>
        public void CalculateRay()
        {
            float val_intersection = float.NaN;

            float step = 0.025f;
            lastSign = 0;

            //1000 is an arbitary big number (we know that the user will not be THAT far away from the terrain)
            //this for-loop steps from the starting point (player location & camera direction) and the intersection point (val_intersection)
            //we are looking for val_intersection
            for (float t = 0.01f; t < 1000; t += step)
            {
                //changing ray coordinates to terrain corrdiantes
                Vector3 ray_origin_terrain = terrain.transform.InverseTransformPoint(ray.origin);
                Vector3 ray_origin_terrain2 = terrain.transform.InverseTransformPoint(ray.origin + ray.direction);
                Vector3 ray_direction_terrain = ray_origin_terrain2 - ray_origin_terrain;
                Vector3 ray_t = ray_origin_terrain + (ray_direction_terrain * t); //t is the distance of the ray

                heightTexture = material.GetTexture("_HeightMap") as Texture2D;

                //out of bounds conditions
                if (ray_t.x > 0.5f || ray_t.x < -0.5f || ray_t.z > 0.5f || ray_t.z < -0.5f) return;

                //converts terrain coordinates to texture coordinates
                int x_pos_img =
                    (int)(((ray_t.x + 0.5f) *
                           heightTexture.width)); //0.5f gets the x value to its relative position in the Unity Scene
                int z_pos_img = (int)(((0.5f - ray_t.z) * heightTexture.height));


                float heightValue =
                    heightTexture.GetPixel(x_pos_img, z_pos_img).r; //height texture is stored in the red channel
                float h_t = heightValue * material.GetFloat("_scaleFactor");

                float ht_minus_rty = (h_t - ray_t.y);
                int index1 = Math.Abs(z_pos_img - heightTexture.height) * heightTexture.width + x_pos_img;

                //first time entering the loop
                if (lastSign == 0)
                {
                    lastSign = (int)Mathf.Sign(ht_minus_rty);
                }

                if (ht_minus_rty == 0 || lastSign == -(Mathf.Sign(ht_minus_rty)))
                {
                    //INTERSECTINING THE TERRAIN GAMEOBJECT
                    val_intersection = heightValue;

                    //placing scalebar
                    if (!placeOnce)
                    {
                        obj = Instantiate(bars[index].prefab);
                        text = obj.GetComponentInChildren<TextMeshPro>();
                        UpdateLabelText(text);
                        placeOnce = true;
                    }
                    else
                    {
                        Vector3 intersectionPoint =
                            terrain.transform
                                .TransformPoint(ray_t); // x y z position of where user is intersecting terrain]
                        // rotate to always face user
                        var targetObj = Camera.main.transform; // get user's camera transform

                        obj.transform.position = intersectionPoint;
                        obj.transform.LookAt(targetObj.transform); // face user's camera
                        obj.transform.localEulerAngles =
                            new Vector3(0, obj.transform.localEulerAngles.y, 0); // only rotate in the yaw axis
                        obj.transform.localScale = Vector3.one * scale;
                    }

                    break;
                }
            }
        }

        #endregion
    }
}