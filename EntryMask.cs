using System;
using System.Linq;
using System.Windows.Forms;

/// <summary>Máscara progressiva, preservando seleção e cursor por dígitos.</summary>
public sealed class EntryMask : IDisposable
{
    readonly TextBox input;
    readonly bool date;
    bool updating;
    public EntryMask(TextBox input, bool date)
    {
        this.input = input; this.date = date;
        input.TextChanged += Changed;
        input.KeyDown += KeyDown;
    }
    public static string Format(string text, bool date)
    {
        string digits = new string(text.Where(c => c >= '0' && c <= '9').ToArray());
        int limit = date ? 8 : 4;
        if (digits.Length > limit) digits = digits.Substring(0, limit);
        if (date && digits.Length > 4) digits = digits.Insert(4, "/");
        if (digits.Length > 2) digits = digits.Insert(2, date ? "/" : ":");
        return digits;
    }
    void Changed(object sender, EventArgs e)
    {
        if (updating) return;
        int count = input.Text.Take(input.SelectionStart).Count(char.IsDigit);
        string formatted = Format(input.Text, date);
        if (formatted == input.Text) return;
        updating = true;
        input.Text = formatted;
        int caret = 0;
        while (caret < formatted.Length && count > 0)
        { if (char.IsDigit(formatted[caret])) count--; caret++; }
        input.SelectionStart = caret;
        updating = false;
    }
    void KeyDown(object sender, KeyEventArgs e)
    {
        if (input.SelectionLength != 0 || e.Control || e.Alt || e.Shift) return;
        int caret = input.SelectionStart;
        if (e.KeyCode == Keys.Back && caret > 0 && !char.IsDigit(input.Text[caret - 1]))
        { input.SelectionStart = caret - 1; }
        if (e.KeyCode == Keys.Delete && caret < input.Text.Length && !char.IsDigit(input.Text[caret]))
        { input.SelectionStart = caret + 1; }
    }
    public void Dispose()
    {
        input.TextChanged -= Changed;
        input.KeyDown -= KeyDown;
    }
}
