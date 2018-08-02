using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class SceneTools : Editor
{
    /// <summary>
    /// 复制所有Collider节点到ColliderRoot
    /// </summary>
    [MenuItem(@"Libs/SceneTools/ColliderToSingleRoot 复制所有Collider节点到ColliderRoot")]
    public static void ColliderToSingleRoot()
    {
        GameObject root = new UnityEngine.GameObject("ColliderRoot");

        Collider[] obj;
        obj = FindObjectsOfType(typeof(Collider)) as Collider[];
        for (int i = 0; i < obj.Length; i++)
        {
            Collider child = obj[i];
            Collider colliderNew = Instantiate(child, root.transform, true);

            Component[] componentArr = colliderNew.GetComponents<Component>();
            for (int j = 0; j < componentArr.Length; j++)
            {
                Component component = componentArr[j];

                if (!component.GetType().Equals(typeof(Transform)) &&
                    !colliderNew.Equals(component))
                {
                    DestroyImmediate(component);
                }

            }
        }
    }
    /// <summary>
    /// 生产所有阻挡到 BlockRoot
    /// </summary>
    [MenuItem(@"Libs/SceneTools/BlockToSingleRoot 生产所有阻挡到 BlockRoot")]
    public static void BlockToSingleRoot()
    {
        GameObject root = new UnityEngine.GameObject("BlockRoot");

        Collider[] obj;
        obj = FindObjectsOfType(typeof(Collider)) as Collider[];
        for (int i = 0; i < obj.Length; i++)
        {
            Collider child = obj[i];

            if (!child.tag.Equals("Block"))
            {
                continue;
            }

            Collider colliderNew = Instantiate(child, root.transform, true);

            Component[] componentArr = colliderNew.GetComponents<Component>();
            for (int j = 0; j < componentArr.Length; j++)
            {
                Component component = componentArr[j];

                if (!component.GetType().Equals(typeof(Transform)) &&
                    !colliderNew.Equals(component)
                    )
                {
                    DestroyImmediate(component);
                }

            }
        }
    }
}
