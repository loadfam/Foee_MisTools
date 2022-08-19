using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using Excel;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using OfficeOpenXml;
using UnityEngine;
using UnityEngine.Serialization;
using FilePathAttribute = Sirenix.OdinInspector.FilePathAttribute;

/*using FileMode = System.IO.FileMode;*/


public class LocalizationTools_CSFile : OdinEditorWindow
{
    enum LocalizationScriptsMode
    {
        None,
        Log,
        ToExcel,
        ExcelToXml,
        ToJson,
        ReplaceKey,
    }

    public LocalizationTools_CSFile()
    {
        
    }
    [Button("设置默认路径", ButtonSizes.Large)]
    public void SetPath()
    {
        localizationCsPath = Application.dataPath + "/Scripts";
        localizationExcelPath = Application.dataPath + "/LocalizationFiles/Excel/LocalizationExcel.xlsx";
        localizationXmlPath = Application.dataPath + "/LocalizationFiles/Xml/LocalizationXml.xml";
    }

    [FormerlySerializedAs("localizationCSPath")] [LabelText("本地化脚本路径")] public string localizationCsPath;
    [LabelText("本地化Excel路径")] public string localizationExcelPath;
    [LabelText("本地化Xml路径")] public string localizationXmlPath;

    [Button("打印脚本中所有中文字符串", ButtonSizes.Large)]
    private void SetCsFiles_Log()
    {
        if (!Checkpath()) return;
        SetCsFiles(LocalizationScriptsMode.Log);
    }

    [Button("将脚本中所有中文导入Excel", ButtonSizes.Large)]
    private void SetCsFiles_ToExcel()
    {
        if (!Checkpath()) return;
        if (EditorUtility.DisplayDialog("CS脚本本地化", "是否将脚本中所有中文导入Excel", "是的", "关闭"))
            SetCsFiles(LocalizationScriptsMode.ToExcel);
    }
    
    [Button("将Excel导入Xml", ButtonSizes.Large)]
    private void SetCsFiles_ExcelToXml()
    {
        if (!Checkpath()) return;
        if(EditorUtility.DisplayDialog("CS脚本本地化", "是否将Excel导入Xml", "是的", "关闭"))
            SetCsFiles(LocalizationScriptsMode.ExcelToXml);
    }
    
    [Button("将CS脚本中的汉字替换成加载Key", ButtonSizes.Large)]
    private void SetCsFiles_ReplaceKey()
    {
        if (!Checkpath()) return;
        if(EditorUtility.DisplayDialog("CS脚本本地化", "是否将CS脚本中的汉字替换成加载Key", "是的", "关闭"))
            SetCsFiles(LocalizationScriptsMode.ReplaceKey);
    }

