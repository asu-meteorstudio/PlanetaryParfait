using System.Collections.Generic;
using UnityEngine;
using TerrainEngine.Tools;

namespace TerrainEngine
{
    public class TerrainColorTextureProvider : MonoBehaviour {
    
        public static TerrainColorTextureProvider singleton;
        
        [HideInInspector] public RenderTexture texture;
        
        private Vector2Int resolution;
        private Camera cam;
        private List<GameObject> quads;
        private static readonly int mainTex = Shader.PropertyToID("_MainTex");
        private static readonly int color = Shader.PropertyToID("_Color");
    
        private void Awake() {
            DontDestroyOnLoad(this);
            singleton = this;
    
            resolution = new Vector2Int(200, 200);
            texture = new RenderTexture(200, 200, 16);
            quads = new List<GameObject>();
            cam = gameObject.AddComponent<Camera>();
            cam.transform.position = Vector3.zero;
            cam.transform.rotation = Quaternion.LookRotation(Vector3.forward);
            cam.backgroundColor = Color.black;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.orthographic = true;
            cam.orthographicSize = .5f;
            cam.aspect = 1;
            cam.cullingMask = 1 << LayerMask.NameToLayer("TerrainMask");
            cam.targetTexture = texture;
        }
    
        private void Update() {
            var scene = SceneMaterializer.singleton.selectedScene;
            if (scene.depthTexture != null)
            {
                UpdateLayers(scene);
                UpdateResolution(scene);
            } //return;
            // ReSharper disable once Unity.PerformanceCriticalCodeInvocation
            
            
        }

        /// <summary>
        /// Clears quads and quad objects from the TerrainTextureProvider, preventing old layers from appearing on new terrains.
        /// </summary>
        public void RemoveOldLayers()
        {
            foreach (Transform child in singleton.transform) {
                Destroy(child.gameObject);
            }
            quads.Clear();
        }
    
        private void UpdateLayers(JMARSScene scene) {
            // ReSharper disable once Unity.NoNullPropagation
    
            var layers = scene.layers;

            if (layers == null) return;

            if (quads.Count != layers.Count) {
                if (quads.Count < layers.Count) {
                    for (var i = quads.Count; i < layers.Count; i++) {
                        var quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
                        var mesh = quad.GetComponent<MeshRenderer>();
                        mesh.material = new Material(Shader.Find("UI/Default"));
                        quad.transform.parent = gameObject.transform;
                        quad.layer = LayerMask.NameToLayer("TerrainMask");
                        quad.transform.position = new Vector3(0, 0, i + .5f);
                        quads.Add(quad);
                    }
                }
            }
            
            for (var i = 0; i < layers.Count; i++) {
                var quad = quads[i];
                var layer = layers[i];
                //Hide nomenclature layer
                if (layer.layer_name == "Nomenclature")
                {
                    //The quad being used for nomenclature will always be transparent.
                    
                    var mat = quad.GetComponent<MeshRenderer>().material;
                    mat.SetTexture(mainTex, layer.graphicTexture);
                    //material.mainTextureScale = new Vector2(1.5f, 2f);
                    mat.SetColor(color, new Color(0,0,0,0));
                    quads[i] = quad;
                    
                    //We will instead use the nomenclature to alter the images on the nomenclatureParent
                    foreach (var pin in NomenclatureDataReader.singleton.nomenclaturePins)
                    {
                        pin.panelImage.material.color = new Color(pin.panelImage.material.color.r,
                            pin.panelImage.material.color.g, pin.panelImage.material.color.b, layer.transparency);
                        pin.pinMarker.color = new Color(pin.pinMarker.color.r, pin.pinMarker.color.g, pin.pinMarker.color.b, layer.transparency);
                        pin.cube.color = new Color(pin.cube.color.r, pin.cube.color.g, pin.cube.color.b, layer.transparency);
                        pin.panelText.alpha = layer.transparency;
                    }
                    
                    continue;
                }
                // ReSharper disable once Unity.PerformanceCriticalCodeInvocation
                var material = quad.GetComponent<MeshRenderer>().material;
                material.SetTexture(mainTex, layer.graphicTexture);
                //material.mainTextureScale = new Vector2(1.5f, 2f);
                material.SetColor(color, new Color(material.color.r, material.color.g, material.color.b, layer.transparency));
                quads[i] = quad;
            }
            
            
        }
    
        private void UpdateResolution(JMARSScene scene) {
            var width = scene.depthTexture.width;
            var height = scene.depthTexture.height;
    
            // ReSharper disable once InvertIf
            if (resolution.x != width || resolution.y != height) {
                resolution = new Vector2Int(width, height);
                texture = new RenderTexture(width, height, 16);
                cam.targetTexture = texture;
            }
        }
    }

}