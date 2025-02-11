using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using OfficeOpenXml;
using OfficeOpenXml.Table;

public class ExcelImporter
{
    /// <summary>
    /// Represents a table with named columns.
    /// </summary>
    public struct Table {
        readonly ExcelTable _source;
        
        /// <summary>
        /// Number of data rows in the table, not counting headers or totals.
        /// </summary>
        public int RowCount { get { return _source.Address.Rows - (_source.ShowTotal ? 1 : 0) - (_source.ShowHeader ? 1 : 0); } }

        /// <summary>
        /// Number of columns in the table.
        /// </summary>
        public int ColumnCount { get { return _source.Columns.Count; } }

        // TODO: Investigate why built-in ExcelTableColumnCollection[name] fails.
        readonly Dictionary<string, ExcelTableColumn> _columnLookup;        

        public Table(ExcelTable source) {
            _source = source;
            _columnLookup = new Dictionary<string, ExcelTableColumn>(source.Columns.Count);
            foreach (var column in source.Columns) {
                _columnLookup.Add(column.Name, column);
            }
        }

        /// <summary>
        /// Checks whether the given column name is present in this table.
        /// </summary>
        /// <param name="columnName"></param>
        /// <returns></returns>
        public bool HasColumn(string columnName) {
            return _columnLookup.ContainsKey(columnName);
        }

        /// <summary>
        /// Gets a label from the table and tries to map it to an Enumeration.
        /// </summary>
        /// <typeparam name="T">The type of the Enumeration to match.</typeparam>
        /// <param name="row">The row of the table to read, starting at 1.</param>
        /// <param name="columnName">The name of the column to read - case sensitive.</param>
        /// <param name="value">If successful, the matched Enumeration value will be stored here.</param>
        /// <returns>true if a matching Enumeration value was found, false otherwise.</returns>
        public bool TryGetEnum<T>(int row, string columnName, out T value) where T:struct, System.Enum {
            string name = GetValue<string>(row, columnName);

            value = default;
            if (string.IsNullOrWhiteSpace(name)) return false;

            if (System.Enum.TryParse<T>(name, out value)) return true;
            
            Debug.LogError($"Unknown {typeof(T).Name} value '{name}' in table {_source.Name}, row {row}, column {columnName}.");
            return false;
        }

        public T GetValue<T>(int row, string columnName) {
            if (row > RowCount) {                
                Debug.LogError($"Tried to access row {row} of table '{_source.Name}' which has only {RowCount} rows.");
                return default;
            }

            if (!TryGetColumn(columnName, out ExcelTableColumn column)) {
                return default;
            }

            var start = _source.Address.Start;
            return _source.WorkSheet.GetValue<T>(start.Row + row, start.Column + column.Position);
        }

        public T[] GetValues<T>(string columnName) {
            if (!TryGetColumn(columnName, out ExcelTableColumn firstColumn))
                return null;

            int rows = RowCount;
            var values = new T[rows];

            var start = _source.Address.Start;
            int startRow = start.Row + 1;
            int startColumn = start.Column + firstColumn.Position;

            for (int row = 0; row < rows; row++) {
                values[row] = _source.WorkSheet.GetValue<T>(startRow + row, startColumn);
            }

            return values;
        }

        public T[,] GetValues<T>(string startColumnName, int columns) {
            if (!TryGetColumn(startColumnName, out ExcelTableColumn firstColumn))
                return null;

            int rows = RowCount;
            var values = new T[rows, columns];

            var start = _source.Address.Start;
            int startRow = start.Row + 1;
            int startColumn = start.Column + firstColumn.Position;

            for (int row = 0; row < rows; row++) {
                for (int column = 0; column < columns; column++) {
                    values[row, column] = _source.WorkSheet.GetValue<T>(startRow + row, startColumn + column);
                }
            }
            return values;
        }

        public bool TrySetValues<T>(string startColumnName, T[,] values) {
            if (!TryGetColumn(startColumnName, out ExcelTableColumn firstColumn))
                return false;                        

            int rows = values.GetLength(0);

            int columns = values.GetLength(1);
            var start = _source.Address.Start;
            int startRow = start.Row + 1;
            int startColumn = start.Column + firstColumn.Position;

            if (rows > RowCount) {
                _source.WorkSheet.InsertRow(startRow, rows - RowCount);
            }

            for (int row = 0; row < rows; row++) {
                for (int column = 0; column < columns; column++) {
                    _source.WorkSheet.SetValue(startRow + row, startColumn + column, values[row, column]);
                }
            }
            return true;
        }

