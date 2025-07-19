// Assets/Editor/AutoMatAndCollider.cs
using UnityEditor;
using UnityEngine;

public class AutoMatAndCollider : AssetPostprocessor
{
    const string k_DefaultMatPath = "Assets/Materials/DefaultGreybox.mat";

    // ① 控制“导入设置”阶段：勾选生成碰撞体
    void OnPreprocessModel()
    {
        var importer = assetImporter as ModelImporter;
        importer.addCollider = false;                        // 先关掉 Unity 内置的自动碰撞
    }

    // ② 模型生成完毕，批量布置材质 + 碰撞
    void OnPostprocessModel(GameObject root)
    {
        Material defaultMat = 
            AssetDatabase.LoadAssetAtPath<Material>(k_DefaultMatPath);

        // -----  批量匹配材质  -----
        foreach (var rend in root.GetComponentsInChildren<Renderer>(true))
        {
            // 按材质名直配：若存在同名材质则用之
            var slots = rend.sharedMaterials;
            for (int i = 0; i < slots.Length; i++)
            {
                if (slots[i] != null) continue;

                string search = rend.gameObject.name + "_" + i;
                slots[i] = FindMaterialByName(search) ?? defaultMat;
            }
            rend.sharedMaterials = slots;
        }

        // -----  自动碰撞体  -----
        foreach (var mf in root.GetComponentsInChildren<MeshFilter>(true))
        {
            var go = mf.gameObject;
            if (go.GetComponent<Collider>() != null) continue;

            /*if (go.isStatic)
            {
                var mc = go.AddComponent<MeshCollider>();
                mc.sharedMesh = mf.sharedMesh;
                mc.convex     = false;   // 静态，保留非凸更精确
            }
            else
            {
                go.AddComponent<BoxCollider>();              // 移动物体 → 用盒体
            }*/
            
            var mc = go.AddComponent<MeshCollider>();
            mc.sharedMesh = mf.sharedMesh;
            mc.convex     = false;   // 静态，保留非凸更精确
        }
        Debug.Log($"[AutoMatAndCollider] done: {root.name}");
    }

    // （可选）在工程里搜索同名材质
    static Material FindMaterialByName(string matName)
    {
        string[] guids = AssetDatabase.FindAssets(matName + " t:Material");
        if (guids.Length > 0)
            return AssetDatabase.LoadAssetAtPath<Material>(
                AssetDatabase.GUIDToAssetPath(guids[0]));
        return null;
    }
}
