using System;
using System.Data;
using System.IO;
using ClosedXML.Excel;

namespace DTO_PremierDucts.Utils
{
	public class CSVUltils
	{
		public void ToCSVWeekend(DataTable dtDataTable, string file_name)
		{
			StreamWriter sw = new StreamWriter(file_name + ".xlsx", false);
			//headers    
			for (int i = 0; i < dtDataTable.Columns.Count; i++)
			{
				sw.Write(dtDataTable.Columns[i]);
				if (i < dtDataTable.Columns.Count - 1)
				{
					sw.Write(",");
				}
			}
			sw.Write(sw.NewLine);
			foreach (DataRow dr in dtDataTable.Rows)
			{
				for (int i = 0; i < dtDataTable.Columns.Count; i++)
				{
					if (!Convert.IsDBNull(dr[i]))
					{
						string value = dr[i].ToString();
						if (value.Contains(','))
						{
							value = String.Format("\"{0}\"", value);
							sw.Write(value);
						}
						else
						{
							sw.Write(dr[i].ToString());
						}
					}
					if (i < dtDataTable.Columns.Count - 1)
					{
						sw.Write(",");
					}
				}
				sw.Write(sw.NewLine);
			}
			sw.Close();
		}

		private void toCSVWithSheets(int period, string date, params DataTable[] report_dataTable)
		{
			XLWorkbook wb = new XLWorkbook();
			if (report_dataTable[0] != null)
			{

				var worksheet_packing_dataTable = wb.Worksheets.Add("Report Daily");
				worksheet_packing_dataTable.Cell(1, 1).Value = "REPORT PERIOD:";
				worksheet_packing_dataTable.Cell(1, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
				worksheet_packing_dataTable.Cell(1, 1).Style.Font.Bold = true;
				worksheet_packing_dataTable.Cell(1, 2).Value = period; //REPORT_PERIOD_STRING
				worksheet_packing_dataTable.Cell(1, 2).Style.Font.Bold = true;
				worksheet_packing_dataTable.Cell(2, 1).Value = "REPORT STATION NAME:";
				worksheet_packing_dataTable.Cell(2, 1).Style.Font.Bold = true;
				worksheet_packing_dataTable.Cell(2, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
				worksheet_packing_dataTable.Cell(2, 2).Value = "PACKING";
				worksheet_packing_dataTable.Cell(2, 2).Style.Font.Bold = true;
				worksheet_packing_dataTable.Cell(3, 1).Value = "REPORT CREATED TIME:";
				worksheet_packing_dataTable.Cell(3, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
				worksheet_packing_dataTable.Cell(3, 1).Style.Font.Bold = true;
				worksheet_packing_dataTable.Cell(3, 2).Value = date;
				worksheet_packing_dataTable.Cell(3, 2).Style.Font.Bold = true;
				worksheet_packing_dataTable.Cell(3, 2).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
				var table_pharmacies = worksheet_packing_dataTable.Cell(5, 1).InsertTable(report_dataTable);
				worksheet_packing_dataTable.ColumnWidth = 15;

			}

			if (report_dataTable[1] != null)
			{
				var worksheet_packing_dataTable = wb.Worksheets.Add("Packing Report");
				worksheet_packing_dataTable.Cell(1, 1).Value = "REPORT PERIOD:";
				worksheet_packing_dataTable.Cell(1, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
				worksheet_packing_dataTable.Cell(1, 1).Style.Font.Bold = true;
				worksheet_packing_dataTable.Cell(1, 2).Value = period; //REPORT_PERIOD_STRING
				worksheet_packing_dataTable.Cell(1, 2).Style.Font.Bold = true;

				worksheet_packing_dataTable.Cell(2, 1).Value = "REPORT STATION NAME:";
				worksheet_packing_dataTable.Cell(2, 1).Style.Font.Bold = true;

				worksheet_packing_dataTable.Cell(2, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
				worksheet_packing_dataTable.Cell(2, 2).Value = "PACKING";
				worksheet_packing_dataTable.Cell(2, 2).Style.Font.Bold = true;

				worksheet_packing_dataTable.Cell(3, 1).Value = "REPORT CREATED TIME:";
				worksheet_packing_dataTable.Cell(3, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
				worksheet_packing_dataTable.Cell(3, 1).Style.Font.Bold = true;
				worksheet_packing_dataTable.Cell(3, 2).Value = date;
				worksheet_packing_dataTable.Cell(3, 2).Style.Font.Bold = true;
				worksheet_packing_dataTable.Cell(3, 2).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
				var table_pharmacies = worksheet_packing_dataTable.Cell(5, 1).InsertTable(report_dataTable[1]);
				worksheet_packing_dataTable.ColumnWidth = 15;
				table_pharmacies.Theme = XLTableTheme.TableStyleLight18;
				table_pharmacies.ShowTotalsRow = true;
				table_pharmacies.Field("METAL SQM").TotalsRowFunction = XLTotalsRowFunction.Sum;
				table_pharmacies.Field("INSULATION SQM").TotalsRowFunction = XLTotalsRowFunction.Sum;
				table_pharmacies.Field(0).TotalsRowLabel = "TOTAL SUM:";

			}
			if (report_dataTable[2] != null)
			{
				var IC_sheet = wb.Worksheets.Add("Insulation Cutting");
				var table_pharmacies = IC_sheet.Cell(1, 1).InsertTable(report_dataTable[3]);
				IC_sheet.ColumnWidth = 15;
				table_pharmacies.Theme = XLTableTheme.TableStyleLight10;

				var ST_sheet = wb.Worksheets.Add("Seal/Tape");
				var ST_table = ST_sheet.Cell(0, 0).InsertTable(report_dataTable[4]);
				ST_sheet.ColumnWidth = 15;
				ST_table.Theme = XLTableTheme.TableStyleLight11;

				var SF_sheet = wb.Worksheets.Add("Straight Finish");
				var SF_table = SF_sheet.Cell(0, 0).InsertTable(report_dataTable[5]);
				SF_sheet.ColumnWidth = 15;
				SF_table.Theme = XLTableTheme.TableStyleLight12;
			}


			wb.SaveAs(date.Replace("/", "") + ".xlsx");

		}

	}
}

