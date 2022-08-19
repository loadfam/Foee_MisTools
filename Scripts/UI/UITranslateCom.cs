using System.Collections;
using System.Collections.Generic;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Text = UnityEngine.UI.Text;

public class UITranslateCom: MonoBehaviour
{
   /// <summary>
   /// text组件字典
   /// </summary>
   public Dictionary<string, Text> textComDic = new Dictionary<string, Text>();
   
   /// <summary>
   /// TextMeshPro组件字典
   /// </summary>
   public Dictionary<string, TextMeshProUGUI> tmpComDic = new Dictionary<string, TextMeshProUGUI>();
   
   
   /// <summary>
   /// 翻译ui组件
   /// </summary>
   public void TranslateUICom()
   {
      foreach (var item in textComDic)
      {
         item.Value.text = LocalizationMgr.Instance.LoadText(item.Key);
      }
      foreach (var item in tmpComDic)
      {
         item.Value.text = LocalizationMgr.Instance.LoadText(item.Key);
      }
   }
}
