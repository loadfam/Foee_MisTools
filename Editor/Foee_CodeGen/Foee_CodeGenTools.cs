using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public enum Member
{
    File,
}

public interface ICodeGen
{
    /// <summary>
    /// 访问类型
    /// </summary>
    public int AccessType { get; set; }

    /// <summary>
    /// 存储类型
    /// </summary>
    public int Type { get; set; }

    /// <summary>
    /// 名称
    /// </summary>
    public int Name { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public int Content { get; set; }
}


public static class Foee_CodeGenTools
{
    public static void CreateCsCodeFiles(string className, string filePath,List<string> content)
    {
        int tCount = 0;
        using (StreamWriter sw = new StreamWriter($"{filePath}/{className}.cs"))
        {
            for (int i = 0; i < content.Count; i++)
            {
                string tStr = string.Empty;
                if (content[i]=="}"||content[i]=="};")
                {
                    if (tCount > 0)
                    {
                        tCount--;
                    }
                }
                for (int j = 0; j < tCount; j++)
                {
                    tStr += "\t";
                }
                sw.WriteLine(tStr+content[i]);
                if (content[i]=="{")
                {
                    tCount++;
                }
                
                if (content[i]=="}"|| content[i].Contains(";"))
                {
                    sw.WriteLine("");
                }
               
              
            }
          
        }
        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("提示", $"{className}代码生成完毕", "OK");
    }
}