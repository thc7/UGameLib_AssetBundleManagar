using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
using System;

public class DebugSceneInfo : Editor
{
    public static void OutPut<T>()
    {
        T[] obj;
        obj = FindObjectsOfType(typeof(T)) as T[];
        foreach (T child in obj)
        {
            Debug.LogWarning(child.ToString());
        }
        Debug.LogError(typeof(T).ToString() + " count = " + obj.Length);
    }

    [MenuItem(@"Libs/DebugSceneInfo/GameObject")]
    public static void GameObject()
    {
        GameObject[] obj;
        obj = FindObjectsOfType(typeof(GameObject)) as GameObject[];
        //遍历所有gameobject
        foreach (GameObject child in obj)
        {
            Debug.LogWarning(child.name);
        }
        Debug.LogError("GameObject count = " + obj.Length);
    }

    [MenuItem(@"Libs/DebugSceneInfo/MeshRenderer")]
    public static void MeshRenderer()
    {
        MeshRenderer[] obj;
        obj = FindObjectsOfType(typeof(MeshRenderer)) as MeshRenderer[];
        //遍历所有gameobject
        foreach (MeshRenderer child in obj)
        {
            Debug.LogWarning(child.name);
        }
        Debug.LogError("MeshRenderer count = " + obj.Length);
    }

    [MenuItem(@"Libs/DebugSceneInfo/SkinnedMeshRenderer")]
    public static void SkinnedMeshRenderer()
    {
        OutPut<SkinnedMeshRenderer>();
    }

    [MenuItem(@"Libs/DebugSceneInfo/Collider")]
    public static void Collider()
    {
        OutPut<Collider>();
    }

    public static string GetGameObjectPath(GameObject obj)
    {
        string path = "/" + obj.name;
        while (obj.transform.parent != null)
        {
            obj = obj.transform.parent.gameObject;
            path = "/" + obj.name + path;
        }
        return path;
    }

    class Prefab2GameCount
    {
        public string path;
        // public int num;
        public List<GameObject> goArr = new List<UnityEngine.GameObject>();
        public int getNum() { return  goArr.Count; }
    }

    class Prefab2GameCountComparerAscending : IComparer<Prefab2GameCount>
    {
        int IComparer<Prefab2GameCount>.Compare(Prefab2GameCount x, Prefab2GameCount y)
        {
            return ((Prefab2GameCount)x).getNum() - ((Prefab2GameCount)y).getNum();  //升序
        }
    }
    class Prefab2GameCountComparerDecending : IComparer<Prefab2GameCount>
    {
        int IComparer<Prefab2GameCount>.Compare(Prefab2GameCount y, Prefab2GameCount x)
        {
            return ((Prefab2GameCount)x).getNum() - ((Prefab2GameCount)y).getNum();  //降序
        }
    }

    [MenuItem(@"Libs/DebugSceneInfo/Prefab")]
    public static void Prefab()
    {
        Dictionary<string, Prefab2GameCount> prefabPathDic = new Dictionary<string, Prefab2GameCount>();
        List<Prefab2GameCount> list = new List<Prefab2GameCount>();

        GameObject[] obj; 
        obj = FindObjectsOfType(typeof(GameObject)) as GameObject[];
        //遍历所有gameobject
        foreach (GameObject child in obj)
        {
            //判断GameObject是否为一个Prefab的引用
            if (PrefabUtility.GetPrefabType(child) == PrefabType.PrefabInstance ){
                
                //UnityEngine.Object parentObject = EditorUtility.GetPrefabParent(child);
                UnityEngine.Object parentObject = PrefabUtility.GetPrefabParent(child);
                string path = AssetDatabase.GetAssetPath(parentObject);  

                //判断GameObject的Prefab是否和右键选择的Prefab是同一路径。
                //if (path == AssetDatabase.GetAssetPath(Selection.activeGameObject))
                {
                    //输出场景名，以及Prefab引用的路径
                    //Debug.Log( GetGameObjectPath(child));
     
                    Prefab2GameCount prefab2GameCount = null;
                    prefabPathDic.TryGetValue(path,out prefab2GameCount);
                    if (prefab2GameCount == null) {
                        prefab2GameCount = new Prefab2GameCount();
                        prefab2GameCount.path = path;
                        prefab2GameCount.goArr.Add(child);
                        prefabPathDic.Add(path, prefab2GameCount);

                        list.Add(prefab2GameCount);
                    }
                    else {
                        prefab2GameCount.goArr.Add(child);
                    }
                }
            }
        }

        list.Sort(new Prefab2GameCountComparerDecending());

        System.Text.StringBuilder sb = new System.Text.StringBuilder();

        foreach (Prefab2GameCount prefab2GameCount in list)
        {
            sb.AppendLine("#region  Num = " + prefab2GameCount.getNum() + ",path = " + prefab2GameCount.path);
            sb.AppendLine("/*");
            sb.AppendLine("Start >>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>> Start >>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>");
            //sb.AppendLine("PrefabPath = " + prefab2GameCount.path + ", Num = " + prefab2GameCount.getNum());
            
            foreach (GameObject go in prefab2GameCount.goArr)
            {
                string line = "PrefabPath = " + prefab2GameCount.path + " <-- Scene = " + GetGameObjectPath(go);
                //sb.Append(line + "/n");
                sb.AppendLine(line);
                Debug.LogWarning(line);
            }

            sb.AppendLine("PrefabPath = " + prefab2GameCount.path + ", Num = " + prefab2GameCount.getNum());
            sb.AppendLine("*/");
            sb.AppendLine("#endregion");
            //sb.AppendLine("End >>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>> End >>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>");
        }
        /*
            //输出 prefab 引用信息
            foreach (string path in prefabPathDic.Keys) {
            List<GameObject> rlist = prefabPathDic[path];
            sb.AppendLine("Start >>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>> Start >>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>");
            sb.AppendLine("PrefabPath = " + path + ", Num = " + rlist.Count);
            foreach (GameObject go in rlist) {
                string line = "PrefabPath = " + path + " <-- Scene = " + GetGameObjectPath(go);
                //sb.Append(line + "/n");
                sb.AppendLine(line);
                Debug.LogWarning(line);
            }
            sb.AppendLine("PrefabPath = " + path + ", Num = " + rlist.Count);
            sb.AppendLine("End >>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>> End >>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>");
        }*/

        //保存
        FileStream fs1 = new FileStream(Application.dataPath + "/PrefabInfo.cs", FileMode.Create);
        //BinaryWriter bw = new BinaryWriter(fs1);
        StreamWriter sw = new StreamWriter(fs1);
        sw.Write(sb);
        sw.Close();
        fs1.Close();
 
        AssetDatabase.Refresh();
    }

}