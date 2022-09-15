using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;

namespace API_premierductsqld.Service
{
	public class Utils
	{
		public static List<T> ConvertTo<T>(DataTable datatable) where T : new()
		{
			List<T> Temp = new List<T>();
			try
			{
				List<string> columnsNames = new List<string>();
				foreach (DataColumn DataColumn in datatable.Columns)
					columnsNames.Add(DataColumn.ColumnName);
				Temp = datatable.AsEnumerable().ToList().ConvertAll<T>(row => getObject<T>(row, columnsNames));
				return Temp;
			}
			catch
			{
				return Temp;
			}

		}
		public static T getObject<T>(DataRow row, List<string> columnsName) where T : new()
		{
			T obj = new T();
			try
			{
				string columnname = "";
				string value = "";
				PropertyInfo[] Properties;
				Properties = typeof(T).GetProperties();
				foreach (PropertyInfo objProperty in Properties)
				{
					columnname = columnsName.Find(name => name.ToLower() == objProperty.Name.ToLower());
					if (!string.IsNullOrEmpty(columnname))
					{
						value = row[columnname].ToString();
						if (!string.IsNullOrEmpty(value))
						{
							if (Nullable.GetUnderlyingType(objProperty.PropertyType) != null)
							{
								value = row[columnname].ToString().Replace("$", "").Replace(",", "");
								objProperty.SetValue(obj, Convert.ChangeType(value, Type.GetType(Nullable.GetUnderlyingType(objProperty.PropertyType).ToString())), null);
							}
							else
							{
								value = row[columnname].ToString().Replace("%", "");
								objProperty.SetValue(obj, Convert.ChangeType(value, Type.GetType(objProperty.PropertyType.ToString())), null);
							}
						}
					}
				}
				return obj;
			}
			catch
			{
				return obj;
			}
		}

		public static List<T> ConvertToList<T>(DataTable dt)
		{
			var columnNames = dt.Columns.Cast<DataColumn>()
				.Select(c => c.ColumnName)
				.ToList();
			var properties = typeof(T).GetProperties();
			return dt.AsEnumerable().Select(row =>
			{
				var objT = Activator.CreateInstance<T>();
				foreach (var pro in properties)
				{
					if (columnNames.Contains(pro.Name))
						pro.SetValue(objT, row[pro.Name]);
				}
				return objT;
			}).ToList();
		}

	}
}

