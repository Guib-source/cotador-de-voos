using System;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;

/// <summary>Entrada em centavos: 245000 vira 2.450,00.</summary>
public class MoneyTextBox : TextBox
{
    bool formatting;
    string previous = "";

    protected override void OnKeyPress(KeyPressEventArgs e)
    {
        if (!char.IsControl(e.KeyChar) && (e.KeyChar < '0' || e.KeyChar > '9'))
            e.Handled = true;
        base.OnKeyPress(e);
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (!e.Control && !e.Alt && !e.Shift && SelectionLength == 0 &&
            (e.KeyCode == Keys.Back || e.KeyCode == Keys.Delete))
        {
            int direction = e.KeyCode == Keys.Back ? -1 : 1;
            int index = SelectionStart + (direction < 0 ? -1 : 0);
            while (index >= 0 && index < Text.Length && !char.IsDigit(Text[index])) index += direction;
            if (index >= 0 && index < Text.Length)
            {
                Select(index, 1);
                SelectedText = "";
            }
            e.SuppressKeyPress = true;
        }
        base.OnKeyDown(e);
    }

    protected override void OnTextChanged(EventArgs e)
    {
        if (formatting) return;
        string input = Text;
        int digitsToRight = input.Substring(Math.Min(SelectionStart, input.Length)).Count(c => c >= '0' && c <= '9');
        string clean = input.Trim();
        if (clean.StartsWith("R$", StringComparison.OrdinalIgnoreCase)) clean = clean.Substring(2).Trim();
        string digits = new string(clean.Where(c => c >= '0' && c <= '9').ToArray()).TrimStart('0');
        bool valid = clean.All(c => (c >= '0' && c <= '9') || c == '.' || c == ',' || char.IsWhiteSpace(c)) && digits.Length <= 15;
        string normalized = previous;
        if (valid)
            normalized = input.Length == 0 ? "" : ((digits.Length == 0 ? 0m : decimal.Parse(digits, CultureInfo.InvariantCulture)) / 100m).ToString("N2", QuoteValidation.BrazilianCulture);
        formatting = true;
        try
        {
            if (Text != normalized) Text = normalized;
            int caret = Text.Length;
            if (valid)
                while (caret > 0 && digitsToRight > 0)
                    if (char.IsDigit(Text[--caret])) digitsToRight--;
            Select(caret, 0);
        }
        finally { formatting = false; }
        bool changed = previous != normalized;
        previous = normalized;
        if (changed) base.OnTextChanged(e);
    }
}
