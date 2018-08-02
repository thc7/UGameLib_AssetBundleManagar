using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AssetBundleLoaderTest : MonoBehaviour {

    Dictionary<string, string> pb2abDic = new Dictionary<string, string>();

	void Start () {
        
        Libs.ManifestFileTools.ReadAssetsName2AssetBundleInDic("StreamingAssets_AssetBundleManagar_AssetsName2AssetBundleAll.txt", pb2abDic);

        string ab = pb2abDic["Cube"];
        Debug.Log(ab);

        Libs.ABM.I.LoadAssetBundleManifest(HandleOnLoadCmpCallBack);
	}

    void HandleOnLoadCmpCallBack()
    {
        Debug.Log("LoadAssetBundleManifest!");
  
    }

    GameObject goAsset;

	void OnABCmp(string name,AssetBundle ab)
    {
		string [] ns = ab.GetAllAssetNames ();
        goAsset = ab.LoadAsset<GameObject>(ns[0]);

        Debug.LogWarningFormat("已加载到 Resources 区:{0}",goAsset.name);
        //Instantiate (goAsset);
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    void OnGUI(){

        if (GUI.Button(new Rect(0,0,120,30)," Load cube"))
        {
            Libs.ABM.I.Load ("cube",OnABCmp);

            //Libs.ABM.I.Release("cube");
        }

        if (goAsset)
        {
            if (GUI.Button(new Rect(120, 0, 120, 30), " Release cubesd"))
            {
                Libs.ABM.I.Release("cubesd");
            }

            if (GUI.Button(new Rect(120 * 2, 0, 120, 30), " create cube"))
            {
                GameObject gameObjectNew = Instantiate(goAsset);
                gameObjectNew.transform.position = new Vector3(UnityEngine.Random.Range(-1f, 1f),
                                                               UnityEngine.Random.Range(-1f, 1f),
                                                               UnityEngine.Random.Range(-1f, 1f));

            }
        }

        if (GUI.Button(new Rect(0,30,120,30)," Release cube"))
        {
            Libs.ABM.I.Release("cube");

            GC.Collect();
        }

        if (GUI.Button(new Rect(0,30 * 2,120,30)," Load Sphere"))
        {
            Libs.ABM.I.Load ("Sphere",(string name, AssetBundle ab) => {
                
                string[] ns = ab.GetAllAssetNames();
                GameObject gameObjectAsset = ab.LoadAsset<GameObject>(ns[0]);

                Debug.LogWarningFormat("已加载到 Resources 区:{0}", gameObjectAsset.name);

                GameObject gameObjectNew = Instantiate(gameObjectAsset);
                gameObjectNew.transform.position = new Vector3(UnityEngine.Random.Range(-1f, 1f),
                                                               UnityEngine.Random.Range(-1f, 1f),
                                                               UnityEngine.Random.Range(-1f, 1f));
            });
        }

        if (GUI.Button(new Rect(0,30 * 3,120,30)," Release Sphere"))
        {
            Libs.ABM.I.Release("Sphere");
        }

        if (GUI.Button(new Rect(0, 30 * 4, 120, 30), " ClearCache "))
        {
            Libs.ABM.I.ClearCache();

            Resources.UnloadUnusedAssets();
        }

        if (GUI.Button(new Rect(0, 30 * 5, 120, 30), " CacheInfo "))
        {
            Libs.ABM.I.LogCacheInfo();
        }

    }
}
