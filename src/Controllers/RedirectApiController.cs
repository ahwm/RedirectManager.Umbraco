using Newtonsoft.Json;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Web;
using Umbraco.Cms.Web.BackOffice.Controllers;

namespace RedirectManager.Controllers
{
    public class RedirectApiController : UmbracoAuthorizedApiController
    {
        // /Umbraco/backoffice/Api/RedirectApi

        private readonly RedirectService RedirectService;

        public RedirectApiController(RedirectService redirectService)
        {
            RedirectService = redirectService;
        }

        public IEnumerable<Redirect> ListRedirects(int page = 1)
        {
            return RedirectService.ListRedirects(page);
        }

        public int GetRedirectPageCount()
        {
            return RedirectService.GetRedirectPageCount();
        }

        public IEnumerable<Redirect> FilterRedirects(string searchTerm, int page = 1)
        {
            return RedirectService.FilterRedirects(searchTerm, page);
        }

        public int GetFilterRedirectPageCount(string searchTerm)
        {
            return RedirectService.GetFilterRedirectPageCount(searchTerm);
        }

        public void DeleteRedirect(string id)
        {
            RedirectService.DeleteRedirect(id);
        }

        public string GetPrimaryDomain()
        {
            return RedirectService.GetPrimaryDomain();
        }

        public void SetPrimaryDomain(string domain)
        {
            RedirectService.SetPrimaryDomain(domain);
        }

        public void AddRedirect()
        {
            var data = Request.Body;
            string urls;
            using (StreamReader sr = new StreamReader(data))
                urls = sr.ReadToEnd();
            Dictionary<string, string> d = JsonConvert.DeserializeObject<Dictionary<string, string>>(urls);
            RedirectService.AddRedirect(d["oldUrl"], d["newUrl"]);
        }

        public void ImportRedirects()
        {
            var file = Request.Form.Files[0];
            DataTable data = new DataTable();
            string ext = Path.GetExtension(file.FileName);
            using MemoryStream ms = new MemoryStream();
            file.CopyTo(ms);
            ms.Position = 0;
            switch (ext)
            {
                case ".xls":
                    data = xlsToDT(ms);
                    break;
                case ".xlsx":
                    data = xlsxToDT(ms);
                    break;
                case ".txt":
                case ".tsv":
                    data = ImportTxt(ms, '\t');
                    break;
                case ".csv":
                    data = ImportTxt(ms,',');
                    break;
            }
            foreach (DataRow r in data.Rows)
            {
                RedirectService.AddRedirect((string)r["OldUrl"], (string)r["NewUrl"]);
            }
        }

        private DataTable ImportTxt(Stream data, char delim)
        {
            DataTable t = new DataTable();
            using (TextReader tr = new StreamReader(data))
            {
                string line;
                while ((line = tr.ReadLine()) != null)
                {
                    string[] items = line.Split(delim);
                    if (t.Columns.Count == 0)
                    {
                        foreach (string columnName in items)
                        {
                            if (columnName.Length > 0 && columnName != " ")
                                t.Columns.Add(new DataColumn(columnName.ToLower()));
                        }
                    }
                    else t.Rows.Add(items);
                }
            }

            return t;
        }
        private DataTable xlsxToDT(Stream file)
{
            DataTable table = new DataTable();
            XSSFWorkbook workbook = new XSSFWorkbook(file);
            ISheet sheet = workbook.GetSheetAt(0);
            IRow headerRow = sheet.GetRow(0);
            int cellCount = headerRow.LastCellNum;
            for (int i = headerRow.FirstCellNum; i < cellCount; i++)
            {
                DataColumn column = new DataColumn(headerRow.GetCell(i).StringCellValue.ToLower());
                table.Columns.Add(column);
            }
            int rowCount = sheet.LastRowNum;
            for (int i = (sheet.FirstRowNum == 0 ? sheet.FirstRowNum + 1 : sheet.FirstRowNum); i <= sheet.LastRowNum; i++)
            {
                IRow row = sheet.GetRow(i);
                DataRow dataRow = table.NewRow();
                for (int j = row.FirstCellNum; j < cellCount; j++)
                {
                    if (row.GetCell(j) != null)
                    {
                        dataRow[j] = row.GetCell(j).ToString();
                    }
                }
                table.Rows.Add(dataRow);
            }
            workbook = null;
            sheet = null;
            return table;
        }
        private DataTable xlsToDT(Stream file)
        {
            DataTable table = new DataTable();
            HSSFWorkbook workbook = new HSSFWorkbook(file);
            ISheet sheet = workbook.GetSheetAt(0);
            IRow headerRow = sheet.GetRow(0);
            int cellCount = headerRow.LastCellNum;
            for (int i = headerRow.FirstCellNum; i < cellCount; i++)
            {
                DataColumn column = new DataColumn(headerRow.GetCell(i).StringCellValue.ToLower());
                table.Columns.Add(column);
            }
            int rowCount = sheet.LastRowNum;
            for (int i = (sheet.FirstRowNum == 0 ? sheet.FirstRowNum + 1 : sheet.FirstRowNum); i <= sheet.LastRowNum; i++)
            {
                IRow row = sheet.GetRow(i);
                DataRow dataRow = table.NewRow();
                for (int j = row.FirstCellNum; j < cellCount; j++)
                {
                    if (row.GetCell(j) != null)
                    {
                        dataRow[j] = row.GetCell(j).ToString();
                    }
                }
                table.Rows.Add(dataRow);
            }
            workbook = null;
            sheet = null;
            return table;
        }
    }
}