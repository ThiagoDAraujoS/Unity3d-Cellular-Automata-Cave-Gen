///-----------------------------------------------------------------
///   Class:          Loader
///   Description:    It reads bool maps and transforms them into Cave Maps assets
///   Author:         Thiago de Araujo Silva  Date: 8/11/2016
///-----------------------------------------------------------------
using UnityEditor;
using UnityEngine;
using System.IO;

namespace CaveMapGenerator
{
	public class Loader
	{
		public void Export( bool[,] map, float[,] height )
		{
			CaveMap caveMap = ScriptableObject.CreateInstance<CaveMap>();
			caveMap.InitializeMap(map, height);

			string path = AssetDatabase.GetAssetPath(Selection.activeObject);
			if ( path == "" )
			{
				path = "Assets";
			} else if ( Path.GetExtension(path) != "" )
			{
				path = path.Replace(Path.GetFileName(AssetDatabase.GetAssetPath(Selection.activeObject)), "");
			}

			string assetPathAndName = AssetDatabase.GenerateUniqueAssetPath(path + "/New " + typeof(CaveMap).ToString() + ".asset");

			AssetDatabase.CreateAsset(caveMap, assetPathAndName);

			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();
			//   EditorUtility.FocusProjectWindow();
			Selection.activeObject = caveMap;
		}
	}
}