using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text;
using System;

public class Packager {

    [MenuItem("Assets/1.Build 2.Manifest 3.Md5", false, 11)]
    public static void BuildiSceneAssetBundles()
    {
        string resPath = Application.dataPath + "/StreamingAssets/";
        BuildPipeline.BuildAssetBundles(resPath, BuildAssetBundleOptions.None, EditorUserBuildSettings.activeBuildTarget);

        CopyAssetBundleManifest();

        CreateStreamingAssetsMd5filelist();
    }

    [MenuItem("Assets/1.Build 2.Manifest 3.Md5 4.Copy", false, 11)]
    public static void BuildiManifestMd5Copy()
    {
        string resPath = Application.dataPath + "/StreamingAssets/";
        BuildPipeline.BuildAssetBundles(resPath, BuildAssetBundleOptions.None, EditorUserBuildSettings.activeBuildTarget);

        CopyAssetBundleManifest();

        CreateStreamingAssetsMd5filelist();

        Libs.LibsTools.CopyStreamingAssetsPathToOther();
    }

    [MenuItem("Assets/Build Android Resource", false, 11)]
    public static void BuildiSceneAssetAndroid()
    {
        string resPath = Application.dataPath + "/StreamingAssets/";
        BuildPipeline.BuildAssetBundles(resPath, BuildAssetBundleOptions.UncompressedAssetBundle ,
            BuildTarget.Android);
    }

    [MenuItem("Assets/Build IOS Resource", false, 11)]
    public static void BuildiSceneAssetIOS()
    {
        string resPath = Application.dataPath + "/StreamingAssets/";
        BuildPipeline.BuildAssetBundles(resPath, BuildAssetBundleOptions.UncompressedAssetBundle ,
            BuildTarget.iOS);
    }
    /// <summary>
    /// 拷贝一份依赖列表 以工程名命名
    /// </summary>
    [MenuItem("Assets/Build Copy StreamingAssets Manifest", false, 11)]
    public static void CopyAssetBundleManifest()
    {
        string prjName = Application.dataPath.Replace("/Assets", "");
        prjName = prjName.Substring(prjName.LastIndexOf("/") + 1);

        string toPath = Application.dataPath + "/StreamingAssets/StreamingAssets_" + prjName;
   
        if (File.Exists(toPath))
        {
            Debug.LogWarningFormat("已经存在 {0} " , toPath);
        }    
        File.Copy(Application.dataPath+"/StreamingAssets/StreamingAssets", toPath , true);
        //生成映射文件
        Libs.ManifestFileTools.CreateAssetsName2AssetBundle(toPath);

        AssetDatabase.Refresh();
    }
    /// <summary>
    /// Creates the streaming assets md5filelist.
    /// </summary>
    [MenuItem("Assets/Build Create StreamingAssets md5filelist", false, 11)]
    public static void CreateStreamingAssetsMd5filelist(){
        //string[] files = System.IO.Directory.GetFiles(Application.streamingAssetsPath, "*.*", SearchOption.AllDirectories);
        StringBuilder md5Str = new StringBuilder();
        md5Str.AppendLine(DateTime.Now.ToString("F", new System.Globalization.CultureInfo("zh-cn")) );

        MD5Path( Application.streamingAssetsPath ,md5Str );

        string listFilePath = Application.streamingAssetsPath + "/md5filelist.txt"; 

        if (File.Exists(listFilePath))
            File.Delete(listFilePath);

        FileStream fs =  File.Create(listFilePath);

        byte[] bytes = Encoding.GetEncoding("utf-8").GetBytes(md5Str.ToString());
        fs.Write(bytes,0,bytes.Length);
        fs.Flush();
        fs.Close();
    }

    public static void MD5Path(string path,StringBuilder md5Str){
        
        string[] files = System.IO.Directory.GetFiles(path, "*.*", SearchOption.AllDirectories);

        for(int i = 0; i< files.Length; i++){

            string file = files[i];
            if(file.EndsWith(".meta") || file.EndsWith(".DS_Store") ){
                continue;
            }
            md5Str.AppendLine( string.Format("{0}={1}", 
                               //file.Substring(file.LastIndexOf("/") + 1 ),
                               file.Replace(Application.streamingAssetsPath,""),
                               Md5Tools.md5file(file) ) );
        }
    }

}//end class 