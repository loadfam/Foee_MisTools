using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

public class EditorTree : OdinMenuEditorWindow
{
    [MenuItem("Foee_Editor/LocalizationTools")]
    private static void OpenWindow()
    {
        GetWindow<EditorTree>().Show();
    }

    protected override OdinMenuTree BuildMenuTree()
    {
        var tree = new OdinMenuTree();
        tree.Selection.SupportsMultiSelect = false;
        /*tree.Add("Settings", GeneralDrawerConfig.Instance);
        tree.AddAllAssetsAtPath("Odin Settings", "Assets/Plugins/Sirenix", typeof(ScriptableObject), true, true);*/
        tree.Add("CS脚本本地化", ScriptableObject.CreateInstance<LocalizationTools_CSFile>());
        tree.Add("UGUI本地化", ScriptableObject.CreateInstance<LocalizationTools_UGUI>());
        tree.Add("Sqlite工具", ScriptableObject.CreateInstance<SqliteTools>());
        return tree;
    }

   
}
