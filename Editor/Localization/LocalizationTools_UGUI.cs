using System.Collections;
using System.Collections.Generic;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Text;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Text = UnityEngine.UI.Text;

public class LocalizationTools_UGUI :OdinEditorWindow
{
    [LabelText("UI预制体")]
    public List<GameObject> uiPrefabs = new List<GameObject>();

    [Button("翻译UI预制体")]
    private void TranslateUIPrefab()
    {
        for (int i = 0; i < uiPrefabs.Count; i++)
        {
            var uiObj = uiPrefabs[i];
            var texts= uiObj.GetComponentsInChildren<Text>();
            var tmps= uiObj.GetComponentsInChildren<TextMeshProUGUI>();
            for (int j = 0; j < texts.Length; j++)
            {
               
            }
        }
    }
}
