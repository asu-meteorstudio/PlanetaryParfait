using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Events;
using UserInterface;
using TerrainEngine.Tools;

namespace TerrainEngine
{

    /// <summary>
    /// LOAD-BEARING SCRIPT. Ran on start.
    /// </summary>
    /// <summary>
    /// The <c>DownloadScene</c> class is responsible for consuming the JMars Scene API. Refer to the JMars API document for
    /// more information on about exactly a <c>Scene</c> is. However, you can view the internal definition at the bottom of
    /// this class. <c>DownloadScene</c> uses Unity's WebRequest to download the JSON data from the endpoints and deserialize
    /// it into C# classes for later use.
    /// </summary>
    public class SceneDownloader : MonoBehaviour
    {
        public List<Texture2D> datalayertextures;
        public int imageWidth;
        public int imageHeight;
        public JMARSScene.Layer.LayerData nomenclature;

        #region FIELDS

        public static SceneDownloader singleton;

        /// <value>
        /// <c>UserScene</c> is a class defined for the <c>viewList</c> endpoint which contains data of all of a user's scenes.
        /// This list of userScene represents all of the data found on that endpoint.
        /// </value>
        public List<JMARSScene.Metadata> userScenes;

        /// <value>
        /// This field contains the object of the scene downloaded from the <c>viewScene</c> endpoint. This scene is downloaded
        /// based on which scene button was selected from the main menu in game.
        /// </value>
        public JMARSScene scene;
        public string guid; //For Multi-Player Pins


        public string terrainURL = "";

        public List<string> dataLayers;
        //public List<Scene> scenes;

        /// <summary>
        /// These fields are temporary data for the downloaded JSON from each endpoint.
        /// </summary>
        private string _viewListJson;
        private string _viewSceneJson;
        private Texture2D _thumbnailImg;
        [HideInInspector] public float[] heightData;

        [HideInInspector] public Texture2D _texture;
        [HideInInspector] public float _heightOffset; //95th percentile of height
        [HideInInspector] public float _heightRange; //range from bottom to top of heightmap values
        #endregion

        #region STATEMACHINE

        public static SceneSession currentState;
        [HideInInspector] public UnityEvent stateChanged;

        /// <summary>
        /// All states for the <c>SceneSession</c> state machine.
        /// </summary>
        public enum SceneSession{
            INITIAL, LISTSCENES, DOWNLOADING, READY, DONE
        }
        

        /// <summary>
        /// Called every time a state change needs to be made. <c>stateChanged</c> event invoked once state has changed. 
        /// </summary>
        /// <param name="state"></param>
        /// <returns></returns>
        public IEnumerator ChangeState(SceneSession state)
        {
            currentState = state;
            //Debug.Log("state = " + currentState);
            stateChanged.Invoke();

            switch (state){
                case SceneSession.INITIAL:
                    break;
                case SceneSession.LISTSCENES:
                    //yield return StartCoroutine(DownloadViewList(NewUIScript.singleton.username.text, NewUIScript.singleton.password.text));
                    break;
                case SceneSession.DOWNLOADING:
                    yield return StartCoroutine(DownloadJMARSSceneData(terrainURL));
                    StartCoroutine(ChangeState(SceneSession.READY));
                    break;
                case SceneSession.READY:
                    SceneMaterializer.singleton.SetMaterials(scene);
                    SceneMaterializer.singleton.selectedScene = scene;
                    if(nomenclature != null) NomenclatureDataReader.singleton.InstantiateNomenclature(nomenclature);
                    
                    InfoPanel.Panel.UpdateInfo(scene);
                    ScaleBar.singleton.CalculatePrefabs(scene);
                    MainMenu.OpenPrimaryMenus(false);
                    break;
                case SceneSession.DONE:
                    break;
            }

        }

        #endregion


        #region MONO

        private void Awake()
        {
            singleton = this;
            //Every player is spawned with a unique GUID at the start of game
            //These guids can be used for specifying multiplayer players easily via RPCs
            guid = Guid.NewGuid().ToString(); 

        }
        #endregion


