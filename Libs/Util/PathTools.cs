using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathTools {
	
	static public string UpdateRoot = "";
    /// <summary>
    /// Gets the asset path for load path.
    /// AssetBundle.LoadFromFile(filePath);
    /// 方法不能加 "file://" 
    /// </summary>
    /// <returns>The asset path for load path.</returns>
    /// <param name="assetName">Asset name.</param>
    static public string GetAssetPathForLoadPath(string assetName){
        string path = System.IO.Path.Combine (PersistentDataPath(), assetName);
        if (System.IO.File.Exists (path))
            return path;
        else
            return System.IO.Path.Combine (AppContentPath ().Replace("file://",""), assetName);
    }
    /// <summary>
    /// 针对 WWW www = new WWW(PathTools.GetAssetPath(name));
    /// </summary>
    /// <returns>The asset path.</returns>
    /// <param name="assetName">Asset name.</param>
	static public string GetAssetPath(string assetName){
		string path = System.IO.Path.Combine (PersistentDataPath(), assetName);
		if (System.IO.File.Exists (path))
            return "file://" + path;
		else
			return System.IO.Path.Combine (AppContentPath (), assetName);
	}
        
	// Use this for initialization

	static public string PersistentDataPath(){
		#if UNITY_EDITOR
		//return "file://"+Application.dataPath + "/StreamingAssets/";
        return Application.streamingAssetsPath;
		#else
		return Application.persistentDataPath;
		#endif
	}

	static public string AppContentPath()
	{
		string path = string.Empty;
		switch (Application.platform)
		{
		case RuntimePlatform.Android:
			path = "jar:file://" + Application.dataPath + "!/assets/";
			break;
		case RuntimePlatform.IPhonePlayer:
			path = Application.dataPath + "/Raw/";
			break;
		default:
			path = "file://"+Application.dataPath + "/StreamingAssets/";
			break;
		}
		return path;
	}

}
