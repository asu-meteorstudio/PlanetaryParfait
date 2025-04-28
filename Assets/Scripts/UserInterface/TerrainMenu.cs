using System;
using System.Collections.Generic;
using Multiuser.Sync;
using TerrainEngine;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UserInterface
{
    public class TerrainMenu : Menu
    {
        public delegate void SceneDelegate(List<JMARSScene.Metadata> scene);
        public static SceneDelegate sceneDelegate { get; private set; }

        public delegate void LayersDelegate(JMARSScene scene);
        public static LayersDelegate layersDelegate { get; private set; }

        public static ToggleDelegate OpenMenu;
        
        [Header("Terrain Menu GameObjects")]
        public GameObject SceneParent;
        public GameObject ScenePrefab;
        public GameObject LayersParent;
        public GameObject LayersPrefab;
        public GameObject ExagParent;
        
        [Header("Buttons")]
        public Button refreshButton;
        public Button backToMenuButton;
        public Button backToGameButton;
        
        void Awake()
        {
            OpenMenu = ToggleMenu;
            sceneDelegate = PopulateScenes;
            layersDelegate = PopulateLayers;
        }

        new void Start()
        {
            base.Start();
        }

        public override void ToggleMenu(bool active)
        {
            parentObject.SetActive(active);
        }

        public override void SetListeners()
        {
            refreshButton.onClick.AddListener(delegate
            {
                ToggleMenu(false);
                LoadingBar.OpenMenu.Invoke(true);
                PreviousMenu = this;
                StartCoroutine(SceneDownloader.singleton.DownloadViewList(Login.username, Login.password));
            });
            
            backToMenuButton.onClick.AddListener(delegate
            {
                ToggleMenu(false);
                MainMenu.OpenMenu.Invoke(true);
                //Login.username = "";
                Login.password = "";
            });
            
            backToGameButton.onClick.AddListener(delegate
            {
                PreviousMenu = this;
                ToggleMenu(false);
                MainMenu.OpenPrimaryMenus(false);
                GameState.InTerrain = true;
            });
        }

        /// <summary>
        /// Populates custom user scenes onto a UI panel in-game
        /// </summary>
        /// <param name="userScenes"></param>
        private void PopulateScenes(List<JMARSScene.Metadata> userScenes)
        {
            if (SceneParent.transform.childCount != 0) //Checks if there are already scenes populated on the userScenes page
            {
                //destroys previous scenes
                foreach (Transform child in SceneParent.transform)
                    Destroy(child.gameObject);
            }

            //loop through all new scenes
            foreach (var scene in userScenes)
            {
                var prefab = gameObject;
                prefab = Instantiate(ScenePrefab, SceneParent.transform); //create new scene button game object
                prefab.name = scene.scene_name; //name of game object in hierarchy
                prefab.GetComponentInChildren<TextMeshProUGUI>().text =
                    scene.scene_name; //text of textmeshpro comp on layer prefab
                var button = prefab.GetComponent<Button>();
                prefab.GetComponentInChildren<RawImage>().texture = scene.thumbnail;

                LoadingBar.Loading.Invoke(1f / userScenes.Count, "Populating Scenes");

                //button listener for each scene in viewport
                button.onClick.AddListener(delegate
                {
                    DepopulateLayers(); //Get rid of the layers from the previous terrain
                    
                    //NewMenuManager.singleton.previouslyOpenedMenu = "CustomTerrains";
                    SceneDownloader.singleton.terrainURL =
                        "https://cm.mars.asu.edu/api/vr/viewScene.php?access_key=" + scene.access_key;
                    
                    LoadingBar.OpenMenu.Invoke(true);
                    StartCoroutine(SceneDownloader.singleton.ChangeState(SceneDownloader.SceneSession.DOWNLOADING));
                    
                    ToggleMenu(false);
                    PreviousMenu = this;
                });

            }

            LoadingBar.DoneLoading();
            ToggleMenu(true);
        }

        /// <summary>
        /// Populates terrain layers onto UI panel in-game
        /// </summary>
        /// <param name="scene"></param>
        private void PopulateLayers(JMARSScene scene)
        {
            GameState.InTerrain = true;
            //destroys old layers
            if (LayersParent.transform.childCount != 0)
            {
                foreach (Transform child in LayersParent.transform)
                {
                    Destroy(child.gameObject);
                }
            }

            //8C5F40 <- HEX code for button colors

            //loop through all layers
            foreach (var layer in scene.layers)
            {
                //create new layer gameobject
                var prefab = gameObject;
                prefab = Instantiate(LayersPrefab, LayersParent.transform);
                prefab.name = layer.layer_name; //name of gameobject in hierarchy
                prefab.GetComponentInChildren<TextMeshProUGUI>().text =
                    layer.layer_name; //text of textmeshpro comp on layer prefab
                var slider = prefab.GetComponentInChildren<Slider>(); //slider component in child object
                //Get Layer toggle from children
                //It starts off as ON.
                var toggle = prefab.GetComponentInChildren<Toggle>();

                slider.onValueChanged.AddListener(t =>
                {
                    toggle.isOn = (t != 0);
                    layer.transparency = t;
                });

                slider.onValueChanged.AddListener(t =>
                {
                    layer.transparency = (float)(slider.value);

                    if (GameState.InMultiuser)
                    {
                        LayerSync.singleton.SendUnnamedMessage(layer.layer_name);
                        LayerSync.singleton.SendUnnamedMessage((layer.transparency).ToString());
                    }
                });

                layer.slider = slider;

                toggle.onValueChanged.AddListener(t =>
                {
                    layer.transparency = toggle.isOn ? slider.value : 0;
                    //layerBackground.color = new Color32(198, 117, 63, 80);

                    if (GameState.InMultiuser)
                    {
                        LayerSync.singleton.SendUnnamedMessage(layer.layer_name);
                        LayerSync.singleton.SendUnnamedMessage((layer.transparency).ToString());
                    }

                });

                layer.toggle = toggle;

                //loop through all layers
            }

            if (GameState.generateDataImages)
            {
                //print("adding data layers as images in terrain");

                for (int i = 0; i < SceneDownloader.singleton.datalayertextures.Count; i++)
                {
                    Texture2D flippedDataTexture = new Texture2D(SceneDownloader.singleton.datalayertextures[i].width,
                        SceneDownloader.singleton.datalayertextures[i].height);

                    for (int x = 0; x < SceneDownloader.singleton.datalayertextures[i].width; x++)
                    {
                        for (int y = 0; y < SceneDownloader.singleton.datalayertextures[i].height; y++)
                        {
                            flippedDataTexture.SetPixel(x, SceneDownloader.singleton.datalayertextures[i].height - y,
                                SceneDownloader.singleton.datalayertextures[i].GetPixel(x, y));
                        }
                    }

                    flippedDataTexture.Apply();
                    JMARSScene.Layer layer = new JMARSScene.Layer();
                    layer.graphicTexture = flippedDataTexture;
                    layer.layer_name = "Data Layer " + i;
                    layer.transparency = 1; // Turn the layer on by default
                    layer.toggle_state = "true";

                    //create new layer gameobject
                    var prefab = gameObject;
                    prefab = Instantiate(LayersPrefab, LayersParent.transform);
                    prefab.name = layer.layer_name; //name of gameobject in hierarchy
                    prefab.GetComponentInChildren<TextMeshProUGUI>().text =
                        layer.layer_name; //text of textmeshpro comp on layer prefab

                    var slider = prefab.GetComponentInChildren<Slider>(); //slider component in child object
                    //Get Layer toggle from children
                    //It starts off as ON.
                    var toggle = prefab.GetComponentInChildren<Toggle>();
                    toggle.isOn = true; // Turn the layer on by default
                    slider.onValueChanged.AddListener(t =>
                    {
                        toggle.isOn = (t != 0);
                        layer.transparency = t;
                    });

                    slider.onValueChanged.AddListener(t =>
                    {
                        layer.transparency = slider.value;

                        if (GameState.InMultiuser)
                        {
                            LayerSync.singleton.SendUnnamedMessage(layer.layer_name);
                            LayerSync.singleton.SendUnnamedMessage((layer.transparency).ToString());
                        }
                    });

                    toggle.onValueChanged.AddListener(t =>
                    {
                        layer.transparency = toggle.isOn ? slider.value : 0;
                        //layerBackground.color = new Color32(198, 117, 63, 80);

                        if (GameState.InMultiuser)
                        {
                            LayerSync.singleton.SendUnnamedMessage(layer.layer_name);
                            LayerSync.singleton.SendUnnamedMessage((layer.transparency).ToString());
                        }
                    });
                    
                    SceneDownloader.singleton.scene.layers.Add(layer);
                }

            }

            //creates another "layer" for dymanic exaggeration
            DynamicExaggeration(scene);

        }

        private void DepopulateLayers()
        {
            foreach (Transform VARIABLE in LayersParent.transform)
            {
                Destroy(VARIABLE.gameObject);
            }
        }

        /// <summary>
        /// Creates a layer onto each scene that allows the user to change the exaggeration of the selected terrain.
        /// </summary>
        /// <param name="scene"></param>
        private void DynamicExaggeration(JMARSScene scene)
        {
            //creates exaggeration slider on layers menu after loading all layers
            var exagObj = gameObject;
            exagObj = ExagParent;
            //RESET EXAGGERATION BUTTON
            var exagReset = exagObj.GetComponentInChildren<Button>();

            //calculates scaledheight value in Unity "units" -- from SceneMaterializer 
            var exagSlider = exagObj.GetComponentInChildren<Slider>();

            var exag = scene.exaggeration.Split(", ")[0];
            var exaggeration = Convert.ToSingle(exag);

            SceneMaterializer.singleton.exaggerationSlider = exagSlider;

            //EXAGGERATION SLIDER VALUES -- based on scale values in inspector
            exagSlider.minValue = 1;
            exagSlider.maxValue = 5;
            exagSlider.value = 1;
            //exagSlider.wholeNumbers = true;
            //SceneMaterializer.singleton.heightMaterial.SetFloat("_scaleFactor", -(float)exaggeration * 0.001f * exagSlider.value); 
            //SceneMaterializer.singleton.terrain.transform.localScale = new Vector3(SceneMaterializer.singleton.terrain.transform.localScale.x, exagSlider.value, SceneMaterializer.singleton.terrain.transform.localScale.z);

            //EXAGGERATION VALUE TEXT FIELD
            var exagValue = exagObj.transform.GetChild(3).GetComponent<TextMeshProUGUI>();
            exagValue.text = "Exaggeration: " + (exagSlider.value);

            exagSlider.onValueChanged.AddListener(t =>
            {
                if (GameState.InMultiuser)
                {
                    LayerSync.singleton.SendUnnamedMessage("Exaggeration");
                    LayerSync.singleton.SendUnnamedMessage(exagSlider.value.ToString());
                }

                //print("Exaggeration Slider set to " + exagSlider.value);

                //SceneMaterializer.singleton.exaggerationSlider.value = exagSlider.value;
                //SceneMaterializer.singleton.heightMaterial.SetFloat("_scaleFactor", -(float)exaggeration * 0.001f * t); 
                //float heightValue = scene.depthTexture.GetPixel(scene.depthTexture.width/2, scene.depthTexture.height/2).r * SceneMaterializer.singleton.heightMaterial.GetFloat("_scaleFactor");
                //SceneMaterializer.singleton.tiles.transform.localPosition = new Vector3(0, -heightValue, 0);

                //original scaling is (200, 200, 200) - multiply current scale value by 200 to get approporiate values
                SceneMaterializer.singleton.terrain.transform.localScale = new Vector3(
                    SceneMaterializer.singleton.terrain.transform.localScale.x, t * 200,
                    SceneMaterializer.singleton.terrain.transform.localScale.z);

                //converts unity scaledheight back into height value to get accurate height in meters
                exagValue.text = "Exaggeration: " + (float)Math.Round(exagSlider.value, 2);
                InfoPanel.Panel.ChangeExaggeration((float)Math.Round(exagSlider.value, 2));

            });

            exagReset.onClick.AddListener(delegate
            {
                exagSlider.value = 1;
                SceneMaterializer.singleton.terrain.transform.localScale = new Vector3(
                    SceneMaterializer.singleton.terrain.transform.localScale.x, 200,
                    SceneMaterializer.singleton.terrain.transform.localScale.z);
                exagValue.text = "Exaggeration: " + (float)Math.Round(exagSlider.value, 2);
                InfoPanel.Panel.ChangeExaggeration((float)Math.Round(exagSlider.value, 2));

            });


        }
    }
}
