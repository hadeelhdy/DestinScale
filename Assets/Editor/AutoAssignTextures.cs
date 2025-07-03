using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

public class GenerateMaterialsFromTextures : EditorWindow
{
    [MenuItem("Tools/Auto Create Materials from Texture Folders")]
    static void CreateMaterials()
    {
        string baseTextureFolder = "Assets/Texture1";
        string[] subfolders = Directory.GetDirectories(baseTextureFolder, "*", SearchOption.AllDirectories);

        foreach (string subfolder in subfolders)
        {
            string[] textureFiles = Directory.GetFiles(subfolder);
            Dictionary<string, Texture> textureMap = new Dictionary<string, Texture>();

            string materialName = new DirectoryInfo(subfolder).Name;
            string matPath = Path.Combine(subfolder, materialName + ".mat").Replace("\\", "/");

            foreach (string file in textureFiles)
            {
                if (!file.EndsWith(".jpg") && !file.EndsWith(".png") && !file.EndsWith(".tif")) continue;
                if (file.ToLower().Contains("sphere")) continue;

                string lowerName = Path.GetFileName(file).ToLower();
                Texture tex = AssetDatabase.LoadAssetAtPath<Texture>(file.Replace("\\", "/"));

                if (lowerName.Contains("_col"))
                    textureMap["_MainTex"] = tex;
                else if (lowerName.Contains("_nrm"))
                    textureMap["_BumpMap"] = tex;
                else if (lowerName.Contains("_gloss") || lowerName.Contains("_rough"))
                    textureMap["_GlossMap"] = tex;
                else if (lowerName.Contains("_refl"))
                    textureMap["_MetallicGlossMap"] = tex;
            }

            // Create material and assign
            Material mat = new Material(Shader.Find("Standard"));
            mat.name = materialName;

            foreach (var pair in textureMap)
            {
                if (pair.Key == "_BumpMap")
                {
                    mat.EnableKeyword("_NORMALMAP");
                    Texture2D normalTex = pair.Value as Texture2D;
                    TextureImporter ti = (TextureImporter)AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(normalTex));
                    if (ti != null && ti.textureType != TextureImporterType.NormalMap)
                    {
                        ti.textureType = TextureImporterType.NormalMap;
                        ti.SaveAndReimport();
                    }
                }

                if (pair.Key == "_MetallicGlossMap") mat.EnableKeyword("_METALLICGLOSSMAP");

                mat.SetTexture(pair.Key, pair.Value);
            }

            AssetDatabase.CreateAsset(mat, matPath);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Materials created for texture folders.");
    }
}
