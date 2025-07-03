using UnityEngine;
using UnityEditor;

public class ReplaceBoxColliderIfNegativeScale
{
    [MenuItem("Tools/Replace BoxColliders on Negative Scale Objects")]
    static void ReplaceInvalidBoxColliders()
    {
        int replaced = 0;
        int logged = 0;
        int skipped = 0;

        foreach (BoxCollider box in GameObject.FindObjectsByType<BoxCollider>(FindObjectsSortMode.None))
        {
            Transform tf = box.transform;
            Vector3 worldScale = GetWorldScale(tf);

            if (worldScale.x < 0 || worldScale.y < 0 || worldScale.z < 0)
            {
                MeshFilter mf = tf.GetComponent<MeshFilter>();
                Renderer renderer = tf.GetComponent<Renderer>();

                if (mf != null && mf.sharedMesh != null)
                {
                    int triangleCount = mf.sharedMesh.triangles.Length / 3;
                    Object.DestroyImmediate(box);
                    MeshCollider meshCol = tf.gameObject.AddComponent<MeshCollider>();

                    if (triangleCount <= 255)
                    {
                        meshCol.convex = true;

                        if (!meshCol.convex)
                        {
                            Debug.LogWarning($"⚠️ Unity could not generate a convex MeshCollider for '{GetFullPath(tf)}'. Mesh name: '{mf.sharedMesh.name}'");
                            logged++;
                        }
                        else
                        {
                            replaced++;
                        }
                    }
                    else
                    {
                        // Check if object is static
                        Rigidbody rb = tf.GetComponent<Rigidbody>();
                        if (rb == null || rb.isKinematic)
                        {
                            meshCol.convex = false;
                            Debug.Log($"ℹ️ Replaced BoxCollider with non-convex MeshCollider on static object '{GetFullPath(tf)}'.");
                            replaced++;
                        }
                        else
                        {
                            Object.DestroyImmediate(meshCol);
                            Debug.LogWarning($"⚠️ Skipped '{GetFullPath(tf)}': Mesh '{mf.sharedMesh.name}' exceeds convex limit and object is dynamic.");
                            skipped++;
                        }
                    }
                }
                else
                {
                    if (renderer != null)
                    {
                        Debug.LogWarning($"⚠️ Cannot replace BoxCollider on '{GetFullPath(tf)}' — no MeshFilter or valid mesh found.");
                        logged++;
                    }
                }
            }
        }

        Debug.Log($"✅ Replaced {replaced} BoxColliders. Logged {logged} warnings. Skipped {skipped} dynamic high-poly meshes.");
    }

    static Vector3 GetWorldScale(Transform tf)
    {
        Vector3 scale = tf.localScale;
        Transform current = tf.parent;
        while (current != null)
        {
            scale = Vector3.Scale(scale, current.localScale);
            current = current.parent;
        }
        return scale;
    }

    static string GetFullPath(Transform tf)
    {
        return tf.parent == null ? tf.name : GetFullPath(tf.parent) + "/" + tf.name;
    }
}