    private void SetCsFiles(LocalizationScriptsMode mode)
    {
        switch (mode)
        {
            case LocalizationScriptsMode.None:
                break;
            case LocalizationScriptsMode.Log:
                break;
            case LocalizationScriptsMode.ToExcel:
                break;
            case LocalizationScriptsMode.ExcelToXml:
                CreateXMLTable();
                return;
            case LocalizationScriptsMode.ToJson:
                break;
            case LocalizationScriptsMode.ReplaceKey:
                break;
        }
        bool isChangedInAllFile = false;
        var directoryInfo = new DirectoryInfo(localizationCsPath);
            var fileInfos = directoryInfo.GetFiles("*.cs", SearchOption.AllDirectories);
            List<LocalizationContent> localizationContents = new List<LocalizationContent>();
            foreach (var t in fileInfos)
            {
                bool isChangedFile = false;
                var filePath = t.DirectoryName + $"\\{t.Name}";
                var allstrs = File.ReadAllText(filePath);
                var iEData = File.ReadLines(filePath);
                foreach (var item in iEData)
                {
                    var strs = item.Split('\"');
                    for (int i = 0; i < strs.Length; i++)
                    {
                        if ((i + 1) % 2 == 0)
                        {
                            var strUnit = strs[i];
                            var lastStrIdx = i - 1;
                            if (lastStrIdx >= 0)
                            {
                                //不需要提取的中文 如:log中的
                                if (SkipSomeStr(strs[lastStrIdx]))
                                    continue;
                            }
                            if (HasChinese(strUnit))
                            {
                                switch (mode)
                                {
                                    case LocalizationScriptsMode.None:
                                        break;
                                    //打出中文log
                                    case LocalizationScriptsMode.Log:
                                        Debug.Log(strUnit);
                                        break;
                                    //转到excel表
                                    case LocalizationScriptsMode.ToExcel:
                                        var hashStr = strUnit.GetHashCode().ToString();
                                        //哈希值不相同的则加入到列表中
                                        if (localizationContents.Find(o => o.localizationKey == hashStr) == null)
                                        {
                                            localizationContents.Add(new LocalizationContent()
                                            {
                                                localizationKey = hashStr,
                                                localizationStrs = new Dictionary<EnumLanguage, string>()
                                                {
                                                    { EnumLanguage.Chinese, strUnit },
                                                    { EnumLanguage.English, "EnglishContent" }
                                                }
                                            });
                                        }
                                        break;
                                    case LocalizationScriptsMode.ToJson:
                                        break;
                                    //将CS脚本中的汉字替换成加载Key
                                    case LocalizationScriptsMode.ReplaceKey:
                                        var hashStr1 = strUnit.GetHashCode().ToString();
                                        allstrs = Regex.Replace(allstrs,$"\"{strUnit}\"",$"LocalizationMgr.Instance.LoadText(\"{hashStr1}\")");
                                        isChangedFile = true;
                                        isChangedInAllFile = true;
                                        break;
                                }
                            }
                        }
                    }
                }

                if (isChangedFile)
                {
                    File.WriteAllText(filePath, allstrs);
                }
                
            }

            if (isChangedInAllFile)
            {
                EditorUtility.DisplayDialog("CS脚本本地化", "CS脚本替换完成",  "好的");
            }
            
            if (localizationContents.Count > 0)
            {
                ExcelExport(localizationContents);
            }
    }


    /// <summary>
    /// 跳过一些字符串文本
    /// 如:Log相关的
    /// </summary>
    /// <returns></returns>
    private bool SkipSomeStr(string str)
    {
        return str.Contains("Log(") || str.Contains("LogError(") || str.Contains("LogWarning(");
    }