        /// <summary>
        /// Tries to set a whole column of values from an array of data.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="columnName"></param>
        /// <param name="values"></param>
        /// <param name="appendIfAbsent"></param>
        /// <returns></returns>
        public bool TrySetValues<T>(string columnName, T[] values, bool appendIfAbsent = false) {

            var start = _source.Address.Start;
            int startColumn;
            if (!TryGetColumn(columnName, out ExcelTableColumn firstColumn, !appendIfAbsent)) {
                if (!appendIfAbsent) return false;

                startColumn = start.Column + ColumnCount;
                _source.WorkSheet.InsertColumn(startColumn, 1);
                _source.WorkSheet.SetValue(_source.Address.Start.Row, startColumn, columnName);
            } else {
                startColumn = start.Column + firstColumn.Position;
            }

            int rows = values.Length;            
            int startRow = start.Row + 1;             

            if (rows > RowCount) {
                _source.WorkSheet.InsertRow(startRow, rows - RowCount);
            }

            for (int row = 0; row < rows; row++) {
                    _source.WorkSheet.SetValue(startRow + row, startColumn, values[row]);
            }

            return true;
        }

        /// <summary>
        /// Convenience method to fill a column with row numbers 1-n.
        /// </summary>
        /// <param name="columnName">Name of the column to fill.</param>
        public void NumberRows(string columnName) {
            if (!TryGetColumn(columnName, out ExcelTableColumn firstColumn))
                return;

            int rows = RowCount;
            var start = _source.Address.Start;
            int startRow = start.Row;
            int startColumn = start.Column + firstColumn.Position;
            for (int row = 1; row <= rows; row++) {
                _source.WorkSheet.SetValue(startRow + row, startColumn, row);
            }
        }

        bool TryGetColumn(string columnName, out ExcelTableColumn result, bool reportError = true) {
            if (_columnLookup.TryGetValue(columnName, out result))
                return true;
            
            if (!reportError) return false;
                
            string info = "Valid columns are...";
            string comparison = columnName.Trim().ToLowerInvariant();
            bool nearMatch = false;
            foreach (var label in _source.Columns){
                info += $" '{label.Name}'";
                nearMatch |= label.Name.Trim().ToLowerInvariant() == comparison;
            }
            if (nearMatch) info += "\n(Check capitalization and whitespace)";
            Debug.LogError($"Cannot find column named '{columnName}' in table {_source.Name}.\n{info}");
            
            return false;
        }
    }

    public struct Range {
        readonly ExcelNamedRange _source;
        readonly ExcelWorksheet _sheet;

        public int RowCount { get { return _source.Rows; } }
        public int ColumnCount { get { return _source.Columns; } }

        public Range(ExcelWorksheet sheet, ExcelNamedRange source) {
            _sheet = sheet;
            _source = source;
        }

        public bool TryGetValue<T>(int row, int column, out T value) where T : struct, System.Enum
        {
            string name = GetValue<string>(row, column);
            if (System.Enum.TryParse<T>(name, out value))
                return true;

            Debug.LogError($"Unknown {typeof(T).Name} value '{name}' in range '{_source.Name}', row {row}, column {column}.");
            return false;
        }

        public T GetValue<T>(int row, int column) {
            var start = _source.Start;            
            return _sheet.GetValue<T>(start.Row + row-1, start.Column + column-1);
        }

        public bool TryGetValue<T>(out T value) where T : struct, System.Enum
        {
            string name = GetValue<string>();
            if (System.Enum.TryParse<T>(name, out value))
                return true;

            Debug.LogError($"Unknown {typeof(T).Name} value '{name}' in range '{_source.Name}'.");
            return false;
        }

        public T GetValue<T>() {
            var start = _source.Start;
            return _sheet.GetValue<T>(start.Row, start.Column);
        }

        public T[,] GetValues<T>() {
            int rows = RowCount;
            int columns = ColumnCount;
            var values = new T[rows, columns];
            var start = _source.Start;
            int startRow = start.Row;
            int startColumn = start.Column;

            for (int row = 0; row < rows; row++) {
                for (int column = 0; column < columns; column++)
                    values[row, column] = _sheet.GetValue<T>(startRow + row, startColumn + column);
            }
            return values;
        }

