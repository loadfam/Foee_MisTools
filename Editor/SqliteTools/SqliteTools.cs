using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using Excel;
using Mono.Data.Sqlite;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using SqliteCfg;
using UnityEditor;
using UnityEngine;

public class SqliteTools : OdinEditorWindow
{
    /// <summary>
    /// 创建sqlite Db
    /// </summary>
    /// <param name="dbName"></param>
    [Button("创建Db", ButtonSizes.Large)]
    private void CreateDb(string dbName)
    {
        string path = Application.dataPath + "/SqliteFiles/";
        DbAccess db = new DbAccess(@"data source=" + path + dbName + ".db");
        var (nameTable, csTypeTable, sqlTypeTable, tableList) = ReadExcel();
        db.CreateTable("Test", nameTable, sqlTypeTable);
        for (int i = 0; i < tableList.Count; i++)
        {
            var cloData = tableList[i];
            db.InsertInto("Test", cloData);
        }

        db.CloseSqlConnection();

        GenAutoCreateCode1(db, dbName, path, nameTable, csTypeTable);
    }

    /// <summary>
    /// 读取Db
    /// </summary>
    /// <param name="dbName"></param>
    [Button("读取Db打Log", ButtonSizes.Large)]
    private void ReadSqlDb(string dbName)
    {
        Debug.Log(B2SqlCfg.Instance.GetB2SqlData(1).Name);
    }

    /// <summary>
    /// 读Excel
    /// </summary>
    /// <param name="ExcelPath"></param>
    /// <returns></returns>
    public (string[], string[], string[], List<string[]>) ReadExcel()
    {
        string excelPath = Application.dataPath + "/SqliteFiles/TestExcel.xlsx";
        List<string[]> tableList = new List<string[]>();
        //excel文件位置 /MaskGame/ReadExcel/excel文件名
        FileStream stream = File.Open(excelPath,
            FileMode.Open, FileAccess.Read);
        IExcelDataReader excelReader = ExcelReaderFactory.CreateOpenXmlReader(stream);

        DataSet result = excelReader.AsDataSet();

        int rows = result.Tables[0].Rows.Count; //获取行数(多少行信息)
        int columns = result.Tables[0].Columns.Count; //获取列数(多少列字段)

        //初始化字段
        string[] nameTable = new string[columns];
        string[] csTypeTable = new string[columns];
        string[] sqlTypeTable = new string[columns];

        //存字段
        for (int i = 0; i < columns; i++)
        {
            nameTable[i] = result.Tables[0].Rows[1][i].ToString();
        }

        for (int i = 0; i < columns; i++)
        {
            csTypeTable[i] = result.Tables[0].Rows[2][i].ToString();
        }

        for (int i = 0; i < columns; i++)
        {
            sqlTypeTable[i] = result.Tables[0].Rows[3][i].ToString();
        }

        //从第二行开始读 读信息
        for (int i = 4; i < rows; i++)
        {
            //临时表
            string[] table = new string[columns];
            //赋值表信息
            for (int j = 0; j < columns; j++)
            {
                string nvalue = result.Tables[0].Rows[i][j].ToString();
                nvalue = CheckExcelStr(nvalue);
                table[j] = $"'{nvalue}'";
            }

            //添加到List
            tableList.Add(table);
        }

        return (nameTable, csTypeTable, sqlTypeTable, tableList);
    }

