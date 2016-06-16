using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace app.Dbf
{
	public class Value
	{
		public Action<RowBuilder> Build;

		public Value(Action<RowBuilder> build)
		{
			Build = build;
		}

		public static Value For(string name, object value)
		{
			return new Value(builder => builder.ValueForColumn(name, value));
		}
	}

	public class Column
	{
		public Action<ColumnBuilder> BuildColumn;

		public Column(Action<ColumnBuilder> buildColumn)
		{
			BuildColumn = buildColumn;
		}

		public static Column Numeric(string name, int presisionDigitCount, int scaleDigitCount)
		{
			return new Column(builder => builder.AddNubmericColumn(name, presisionDigitCount, scaleDigitCount));
		}

		public static Column Numeric(string name, int presisionDigitCount, params Transform[] transforms)
		{
			return new Column(builder => builder.AddNubmericColumn(name, presisionDigitCount, transforms));
		}

		public static Column Numeric(string name, int presisionDigitCount, int scaleDigitCount, params Transform[] transforms)
		{
			return new Column(builder => builder.AddNubmericColumn(name, presisionDigitCount, scaleDigitCount, transforms));
		}

		public static Column Char(string name, int maxLength)
		{
			return new Column(builder => builder.AddCharacterColumn(name, maxLength));
		}

		public static Column Char(string name, int maxLength, params Transform[] transforms)
		{
			return new Column(builder => builder.AddCharacterColumn(name, maxLength, transforms));
		}

		public static Column Bool(string name)
		{
			return new Column(builder => builder.AddBooleanColumn(name));
		}

		public static Column Date(string name)
		{
			return new Column(builder => builder.AddDateColumn(name));
		}
	}

	public class DbfTable
	{
		private readonly DataTable _dataTable;
		private readonly Dictionary<string, Transform[]> _columnTransforms = new Dictionary<string, Transform[]>();

		public DbfTable()
		{
			_dataTable = new DataTable();
		}


		public DbfTable(string name)
		{
			_dataTable = new DataTable(name);
		}

		protected DbfTable(DataTable dataTable)
		{
			_dataTable = dataTable;
		}

		public void AddTransformsForColumn(string columnName, Transform[] transforms)
		{
			_columnTransforms.Add(columnName, transforms);
		}

		public IEnumerable<Transform> ColumnTransforms(string columnName)
		{
			if (_columnTransforms.ContainsKey(columnName))
				foreach (var transform in _columnTransforms[columnName])
					yield return transform;
		}

		public DbfTable Columns(params Column[] columns)
		{
			var builder = BuidColumns();
			foreach (var column in columns)
			{
				column.BuildColumn(builder);
			}
			builder.End();
			return this;
		}

		public DbfTable Row(params Value[] values)
		{
			var builder = NewRow();
			foreach (var value in values)
			{
				value.Build(builder);
			}
			builder.End();
			return this;
		}

		public static DbfTable BuildTable(string name)
		{
			return new DbfTable(new DataTable(name));
		}

		public static DbfTable BuildTable()
		{
			return new DbfTable(new DataTable());
		}

		public ColumnBuilder BuidColumns()
		{
			return new ColumnBuilder(this);
		}

		public RowBuilder NewRow()
		{
			return new RowBuilder(this);
		}

		public DataTable Table
		{
			get { return _dataTable; }
		}

		public DataTable ToDataTable()
		{
			return _dataTable;
		}
	}

	public class ColumnBuilder
	{
		private readonly DataTable _table;
		private readonly DbfTable _dbfTable;

		public ColumnBuilder(DbfTable dbfTable)
		{
			_dbfTable = dbfTable;
			_table = dbfTable.Table;
		}

		public ColumnBuilder AddCharacterColumn(string columnName, int maxLength, params Transform[] transforms)
		{
			return AddColumn(columnName, typeof(string), maxLength, transforms);
		}

		public ColumnBuilder AddNubmericColumn(string columnName, int presisionDigitCount, int scaleDigitCount, params Transform[] transforms)
		{
			var column = new DataColumn(columnName, typeof(decimal));
			column.ExtendedProperties.Add("presision", presisionDigitCount);
			column.ExtendedProperties.Add("scale", scaleDigitCount);
			_table.Columns.Add(column);

			if (transforms != null && transforms.Length > 0)
				_dbfTable.AddTransformsForColumn(columnName, transforms);

			return this;
		}

		public ColumnBuilder AddNubmericColumn(string columnName, int presisionDigitCount, params Transform[] transforms)
		{
			return AddNubmericColumn(columnName, presisionDigitCount, 0, transforms);
		}

		public ColumnBuilder AddBooleanColumn(string columnName, params Transform[] transforms)
		{
			return AddColumn(columnName, typeof(bool), 0, transforms);
		}

		public ColumnBuilder AddDateColumn(string columnName, params Transform[] transforms)
		{
			return AddColumn(columnName, typeof(DateTime), 0, transforms);
		}

		private ColumnBuilder AddColumn(string columnName, Type columnType, int maxLength, params Transform[] transforms)
		{
			var column = new DataColumn(columnName, columnType);
			if (columnType == typeof(string))
				column.MaxLength = maxLength;
			_table.Columns.Add(column);

			if (transforms != null && transforms.Length > 0)
				_dbfTable.AddTransformsForColumn(columnName, transforms);

			return this;
		}

		public DbfTable End()
		{
			return _dbfTable;
		}
	}

	public class RowBuilder
	{
		private readonly DbfTable _dbfTable;
		private readonly DataRow _row;
		private readonly Dictionary<string, object> _values = new Dictionary<string, object>();

		public RowBuilder(DbfTable dbfTable)
		{
			_dbfTable = dbfTable;
			_row = _dbfTable.Table.NewRow();
			_row.CancelEdit();
		}

		public RowBuilder ValueForColumn(string columnName, params object[] value)
		{
			if (!_dbfTable.Table.Columns.Contains(columnName))
				throw new Exception(String.Format("Колонка {0} не найдена", columnName));

			object resultValue;

			if (value != null && value.Length == 1)
				resultValue = value[0];
			else
				resultValue = value;

			_values.Add(columnName, resultValue);
			return this;
		}

		private object ApplyTramsformations(DataColumn column, object value)
		{
			value = _dbfTable.ColumnTransforms(column.ColumnName).Aggregate(value, (v, t) => t(v));

			if (column.DataType == typeof(string) && value != null && value.ToString().Length > column.MaxLength)
				value = value.ToString().Substring(0, column.MaxLength);

			return value;
		}

		public DbfTable End()
		{
			foreach (DataColumn column in _dbfTable.Table.Columns)
			{
				object value = DBNull.Value;
				if (_values.ContainsKey(column.ColumnName))
					value = ApplyTramsformations(column, _values[column.ColumnName]);
				else if (ApplyTramsformations(column, null) != null)
					value = ApplyTramsformations(column, null);
				//игнорируем ошибки преобразования типов
				try
				{
					_row[column.ColumnName] = value ?? DBNull.Value;
				}
				catch (ArgumentException)
				{
				}
			}

			_dbfTable.Table.Rows.Add(_row);
			return _dbfTable;
		}
	}
}