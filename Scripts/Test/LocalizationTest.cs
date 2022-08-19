using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LocalizationTest
{
    /// <summary>
    /// 测试1
    /// </summary>
    void Test1()
    {
        string str1 = "1e21";
        string str2 = LocalizationMgr.Instance.LoadText("-1495396232");
        string str3 = LocalizationMgr.Instance.LoadText("-939628698");
        string str4 = LocalizationMgr.Instance.LoadText("1793433216");
        
        Debug.Log("发给房");
    }

    private string strA = "adsasd";
    
    private string strA2 = LocalizationMgr.Instance.LoadText("-1085987848");
}
