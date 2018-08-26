using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using ClosedXML.Excel;
using Excel;
using KMS_batch_backend.V27Production;

namespace KMS_batch_backend
{
    public static class Program
    {
        private static readonly Dictionary<int, string> RemainderDictionary = new Dictionary<int, string>
        {
            {0, "1"},
            {1, "0"},
            {2, "X"},
            {3, "9"},
            {4, "8"},
            {5, "7"},
            {6, "6"},
            {7, "5"},
            {8, "4"},
            {9, "3"},
            {10, "2"}
        };

        private static void Main(string[] args)
        {
            var filePath = Directory.GetCurrentDirectory() + "\\Input.xlsx";
            var stream = File.Open(filePath, FileMode.Open, FileAccess.Read);
            var excelReader = ExcelReaderFactory.CreateOpenXmlReader(stream);

            var input = new InputBindingModel();
            var outputList = new List<OutputBindingModel>();

            var client = new SearchService_v27SoapClient();

            var token = client.Authenticate("1234", "1234");
            
            token.DataSources =
                token.DataSources.Where(
                    v27 => v27.DataSourceName == "China National ID" || v27.DataSourceName == "Watchlist AML").ToArray();

            var recordCount = 0;
            while (excelReader.Read())
            {
                if (recordCount == 0)
                {
                    recordCount++;
                    continue;
                }
                input.DZID = recordCount.ToString();
                input.CustomerReference = excelReader[1].ToString();
                input.ShowPhoto = excelReader[2].Equals("TRUE");
                input.FullName = excelReader[3].ToString();
                input.IdCardnumber = excelReader[4].ToString();
                input.DateOfBirth = DateTime.ParseExact(excelReader[5].ToString(),
                    "d/M/yyyy hh:mm:ss tt",
                    null);
                outputList.Add(DataProcessing(input, token));
                recordCount++;
            }
            Console.WriteLine($"In total, {recordCount} are being processed.");
            BuildingOutput(outputList);

            excelReader.Close();
        }

        private static OutputBindingModel DataProcessing(InputBindingModel input, SessionManager_v27 token)
        {
            var output = new OutputBindingModel();
            var result = new VerifyResults_v27();

            using (var client = new SearchService_v27SoapClient())
            {
                var content = new SearchCriteria_v27
                {
                    FullName = input.FullName,
                    IDCardNo = input.IdCardnumber,
                    DateOfBirth = input.DateOfBirth,
                    ShowPhoto = input.ShowPhoto
                };

                if (IdValidCheck(content.IDCardNo))
                {
                    result = client.Verify(token, content);
                }
                if (result.Message != null && result.Message.Equals("Success"))
                {
                    output.Message = "Success";
                    if (result.Results[0].Item == null)
                    {
                        output.DZID = input.DZID;
                        output.CustomerReference = input.CustomerReference;
                        output.WatchListPdf = "None";
                        output.WatchListCategory = "None";
                        output.ScanId = "None";

                    }
                    else
                    {
                        output.DZID = input.DZID;
                        output.CustomerReference = input.CustomerReference;
                        output.WatchListPdf = result.Results[0].url_more;
                        output.WatchListCategory = result.Results[0].Item[0].WatchlistCategory;
                        output.ScanId = result.Results[0].scan_id;
                    }
                    if (result.Results[1].Item == null) return output;
                    var chinaIdResult = result.Results[1].Item[0];
                    output.DZID = input.DZID;
                    output.CustomerReference = input.CustomerReference;
                    output.InputFullName = content.FullName;
                    output.InputDOB = content.DateOfBirth.ToShortDateString();
                    output.SourceVerfied = chinaIdResult.SourceVerified;
                    output.IdCardNoValid = chinaIdResult.IDCardNoValid;
                    output.DateOfBirthVerified = chinaIdResult.DateofBirthVerified;
                    output.AddressLocality = chinaIdResult.Addresses[0].AddressLine1;
                    output.Gender = chinaIdResult.Gender;
                    output.PhotoUrl = input.ShowPhoto ? chinaIdResult.PhotoURL : "Not requested";
                    output.ErrorMessages = chinaIdResult.ErrorMessage;
                }
                else
                {
                    output.Message = "Fail";
                    output.ErrorMessages = "ID number is not valid";
                }
                Console.WriteLine($"{output.Message} {content.IDCardNo} {output.ErrorMessages}");
            }
            return output;
        }

