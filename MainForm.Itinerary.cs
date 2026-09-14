using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;

public partial class MainForm
{
    CheckBox multipleMode = new CheckBox();
    Button addSegment = new ModernButton(), removeSegment = new ModernButton();
    TextBox connectionEditor = new TextBox();
    Label connectionTitle = new Label();
    bool syncingConnection;
    EntryMask entryMask;

    void LabelRows()
    {
        grid.TopLeftHeaderCell.Value = multipleMode.Checked ? "Trecho" : "";
        foreach (DataGridViewRow row in grid.Rows)
        {
            row.Height = 40;
            row.HeaderCell.Value = multipleMode.Checked ? (row.Index + 1).ToString() : (row.Index == 0 ? "IDA" : "VOLTA");
        }
        addSegment.Enabled = removeSegment.Enabled = multipleMode.Checked;
        UpdateConnection();
    }
    void UpdateConnection()
    {
        syncingConnection = true;
        var row = grid.CurrentRow;
        connectionTitle.Text = "CONEXÕES / " + (row == null ? "selecione um trecho" : (multipleMode.Checked ? "TRECHO " : "") + Convert.ToString(row.HeaderCell.Value));
        connectionEditor.Text = row == null ? "" : Convert.ToString(row.Cells[7].Value);
        connectionEditor.Enabled = row != null;
        syncingConnection = false;
    }
    void ConfigureItinerary()
    {
        multipleMode.CheckedChanged += (s, e) =>
        {
            if (!multipleMode.Checked && grid.Rows.Count > 2)
            {
                multipleMode.Checked = true;
                status.Text = "Remova os trechos extras antes de voltar ao modo ida e volta.";
                return;
            }
            if (!multipleMode.Checked && grid.Rows.Count < 2) grid.Rows.Add();
            LabelRows(); InvalidateReview();
        };
        addSegment.Click += (s, e) =>
        {
            grid.EndEdit();
            int index = grid.Rows.Add();
            LabelRows(); grid.CurrentCell = grid.Rows[index].Cells[0]; InvalidateReview();
        };
        removeSegment.Click += (s, e) =>
        {
            if (grid.CurrentRow == null || grid.Rows.Count <= 1) return;
            grid.EndEdit(); grid.Rows.RemoveAt(grid.CurrentRow.Index); LabelRows(); InvalidateReview();
        };
        grid.SelectionChanged += (s, e) => UpdateConnection();
        grid.CellValueChanged += (s, e) => { if (e.ColumnIndex == 7) UpdateConnection(); };
        connectionEditor.TextChanged += (s, e) =>
        {
            if (!syncingConnection && grid.CurrentRow != null)
                grid.CurrentRow.Cells[7].Value = connectionEditor.Text;
        };
        grid.CellFormatting += (s, e) =>
        {
            DateTime value;
            if ((e.ColumnIndex == 2 || e.ColumnIndex == 3) && QuoteValidation.TryReadDate(Convert.ToString(e.Value), out value) && value.Year >= 2000 && value.Year < 2100)
            { e.Value = value.ToString("dd/MM/yy"); e.FormattingApplied = true; }
        };
        grid.EditingControlShowing += (s, e) =>
        {
            if (entryMask != null) { entryMask.Dispose(); entryMask = null; }
            var text = e.Control as TextBox;
            int column = grid.CurrentCell.ColumnIndex;
            if (text != null && column >= 2 && column <= 5)
                entryMask = new EntryMask(text, column <= 3);
        };
        LabelRows();
    }
    List<Flight> IdentifyPrint(string text, int selectedYear)
    {
        bool multiple;
        var result = Quote.Identify(Quote.Parse(text, selectedYear), multipleMode.Checked, out multiple);
        multipleMode.Checked = multiple;
        return result;
    }
    void PopulateFlights(List<Flight> flights)
    {
        grid.Rows.Clear();
        grid.Rows.Add(multipleMode.Checked ? flights.Count : 2);
        for (int i = 0; i < flights.Count; i++)
        {
            var f = flights[i];
            grid.Rows[i].Tag = f.Notices;
            grid.Rows[i].SetValues(f.From, f.To, f.Date, f.ArrivalDate, f.Departure, f.Arrival, f.Airline, f.Connection);
        }
        LabelRows();
        grid.CurrentCell = grid.Rows[0].Cells[0];
        UpdateConnection();
        RefreshReview();
    }
}
