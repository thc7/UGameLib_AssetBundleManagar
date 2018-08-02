using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class SaveAsset
{
    [UnityEditor.MenuItem("Libs/SaveAsset/Mesh")]
    static void SaveAssetMesh()
    {
        GameObject go = Selection.activeGameObject;

        if (go.GetComponent<MeshFilter>()) {

            Mesh mesh = go.GetComponent<MeshFilter>().sharedMesh;
            Mesh meshInstantiate = Object.Instantiate(mesh);

            string resourcesPath = string.Format("Assets/Resources/Mesh/{0}.asset", mesh.name);
            AssetDatabase.CreateAsset(meshInstantiate, resourcesPath);
        }

    }

    [UnityEditor.MenuItem("Libs/SaveAsset/AnimationClip")]
    static void SaveAnimationClip()
    {
        GameObject go = Selection.activeGameObject;

        if (!System.IO.Directory.Exists(Application.dataPath + "/Resources/AnimationClip"))
        {
            System.IO.Directory.CreateDirectory(Application.dataPath + "/Resources/AnimationClip");
        }

        if (go.GetComponent<Animation>()) {
            
            Animation animation = go.GetComponent<Animation>();

            foreach(AnimationState  animationState in animation ){

                AnimationClip animationClip = animation.GetClip( animationState.name );

                AnimationClip animationClipSave = Object.Instantiate(animationClip);

                string resourcesPath = string.Format("Assets/Resources/AnimationClip/{0}_{1}.asset", go.name ,animationClipSave.name);
                AssetDatabase.CreateAsset(animationClipSave, resourcesPath);
            }

        }
    }

}