        #region METHODS
        /// <summary>
        /// Tests to make sure that username and password exists in JMARS. Logs in user and downloads all custom terrains if successful, 
        /// throws false username/password error otherwise. 
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public IEnumerator DownloadViewList(string username, string password)
        {
            // Download json for list of scenes
            var url = "http://cm.mars.asu.edu/api/vr/viewList.php";
            var form = new WWWForm();
            form.AddField("userid", username);
            form.AddField("passwd", password);

            using var webRequest = UnityWebRequest.Post(url, form);
            var downloadHandler = webRequest.downloadHandler;
            yield return webRequest.SendWebRequest();

            if (webRequest.result != UnityWebRequest.Result.Success)
            {
                Login.FailedLogin.Invoke(webRequest.result);
                StartCoroutine(ChangeState(SceneSession.INITIAL));
            }
            else
            {
                _viewListJson = downloadHandler.text;
            
                //populates a list of user scenes with all relevant metadata
                userScenes = JsonConvert.DeserializeObject<List<JMARSScene.Metadata>>(_viewListJson);

                foreach (JMARSScene.Metadata m in userScenes)
                {
                    LoadingBar.Loading(1f / userScenes.Count, "Downloading User Scenes");
                    yield return StartCoroutine(GetThumbnailData(m));
                    m.thumbnail = _thumbnailImg;
                }
                
                StartCoroutine(ChangeState(SceneSession.DONE));
                TerrainMenu.sceneDelegate.Invoke(userScenes);   
            }
        }

        /// <summary>
        /// <c>DownloadJMARSSceneData</c> is a coroutine used for downloading the data from the <c>viewScene</c> api endpoint.
        /// </summary>
        /// <returns></returns>
        public IEnumerator DownloadJMARSSceneData(string jsonURL)//, bool loadScreen)
        {
            datalayertextures.Clear();
            dataLayers.Clear();
            PerPixelDataReader.singleton.ClearPerPixelData();
            NomenclatureDataReader.singleton.DeleteNomenclature();
            nomenclature = null;
            //terrainURL = ""; //resets terrain URL
            
            // Download json for individual scene
            int width = 0;
            int height = 0;

            LoadingBar.Loading(0.0f, "Downloading terrain from JMARS");
            var downloadHandlerBuffer = new DownloadHandlerBuffer();
            using var webRequest = new UnityWebRequest(terrainURL, "GET", downloadHandlerBuffer, null);
            
            LoadingBar.Loading(0.1f, "Sending web request for terrain data");
            yield return webRequest.SendWebRequest();
            _viewSceneJson = downloadHandlerBuffer.text;
            var currentScene = JsonConvert.DeserializeObject<JMARSScene>(_viewSceneJson);
            
            //rawDataLocation = currentScene.depth_img.Replace(".tif", ".raw");

            //Gets image resolution
            if (currentScene.layers.Count >= 1)
            {
                //Nomenclature layers do not have a graphic image, thus cannot be used to find the image resolution
                //Search through each layer until a layer with a graphic image is found and use that image's resolution
                foreach (var layer in currentScene.layers)
                {
                    if (layer.graphic_img != "")
                    {
                        yield return StartCoroutine(DownloadTexture(layer.graphic_img));
                        width = _texture.width;
                        height = _texture.height;
                        imageWidth = _texture.width;
                        imageHeight = _texture.height;
                        break; //only want one image to get resolution
                    }
                }
            }

            if (currentScene != null)
            {
                yield return StartCoroutine(DownloadRawData(currentScene.depth_img, width, height, currentScene.depth_data_type));
                currentScene.depthTexture = _texture;

                //populates layer data into layer list
                foreach (var layer in currentScene.layers)
                {
                    if (layer.graphic_img != "")
                    {
                        yield return StartCoroutine(DownloadTexture(layer.graphic_img));
                        LoadingBar.Loading(0.5f/currentScene.layers.Count, "Downloading Layers");
                        layer.graphicTexture = _texture;
                    }

                    if(layer.layer_data != null)
                    {
                        foreach(var data in layer.layer_data)
                        {
                            yield return StartCoroutine(DownloadNumericData(layer));
                            LoadingBar.Loading(0.3f/layer.layer_data.Count, "Downloading Layer Data");
                            data.numericDataTexture = _texture;
                        }
                    }
                }

                scene = currentScene; // makes the JMARS scene object publicly visible
                TerrainMenu.layersDelegate.Invoke(scene);
                LoadingBar.DoneLoading();
            }
        }
        
