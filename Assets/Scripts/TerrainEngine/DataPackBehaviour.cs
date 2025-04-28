using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace TerrainEngine
{
    /// <summary>
    /// When added onto a gameObject, it downloads data from a JMARSScene
    /// </summary>
    public class DataPackBehaviour : MonoBehaviour
    {   
        public JMARSScene.DataPack dataPack;
        public bool autoLoad = false;

        public void Start()
        {
            if (autoLoad)
            {
                LoadData();
            }
        }
        //bool loaded = false;
        //private void Update()
        //{
        //    //if (Time.time > 2 && !loaded)
        //    //{
        //    //    loaded = true;
        //    //    LoadData();

        //    //}
        //}
        [NaughtyAttributes.Button]
        public void LoadData()
        {
            dataPack.LoadData();
        }
        
        /// <summary>
        /// For new sample terrains, this method converts raw dataTextAssets to byte dataTextAssets for local file storage
        /// </summary>
        [NaughtyAttributes.Button]
        public void LoadByteData()
        {
            try
            {
                foreach (string f in Directory.GetFiles(Application.dataPath +
                                                      $"/Resources/Sample Terrains/{gameObject.name}"))
                {
                    print(Path.GetExtension(f));
                    if (Path.GetExtension(f) == ".raw")
                    {
                        byte[] bytes = File.ReadAllBytes(f);
                        File.WriteAllBytes(Application.dataPath +$"/Resources/Sample Terrains/{gameObject.name}/{Path.GetFileNameWithoutExtension(f)}.txt", bytes);
                    }
                }
            }
            catch(Exception e)
            {
                print(e.ToString());
            }
            
        }
    }
}