    /// <summary>
    /// 自动生成代码
    /// </summary>
    private void GenAutoCreateCode(DbAccess dbAccess, string dbName, string path, string[] nameTable,
        string[] csTypeTable)
    {
        string className = dbName + "SqlCfg";
        string classDataName = dbName + "SqlData";
        string codePath = path;
        using (StreamWriter sw = new StreamWriter($"{codePath}/{classDataName}.cs"))
        {
            sw.WriteLine("using UnityEngine;");
            sw.WriteLine("using UnityEngine.UI;");
            sw.WriteLine("using System.Collections.Generic;");
            sw.WriteLine("");
            sw.WriteLine("//自动生成于：" + DateTime.Now);
            //命名空间
            sw.WriteLine("namespace " + "SqliteCfg");
            sw.WriteLine("{");
            sw.WriteLine($"\tpublic class {classDataName}");
            sw.WriteLine("\t{");
            for (int i = 0; i < nameTable.Length; i++)
            {
                sw.WriteLine($"\t\tpublic {csTypeTable[i]} {nameTable[i]};");
            }

            sw.WriteLine("\t}");
            sw.WriteLine("}");
        }

        using (StreamWriter sw = new StreamWriter($"{codePath}/{className}.cs"))
        {
            sw.WriteLine("using UnityEngine;");
            sw.WriteLine("using UnityEngine.UI;");
            sw.WriteLine("using System.Collections.Generic;");
            sw.WriteLine("using Mono.Data.Sqlite;");
            sw.WriteLine("");

            sw.WriteLine("//自动生成于：" + DateTime.Now);

            //命名空间
            sw.WriteLine("namespace " + "SqliteCfg");
            sw.WriteLine("{");
            sw.WriteLine("");

            //类名
            sw.WriteLine($"\tpublic class {className}");
            sw.WriteLine("\t{");
            sw.WriteLine("");

            #region 单例

            sw.WriteLine($"\t\tpublic static {className} Instance");
            sw.WriteLine("\t\t{");
            sw.WriteLine("\t\t\tget { return _instance ?? (_instance = new " + className + "());}");
            sw.WriteLine("\t\t}");
            sw.WriteLine("");

            sw.WriteLine($"\t\tprivate static {className} _instance;");
            sw.WriteLine("");

            #endregion

            #region 缓存池

            sw.WriteLine(
                $"\t\tprivate Dictionary<{csTypeTable[0]}, {classDataName}> {classDataName}Pool = new Dictionary<{csTypeTable[0]}, {classDataName}>();");
            sw.WriteLine("");

            #endregion

            #region 初始化方法

            sw.WriteLine($"\t\tpublic void Init{classDataName}Pool()");
            sw.WriteLine("\t\t{");
            string endPath = "/SqliteFiles/";
            sw.WriteLine($"\t\t\tstring path = Application.dataPath + \"{endPath}\";");
            sw.WriteLine($"\t\t\tvar db = DbAccess.Instance;");
            sw.WriteLine($"\t\t\tdb.OpenDB(@\"data source=\" + path +\"{dbName}\"+ \".db\");");
            sw.WriteLine($"\t\t\tvar sqReader = db.ReadFullTable(\"Test\");");
            sw.WriteLine($"\t\t\twhile (sqReader.Read())");
            sw.WriteLine("\t\t\t{");

            sw.WriteLine(
                $"\t\t\t\tvar key = sqReader.{GetCsTypeFuncStr(csTypeTable[0])}(sqReader.GetOrdinal(\"{nameTable[0]}\"));");
            sw.WriteLine($"\t\t\t\tvar value = new {classDataName}()");

            sw.WriteLine("\t\t\t\t{");
            sw.WriteLine($"\t\t\t\t\t{nameTable[0]}=key,");
            for (int i = 1; i < nameTable.Length; i++)
            {
                sw.WriteLine(
                    $"\t\t\t\t\t{nameTable[i]}=sqReader.{GetCsTypeFuncStr(csTypeTable[i])}(sqReader.GetOrdinal(\"{nameTable[i]}\")),");
            }

            sw.WriteLine("\t\t\t\t};");
            sw.WriteLine($"\t\t\t\t{classDataName}Pool.Add(key,value);");

            sw.WriteLine("\t\t\t}");
            sw.WriteLine("\t\t\tDbAccess.Instance.CloseSqlConnection();");
            sw.WriteLine("\t\t}");
            sw.WriteLine("");

            #endregion

            #region 外部获取方法

            sw.WriteLine($"\t\tpublic {classDataName} Get{classDataName}({csTypeTable[0]} key)");
            sw.WriteLine("\t\t{");


            sw.WriteLine($"\t\t\tif(!{classDataName}Pool.ContainsKey(key))");
            sw.WriteLine("\t\t\t{");
            sw.WriteLine($"\t\t\t\tstring path = Application.dataPath + \"{endPath}\";");
            sw.WriteLine($"\t\t\t\tvar db = DbAccess.Instance;");
            sw.WriteLine($"\t\t\t\tdb.OpenDB(@\"data source=\" + path +\"{dbName}\"+ \".db\");");
            sw.WriteLine($"\t\t\t\t using (SqliteDataReader sqReader = db.SelectWhere(\"Test\",");
            sw.WriteLine($"\t\t\t\t{GetStringArrDesc(nameTable)}");
            sw.WriteLine($"\t\t\t\t{GetStringArrDesc(new string[] { nameTable[0] })}");
            sw.WriteLine("\t\t\t\tnew string[] { \"=\" },");
            sw.WriteLine("\t\t\t\tnew string[] { $\"{key}\" }))");
            sw.WriteLine("\t\t\t\t{");
            sw.WriteLine("\t\t\t\t\twhile (sqReader.Read())");
            sw.WriteLine("\t\t\t\t\t{");
            sw.WriteLine(
                $"\t\t\t\t\t\tvar readKey = sqReader.{GetCsTypeFuncStr(csTypeTable[0])}(sqReader.GetOrdinal(\"{nameTable[0]}\"));");
            sw.WriteLine($"\t\t\t\t\t\tvar value = new {classDataName}()");

            sw.WriteLine("\t\t\t\t\t\t{");
            sw.WriteLine($"\t\t\t\t\t\t\t{nameTable[0]}=readKey,");
            for (int i = 1; i < nameTable.Length; i++)
            {
                sw.WriteLine(
                    $"\t\t\t\t\t\t\t{nameTable[i]}=sqReader.{GetCsTypeFuncStr(csTypeTable[i])}(sqReader.GetOrdinal(\"{nameTable[i]}\")),");
            }

            sw.WriteLine("\t\t\t\t\t\t};");
            sw.WriteLine($"\t\t\t\t\t\t{classDataName}Pool.Add(key,value);");
            sw.WriteLine("\t\t\t\t\t}");
            sw.WriteLine($"\t\t\t\t\tsqReader.Close();");
            sw.WriteLine("\t\t\t\t}");
            sw.WriteLine($"\t\t\t\t\tDbAccess.Instance.CloseSqlConnection();");
            sw.WriteLine("\t\t\t}");

            sw.WriteLine($"\t\t\treturn {classDataName}Pool[key];");

            sw.WriteLine("\t\t}");

            #endregion

            sw.WriteLine("");
            sw.WriteLine("\t}");
            sw.WriteLine("}");
        }

        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("提示", "代码生成完毕", "OK");
    }

