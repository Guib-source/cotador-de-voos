using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

public partial class MainForm : Form
{
    static Label CreateLabel(string text, float size, Color color, bool bold = false)
    {
        return new Label
        {
            Text = text,
            AutoSize = false,
            Dock = DockStyle.Fill,
            ForeColor = color,
            Font = new Font("Segoe UI", size, bold ? FontStyle.Bold : FontStyle.Regular),
            TextAlign = ContentAlignment.MiddleLeft,
            Margin = new Padding(0)
        };
    }

    static TableLayoutPanel CreateVerticalLayout(params int[] heights)
    {
        var p = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = heights.Length,
            Margin = new Padding(0),
            BackColor = Color.Transparent
        };
        foreach (int h in heights)
            p.RowStyles.Add(new RowStyle(h == 0 ? SizeType.Percent : SizeType.Absolute, h == 0 ? 100 : h));
        return p;
    }

    static Panel CreateLabeledField(Control input, string label)
    {
        var box = CreateVerticalLayout(24, 0);
        box.Margin = new Padding(0, 0, 14, 0);
        box.Controls.Add(CreateLabel(label, 9, Theme.Muted));
        var field = new ModernField
        {
            Dock = DockStyle.Fill,
            Margin = new Padding(0, 0, 0, 15),
            Padding = new Padding(12, 10, 10, 8)
        };
        input.Dock = DockStyle.Fill;
        input.Font = new Font("Segoe UI", 11);
        var text = input as TextBox;
        if (text != null)
        {
            text.BorderStyle = BorderStyle.None;
            text.BackColor = Theme.Background;
        }

        var number = input as NumericUpDown;
        if (number != null)
        {
            number.BorderStyle = BorderStyle.None;
            number.BackColor = Theme.Background;
        }

        field.Controls.Add(input);
        box.Controls.Add(field);
        return box;
    }

    public MainForm()
    {
        Text = "Cotador de voos 3.8";
        using (var iconStream = typeof(MainForm).Assembly.GetManifestResourceStream("Cotador.ico"))
            if (iconStream != null)
                using (var loadedIcon = new Icon(iconStream)) Icon = (Icon)loadedIcon.Clone();
        ClientSize = new Size(1400, 900);
        MinimumSize = new Size(1160, 800);
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = Theme.Background;
        ForeColor = Theme.Ink;
        Font = new Font("Segoe UI", 10);
        AutoScaleMode = AutoScaleMode.Dpi;
        var page = CreateVerticalLayout(90, 0, 34);
        page.Padding = new Padding(28, 14, 28, 10);
        Controls.Add(page);
        ModernButton reset = BuildHeader(page);
        var columns = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            Margin = new Padding(0, 8, 0, 0)
        };
        columns.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 65));
        columns.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35));
        page.Controls.Add(columns);
        var leftHost = new Panel
        {
            Dock = DockStyle.Fill,
            AutoScroll = true,
            Margin = new Padding(0, 0, 18, 0)
        };
        columns.Controls.Add(leftHost);
        var left = CreateVerticalLayout(250, 0, 158);
        left.Dock = DockStyle.Top;
        left.Height = 740;
        leftHost.Controls.Add(left);
        leftHost.Resize += (s, e) =>
        {
            left.Height = Math.Max(740, leftHost.ClientSize.Height);
        };
        BuildImageCard(left);
        BuildFlightCard(left);
        BuildDetailsCard(left);
        ModernButton copy = BuildQuoteCard(columns, page);
        BindActions(copy, reset);
    }

    // A construção visual fica separada dos eventos e das regras de cotação.
    private ModernButton BuildHeader(TableLayoutPanel page)
    {
        var header = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            Margin = new Padding(0)
        };
        header.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 75));
        header.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
        page.Controls.Add(header);
        var identity = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, Margin = new Padding(0) };
        identity.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 68));
        identity.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        header.Controls.Add(identity);
        var brandIcon = new PictureBox { Size = new Size(52, 52), SizeMode = PictureBoxSizeMode.Zoom, Margin = new Padding(0, 10, 16, 0) };
        using (var logoStream = typeof(MainForm).Assembly.GetManifestResourceStream("Cotador.png"))
            if (logoStream != null)
                using (var logo = Image.FromStream(logoStream)) brandIcon.Image = new Bitmap(logo);
        brandIcon.Disposed += (s, e) => { if (brandIcon.Image != null) brandIcon.Image.Dispose(); };
        identity.Controls.Add(brandIcon);
        var brand = CreateVerticalLayout(20, 38, 24);
        identity.Controls.Add(brand);
        brand.Controls.Add(CreateLabel("C O T A D O R   /   V O O S    ·    3.8", 9, Theme.Accent, true));
        brand.Controls.Add(CreateLabel("Sua próxima cotação começa aqui.", 22, Theme.Ink, true));
        brand.Controls.Add(CreateLabel("Transforme um print em uma mensagem pronta para o seu cliente.", 10, Theme.Muted));
        var reset = new ModernButton
        {
            Text = "＋  Nova cotação",
            Width = 160,
            Height = 40,
            Anchor = AnchorStyles.Right | AnchorStyles.Top,
            Margin = new Padding(0, 20, 0, 0)
        };
        header.Controls.Add(reset);
        return reset;
    }

    private void BuildImageCard(TableLayoutPanel left)
    {
        var imageCard = new ModernCard
        {
            Dock = DockStyle.Fill,
            Margin = new Padding(0, 0, 0, 14)
        };
        left.Controls.Add(imageCard);
        var imageLayout = CreateVerticalLayout(30, 0, 42);
        imageCard.Controls.Add(imageLayout);
        var imageHead = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            Margin = new Padding(0)
        };
        imageHead.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60));
        imageHead.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40));
        imageLayout.Controls.Add(imageHead);
        imageHead.Controls.Add(CreateLabel("01   Imagem dos voos", 12, Theme.Ink, true));
        var toggle = new LinkLabel
        {
            Text = "Ver texto reconhecido",
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleRight,
            LinkColor = Theme.Accent,
            ActiveLinkColor = Theme.Ink,
            LinkBehavior = LinkBehavior.HoverUnderline,
            Font = new Font("Segoe UI", 9)
        };
        imageHead.Controls.Add(toggle);
        var imageArea = new Panel
        {
            Dock = DockStyle.Fill,
            Margin = new Padding(0, 5, 0, 8)
        };
        imageLayout.Controls.Add(imageArea);
        preview.Dock = DockStyle.Fill;
        imageArea.Controls.Add(preview);
        preview.Cursor = Cursors.Hand;
        preview.DoubleClick += (s, e) =>
        {
            if (preview.Image == null)
                return;
            using (var viewer = new Form
            {
                Text = "Conferir print • feche para voltar",
                Size = new Size(1150, 650),
                StartPosition = FormStartPosition.CenterParent
            }

            )
            {
                viewer.Controls.Add(new PictureBox { Dock = DockStyle.Fill, SizeMode = PictureBoxSizeMode.Zoom, Image = preview.Image, BackColor = Color.White });
                viewer.ShowDialog(this);
            }
        };
        raw.Multiline = true;
        raw.ScrollBars = ScrollBars.Vertical;
        raw.Dock = DockStyle.Fill;
        raw.ReadOnly = true;
        raw.BorderStyle = BorderStyle.None;
        raw.BackColor = Theme.Background;
        raw.Font = new Font("Segoe UI", 9);
        raw.Visible = false;
        imageArea.Controls.Add(raw);
        toggle.LinkClicked += (s, e) =>
        {
            raw.Visible = !raw.Visible;
            preview.Visible = !raw.Visible;
            toggle.Text = raw.Visible ? "Voltar para imagem" : "Ver texto reconhecido";
        };
        var imageActions = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 4,
            Margin = new Padding(0)
        };
        imageActions.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 148));
        imageActions.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 140));
        imageActions.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        imageActions.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 82));
        imageLayout.Controls.Add(imageActions);
        import.Text = "↑  Importar print";
        paste.Text = "Colar imagem";
        import.Dock = paste.Dock = DockStyle.Fill;
        import.Margin = new Padding(0, 0, 8, 0);
        paste.Margin = new Padding(0);
        imageActions.Controls.Add(import);
        imageActions.Controls.Add(paste);
        imageActions.Controls.Add(new Label { Text = "Ano da ida", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleRight, ForeColor = Theme.Muted, Padding = new Padding(0, 0, 10, 0) });
        year.Minimum = 2020;
        year.Maximum = 2100;
        year.Value = DateTime.Now.Year;
        year.BorderStyle = BorderStyle.FixedSingle;
        year.Dock = DockStyle.Fill;
        year.Margin = new Padding(0, 9, 0, 0);
        imageActions.Controls.Add(year);
    }

    private void BuildFlightCard(TableLayoutPanel left)
    {
        var flightCard = new ModernCard
        {
            Dock = DockStyle.Fill,
            Margin = new Padding(0, 0, 0, 14)
        };
        left.Controls.Add(flightCard);
        var flightLayout = CreateVerticalLayout(29, 26, 116, 24, 0);
        flightCard.Controls.Add(flightLayout);
        flightLayout.Controls.Add(CreateLabel("02   Revise seu itinerário", 12, Theme.Ink, true));
        flightLayout.Controls.Add(CreateLabel("Edite os campos abaixo. Para somente ida, deixe a volta vazia.", 9, Theme.Muted));
        grid.Dock = DockStyle.Fill;
        grid.AllowUserToAddRows = false;
        grid.AllowUserToDeleteRows = false;
        grid.AllowUserToResizeRows = false;
        grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        grid.RowHeadersWidth = 96;
        grid.BackgroundColor = Color.White;
        grid.BorderStyle = BorderStyle.None;
        grid.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
        grid.GridColor = Theme.Line;
        grid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 250, 251);
        grid.EnableHeadersVisualStyles = false;
        grid.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
        grid.RowHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
        grid.ColumnHeadersHeight = 30;
        grid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
        grid.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
        {
            BackColor = Theme.Background,
            ForeColor = Theme.Muted,
            Font = new Font("Segoe UI", 8),
            WrapMode = DataGridViewTriState.False,
            Padding = new Padding(1)
        };
        grid.RowHeadersDefaultCellStyle = new DataGridViewCellStyle
        {
            BackColor = Color.White,
            ForeColor = Theme.Accent,
            SelectionBackColor = Color.White,
            SelectionForeColor = Theme.Accent,
            Font = new Font("Segoe UI", 9, FontStyle.Bold)
        };
        grid.DefaultCellStyle = new DataGridViewCellStyle
        {
            BackColor = Color.White,
            ForeColor = Theme.Ink,
            SelectionBackColor = Color.FromArgb(222, 243, 237),
            SelectionForeColor = Theme.Ink,
            Font = new Font("Segoe UI", 10),
            Padding = new Padding(3)
        };
        foreach (var name in new[]
        {
            "Origem",
            "Destino",
            "Data saída",
            "Data chegada",
            "Saída",
            "Chegada",
            "CIA",
            "Tipo de voo"
        }

        )
            grid.Columns.Add(name, name);
        grid.Columns[7].Visible = false;
        grid.Columns[2].HeaderText = "Dt. saída";
        grid.Columns[3].HeaderText = "Dt. chegada";
        grid.Columns[2].ToolTipText = "Data de saída (dd/MM/aaaa)";
        grid.Columns[3].ToolTipText = "Data de chegada (dd/MM/aaaa)";
        grid.Columns[2].FillWeight = 125;
        grid.Columns[3].FillWeight = 125;
        grid.Columns[4].FillWeight = 80;
        grid.Columns[5].FillWeight = 100;
        grid.Rows.Add(2);
        grid.Rows[0].Height = 40;
        grid.Rows[1].Height = 40;
        grid.Rows[0].HeaderCell.Value = "IDA";
        grid.Rows[1].HeaderCell.Value = "VOLTA";
        flightLayout.Controls.Add(grid);
        flightLayout.Controls.Add(CreateLabel("CONEXÕES   /   confira os aeroportos e horários", 8, Theme.Muted, true));
        var connections = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            Margin = new Padding(0)
        };
        connections.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        connections.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        flightLayout.Controls.Add(connections);
        for (int i = 0; i < 2; i++)
        {
            int row = i;
            var connectionBox = CreateVerticalLayout(22, 0);
            connectionBox.BackColor = Theme.Background;
            connectionBox.Padding = new Padding(10, 5, 10, 7);
            connectionBox.Margin = new Padding(i == 0 ? 0 : 5, 0, i == 0 ? 5 : 0, 0);
            connections.Controls.Add(connectionBox);
            connectionBox.Controls.Add(CreateLabel(i == 0 ? "IDA" : "VOLTA", 8, Theme.Accent, true));
            var editor = new TextBox
            {
                Multiline = true,
                Dock = DockStyle.Fill,
                BorderStyle = BorderStyle.None,
                ScrollBars = ScrollBars.Vertical,
                BackColor = Theme.Background,
                ForeColor = Theme.Ink,
                Font = new Font("Segoe UI", 9)
            };
            connectionBox.Controls.Add(editor);
            editor.TextChanged += (s, e) =>
            {
                if (Convert.ToString(grid.Rows[row].Cells[7].Value) != editor.Text)
                    grid.Rows[row].Cells[7].Value = editor.Text;
            };
            grid.CellValueChanged += (s, e) =>
            {
                if (e.RowIndex == row && e.ColumnIndex == 7)
                {
                    var value = Convert.ToString(grid.Rows[row].Cells[7].Value);
                    if (editor.Text != value)
                        editor.Text = value;
                }
            };
        }
    }

    private void BuildDetailsCard(TableLayoutPanel left)
    {
        var detailCard = new ModernCard
        {
            Dock = DockStyle.Fill,
            Margin = new Padding(0)
        };
        left.Controls.Add(detailCard);
        var detailLayout = CreateVerticalLayout(32, 0);
        detailCard.Controls.Add(detailLayout);
        detailLayout.Controls.Add(CreateLabel("03   Valor e bagagem", 12, Theme.Ink, true));
        var details = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 3,
            Margin = new Padding(0)
        };
        details.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 26));
        details.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 19));
        details.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 55));
        detailLayout.Controls.Add(details);
        passengers.Minimum = 1;
        passengers.Maximum = 99;
        passengers.Value = 1;
        details.Controls.Add(CreateLabeledField(price, "Valor total · R$"));
        details.Controls.Add(CreateLabeledField(passengers, "Passageiros"));
        var bagPanel = CreateVerticalLayout(24, 28, 0);
        bagPanel.Margin = new Padding(8, 0, 0, 0);
        details.Controls.Add(bagPanel);
        bagPanel.Controls.Add(CreateLabel("Bagagem incluída", 9, Theme.Muted));
        baggage.Text = "Inclui bagagem despachada";
        baggage.Dock = DockStyle.Fill;
        baggage.Font = new Font("Segoe UI", 10);
        bagPanel.Controls.Add(baggage);
        var bagHint = CreateLabel("Desmarcado: somente bagagem de mão", 8.5f, Theme.Muted);
        bagPanel.Controls.Add(bagHint);
    }

    private ModernButton BuildQuoteCard(TableLayoutPanel columns, TableLayoutPanel page)
    {
        var quoteCard = new ModernCard
        {
            Dock = DockStyle.Fill,
            Margin = new Padding(0)
        };
        columns.Controls.Add(quoteCard);
        var quoteLayout = CreateVerticalLayout(24, 35, 43, 0, 60, 48, 25);
        quoteCard.Controls.Add(quoteLayout);
        quoteLayout.Controls.Add(CreateLabel("MENSAGEM PARA O CLIENTE", 8, Theme.Accent, true));
        quoteLayout.Controls.Add(CreateLabel("Sua cotação", 21, Theme.Ink, true));
        quoteLayout.Controls.Add(CreateLabel("Revise os dados e gere a mensagem\nno formato de atendimento.", 9.5f, Theme.Muted));
        var quoteArea = new Panel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(14),
            BackColor = Color.FromArgb(246, 249, 250),
            Margin = new Padding(0, 8, 0, 10)
        };
        quoteLayout.Controls.Add(quoteArea);
        output.Dock = DockStyle.Fill;
        output.Multiline = true;
        output.ScrollBars = ScrollBars.Vertical;
        output.Font = new Font("Segoe UI", 10);
        output.BackColor = quoteArea.BackColor;
        output.ForeColor = Theme.Ink;
        output.BorderStyle = BorderStyle.None;
        output.ReadOnly = true;
        quoteArea.Controls.Add(output);
        var empty = CreateLabel("Tudo pronto para começar.\n\nImporte o print, confira os voos\ne informe o valor da viagem.\n\nSua mensagem aparecerá aqui\nao gerar a cotação.", 11, Theme.Muted);
        empty.TextAlign = ContentAlignment.MiddleCenter;
        empty.BackColor = quoteArea.BackColor;
        quoteArea.Controls.Add(empty);
        empty.BringToFront();
        output.Visible = false;
        output.TextChanged += (s, e) =>
        {
            empty.Visible = output.Text.Length == 0;
            output.Visible = output.Text.Length > 0;
        };
        reviewed.Text = "Conferi os voos, as datas,\no valor e a bagagem.";
        reviewed.Dock = DockStyle.Fill;
        reviewed.Font = new Font("Segoe UI", 9.5f);
        quoteLayout.Controls.Add(reviewed);
        var copy = new ModernButton
        {
            Text = "Gerar e copiar cotação   →",
            Primary = true,
            Dock = DockStyle.Fill,
            Margin = new Padding(0)
        };
        quoteLayout.Controls.Add(copy);
        quoteLayout.Controls.Add(CreateLabel("Depois, é só colar no atendimento.", 8.5f, Theme.Muted));
        status.Dock = DockStyle.Fill;
        status.Font = new Font("Segoe UI", 9);
        status.ForeColor = Theme.Muted;
        status.TextAlign = ContentAlignment.BottomLeft;
        status.Text = "Pronto para uma nova cotação.";
        page.Controls.Add(status);
        return copy;
    }

    private void BindActions(ModernButton copy, ModernButton reset)
    {
        import.Click += async (s, e) =>
        {
            using (var d = new OpenFileDialog
            {
                Filter = "Imagens|*.png;*.jpg;*.jpeg;*.bmp"
            }

            )
                if (d.ShowDialog() == DialogResult.OK)
                    await LoadImage(d.FileName);
        };
        paste.Click += async (s, e) =>
        {
            try
            {
                if (!Clipboard.ContainsImage())
                {
                    MessageBox.Show("Copie uma imagem para a área de transferência.");
                    return;
                }

                string p = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".png");
                using (var img = Clipboard.GetImage())
                    img.Save(p, System.Drawing.Imaging.ImageFormat.Png);
                try
                {
                    await LoadImage(p);
                }
                finally
                {
                    File.Delete(p);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        };
        copy.Click += (s, e) => Generate();
        reset.Click += (s, e) =>
        {
            foreach (DataGridViewRow r in grid.Rows)
                foreach (DataGridViewCell c in r.Cells)
                    c.Value = "";
            raw.Clear();
            output.Clear();
            price.Clear();
            baggage.Checked = false;
            reviewed.Checked = false;
            if (preview.Image != null)
                preview.Image.Dispose();
            preview.Image = null;
            status.Text = "Pronto para uma nova cotação.";
        };
        grid.CellValueChanged += (s, e) =>
        {
            InvalidateReview();
        };
        baggage.CheckedChanged += (s, e) =>
        {
            InvalidateReview();
        };
        price.TextChanged += (s, e) =>
        {
            InvalidateReview();
        };
        passengers.ValueChanged += (s, e) =>
        {
            InvalidateReview();
        };
    }
}
