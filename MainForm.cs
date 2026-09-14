using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

public partial class MainForm : Form
{
    DataGridView grid = new DataGridView();
    PictureBox preview = new PrintPreview();
    TextBox output = new TextBox(), raw = new TextBox(), price = new MoneyTextBox();
    CheckBox baggage = new CheckBox();
    NumericUpDown passengers = new NumericUpDown(), year = new NumericUpDown();
    CheckBox reviewed = new CheckBox();
    Label status = new Label();
    Button import = new ModernButton(), paste = new ModernButton();
    // Ponto de acesso mantido para as rotinas de importação e diagnóstico.
    public static Task<string> ReadPrint(string path, int scale)
    {
        return OcrClient.ReadAsync(path, scale);
    }

    async Task LoadImage(string path)
    {
        import.Enabled = paste.Enabled = false;
        reviewed.Checked = false;
        output.Clear();
        try
        {
            using (var img = Image.FromFile(path))
            {
                if (preview.Image != null)
                    preview.Image.Dispose();
                preview.Image = new Bitmap(img);
            }

            foreach (DataGridViewRow r in grid.Rows)
                foreach (DataGridViewCell c in r.Cells)
                    c.Value = "";
            status.Text = "Lendo o print no seu computador…";
            int selectedYear = (int)year.Value;
            raw.Text = await ReadPrint(path, 3);
            List<Flight> flights = null;
            try
            {
                flights = IdentifyPrint(raw.Text, selectedYear);
            }
            catch (QuoteReadException)
            {
            // Uma segunda escala pode separar caracteres que o primeiro OCR uniu.
            }

            if (flights == null)
            {
                // Releia a imagem: não atribua um horário arbitrário a um token ilegível.
                // A segunda falha é exibida ao usuário para preenchimento manual.
                status.Text = "Reprocessando caracteres pequenos com outra ampliação…";
                raw.Text = await ReadPrint(path, 2);
                flights = IdentifyPrint(raw.Text, selectedYear);
            }

            PopulateFlights(flights);

            bool airportChange = flights.Count == 2 &&
                (flights[0].To != flights[1].From || flights[0].From != flights[1].To);
            status.Text = multipleMode.Checked ? "Múltiplos trechos preenchidos: cada linha corresponde a um voo do print. Confira todos os dados." : airportChange
                ? "Ida e volta com troca de aeroporto. Confira os IATA, a companhia, o ano e a bagagem; informe o valor."
                : "Trechos identificados. Confira os voos, a companhia, o ano e a bagagem; informe o valor.";
        }
        catch (Exception ex)
        {
            status.Text = "Não foi possível preencher automaticamente. Você pode digitar os dados na tabela.";
            MessageBox.Show(ex.Message, "Leitura do print");
        }
        finally
        {
            import.Enabled = paste.Enabled = true;
        }
    }

    // Uma alteração invalida a confirmação anterior e evita copiar uma mensagem antiga.
    private void InvalidateReview()
    {
        reviewed.Checked = false;
        output.Clear();
    }

    private List<Flight> ReadReviewedFlights()
    {
        var flights = new List<Flight>();
        foreach (DataGridViewRow row in grid.Rows)
        {
            string[] values = row.Cells.Cast<DataGridViewCell>().Select(cell => Convert.ToString(cell.Value).Trim()).ToArray();
            Flight flight = QuoteValidation.ReadFlight(values);
            if (flight != null)
                flights.Add(flight);
        }

        if (flights.Count == 0)
            throw new ArgumentException("Preencha pelo menos a ida.");
        if (string.IsNullOrWhiteSpace(Convert.ToString(grid.Rows[0].Cells[0].Value)))
            throw new ArgumentException("Preencha a primeira linha com a ida.");
        if (!multipleMode.Checked && flights.Count == 2 && DateTime.ParseExact(flights[1].Date, QuoteValidation.DateFormat, CultureInfo.InvariantCulture) < DateTime.ParseExact(flights[0].Date, QuoteValidation.DateFormat, CultureInfo.InvariantCulture))
            throw new ArgumentException("A volta não pode ocorrer antes da ida.");
        if (multipleMode.Checked) Quote.MultipleSegments(flights);
        return flights;
    }

    private void Generate()
    {
        try
        {
            grid.EndEdit();
            if (!reviewed.Checked)
                throw new ArgumentException("Confira os dados com o print e marque a confirmação antes de copiar.");
            List<Flight> flights = ReadReviewedFlights();
            decimal amount = QuoteValidation.ReadAmount(price.Text);
            output.Text = Quote.Format(flights, (int)passengers.Value, amount.ToString("C", QuoteValidation.BrazilianCulture), Quote.Baggage(baggage.Checked), multipleMode.Checked);
            Clipboard.SetText(output.Text);
            status.Text = "Cotação copiada! Cole no WhatsApp ou no canal de atendimento.";
        }
        catch (Exception error)
        {
            // Fronteira da interface: inclui falhas transitórias do clipboard.
            MessageBox.Show(error.Message, "Confira os dados", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing && preview.Image != null)
        {
            preview.Image.Dispose();
            preview.Image = null;
        }

        if (disposing && entryMask != null) entryMask.Dispose();
        base.Dispose(disposing);
    }
}