        public void SetValue<T>(T value) {
            var start = _source.Start;
            _sheet.SetValue(start.Row, start.Column, value);
        }

        public void SetValues<T>(T[,] values) {
            int rows = values.GetLength(0);
            int columns = values.GetLength(1);

            var start = _source.Start;
            int startRow = start.Row;
            int startColumn = start.Column;

            if (rows > RowCount) {
                _sheet.InsertRow(startRow+1, rows - RowCount);
            }

            if (columns > ColumnCount) {
                _sheet.InsertColumn(startColumn+1, columns - ColumnCount);
            }

            for (int row = 0; row < rows; row++) {
                for (int column = 0; column < columns; column++)
                    _sheet.SetValue(startRow + row, startColumn + column, values[row, column]);
            }
        }

        public void NumberRows() {
            int rows = RowCount;
            var start = _source.Start;
            int startRow = start.Row-1;
            int startColumn = start.Column;
            for (int row = 1; row <= rows; row++) {
                _sheet.SetValue(startRow + row, startColumn, row);
            }
        }

        public void NumberColumns() {
            int columns = ColumnCount;
            var start = _source.Start;
            int startRow = start.Row;
            int startColumn = start.Column - 1;
            for (int column = 1; column <= columns; column++) {
                _sheet.SetValue(startRow, startColumn + column, column);
            }
        }

        public void ExpandToFit(int rows, int columns) {
            var start = _source.Start;
            int startRow = start.Row;
            int startColumn = start.Column;

            if (rows > RowCount) {
                _sheet.InsertRow(startRow+1, rows - RowCount);
            }

            if (columns > ColumnCount) {
                _sheet.InsertColumn(startColumn+1, columns - ColumnCount);
            }
        }
    }

    ExcelPackage _package;

    Dictionary<string, Table> _tables = new Dictionary<string, Table>();
    Dictionary<string, Range> _namedRanges = new Dictionary<string, Range>();

    
    /// <summary>
    /// Imports an Excel file from the given path and wraps it in an editable form.
    /// </summary>
    /// <param name="filePath">Path to the .xlsx workbook. Relative paths are assumed to start at the Assets folder.</param>
    public ExcelImporter(string filePath) {        
        var path = Path.IsPathRooted(filePath) ? filePath : Path.Combine(Application.dataPath, filePath);
      
        using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)) {
            _package = new ExcelPackage();
            _package.Load(stream);
        }

        var workbook = _package.Workbook;

        foreach (var sheet in workbook.Worksheets) {
            
            foreach (var table in sheet.Tables) {
                _tables.Add(table.Name, new Table(table));
            }               
        }

        foreach (var range in workbook.Names) {
            var sheetName = range.FullAddress.Substring(0, range.FullAddress.IndexOf('!'));
            if (sheetName.StartsWith("'")) {
                sheetName = sheetName.Substring(1, sheetName.Length - 2);
            }
            var sheet = workbook.Worksheets[sheetName];
            
            _namedRanges.Add(range.Name, new Range(sheet, range));
        }
    }

    /// <summary>
    /// Gets a table from the workbook by name.
    /// </summary>
    /// <param name="name">The name of the table. Case sensitive.</param>
    /// <param name="table">Populated with the table found, if any.</param>
    /// <returns>True if the table is found, false otherwise.</returns>
    public bool TryGetTable(string name, out Table table) {
        if (_tables.TryGetValue(name, out table)) {
            return true;
        }
        return false;
    }

    /// <summary>
    /// Gets a named range from the workbook by name.
    /// </summary>
    /// <param name="name">The name of the range. Case sensitive.</param>
    /// <param name="range">Populated with the range found, if any.</param>
    /// <returns>True if the range is found, false otherwise.</returns>
    public bool TryGetNamedRange(string name, out Range range) {
        if (_namedRanges.TryGetValue(name, out range)) {
            return true;
        }
        return false;
    }

    /// <summary>
    /// Saves the excel file under a new name.
    /// </summary>
    /// <param name="filePath">Path to the .xlsx workbook. Relative paths are assumed to start at the Assets folder.</param>
    public void SaveAs(string filePath) {
        var path = Path.IsPathRooted(filePath) ? filePath : Path.Combine(Application.dataPath, filePath);
        var data = _package.GetAsByteArray();
        File.WriteAllBytes(path, data);
    }

    
}
