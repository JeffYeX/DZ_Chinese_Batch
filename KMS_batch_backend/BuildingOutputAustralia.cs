using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClosedXML.Excel;

namespace KMS_batch_backend
{
    class BuildingOutputAustralia
    {
        public void BuildingOutput(IEnumerable<OutputBindingModelAustralia> output)
        {
            var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Output");
            worksheet.ColumnWidth = 20;
            BuildingOutputHeader(worksheet);
            BuildingOutputBody(worksheet, output);
            var datetimenow = DateTime.Now.ToString("yyyyMMddHHmmss");
            workbook.SaveAs($"DataZoo_Output_{datetimenow}.xlsx");
        }

        private void BuildingOutputHeader(IXLWorksheet worksheet)
        {
            const int rowIndicator = 1;
            worksheet.Cell($"A{rowIndicator}").Value = "Color";
            worksheet.Cell($"B{rowIndicator}").Value = "Referece";
            worksheet.Cell($"C{rowIndicator}").Value = "FirstName";
            worksheet.Cell($"D{rowIndicator}").Value = "LastName";
            worksheet.Cell($"E{rowIndicator}").Value = "PhoneNumber";
            worksheet.Cell($"F{rowIndicator}").Value = "MobilNumber";
            worksheet.Cell($"G{rowIndicator}").Value = "Address";
            worksheet.Cell($"H{rowIndicator}").Value = "Suburb";
            worksheet.Cell($"I{rowIndicator}").Value = "State";
            worksheet.Cell($"J{rowIndicator}").Value = "PostCode";
            //worksheet.Cell($"K{rowIndicator}").Value = "PhotoURL";
            /*worksheet.Cell($"L{rowIndicator}").Value = "WatchListPDF";
            worksheet.Cell($"M{rowIndicator}").Value = "WatchListCategory";
            worksheet.Cell($"N{rowIndicator}").Value = "ScanID";*/
            worksheet.Cell($"K{rowIndicator}").Value = "DateOfBirth";
            worksheet.Cell($"L{rowIndicator}").Value = "SourceVerified";
            worksheet.Cell($"M{rowIndicator}").Value = "NameMatchScore";
            worksheet.Cell($"N{rowIndicator}").Value = "AddressMatchScore";
            worksheet.Cell($"O{rowIndicator}").Value = "Message";
        }

        private void BuildingOutputBody(IXLWorksheet worksheet, IEnumerable<OutputBindingModelAustralia> output)
        {
            var rowIndicator = 2;
            foreach (var item in output)
            {
                worksheet.Cell($"A{rowIndicator}").Value = item.Color;
                worksheet.Cell($"B{rowIndicator}").Value = item.Reference;
                worksheet.Cell($"C{rowIndicator}").Value = item.FirstName;
                worksheet.Cell($"D{rowIndicator}").Value = item.LastName;
                worksheet.Cell($"E{rowIndicator}").Value = item.PhoneNumber;
                worksheet.Cell($"F{rowIndicator}").Value = item.MobileNumber;
                worksheet.Cell($"G{rowIndicator}").Value = item.Address;
                worksheet.Cell($"H{rowIndicator}").Value = item.Suburb;
                worksheet.Cell($"I{rowIndicator}").Value = item.State;
                worksheet.Cell($"J{rowIndicator}").Value = item.PostCode;
                // worksheet.Cell($"K{rowIndicator}").Value = item.PhotoUrl;
                /*worksheet.Cell($"L{rowIndicator}").Value = item.WatchListPdf;
                worksheet.Cell($"M{rowIndicator}").Value = item.WatchListCategory;
                worksheet.Cell($"N{rowIndicator}").Value = item.ScanId;*/
                worksheet.Cell($"K{rowIndicator}").Value = item.DateOfBirth;
                worksheet.Cell($"L{rowIndicator}").Value = item.SourceVerfied;
                worksheet.Cell($"M{rowIndicator}").Value = item.NameMatchScore;
                worksheet.Cell($"N{rowIndicator}").Value = item.AddressMatchScore;
                worksheet.Cell($"O{rowIndicator}").Value = item.Message;
                rowIndicator++;
            }
        }
    }
}
