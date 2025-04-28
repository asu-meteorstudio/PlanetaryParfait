using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UserInterface;
using TerrainEngine.Tools;

namespace TerrainEngine
{

    /// <summary>
    /// ///  A JMARS scene is a collection of terrain data, such as layer information, images, and depth-maps.
    /// **DO NOT ADD/REMOVE CODE FROM THIS SCRIPT. This is structured with the same format as the JSON code we get from JMARS
    /// </summary>
    [Serializable]
    public class JMARSScene
    {
        public string name; // JMARS Scene name 
        public string body; // planet the terrain is from
        public string depth_img; // URL for the raw file (raw file: contains the height data for each pixel on the terrain map. Cannot parse through RAW file since we cant open it)
        public int index; // ** What does this mean???
        public Texture2D depthTexture; // after converting the RAW file to texture2D, this refers to the converted Texture2D object
        public RenderTexture layersTexture; // ** What does this mean??? 
        public string thumbnail_img; // the URL to the thumbnail we show on the terrain select button
        public Texture2D thumbnail_texture; // after converting thumbnail_img PNG file to texture2D, this refers to the converted Texture2D object
        public string exaggeration; // the exaggeration, which is converted to a float so we can use it
        public string dimension; // length*width*height (parsed by x)
        public string units; // meters, km, etc
        public string depth_data_type; // float, byte data
        public string starting_point; // ** What does this mean???
        public string last_mod_date; // **Not implemented. Represents the last time that the JMARS Scene was modified. Want to use for custom terrain query
        public string last_accessed_date; // **Not implemented. Represents the last time that the JMARS Scene was accessed. Want to use for custom terrain query
        public float heightOffset; // highest value in the heightmap 
        public float heightMapRange; // range of values calculated as (highest value - lowest value) 

        /// <summary>
        /// The lists of layers is currently non-reorderable because this version of Unity has BROKEN LISTS! 2020.3.f1
        /// </summary>
        [NonReorderable] public List<Layer> layers;

        /// <summary>
        /// Class that specifies a Layer object and pulls all layer data from the JSON 
        /// </summary>
        [Serializable]
        public class Layer
        {
            public string layer_name; // layer name from JMARS Scene
            public string graphic_img; // URL that points to the PNG of the current layer
            public Texture2D graphicTexture; // the converted Texture2D of the graphic_img
            public float transparency; // the alpha
            public string global_flag; // ** What does this mean???
            public string toggle_state; // If a layer is toggled on or off in the Unity scene
            public string layer_key; // ** What does this mean???
            public List<LayerData> layer_data; // IF this layer has data (per-pixel data), it will populate in this list
            public List<TimeSliderData> time_slider_data; // **Not implemented. Intended to show changes to terrains over time
            public Slider slider; 
            public Toggle toggle; // the toggle for the layer in Unity

            [Serializable]
            public class LayerData
            {
                public string numeric_flag; // either a 0 or 1. Represents if the data is numeric or nomenclature. True if numeric data, false if nomenclature
                public string numeric_data_img; // URL to the RAW file that contains the data for the numeric_data layer
                public string data_type; // what type of data the numeric data is formatted in (float, byte, short)
                [NonSerialized] public Texture2D numericDataTexture; // the converted texture from numeric_data_img
                public List<TextData> text_data; // the nomenclature data that we get from the JSON
                public string source_units; // the unit that the data layer is in (KM, meters, etc)
                public string source_name; // the name of the numeric_data layer
                
                /// <summary>
                /// This class represents a single nomenclature object with its name and (x,z) coordinates on the terrain
                /// </summary>
                [Serializable]
                public class TextData
                {
                    public string name;
                    public double x;
                    public double y; // this is z in Unity
                }
            }

            /// <summary>
            /// **Not implemented
            /// </summary>
            [Serializable]
            public class TimeSliderData
            {
                public int time_slider_img_index;
                public string time_slider_img_name;
            }
        }
        
        /// <summary>
        /// Downloaded when a user logs into their JMARS account. Used to populate the custom terrains menu with the metadata variables in this class.
        /// </summary>
        [Serializable]
        public class Metadata
        {
            public string scene_name;
            public string body;
            public string access_key; // lets us download the full JSON
            public string last_mod_date;
            public Texture2D thumbnail;
        }

