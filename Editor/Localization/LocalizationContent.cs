using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LocalizationContent
{
    /// <summary>
    /// 本地化键
    /// </summary>
    public string localizationKey;

    /// <summary>
    /// 本地化字符串
    /// </summary>
    public Dictionary<EnumLanguage, string> localizationStrs = new Dictionary<EnumLanguage, string>();
}