    /// <summary>
    /// 根据sqlite字段类型获取cs对应的类型
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    private string GetCsTypeFuncStr(string str)
    {
        switch (str)
        {
            case "long":
                return "GetInt64";
            case "string":
                return "GetString";
            case "int":
                return "GetInt32";
            case "short":
                return "GetInt16";
            case "bool":
                return "GetBoolean";
        }

        return "GetValue";
    }

    /// <summary>
    /// 检查excel表格字段
    /// </summary>
    /// <param name="excelStr"></param>
    /// <returns></returns>
    private string CheckExcelStr(string excelStr)
    {
        switch (excelStr)
        {
            case "是":
                return 1.ToString();
            case "否":
                return 0.ToString();
        }

        return excelStr;
    }

    /// <summary>
    /// 获取字符串数组代码字符
    /// </summary>
    /// <param name="stringArr"></param>
    /// <returns></returns>
    private string GetStringArrDesc(string[] stringArr)
    {
        string desc = "new string[] {";
        for (int i = 0; i < stringArr.Length; i++)
        {
            desc += $"\"{stringArr[i]}\", ";
        }

        desc += "},";
        return desc;
    }

    private void GenAutoCreateCode1(DbAccess dbAccess, string dbName, string path, string[] nameTable,
        string[] csTypeTable)
    {
        string className = dbName + "SqlCfg";
        string classDataName = dbName + "SqlData";
        List<string> contentsLis1 = new List<string>();
        contentsLis1.Add("using UnityEngine;");
        contentsLis1.Add("using UnityEngine.UI;");
        contentsLis1.Add("using System.Collections.Generic;");
     
        contentsLis1.Add("//自动生成于：" + DateTime.Now);
        //命名空间
        contentsLis1.Add("namespace " + "SqliteCfg");
        contentsLis1.Add("{");
        contentsLis1.Add($"public class {classDataName}");
        contentsLis1.Add("{");
        for (int i = 0; i < nameTable.Length; i++)
        {
            contentsLis1.Add($"public {csTypeTable[i]} {nameTable[i]};");
        }

        contentsLis1.Add("}");
        contentsLis1.Add("}");
        
        
        Foee_CodeGenTools.CreateCsCodeFiles(classDataName,path,contentsLis1);
        List<string> contentsLis = new List<string>();
        contentsLis.Add("using UnityEngine;");
        contentsLis.Add("using UnityEngine.UI;");
        contentsLis.Add("using System.Collections.Generic;");
        contentsLis.Add("using Mono.Data.Sqlite;");

        contentsLis.Add("//自动生成于：" + DateTime.Now);

        //命名空间
        contentsLis.Add("namespace " + "SqliteCfg");
        contentsLis.Add("{");

        //类名
        contentsLis.Add($"public class {className}");
        contentsLis.Add("{");

        #region 单例

        contentsLis.Add($"public static {className} Instance");
        contentsLis.Add("{");
        contentsLis.Add("get { return _instance ?? (_instance = new " + className + "());}");
        contentsLis.Add("}");

        contentsLis.Add($"private static {className} _instance;");

        #endregion

        #region 缓存池

        contentsLis.Add(
            $"private Dictionary<{csTypeTable[0]}, {classDataName}> {classDataName}Pool = new Dictionary<{csTypeTable[0]}, {classDataName}>();");

        #endregion

        #region 初始化方法

        contentsLis.Add($"public void Init{classDataName}Pool()");
        contentsLis.Add("{");
        string endPath = "/SqliteFiles/";
        contentsLis.Add($"string path = Application.dataPath + \"{endPath}\";");
        contentsLis.Add($"var db = DbAccess.Instance;");
        contentsLis.Add($"db.OpenDB(@\"data source=\" + path +\"{dbName}\"+ \".db\");");
        contentsLis.Add($"var sqReader = db.ReadFullTable(\"Test\");");
        contentsLis.Add($"while (sqReader.Read())");
        contentsLis.Add("{");

        contentsLis.Add(
            $"var key = sqReader.{GetCsTypeFuncStr(csTypeTable[0])}(sqReader.GetOrdinal(\"{nameTable[0]}\"));");
        contentsLis.Add($"var value = new {classDataName}()");

        contentsLis.Add("{");
        contentsLis.Add($"{nameTable[0]}=key,");
        for (int i = 1; i < nameTable.Length; i++)
        {
            contentsLis.Add(
                $"{nameTable[i]}=sqReader.{GetCsTypeFuncStr(csTypeTable[i])}(sqReader.GetOrdinal(\"{nameTable[i]}\")),");
        }
        contentsLis.Add("};");
        contentsLis.Add($"{classDataName}Pool.Add(key,value);");
        contentsLis.Add("}");
        contentsLis.Add("DbAccess.Instance.CloseSqlConnection();");
        contentsLis.Add("}");
        #endregion

        #region 外部获取方法
        contentsLis.Add($"public {classDataName} Get{classDataName}({csTypeTable[0]} key)");
        contentsLis.Add("{");
        contentsLis.Add($"if(!{classDataName}Pool.ContainsKey(key))");
        contentsLis.Add("{");
        contentsLis.Add($"string path = Application.dataPath + \"{endPath}\";");
        contentsLis.Add($"var db = DbAccess.Instance;");
        contentsLis.Add($"db.OpenDB(@\"data source=\" + path +\"{dbName}\"+ \".db\");");
        contentsLis.Add($"using (SqliteDataReader sqReader = db.SelectWhere(\"Test\",");
        contentsLis.Add($"{GetStringArrDesc(nameTable)}");
        contentsLis.Add($"{GetStringArrDesc(new string[] { nameTable[0] })}");
        contentsLis.Add("new string[] { \"=\" },");
        contentsLis.Add("new string[] { $\"{key}\" }))");
        contentsLis.Add("{");
        contentsLis.Add("while (sqReader.Read())");
        contentsLis.Add("{");
        contentsLis.Add(
            $"var readKey = sqReader.{GetCsTypeFuncStr(csTypeTable[0])}(sqReader.GetOrdinal(\"{nameTable[0]}\"));");
        contentsLis.Add($"var value = new {classDataName}()");
        contentsLis.Add("{");
        contentsLis.Add($"{nameTable[0]}=readKey,");
        for (int i = 1; i < nameTable.Length; i++)
        {
            contentsLis.Add(
                $"{nameTable[i]}=sqReader.{GetCsTypeFuncStr(csTypeTable[i])}(sqReader.GetOrdinal(\"{nameTable[i]}\")),");
        }
        contentsLis.Add("};");
        contentsLis.Add($"{classDataName}Pool.Add(key,value);");
        contentsLis.Add("}");
        contentsLis.Add($"sqReader.Close();");
        contentsLis.Add("}");
        contentsLis.Add($"DbAccess.Instance.CloseSqlConnection();");
        contentsLis.Add("}");
        contentsLis.Add($"return {classDataName}Pool[key];");
        contentsLis.Add("}");
        #endregion
        contentsLis.Add("}");
        contentsLis.Add("}");
        Foee_CodeGenTools.CreateCsCodeFiles(className,path,contentsLis);
    }
}