        private static void BuildingOutput(IEnumerable<OutputBindingModel> output)
        {
            var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Output");
            worksheet.ColumnWidth = 20;
            BuildingOutputHeader(worksheet);
            BuildingOutputBody(worksheet, output);
            var datetimenow = DateTime.Now.ToString("yyyyMMddHHmmss");
            workbook.SaveAs($"DataZoo_Output_{datetimenow}.xlsx");
        }

        private static void BuildingOutputHeader(IXLWorksheet worksheet)
        {
            const int rowIndicator = 1;
            worksheet.Cell($"A{rowIndicator}").Value = "Message";
            worksheet.Cell($"B{rowIndicator}").Value = "DZID";
            worksheet.Cell($"C{rowIndicator}").Value = "CustomerReference";
            worksheet.Cell($"D{rowIndicator}").Value = "InputFullName";
            worksheet.Cell($"E{rowIndicator}").Value = "InputDOB";
            worksheet.Cell($"F{rowIndicator}").Value = "SourceVerified";
            worksheet.Cell($"G{rowIndicator}").Value = "IDCardNoValid";
            worksheet.Cell($"H{rowIndicator}").Value = "DateOfBirthVerified";
            worksheet.Cell($"I{rowIndicator}").Value = "AddressLocality";
            worksheet.Cell($"J{rowIndicator}").Value = "Gender";
            worksheet.Cell($"K{rowIndicator}").Value = "PhotoURL";
            worksheet.Cell($"L{rowIndicator}").Value = "WatchListPDF";
            worksheet.Cell($"M{rowIndicator}").Value = "WatchListCategory";
            worksheet.Cell($"N{rowIndicator}").Value = "ScanID";
            worksheet.Cell($"O{rowIndicator}").Value = "ErrorMessages";
        }

        private static void BuildingOutputBody(IXLWorksheet worksheet, IEnumerable<OutputBindingModel> output)
        {
            var rowIndicator = 2;
            foreach (var item in output)
            {
                worksheet.Cell($"A{rowIndicator}").Value = item.Message;
                worksheet.Cell($"B{rowIndicator}").Value = item.DZID;
                worksheet.Cell($"C{rowIndicator}").Value = item.CustomerReference;
                worksheet.Cell($"D{rowIndicator}").Value = item.InputFullName;
                worksheet.Cell($"E{rowIndicator}").Value = item.InputDOB;
                worksheet.Cell($"F{rowIndicator}").Value = item.SourceVerfied;
                worksheet.Cell($"G{rowIndicator}").Value = item.IdCardNoValid;
                worksheet.Cell($"H{rowIndicator}").Value = item.DateOfBirthVerified;
                worksheet.Cell($"I{rowIndicator}").Value = item.AddressLocality;
                worksheet.Cell($"J{rowIndicator}").Value = item.Gender;
                worksheet.Cell($"K{rowIndicator}").Value = item.PhotoUrl;
                worksheet.Cell($"L{rowIndicator}").Value = item.WatchListPdf;
                worksheet.Cell($"M{rowIndicator}").Value = item.WatchListCategory;
                worksheet.Cell($"N{rowIndicator}").Value = item.ScanId;
                worksheet.Cell($"O{rowIndicator}").Value = item.ErrorMessages;
                rowIndicator++;
            }
        }

        private static bool IdValidCheck(string id)
        {
            var specialCoefficientStack = new Stack<int>(new[] {2, 4, 8, 5, 10, 9, 7, 3, 6, 1, 2, 4, 8, 5, 10, 9, 7});
            var total = 0;
            for (var i = 0; i < id.Length - 1; i++)
            {
                var coefficient = specialCoefficientStack.Pop();
                var idCharStr = id[i].ToString();
                if (idCharStr.ToUpper() == "X")
                {
                    idCharStr = "10";
                }
                int num;
                var idCharInt = 0;
                if (int.TryParse(idCharStr, out num))
                {
                    idCharInt = int.Parse(idCharStr);
                }
                total += idCharInt * coefficient;
            }

            var lastDigit = RemainderDictionary[total%11];

            return id.Length == 18 && id[6].Equals('1') &&
                   long.Parse(id.Substring(10, 2)) <= 12 && long.Parse(id.Substring(10, 2)) > 0 &&
                   long.Parse(id.Substring(12, 2)) <= 31 && long.Parse(id.Substring(12, 2)) > 0 &&
                   id[id.Length - 1].ToString().Equals(lastDigit);
        }
    }
}