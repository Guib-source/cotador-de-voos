using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

public partial class MainForm
{
    readonly Button findAirport = new ModernButton();
    readonly Label reviewHint = new Label();
    readonly ToolTip reviewTips = new ToolTip();
    readonly Color attention = Color.FromArgb(255, 244, 204);
    bool reviewingCells;

    void ConfigureReview()
    {
        findAirport.Text = "Buscar aeroporto";
        findAirport.Size = new Size(145, 32);
        findAirport.Click += (s, e) => SearchAirport();
        reviewTips.SetToolTip(findAirport, "Selecione uma célula de origem ou destino. Atalho: F3.");
        reviewHint.Dock = DockStyle.Fill;
        reviewHint.ForeColor = Color.FromArgb(125, 82, 0);
        reviewHint.Font = new Font("Segoe UI", 9);
        reviewHint.AutoEllipsis = true;
        grid.CellValueChanged += (s, e) => RefreshReview();
        grid.CurrentCellChanged += (s, e) => RefreshReview();
        grid.RowsRemoved += (s, e) => RefreshReview();
        grid.KeyDown += AirportShortcut;
        grid.EditingControlShowing += (s, e) =>
        {
            e.Control.KeyDown -= AirportShortcut;
            e.Control.KeyDown += AirportShortcut;
        };
        grid.CellEndEdit += (s, e) =>
        {
            if (e.ColumnIndex <= 1)
            {
                var cell = grid.Rows[e.RowIndex].Cells[e.ColumnIndex];
                string value = Convert.ToString(cell.Value).Trim().ToUpperInvariant();
                if (Convert.ToString(cell.Value) != value) cell.Value = value;
            }
            RefreshReview();
        };
        RefreshReview();
    }
    void AirportShortcut(object sender, KeyEventArgs e)
    {
        if (e.KeyCode != Keys.F3) return;
        e.Handled = e.SuppressKeyPress = true;
        SearchAirport();
    }
    void SearchAirport()
    {
        if (grid.CurrentRow == null) return;
        grid.EndEdit();
        int column = grid.CurrentCell != null && grid.CurrentCell.ColumnIndex == 1 ? 1 : 0;
        var cell = grid.CurrentRow.Cells[column];
        grid.CurrentCell = cell;
        using (var picker = new AirportSearchDialog(Convert.ToString(cell.Value), (column == 0 ? "Origem" : "Destino") + " · " + grid.CurrentRow.HeaderCell.Value))
            if (picker.ShowDialog(this) == DialogResult.OK)
                cell.Value = picker.SelectedCode;
    }
    void RefreshReview()
    {
        if (reviewingCells || grid.Columns.Count < 8) return;
        reviewingCells = true;
        try
        {
            bool any = false;
            string selected = "";
            foreach (DataGridViewRow row in grid.Rows)
            {
                var notices = row.Tag as List<FlightNotice> ?? new List<FlightNotice>();
                string[] values = row.Cells.Cast<DataGridViewCell>().Select(c => Convert.ToString(c.Value).Trim()).ToArray();
                var warnings = FlightReview.Inspect(values, notices);
                any |= warnings.Count > 0;
                row.ErrorText = warnings.Count > 0 ? "Confira os campos destacados. " + string.Join(" ", warnings.Values) : "";
                foreach (DataGridViewCell cell in row.Cells)
                {
                    string warning;
                    bool flagged = warnings.TryGetValue(cell.ColumnIndex, out warning);
                    cell.Style.BackColor = flagged ? attention : Color.Empty;
                    cell.Style.SelectionBackColor = flagged ? Color.FromArgb(255, 227, 150) : Color.Empty;
                    cell.ToolTipText = flagged ? warning : "";
                    if (grid.CurrentCell == cell && flagged) selected = warning;
                }
                if (grid.CurrentRow == row)
                {
                    string warning;
                    bool flagged = warnings.TryGetValue(7, out warning);
                    connectionEditor.BackColor = flagged ? attention : Theme.Background;
                    reviewTips.SetToolTip(connectionEditor, flagged ? warning : "Conexões ou voo direto do trecho selecionado.");
                    if (selected == "" && flagged) selected = warning;
                }
            }
            reviewHint.Text = selected != "" ? "Atenção: " + selected : (any ? "Atenção: confira os campos amarelos. Selecione um campo para ver o motivo." : "Origem e destino: selecione o campo e use Buscar aeroporto ou F3.");
            reviewHint.AccessibleDescription = reviewHint.Text;
            reviewTips.SetToolTip(reviewHint, reviewHint.Text);
        }
        finally { reviewingCells = false; }
    }
}
