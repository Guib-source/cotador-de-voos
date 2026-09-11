using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;

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
