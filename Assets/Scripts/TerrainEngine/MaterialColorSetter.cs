using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TerrainEngine{

	public class MaterialColorSetter : MonoBehaviour
	{
	public Renderer rend;

		[AddComponentMenu("Button")]
		public void SetMaterialColor(Color c){
			foreach (Material m in rend.materials){
				m.color = c;
			}
		}
	}

}