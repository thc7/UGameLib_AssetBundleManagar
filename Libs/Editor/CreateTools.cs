using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class CreateTools  {

    [MenuItem("Assets/Create/BaseDir",false, 11)]
    static public void CreateBaseDir(){
        CreateResources();
        CreateStreamingAssets();
        CreateScript();
        CreateEditor();
        //CreatePlugins();
    }

    [MenuItem("Assets/Create/Resources",false, 11)]
    static public void CreateResources(){
        if (!FileTools.IsFolderExists("Resources"))
        {
            FileTools.CreateFolder("Resources");
        }
    }   

    [MenuItem("Assets/Create/StreamingAssets",false, 11)]
    static public void CreateStreamingAssets(){
        if (!FileTools.IsFolderExists("StreamingAssets"))
        {
            FileTools.CreateFolder("StreamingAssets");
        }
    }

    [MenuItem("Assets/Create/Script",false, 11)]
    static public void CreateScript(){
        if (!FileTools.IsFolderExists("Script"))
        {
            FileTools.CreateFolder("Script");
        }
    }

    [MenuItem("Assets/Create/Editor",false, 11)]
    static public void CreateEditor(){
        if (!FileTools.IsFolderExists("Editor"))
        {
            FileTools.CreateFolder("Editor");
        }
    }

    [MenuItem("Assets/Create/Plugins",false, 11)]
    static public void CreatePlugins(){
        if (!FileTools.IsFolderExists("Plugins"))
        {
            FileTools.CreateFolder("Plugins");
        }
        if (!FileTools.IsFolderExists("Plugins/Android"))
        {
            FileTools.CreateFolder("Plugins/Android");
        }
        if (!FileTools.IsFolderExists("Plugins/iOS"))
        {
            FileTools.CreateFolder("Plugins/iOS");
        }
    }
}
