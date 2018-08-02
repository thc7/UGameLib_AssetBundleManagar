using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class DebugMeshInfo : Editor

{

    [MenuItem(@"Libs/DebugMesh/OutPutUV")]

    public static void OutPutUV()

    {

        Transform[] transforms = Selection.transforms;

        foreach (Transform transform in transforms)

        {

            //MeshRenderer meshRenderer = transform.GetComponent<MeshRenderer>();

            Mesh mesh = transform.GetComponent<MeshFilter>().sharedMesh;



            Vector2[] uv1 = mesh.uv;

            Vector3[] vertices = mesh.vertices;//顶点

            Vector3[] normals = mesh.normals; //法线

            Vector4[] tangents = mesh.tangents;//切线



            for (int i = 0; i < uv1.Length; i++)

            {

                Debug.LogWarning(uv1[i].ToString());

            }



            //Terrain terrain = transform.GetComponent<Terrain>();

            //terrain.terrainData.g

        }

    }



    [MenuItem(@"Libs/DebugMesh/OutPutnormals")]

    public static void OutPutnormals()

    {

        Transform[] transforms = Selection.transforms;

        foreach (Transform transform in transforms)

        {

            //MeshRenderer meshRenderer = transform.GetComponent<MeshRenderer>();

            Mesh mesh = transform.GetComponent<MeshFilter>().sharedMesh;



            Vector2[] uv1 = mesh.uv;

            Vector3[] vertices = mesh.vertices;//顶点

            Vector3[] normals = mesh.normals; //法线

            Vector4[] tangents = mesh.tangents;//切线



            for (int i = 0; i < normals.Length; i++)

            {

                Debug.LogWarning(normals[i].ToString());

            }



            //AssetDatabase.CreateAsset();



            //Terrain terrain = transform.GetComponent<Terrain>();

            //terrain.terrainData.g

        }

    }

}