        /// <summary>
        /// Formats terrain height data based on scene datatype. 
        /// </summary>
        /// <param name="url"></param>
        /// <param name="width">Height Texture Width</param>
        /// <param name="height">Height Texture Height</param>
        /// <param name="dataType"></param>
        /// <returns></returns>
        public IEnumerator DownloadRawData(string url, int width, int height, string dataType)
        {
            LoadingBar.Loading(0.01f, "Sending web request for height data");
            using var webRequest = UnityWebRequest.Get(url);
            yield return webRequest.SendWebRequest();

            if (webRequest.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError(webRequest.error);
                StopAllCoroutines();
                MultiuserMenu.Instance.ErrorMessage("Web Request Error", "Web request to download height data failed. Please try again");
            }
            else
            {
                LoadingBar.Loading(0.04f, "Downloading Height Data");
                var downloadHandler = webRequest.downloadHandler.data; //converts .raw file to a byte array

                LoadingBar.Loading(0.05f, "Configuring Height Data");
                switch (dataType)
                {
                    case "float": //32-bit float
                        DataToHeightMapFloat(downloadHandler, width, height);
                        break;
                    case "short": //16-bit int
                        DataToHeightMapShort(downloadHandler, width, height);
                        break;
                    default:
                        Debug.LogError("Height map data type error!");
                        break;
                }
            }
        }

        public void ConvertNumericData(JMARSScene.Layer.LayerData data, byte[] byte_array)
        {
            Texture2D newDataTexture = new Texture2D(imageWidth, imageHeight);
            float max = 0;
            float min = 0;
            int difference = 0;
                    
            dataLayers.Add(data.source_name);
            
            switch (data.data_type)
            {
                case "float":
                    //print("Adding data layer " + data.source_name + "with data format " + data.data_type);

                    float[] float_layer_data = new float[byte_array.Length / 4];

                    for (int i = 0; i < byte_array.Length; i += 4)
                    {
                        float converted_data = BitConverter.ToSingle(byte_array, i);
                        float_layer_data[i / 4] = converted_data;
                    }

                    if (GameState.generateDataImages)
                    {

                        max = float_layer_data.Max();
                        min = float_layer_data.Min();
                        difference = Convert.ToInt32(max - min);

                        //print("max = " + max);
                        for (int x = 1; x < imageWidth; x++)
                        {
                            for (int y = 1; y < imageHeight; y++)
                            {
                                float value = (float)(float_layer_data[y * imageWidth + x] - min) / difference;
                                newDataTexture.SetPixel(x, y,
                                    new Color(value,
                                        value,
                                        value));
                            }
                        }

                        newDataTexture.Apply();
                        datalayertextures.Add(newDataTexture);
                    }

                    PerPixelDataReader.floatArrays.Add(float_layer_data);
                    PerPixelDataReader.floatDataNames.Add(data.source_name);
                    PerPixelDataReader.floatDataUnits.Add(data.source_units);
                    break;
                
                case "short":
                    byte[] newshort = new byte[byte_array.Length];

                    short[] short_layer_data = new short[byte_array.Length / 2];

                    for(int i = 0; i < byte_array.Length; i += 2)
                    {
                        short converted_data = BitConverter.ToInt16(byte_array, i);
                        short_layer_data[i / 2] = converted_data;
                    }

                    if (GameState.generateDataImages)
                    {
                        max = short_layer_data.Max();
                        min = short_layer_data.Min();
                        difference = Convert.ToInt32(max - min);
                        for (int x = 1; x < imageWidth; x++)
                        {
                            for (int y = 1; y < imageHeight; y++)
                            {
                                float value = (float)(short_layer_data[y * imageWidth + x] - min) / difference;
                                newDataTexture.SetPixel(x, y,
                                    new Color(value,
                                        value,
                                        value));
                            }
                        }

                        newDataTexture.Apply();
                        datalayertextures.Add(newDataTexture);
                    }

                    PerPixelDataReader.shortArrays.Add(short_layer_data);
                    PerPixelDataReader.shortDataNames.Add(data.source_name);
                    PerPixelDataReader.shortDataUnits.Add(data.source_units);

                    break;
                
                case "int":
                    int[] int_layer_data = new int[byte_array.Length / 4];

                    for (int i = 0; i < byte_array.Length; i += 4)
                    {
                        int converted_data = BitConverter.ToInt32(byte_array, i);
                        int_layer_data[i / 4] = converted_data;
                    }

                    if (GameState.generateDataImages)
                    {
                        max = int_layer_data.Max();
                        min = int_layer_data.Min();
                        difference = Convert.ToInt32(max - min);

                        for (int x = 0; x < imageWidth; x++)
                        {
                            for (int y = 0; y < imageHeight; y++)
                            {
                                float value = (float)(int_layer_data[y * imageWidth + x] - min) / difference;
                                newDataTexture.SetPixel(x, y,
                                    new Color(value,
                                        value,
                                        value));
                            }
                        }

                        newDataTexture.Apply();

                        datalayertextures.Add(newDataTexture);
                    }

                    PerPixelDataReader.intArrays.Add(int_layer_data);
                    PerPixelDataReader.intDataNames.Add(data.source_name);
                    PerPixelDataReader.intDataUnits.Add(data.source_units);
                    break;
                
                case "byte":
                    PerPixelDataReader.byteArrays.Add(byte_array);
                    PerPixelDataReader.byteDataNames.Add(data.source_name);
                    PerPixelDataReader.byteDataUnits.Add(data.source_units);
                    break;
                default:
                    Debug.LogError("Numeric Data Error");
                    break;
            }
        }

