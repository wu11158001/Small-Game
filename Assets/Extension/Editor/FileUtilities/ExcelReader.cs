using UnityEngine;
using System.Data;
using System.Collections;
using System.IO;
using System.Text;
using Excel;
using UnityEditor;

public class ExcelReader
{

    public static void ConvertJsonFromExcel(string excelFile)
    {
        if (!File.Exists(excelFile))
        {
            Debug.LogWarning("ExcelFile :" + excelFile + "is not exist!"); return ;
        }
        Debug.Log("start convert xlsx to json utf8:"+excelFile);
        //获取Excel文件的绝对路径
        string excelPath = excelFile;
        //构造Excel工具类
        ExcelUtility excel = new ExcelUtility(excelPath);

        //判断编码类型
        Encoding encoding = Encoding.GetEncoding("utf-8");

        //判断输出类型
        string output = excelPath.Replace(".xlsx", ".json");
        excel.ConvertToJson(output, encoding);
        Debug.Log("convert xlsx to json utf8 suss!!!");
        //刷新本地资源
        AssetDatabase.Refresh();
    }

    public static DataSet ConvertCsvFromExcel(string excelFile)
    {
        if (!File.Exists(excelFile))
        {
            Debug.LogWarning("ExcelFile :" + excelFile + "is not exist!"); return null;
        }

        using (FileStream stream = File.Open(excelFile, FileMode.Open, FileAccess.Read))
        {
            Debug.LogError("Read Excel :" + excelFile);
            ////Choose one of either 1 or 2
            ////1. Reading from a binary Excel file ('97-2003 format; *.xls)
            //IExcelDataReader excelReader = ExcelReaderFactory.CreateBinaryReader(stream);

            //2. Reading from a OpenXml Excel file (2007 format; *.xlsx)
            IExcelDataReader excelReader = ExcelReaderFactory.CreateOpenXmlReader(stream);

            ////Choose one of either 3, 4, or 5
            ////3. DataSet - The result of each spreadsheet will be created in the result.Tables
            //DataSet result = excelReader.AsDataSet();

            ////4. DataSet - Create column names from first row
            //excelReader.IsFirstRowAsColumnNames = true;
            DataSet result = excelReader.AsDataSet();
            Debug.LogError("REsult == null" + (result == null));

            //5. Data Reader methods
            //while (excelReader.Read())
            //{

            //}

            //6. Free resources (IExcelDataReader is IDisposable)
            excelReader.Close();
            return result;
        }
    }

}
