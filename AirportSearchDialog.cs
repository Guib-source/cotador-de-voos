using System;
using System.Drawing;
using System.Windows.Forms;

public sealed class AirportSearchDialog : Form
{
    readonly TextBox search = new TextBox();
    readonly ListBox results = new ListBox();
    readonly Label count = new Label();
    readonly Button choose = new ModernButton();
    public string SelectedCode { get; private set; }

    public AirportSearchDialog(string initial, string target)
    {
        Text = "Buscar aeroporto — " + target;
        Font = new Font("Segoe UI", 10);
        ClientSize = new Size(740, 450); MinimumSize = new Size(560, 360);
        StartPosition = FormStartPosition.CenterParent;
        ShowInTaskbar = false; MinimizeBox = MaximizeBox = false;
        BackColor = Theme.Background;
        var layout = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(18), ColumnCount = 1, RowCount = 5 };
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 28));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 38));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 28));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
        Controls.Add(layout);
        layout.Controls.Add(new Label { Text = "Digite a cidade, o código IATA ou o nome do aeroporto", Dock = DockStyle.Fill });
        search.Dock = DockStyle.Fill; search.AccessibleName = "Buscar cidade, IATA ou aeroporto";
        layout.Controls.Add(search);
        results.Dock = DockStyle.Fill; results.IntegralHeight = false; results.HorizontalScrollbar = true;
        results.AccessibleName = "Aeroportos encontrados"; results.ItemHeight = 25;
        layout.Controls.Add(results);
        count.Dock = DockStyle.Fill; count.ForeColor = Theme.Muted;
        layout.Controls.Add(count);
        var actions = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft };
        choose.Text = "Selecionar"; choose.Size = new Size(120, 34);
        var cancel = new ModernButton { Text = "Cancelar", Size = new Size(120, 34), DialogResult = DialogResult.Cancel };
        actions.Controls.Add(choose); actions.Controls.Add(cancel); layout.Controls.Add(actions);
        AcceptButton = choose; CancelButton = cancel;
        search.TextChanged += (s, e) => RefreshResults();
        results.SelectedIndexChanged += (s, e) => choose.Enabled = results.SelectedItem != null;
        choose.Click += (s, e) => SelectAirport();
        results.DoubleClick += (s, e) => SelectAirport();
        search.KeyDown += (s, e) => { if (e.KeyCode == Keys.Down && results.Items.Count > 0) { results.Focus(); results.SelectedIndex = 0; e.Handled = true; } };
        search.Text = initial;
        RefreshResults();
        Shown += (s, e) => { search.Focus(); search.SelectAll(); };
    }
    void RefreshResults()
    {
        results.BeginUpdate(); results.Items.Clear();
        foreach (var airport in AirportCatalog.Search(search.Text)) results.Items.Add(airport);
        results.EndUpdate();
        // Exigir escolha explícita: uma cidade pode ter mais de um aeroporto.
        results.SelectedIndex = -1; choose.Enabled = false;
        count.Text = results.Items.Count == 0 ? "Nenhum resultado. Tente outra cidade, nome ou IATA." : "Selecione o aeroporto e confirme. Esc cancela.";
    }
    void SelectAirport()
    {
        var selected = results.SelectedItem as AirportChoice;
        if (selected == null) return;
        SelectedCode = selected.Code; DialogResult = DialogResult.OK; Close();
    }
}