        public IEnumerator DownloadNumericData(JMARSScene.Layer layer, byte[] byte_array = null)
        {
            foreach (var data in layer.layer_data)
            {
                if(data.numeric_flag == "true")
                {
                    //Custom Terrain Data
                    if(byte_array == null) 
                    {
                        using var webRequest = UnityWebRequest.Get(data.numeric_data_img);
                        yield return webRequest.SendWebRequest();
                        if (webRequest.result != UnityWebRequest.Result.Success)
                        {
                            Debug.LogError(webRequest.error);
                            StopAllCoroutines();
                            MultiuserMenu.Instance.ErrorMessage("Web Request Error", "Web request to access numeric data failed. Please try again");
                            break;
                        }
                        
                        byte_array = webRequest.downloadHandler.data;
                    }
                    
                    ConvertNumericData(data, byte_array);
                }
                else if(data.numeric_flag == "false") // nomenclature
                {
                    nomenclature = data;
                }
                else
                {
                    Debug.LogError("Numeric Data Flag Error");
                }
            }
        }
        
        

        /// <summary>
        /// Convert byte data for height map into a 2D texture
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public Texture2D DataToHeightMapFloat(byte[] bytes, int width, int height)
        {
            _texture = new Texture2D(width, height, TextureFormat.RFloat, false);
            byte[] floatByteArray = new byte[bytes.Length];

            //Reverse the order, including swizzle (0,1,2,3) to (3,2,1,0)
            for (int i = 0; i < (bytes.Length / 4); i++)
            {
                floatByteArray[(floatByteArray.Length - 1) - (4 * i + 0)] = bytes[4 * i + 3];
                floatByteArray[(floatByteArray.Length - 1) - (4 * i + 1)] = bytes[4 * i + 2];
                floatByteArray[(floatByteArray.Length - 1) - (4 * i + 2)] = bytes[4 * i + 1];
                floatByteArray[(floatByteArray.Length - 1) - (4 * i + 3)] = bytes[4 * i + 0];
            }

            byte[] newnewpixels = new byte[floatByteArray.Length];

            //Flip horizontally
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    newnewpixels[(y * width + x) * 4] = floatByteArray[(y * width + (width - 1 - x)) * 4];
                    newnewpixels[(y * width + x) * 4 + 1] = floatByteArray[(y * width + (width - 1 - x)) * 4 + 1];
                    newnewpixels[(y * width + x) * 4 + 2] = floatByteArray[(y * width + (width - 1 - x)) * 4 + 2];
                    newnewpixels[(y * width + x) * 4 + 3] = floatByteArray[(y * width + (width - 1 - x)) * 4 + 3];
                }
            }

            
            var floatArray = new float[newnewpixels.Length / 4];
            Buffer.BlockCopy(newnewpixels, 0, floatArray, 0, newnewpixels.Length);

            var max = floatArray.Max();
            var min = floatArray.Min();
            var mid = (max + min) / 2;

            for (int i = 0; i < floatArray.Length; i++)
            {
                floatArray[i] += -mid;
            }

            heightData = floatArray;
            Buffer.BlockCopy(floatArray, 0, newnewpixels, 0, newnewpixels.Length);

