using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml;

public class LocalizationMgr
{
    /// <summary>
    /// 本地化文本字典
    /// </summary>
    private Dictionary<string, string> LocalizatioContents;

    public static LocalizationMgr Instance
    {
        get { return _instance ?? (_instance = new LocalizationMgr()); }
    }

    private static LocalizationMgr _instance;

    /// <summary>
    /// 根据key加载文本
    /// </summary>
    /// <param name="key"></param>
    public string LoadText(string key)
    {
        return LocalizatioContents[key];
    }

    /// <summary>
    /// 根据语言初始化本地化文本字典
    /// </summary>
    /// <param name="language"></param>
    public void InitLocalizatioContents(EnumLanguage language)
    {
        LocalizatioContents = new Dictionary<string, string>();
        //XmlDocument读取xml文件
        var xmlDoc = new XmlDocument();
        xmlDoc.Load(Application.dataPath + "/LocalizationFiles/Xml/LocalizationXml.xml");
        //获取xml根节点
        XmlNode xmlRoot = xmlDoc.DocumentElement;
        if (xmlRoot == null) return;
        foreach (XmlNode xNode in xmlRoot)
        {
            //将节点转换为元素，便于得到节点的属性值
            var xe = (XmlElement)xNode;
            //加入到字典
            var key = xe.GetAttribute("LocalizationKey");
            if (!LocalizatioContents.ContainsKey(key))
            {
                LocalizatioContents.Add(key, xNode.SelectSingleNode($"{language}")?.InnerText);
            }
        }
    }
}