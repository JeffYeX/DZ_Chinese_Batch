using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.Xml;
using System.Xml.Serialization;
using ClosedXML.Excel;
using Excel;
using KMS_batch_backend.LilosoftMain;
//using KMS_batch_backend.V27Production;
using KMS_batch_backend.V27Production;
using Newtonsoft.Json;

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
            var testID = IdValidCheck("310230196111247559");

            var cardWS = new CardServiceDelegateClient();
            const string prodAppkey = "91beea622dfe8176eaa99ab70821ef58";

            var filePath = Directory.GetCurrentDirectory() + "\\InputLilo.xlsx";
            var stream = File.Open(filePath, FileMode.Open, FileAccess.Read);
            var excelReader = ExcelReaderFactory.CreateOpenXmlReader(stream);

            var input = new InputBindingModel();
            var liloInput = new InputBindingModelLilo();
            var outputList = new List<OutputBindingModel>();
            var liloOutputList = new List<string>();

            var client = new SearchService_v27SoapClient();

            //var token = client.Authenticate("Service Test", "f2MEwR=tra");
            var token = client.Authenticate("Service Test", "f2MEwR=tra");

            token.DataSources =
                token.DataSources.Where(
                    //v27 => v27.DataSourceName == "Yellow Pages" || v27.DataSourceName == "NZTA Drivers License" || v27.DataSourceName == "DZ NAD").ToArray();
                    v27 => v27.DataSourceName == "DZ NAD").ToArray();

            token.DataSources[0].ConsentObtained = true;
            var recordCount = 0;
            while (excelReader.Read())
            {
                if (recordCount == 0)
                {
                    recordCount++;
                    continue;
                }

                token = client.Authenticate("Service Test", "f2MEwR=tra");

                    token.DataSources =
                        token.DataSources.Where(
                            //v27 => v27.DataSourceName == "Yellow Pages" || v27.DataSourceName == "NZTA Drivers License" || v27.DataSourceName == "DZ NAD").ToArray();
                            v27 => v27.DataSourceName == "DZ NAD").ToArray();

                token.DataSources[0].ConsentObtained = true;
                input.DZID = "1";
                //input.CustomerReference = excelReader[1].ToString();
                /*input.ShowPhoto = excelReader[2].Equals("FALSE");
                input.FullName = excelReader[3].ToString() + excelReader[2].ToString();
                input.IdCardnumber = excelReader[5].ToString();
                input.DateOfBirth = DateTime.ParseExact(excelReader[4].ToString(),
                    "d/MM/yyyy h:mm:ss tt",
                    null);*/

                
                /*input.FirstName = excelReader[1].ToString() ?? "";
                input.MiddleName = excelReader[2].ToString() ?? "";
                input.LastName = excelReader[3].ToString() ?? "";
                input.StreetNumber = excelReader[4].ToString() ?? "";
                input.StreetName = excelReader[5].ToString() ?? "";
                input.City = excelReader[6].ToString() ;
                input.PostCode = excelReader[8].ToString() ?? "";
                var blah = excelReader[9].ToString() ?? "";
                input.DateOfBirth = DateTime.ParseExact(excelReader[9].ToString(),
                    "d-M-yyyy",
                    null);
                input.DLNumber = excelReader[10].ToString() ?? "";
                input.DLVersion = excelReader[11].ToString() ?? "";*/

                //LiloInput
                liloInput.name = excelReader[0].ToString() ?? "";
                liloInput.cardno = excelReader[1].ToString() ?? "";
                liloInput.phone = excelReader[3].ToString() ?? "";

                var jsonLiloInput = JsonConvert.SerializeObject(liloInput);

                var liloOutput = cardWS.CheckCellphone(prodAppkey, jsonLiloInput);

                //var output = DataProcessing(input, token);

                //outputList.Add(output);

                liloOutputList.Add(liloOutput);

                //Console.WriteLine("**" + recordCount + "** " + token.Token + "**" + output.SourceVerfied);
                Console.WriteLine("**" + recordCount + "** " + "**" + liloOutput);

                recordCount++;
            }
            Console.WriteLine($"In total, {recordCount} are being processed.");
            //BuildingOutput(outputList);
            BuildingLiloOutput(liloOutputList);

            excelReader.Close();
            Console.ReadLine();
        }

        private static OutputBindingModel DataProcessing(InputBindingModel input, SessionManager_v27 token)
        {
            var output = new OutputBindingModel();
            var result = new VerifyResults_v27();

            using (var client = new SearchService_v27SoapClient())
            {
                var content = new SearchCriteria_v27
                {
                    FirstName = input.FirstName,
                    MiddleName = input.MiddleName,
                    LastName = input.LastName,
                    StreetNumber = input.StreetNumber,
                    StreetName = input.StreetName,
                    City = input.City,
                    PostCode = input.PostCode,
                    DriversLicenseNo = input.DLNumber,
                    DriversLicenseVersion = input.DLVersion,
                    DateOfBirth = input.DateOfBirth
                };

                /*if (IdValidCheck(content.IDCardNo))
                {
                result = client.Verify(token, content);
                }*/

                result = client.Verify(token, content);
                
                //Console.WriteLine(json);

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
                        var blah = result.Results[0].Item[0];

                        var json = JsonConvert.SerializeObject(blah);

                        output.Message = result.Message;
                        output.DZID = result.ReportingReference;
                        output.CustomerReference = json;
                        output.SourceVerfied = blah.SourceVerified;

                        /*output.DZID = input.DZID;
                        output.CustomerReference = input.CustomerReference;
                        output.WatchListPdf = result.Results[0].url_more;
                        output.WatchListCategory = result.Results[0].Item[0].WatchlistCategory;
                        output.ScanId = result.Results[0].scan_id;*/
                    }


                    /*if (result.Results[0].Item == null) return output;
                    var chinaIdResult = result.Results[0].Item[0];
                    var addressInfo = chinaIdResult.Addresses[0];
                    output.DZID = result.ReportingReference;
                    output.CustomerReference = input.CustomerReference;
                    //output.InputFullName = content.FullName;
                    //output.InputDOB = content.DateOfBirth.ToShortDateString();
                    output.SourceVerfied = chinaIdResult.SourceVerified;
                    /*output.IdCardNoValid = chinaIdResult.IDCardNoValid;
                    output.DateOfBirthVerified = chinaIdResult.DateofBirthVerified;
                    output.AddressLocality = chinaIdResult.Addresses[0].AddressLine1;
                    output.Gender = chinaIdResult.Gender;
                    output.PhotoUrl = input.ShowPhoto ? chinaIdResult.PhotoURL : "Not requested";#1#
                    output.AddressLine1 = addressInfo.AddressLine1;
                    output.DPID = addressInfo.DPID;
                    output.SafeHarbour = result.Results[0].safe_harbour_score.ToString();
                    output.Firstname = chinaIdResult.FirstName;
                    output.LastName = chinaIdResult.LastName;
                    //output.ErrorMessages = chinaIdResult.ErrorMessage;*/



                }
                else
                {
                    output.Message = "Fail";
                    //output.ErrorMessages = "ID number is not valid";
                    if (result.Results[0].Item == null)
                    {
                        output.DZID = input.DZID;
                        output.CustomerReference = input.CustomerReference;
                        output.WatchListPdf = "None";
                        output.WatchListCategory = "None";
                        output.ScanId = "None";

                    }
                    /*else
                    {
                        output.DZID = input.DZID;
                        output.CustomerReference = input.CustomerReference;
                        output.WatchListPdf = result.Results[0].url_more;
                        output.WatchListCategory = result.Results[0].Item[0].WatchlistCategory;
                        output.ScanId = result.Results[0].scan_id;
                    }*/
                }
                Console.WriteLine($"{output.Message} {content.IDCardNo} {output.ErrorMessages} ");
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
            worksheet.Cell($"D{rowIndicator}").Value = "FirstName";
            worksheet.Cell($"E{rowIndicator}").Value = "LastName";
            worksheet.Cell($"F{rowIndicator}").Value = "AddressLine";
            worksheet.Cell($"G{rowIndicator}").Value = "DPID";
            worksheet.Cell($"H{rowIndicator}").Value = "Safeharbour";
            worksheet.Cell($"I{rowIndicator}").Value = "SourceVerified";

            /*worksheet.Cell($"D{rowIndicator}").Value = "InputFullName";
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
            worksheet.Cell($"O{rowIndicator}").Value = "ErrorMessages";*/
        }

        private static void BuildingOutputBody(IXLWorksheet worksheet, IEnumerable<OutputBindingModel> output)
        {
            var rowIndicator = 2;
            foreach (var item in output)
            {
                worksheet.Cell($"A{rowIndicator}").Value = item.Message;
                worksheet.Cell($"B{rowIndicator}").Value = item.DZID;
                worksheet.Cell($"C{rowIndicator}").Value = item.CustomerReference;
                worksheet.Cell($"D{rowIndicator}").Value = item.Firstname;
                worksheet.Cell($"E{rowIndicator}").Value = item.LastName;
                worksheet.Cell($"F{rowIndicator}").Value = item.AddressLine1;
                worksheet.Cell($"G{rowIndicator}").Value = item.DPID;
                worksheet.Cell($"H{rowIndicator}").Value = item.SafeHarbour;
                worksheet.Cell($"I{rowIndicator}").Value = item.SourceVerfied;

                /*worksheet.Cell($"D{rowIndicator}").Value = item.InputFullName;
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
                worksheet.Cell($"O{rowIndicator}").Value = item.ErrorMessages;*/
                rowIndicator++;
            }
        }

        private static void BuildingLiloOutput(IEnumerable<string> output)
        {
            var rowIndicator = 1;
            var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Output");
            worksheet.ColumnWidth = 20;
            foreach (var item in output)
            {
                worksheet.Cell($"A{rowIndicator}").Value = item;
                rowIndicator++;
            }
            var datetimenow = DateTime.Now.ToString("yyyyMMddHHmmss");
            workbook.SaveAs($"DataZoo_Output_{datetimenow}.xlsx");
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