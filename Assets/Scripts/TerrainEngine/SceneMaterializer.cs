using System;
using System.Collections;
using System.Linq;
using NaughtyAttributes.Test;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;
using TerrainEngine.Tools;

namespace TerrainEngine{

    /// <summary>
    /// LOAD-BEARING SCRIPT. Ran on start. This script will replace and alter the tiles to represent the scene the user is currently on.
    /// Definition: Scene - A scene is a collection of terrain data, such as layer information, images, and depth-maps.
    /// </summary>
    public class SceneMaterializer : MonoBehaviour {
        #region FIELDS

        public static SceneMaterializer singleton;
        public float UnityUnitPerMeter;
        public Slider exaggerationSlider;

        [SerializeField] public Material heightMaterial;
        [HideInInspector] public JMARSScene selectedScene;

        public GameObject terrain;
        public GameObject tiles;
        public Vector3 terrainStartingPosition;

        [SerializeField] private DataPackBehaviour startingTerrain;
        #endregion


        #region MONO
        private void Start()
        {
            singleton = this;
            terrainStartingPosition = new Vector3(-1f, -5f, 0f);

            StartCoroutine(LoadStartingTerrain(startingTerrain));
        }

        private void Update() {
            
            //could probably turn this into an event listener instead of an update function
            if (selectedScene != null) // && !CreateAndJoinRooms.Instance.isSampleTerrain) //mulitplayer checks
                UpdateColor();
        }

        private IEnumerator LoadStartingTerrain(DataPackBehaviour dataPack)
        {
            yield return new WaitForSeconds(0.5f);
            dataPack.LoadData();
        }

        #endregion


        #region METHODS

        public void SetMaterials(JMARSScene scene) {
            TerrainColorTextureProvider.singleton.RemoveOldLayers(); //Remove old layers from TerrainTextureProvider

            if (PerPixelDataReader.singleton != null)
            {
                PerPixelDataReader.singleton.RemoveOldPins();
            }


            //deletes all numeric data pin points
            foreach(Transform s in terrain.transform)
            {
                if(s.gameObject.name == "Sphere")
                {
                    Destroy(s.gameObject);
                }
            }

/*            if (scene.heightMapRange == 0)
            {
                Color[] colors = scene.depthTexture.GetPixels();
                for (int i = 0; i < colors.Length; i++)
                {
                    scene.heightMapRange = Mathf.Max(scene.heightMapRange, colors[i].r);
                }
            }*/

            var exag = scene.exaggeration.Split(", ")[0];
            var xdim = scene.dimension.Split("x")[0];
            var ydim = scene.dimension.Split("x")[1];
            var zdim = scene.dimension.Split("x")[2];
            float xxdim = Convert.ToSingle(xdim);
            float yydim = Convert.ToSingle(ydim);
            float zzdim = Convert.ToSingle(zdim); //height

            UnityUnitPerMeter = 1/Mathf.Max(xxdim, yydim); // gets unity unit per 1 meter
            //1 Unity unit is the largest extent
            //scene.dimension: 663861.7246025101 x 266479.7063545287 x 11178.658
            //Debug.Log("Dimension = "+scene.dimension);
            // desired heigh in unity units = (displacementColor.r - _offset)* UnityUnitPerMeter

            //relative height to normalized range, rH = (displacementColor.r - _offset) / scene.heightMapRange
            //desired height in unity units = rH * zzdim * UnityUnitPerMeter;
            float scaledHeight = UnityUnitPerMeter * zzdim;
            
            //Debug.Log("unity units = " + UnityUnitPerMeter + " zdim = " + zzdim);
            
            var exaggeration = Convert.ToDouble(exag);
            //Debug.Log(exaggeration + "*" + scaledHeight);

            //Debug.Log("DepthTexture: " +(scene.depthTexture == null ? "null" : "not null"));
            heightMaterial.SetTexture("_HeightMap", scene.depthTexture);
            //print("FIRST PIXEL = " + scene.depthTexture.GetPixel(0,0).r);
            
            heightMaterial.SetTexture("_MainTex", TerrainColorTextureProvider.singleton.texture, RenderTextureSubElement.Default);
            heightMaterial.SetFloat("_length", 1f * scene.depthTexture.height);
            heightMaterial.SetFloat("_width", 1f * scene.depthTexture.width);
            heightMaterial.SetFloat("_scaleFactor", -(float)exaggeration * 0.001f); 

            //changes terrain transform so it always spawns below the player
            float heightValue = scene.depthTexture.GetPixel(scene.depthTexture.width/2, scene.depthTexture.height/2).r * heightMaterial.GetFloat("_scaleFactor");
            tiles.transform.localPosition = new Vector3(0, -heightValue, 0);
            
            //Debug.Log("exag" + exaggeration + "scaledHeight"+scaledHeight+ "scaleFactor" + -(float)exaggeration * 0.001f * scaledHeight);
            //Sets the transparency value based on what is stored on JMARS' servers
            foreach (var layer in scene.layers)
                layer.transparency = layer.toggle_state == "true" ? 1 : 0;
            
            //PerPixelDataReader.singleton.InstantiateNomenclature(); 
            //NomenclatureDataReader.Singleton.InstantiateNomenclature(SceneDownloader.singleton.nomenclature, scene.depthTexture);


        }

        /// <summary>
        /// We have to update the texture constantly because it is a render-texture
        /// TO-DO: We should probably turn this into a shader later on....
        /// </summary>
        private void UpdateColor() {
            heightMaterial.SetTexture("_MainTex", TerrainColorTextureProvider.singleton.texture, RenderTextureSubElement.Default);
        }

        #endregion
    }

}