    /// <summary>
    /// 获取文件md5码
    /// </summary>
    /// <param name="fileName"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    private string GetMD5HashFromFile(string fileName)
    {
        try
        {
            FileStream file = new FileStream(fileName, FileMode.Open);
            System.Security.Cryptography.MD5 md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
            byte[] retVal = md5.ComputeHash(file);
            file.Close();

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < retVal.Length; i++)
            {
                sb.Append(retVal[i].ToString("x2"));
            }

            return sb.ToString();
        }
        catch (Exception ex)
        {
            throw new Exception("GetMD5HashFromFile() fail,error:" + ex.Message);
        }
    }


    /// <summary>
    /// 是否存在中文
    /// </summary>
    /// <param name="text"></param>
    /// <returns></returns>
    private bool HasChinese(string text)
    {
        char[] textArr = text.ToCharArray();
        for (int i = 0; i < textArr.Length; i++)
        {
            if (textArr[i] >= 0x4e00 && textArr[i] <= 0x9fbb)
            {
                return true;
            }
        }

        return false;
    }
    
    /// <summary>
    /// 导出excel表
    /// </summary>
    /// <param name="filepath"></param>
    public  void ExcelExport(List<LocalizationContent> contents)
    {
        string filepath = localizationExcelPath;
        FileInfo newFile = new FileInfo(filepath);
        using (ExcelPackage package = new ExcelPackage(newFile))
        {
            //添加sheet
            ExcelWorksheet worksheet = package.Workbook.Worksheets["sheet1"];
            var row = 2;
            for (int i = 0; i < contents.Count; i++)
            {
                LocalizationContent content = contents[i];
                worksheet.Cells[1, 1].Value = "LocalizationKey";
                worksheet.Cells[1, 2].Value = "Chinese";
                worksheet.Cells[1, 3].Value = "English";
                int col = 1;
                worksheet.Cells[row, 1].Value = content.localizationKey;
                foreach (var item in content.localizationStrs)
                {
                    col++;
                    worksheet.Cells[row, col].Value = item.Value;
                }

                row++;
            }

            worksheet.Cells.AutoFitColumns();
            package.Save();
            EditorUtility.DisplayDialog("CS脚本本地化", "Excel导入完成",  "好的");
        }
    }


    /// <summary>
    /// 读Excel
    /// </summary>
    /// <param name="ExcelPath"></param>
    /// <returns></returns>
    public (List<string[]>,string[]) ReadExcel()
    {
        string[] tableTop;
        List<string[]> tableList = new List<string[]>();
        //excel文件位置 /MaskGame/ReadExcel/excel文件名
        FileStream stream = File.Open(localizationExcelPath,
            FileMode.Open, FileAccess.Read);
        IExcelDataReader excelReader = ExcelReaderFactory.CreateOpenXmlReader(stream);

        DataSet result = excelReader.AsDataSet();

        int rows = result.Tables[0].Rows.Count; //获取行数(多少行信息)
        int columns = result.Tables[0].Columns.Count; //获取列数(多少列字段)

        //初始化字段
        tableTop = new string[columns];

        //存字段
        for (int i = 0; i < columns; i++)
        {
            tableTop[i] = result.Tables[0].Rows[0][i].ToString();
        }

        //从第二行开始读 读信息
        for (int i = 1; i < rows; i++)
        {
            //临时表
            string[] table = new string[columns];
            //赋值表信息
            for (int j = 0; j < columns; j++)
            {
                string nvalue = result.Tables[0].Rows[i][j].ToString();
                table[j] = nvalue;
            }

            //添加到List
            tableList.Add(table);
        }

        return (tableList,tableTop);
    }


    /// <summary>
    /// 创建xml表
    /// </summary>
    private void CreateXMLTable()
    {
        var (tableList,tableTop) = ReadExcel();
        if (tableList == null)
        {
            return;
        }
        //表头
        string xmlRoot = "FZW_MASK_XML_TABLE";
        //表名
        string xmlTabeName = "XMLTABLE";
        //路径
        string path = localizationXmlPath;

        //xml对象；
        XmlDocument xmll = new XmlDocument();
        //跟节点
        XmlElement Root = xmll.CreateElement(xmlRoot);

        for (int i = 0; i < tableList.Count; i++)
        {
            XmlElement xmlElement = xmll.CreateElement(xmlTabeName);
            xmlElement.SetAttribute(tableTop[0], tableList[i][0]);

            for (int j = 0; j < tableTop.Length - 1; j++)
            {
                XmlElement infoElement = xmll.CreateElement(tableTop[j + 1]);
                infoElement.InnerText = tableList[i][j + 1];
                xmlElement.AppendChild(infoElement);
            }

            Root.AppendChild(xmlElement);
        }

        xmll.AppendChild(Root);
        xmll.Save(path);
        xmll.Save(Application.dataPath+"/Resources/LocalizationXml.xml");
        EditorUtility.DisplayDialog("CS脚本本地化", "Xml导入完成",  "好的");
    }

    /// <summary>
    /// 检查路径
    /// </summary>
    /// <returns></returns>
    private bool Checkpath()
    {
        //路径错误
        /*if (!File.Exists(localizationCsPath))
        {
            EditorUtility.DisplayDialog("CS脚本本地化", "本地化脚本路径错误", "是的");
            return false;
        }*/
        if (!File.Exists(localizationExcelPath))
        {
            EditorUtility.DisplayDialog("CS脚本本地化", "本地化Excel路径路径错误", "是的");
            return false;
        }
        if (!File.Exists(localizationXmlPath))
        {
            EditorUtility.DisplayDialog("CS脚本本地化", "本地化Xml路径错误", "是的");
            return false;
        }

        return true;

    }

}