        /// <summary>
        /// Used for sample terrains. This is how we load terrains offline by storing them locally 
        /// </summary>
        [Serializable]
        public class DataPack
        {
            public string jsonURL; //for syncing data across the network
            public TextAsset viewSceneJsonTextAsset; // refers to the entire JSON for that JMARS Scene. TextAsset is a textfile
            public string viewSceneJson; // string version of viewSceneJsonTextAsset
            public Texture2D[] textures; // list of textures associated with the JMARS Scene
            public List<TextAsset> layerData;
            public TextAsset heightDataTextAsset; // RAW file as a text asset containing the height data
            public string heightDataType; // float, string, byte, etc
            [HideInInspector] public byte[] heightData; // the RAW file stored into a byte array.
            
            /// <summary>
            /// Loads all of the data from the DataPack into a JMARSScene object
            /// </summary>
            public void LoadData()
            {
                LoadingBar.Loading(0.1f, "Starting Download");
                
                if (PerPixelDataReader.singleton != null)
                {
                    PerPixelDataReader.singleton.ClearPerPixelData();
                    NomenclatureDataReader.singleton.DeleteNomenclature();
                    SceneDownloader.singleton.nomenclature = null;
                }
                
                SceneDownloader.singleton.datalayertextures.Clear();
                SceneDownloader.singleton.dataLayers.Clear();

                if (viewSceneJson == "") viewSceneJson = viewSceneJsonTextAsset.text;
                if(jsonURL != "") SceneDownloader.singleton.terrainURL = jsonURL;
                
                heightData = heightDataTextAsset.bytes;
                ImportJMARSSceneDataFromDataPack(this);

            }
        }

        public static void ImportJMARSSceneDataFromDataPack(DataPack datapack)//, bool loadScreen)
        {
            JMARSScene currentScene = JsonConvert.DeserializeObject<JMARSScene>(datapack.viewSceneJson);
            datapack.heightDataType = currentScene.depth_data_type;
            SceneDownloader.singleton.imageWidth = datapack.textures[0].width;
            SceneDownloader.singleton.imageHeight = datapack.textures[0].height;
            SceneDownloader.singleton.scene = currentScene;
            
            LoadingBar.Loading(0.1f, "Configuring Height Data");
            
            //Interpret Height Data
            switch (currentScene.depth_data_type)
            {
                case "float": //32-bit float
                    currentScene.depthTexture = SceneDownloader.singleton.DataToHeightMapFloat(datapack.heightData, datapack.textures[0].width, datapack.textures[0].height);
                    break;
                case "short": //16-bit int
                    currentScene.depthTexture = SceneDownloader.singleton.DataToHeightMapShort(datapack.heightData, datapack.textures[0].width, datapack.textures[0].height);
                    break;
                default:
                    Debug.Log("Height map dataTextAssets type error!");
                    break;
            }
            
            // Interpret Layer Data
            int layerCount = 0; // count through datapack.textures to assign appropriate graphicTextures
            int layerDataCount = 0;
            
            if (currentScene.layers.Count >= 1)
            {
                foreach (var layer in currentScene.layers)
                {
                    if (layer.graphic_img != "") //if its not nomenclature
                    {
                        layer.graphicTexture = datapack.textures[layerCount];
                        layerCount++;
                        LoadingBar.Loading(0.5f / currentScene.layers.Count, "Loading Layers");
                    }

                    // if this layer has layer data, go through each data layer & convert the byte array to per-pixel data
                    if (layer.layer_data != null)
                    {
                        LoadingBar.Loading(0.2f / layer.layer_data.Count, "Loading Layer Data");
                        foreach (var data in layer.layer_data)
                        {
                            if (data.numeric_flag == "false") // nomenclature
                            {
                                SceneDownloader.singleton.nomenclature = data;
                            }
                            else
                            {
                                SceneDownloader.singleton.ConvertNumericData(data, datapack.layerData[layerDataCount].bytes);
                                layerDataCount++;
                            }
                            
                            data.numericDataTexture = SceneDownloader.singleton._texture;
                        }
                    }
                }
            }
     
            LoadingBar.Loading(0.1f, "Setting Materials");
            
            SceneDownloader.singleton.scene = currentScene;
            TerrainMenu.layersDelegate.Invoke(currentScene);
            SceneMaterializer.singleton.selectedScene = currentScene;
            SceneMaterializer.singleton.SetMaterials(currentScene);
            if(SceneDownloader.singleton.nomenclature != null) NomenclatureDataReader.singleton.InstantiateNomenclature(SceneDownloader.singleton.nomenclature);
            
            InfoPanel.Panel.UpdateInfo(currentScene);
            ScaleBar.singleton.CalculatePrefabs(currentScene);
            LoadingBar.DoneLoading();
            
        }
    }
}