            _texture.LoadRawTextureData(newnewpixels);
            _texture.Apply();
            return _texture;
        }

        /// <summary>
        /// Convert byte data for height map into a 2D texture 
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public Texture2D DataToHeightMapShort(byte[] bytes, int width, int height)
        {
            byte[] shortByteArray = new byte[bytes.Length];
            
            //Swizzle(0, 1, 2, 3) to(3, 2, 1, 0)
            for (int i = 0; i < (shortByteArray.Length / 2); i++)
            {
                shortByteArray[(shortByteArray.Length - 1) - (2 * i + 1)] = bytes[2 * i + 0];
                shortByteArray[(shortByteArray.Length - 1) - (2 * i + 0)] = bytes[2 * i + 1];
            }

            byte[] newnewpixels = new byte[shortByteArray.Length];

            //Flip horizontally
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    newnewpixels[(y * width + x) * 2] = shortByteArray[(y * width + (width - 1 - x)) * 2];
                    newnewpixels[(y * width + x) * 2 + 1] = shortByteArray[(y * width + (width - 1 - x)) * 2 + 1];
                }
            }
            
            short[] shortArray = new short[shortByteArray.Length / 2];
            Buffer.BlockCopy(newnewpixels, 0, shortArray, 0, newnewpixels.Length);

            float[] floatArray = new float[shortArray.Length];

            for (int i = 0; i < shortArray.Length; i++)
            {
                floatArray[i] = shortArray[i];
            }

            var max = floatArray.Max();
            var min = floatArray.Min();
            var mid = (max + min) / 2;

            for (int i = 0; i < floatArray.Length; i++)
            {
                floatArray[i] += -mid;
            }

            heightData = floatArray;
            
            byte[] floatByteArray = new byte[floatArray.Length * 4];
            Buffer.BlockCopy(floatArray, 0, floatByteArray, 0, floatByteArray.Length);

            _texture = new Texture2D(width, height, TextureFormat.RFloat, false);
            _texture.LoadRawTextureData(floatByteArray);
            _texture.Apply();
            return _texture;
        }

        /// <summary>
        /// Sets <c>_texture</c> after downloading png texture from the api endpoint
        /// </summary>
        /// <param name="url">Depth Image url</param>
        /// <returns></returns>
        public IEnumerator DownloadTexture(string url)
        {
            using var webRequest = UnityWebRequestTexture.GetTexture(url);
            yield return webRequest.SendWebRequest();

            if (webRequest.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError(webRequest.error);
                StopAllCoroutines();
                MultiuserMenu.Instance.ErrorMessage("Web Request Error", "Web request to access texture data failed. Please try again");
            }
            else
            {
                var downloadHandlerTexture = webRequest.downloadHandler as DownloadHandlerTexture;
                _texture = downloadHandlerTexture.texture;
            }
        }

        public IEnumerator GetThumbnailData(JMARSScene.Metadata scene)
        {
            var url = "https://cm.mars.asu.edu/api/vr/viewScene.php?access_key=" + scene.access_key;
            using var webRequest = UnityWebRequest.Get(url);
            yield return webRequest.SendWebRequest();

            var downloadHandler = webRequest.downloadHandler;
            _viewSceneJson = downloadHandler.text;
            var currentScene = JsonConvert.DeserializeObject<JMARSScene>(_viewSceneJson); //get scene data
            
            // download thumbnail image
            string thumbnailUrl = currentScene.thumbnail_img;
            var thumbnailDownloadHandler = new DownloadHandlerTexture();
            var req = new UnityWebRequest(thumbnailUrl, "GET", thumbnailDownloadHandler, null);
            yield return req.SendWebRequest();

            _thumbnailImg = thumbnailDownloadHandler.texture;
        }
        
        void DebugShowNegativeValues(Texture2D texture)
        {
            string pixelstring = "";
            int count = 0;
            Color[] colors = texture.GetPixels();
            for (int i = 0; i < colors.Length; i++)
            {
                if (colors[i].r < 0) { 
                    pixelstring += colors[i].r + ",";
                    count++;
                    if (count > 20)
                    {
                        break;
                    }
                }
            }
        }
        void DebugShowStringOfValues(int n, Texture2D texture)
        {
            string pixelstring = "";
            Color[] colors = texture.GetPixels();
            if (n >= 0)
            {
                for (int i = 0; i < n; i++)
                {
                    pixelstring += colors[i].r + ",";
                }
            }
            else
            {
                for (int i = 0; i < -n; i++)
                {
                    pixelstring += colors[colors.Length-1-i].r + ",";
                }
            }
        }
        #endregion
        
        #region GETTERS
        public Texture2D GetTexture()
        {
            return _texture;
        }

        public float GetOffset()
        {
            return _heightOffset;
        }

        public float GetHeightMapRange()
        {
            return _heightRange;
        }
        #endregion

    }

}