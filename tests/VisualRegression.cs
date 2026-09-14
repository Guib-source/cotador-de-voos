using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;
using System.Reflection;
using System.Collections.Generic;

internal class ButtonProbe : ModernButton
{
    public void Hover() { OnMouseEnter(EventArgs.Empty); }
    public void Press() { OnMouseDown(new MouseEventArgs(MouseButtons.Left, 1, 20, 20, 0)); }
    public void Release() { OnMouseUp(new MouseEventArgs(MouseButtons.Left, 1, 20, 20, 0)); OnMouseLeave(EventArgs.Empty); }
}

internal static class VisualRegression
{
    static int assertions;
    static void Check(bool condition, string message)
    {
        if (!condition) throw new Exception(message);
        assertions++;
    }

    [STAThread]
    static int Main(string[] args)
    {
        try
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Directory.CreateDirectory(args[0]);
            // Renderização fora da tela: sem abrir janelas ou alterar clipboard.
            using (var surface = new Panel { Size = new Size(1600, 1000) })
            using (var form = new MainForm())
            {
                form.TopLevel = false;
                form.FormBorderStyle = FormBorderStyle.None;
                surface.Controls.Add(form);
                form.Visible = true;
                foreach (var size in new[] { new Size(1400, 900), new Size(1160, 800) })
                {
                    form.ClientSize = size;
                    form.CreateControl();
                    form.PerformLayout();
                    using (var bitmap = new Bitmap(size.Width, size.Height))
                    {
                        form.DrawToBitmap(bitmap, new Rectangle(Point.Empty, size));
                        bitmap.Save(Path.Combine(args[0], "interface-" + size.Width + ".png"), ImageFormat.Png);
                    }
                }
                Check(form.Icon != null, "Ícone da janela ausente.");
                var flags = BindingFlags.NonPublic | BindingFlags.Instance;
                var table = (DataGridView)typeof(MainForm).GetField("grid", flags).GetValue(form);
                var mode = (CheckBox)typeof(MainForm).GetField("multipleMode", flags).GetValue(form);
                var add = (Button)typeof(MainForm).GetField("addSegment", flags).GetValue(form);
                var remove = (Button)typeof(MainForm).GetField("removeSegment", flags).GetValue(form);
                string sample = "LATAM 3313 12 Nov 04:35h 12 Nov 07:05h SLZ BSB\nLATAM 3024 16 Nov 07:00h 16 Nov 09:35h BSB FOR\nLATAM 3025 20 Nov 07:00h 20 Nov 09:35h FOR REC";
                var flights = (List<Flight>)typeof(MainForm).GetMethod("IdentifyPrint", flags).Invoke(form, new object[] { sample, 2026 });
                typeof(MainForm).GetMethod("PopulateFlights", flags).Invoke(form, new object[] { flights });
                Check(mode.Checked && table.Rows.Count == 3, "Importar e preencher todos os trechos na interface.");
                Check(Convert.ToString(table.Rows[2].Cells[1].Value) == "REC", "Terceiro destino preservado na tabela.");
                Check(Convert.ToString(table.Rows[0].Cells[2].FormattedValue) == "12/11/26", "Exibir data curta do OCR.");
                mode.Checked = false;
                Check(mode.Checked && table.Rows.Count == 3, "Não perder trechos ao trocar modo.");
                add.PerformClick();
                Check(table.Rows.Count == 4 && table.CurrentRow.Index == 3, "Adicionar e selecionar novo trecho.");
                remove.PerformClick();
                Check(table.Rows.Count == 3, "Remover trecho selecionado.");
                table.CurrentCell = table.Rows[0].Cells[2]; table.BeginEdit(true);
                var edit = (TextBox)table.EditingControl;
                edit.SelectAll(); edit.SelectedText = "131126";
                Check(edit.Text == "13/11/26", "Máscara ativa na edição de data da tabela.");
                table.EndEdit();
                table.CurrentCell = table.Rows[0].Cells[4]; table.BeginEdit(true);
                edit = (TextBox)table.EditingControl; edit.SelectAll(); edit.SelectedText = "0545";
                Check(edit.Text == "05:45", "Máscara ativa na edição de hora da tabela.");
                table.EndEdit();
                table.CurrentCell = table.Rows[0].Cells[6]; table.BeginEdit(true);
                edit = (TextBox)table.EditingControl; edit.SelectAll(); edit.SelectedText = "LATAM";
                Check(edit.Text == "LATAM", "Desassociar máscara nas demais colunas.");
                table.EndEdit();
                foreach (var size in new[] { new Size(1400, 900), new Size(1160, 800) })
                {
                    form.ClientSize = size; form.PerformLayout();
                    using (var bitmap = new Bitmap(size.Width, size.Height))
                    { form.DrawToBitmap(bitmap, new Rectangle(Point.Empty, size)); bitmap.Save(Path.Combine(args[0], "multiple-" + size.Width + ".png"), ImageFormat.Png); }
                }
            }
            using (var text = new TextBox())
            using (var mask = new EntryMask(text, true))
            {
                foreach (char c in "121126") { text.SelectionStart = text.TextLength; text.SelectedText = c.ToString(); }
                Check(text.Text == "12/11/26", "Digitação progressiva de data.");
                text.SelectAll(); text.SelectedText = "290224";
                Check(text.Text == "29/02/24", "Substituição de data selecionada.");
                text.SelectAll(); text.SelectedText = "";
                Check(text.Text == "", "Limpar data por seleção.");
            }
            using (var host = new Panel { Size = new Size(700, 390), BackColor = Theme.Background })
            using (var card = new ModernCard { Bounds = new Rectangle(10, 10, 680, 370) })
            using (var layout = new Panel { Bounds = new Rectangle(20, 20, 640, 330), BackColor = Color.Transparent })
            using (var sheet = new Bitmap(640, 330))
            using (var graphics = Graphics.FromImage(sheet))
            {
                host.Controls.Add(card); card.Controls.Add(layout);
                graphics.Clear(Color.White);
                int row = 0;
                foreach (bool primary in new[] { false, true })
                using (var button = new ButtonProbe { Text = primary ? "Gerar e copiar cotação" : "Importar print", Primary = primary, Size = new Size(280, 48) })
                {
                    layout.Controls.Add(button);
                    foreach (string state in new[] { "normal", "hover", "pressionado", "desabilitado" })
                    {
                        button.Release(); button.Enabled = state != "desabilitado";
                        if (state == "hover") button.Hover();
                        if (state == "pressionado") button.Press();
                        using (var image = new Bitmap(button.Width, button.Height))
                        {
                            button.DrawToBitmap(image, button.ClientRectangle);
                            foreach (var point in new[] { new Point(0,0), new Point(button.Width-1,0), new Point(0,button.Height-1), new Point(button.Width-1,button.Height-1) })
                                Check(image.GetPixel(point.X,point.Y).ToArgb() == Color.White.ToArgb(), "Canto incorreto: " + primary + " / " + state);
                            int column = primary ? 320 : 0;
                            graphics.DrawString(state, SystemFonts.DefaultFont, Brushes.Gray, column, row * 80);
                            graphics.DrawImageUnscaled(image, column, row * 80 + 20);
                        }
                        row = (row + 1) % 4;
                    }
                    // O mesmo controle precisa limpar os cantos após mudança de tamanho.
                    button.Size = new Size(140, 40);
                    using (var resized = new Bitmap(140,40))
                    {
                        button.DrawToBitmap(resized, button.ClientRectangle);
                        Check(resized.GetPixel(139,39).ToArgb() == Color.White.ToArgb(), "Canto após redimensionar.");
                    }
                }
                sheet.Save(Path.Combine(args[0], "botoes.png"), ImageFormat.Png);
            }
            using (var money = new MoneyProbe())
            {
                money.CreateControl();
                foreach (string sample in new[] { "1|0,01", "12|0,12", "123|1,23", "245000|2.450,00", "000001|0,01", "R$ 2.450,00|2.450,00", "999999999999999|9.999.999.999.999,99" })
                {
                    var pair = sample.Split('|');
                    money.Text = pair[0];
                    Check(money.Text == pair[1], "Máscara de valor: " + pair[0]);
                }
                money.Text = "245000";
                money.Text = "-500";
                Check(money.Text == "2.450,00", "Não transformar valor negativo colado em positivo.");
                money.Text = "9999999999999999";
                Check(money.Text == "2.450,00", "Valor acima do limite não deve ser truncado.");
                money.Clear();
                foreach (char digit in "245000") { money.Select(money.TextLength, 0); money.SelectedText = digit.ToString(); }
                Check(money.Text == "2.450,00", "Digitação sucessiva com máscara.");
                money.Select(money.TextLength, 0); money.DeleteKey(Keys.Back);
                Check(money.Text == "245,00", "Backspace remove último dígito.");
                money.Text = "123456"; money.Select(5, 0); money.DeleteKey(Keys.Back);
                Check(money.Text == "123,56", "Backspace pula vírgula e remove dígito à esquerda.");
                money.Text = "123456"; money.Select(1, 0); money.DeleteKey(Keys.Delete);
                Check(money.Text == "134,56", "Delete pula ponto e remove dígito à direita.");
                money.SelectAll(); money.SelectedText = "5000";
                Check(money.Text == "50,00", "Substituição da seleção inteira.");
                money.Clear(); Check(money.Text == "", "Nova cotação pode limpar o valor.");
            }
            Console.WriteLine("PASS visual: " + assertions + " verificações.");
            return 0;
        }
        catch (Exception error) { Console.Error.WriteLine(error); return 1; }
    }
}

internal class MoneyProbe : MoneyTextBox
{
    public void DeleteKey(Keys key) { OnKeyDown(new KeyEventArgs(key)); }
}
