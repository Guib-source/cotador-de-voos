using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

// Paleta e geometria compartilhadas pelos controles personalizados.
public static class Theme
{
    public static Color Ink = Color.FromArgb(22, 41, 55), Muted = Color.FromArgb(106, 123, 136), Accent = Color.FromArgb(0, 119, 110), Background = Color.FromArgb(241, 245, 247), Line = Color.FromArgb(222, 231, 235);
    public static GraphicsPath Round(Rectangle r, int radius)
    {
        var p = new GraphicsPath();
        int d = Math.Max(1, Math.Min(radius * 2, Math.Min(r.Width, r.Height)));
        p.AddArc(r.X, r.Y, d, d, 180, 90);
        p.AddArc(r.Right - d, r.Y, d, d, 270, 90);
        p.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
        p.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
        p.CloseFigure();
        return p;
    }
}

// Double buffering evita cintilação ao redesenhar os cantos arredondados.
public class ModernCard : Panel
{
    public ModernCard()
    {
        DoubleBuffered = true;
        BackColor = Color.Transparent;
        Padding = new Padding(20);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        using (var path = Theme.Round(new Rectangle(0, 0, Width - 1, Height - 1), 14))
        {
            e.Graphics.FillPath(Brushes.White, path);
            using (var pen = new Pen(Theme.Line))
                e.Graphics.DrawPath(pen, path);
        }
    }
}

public class ModernButton : Button
{
    public bool Primary = false;
    bool hover, pressed;
    public ModernButton()
    {
        FlatStyle = FlatStyle.Flat;
        FlatAppearance.BorderSize = 0;
        UseVisualStyleBackColor = false;
        Cursor = Cursors.Hand;
        Height = 40;
        Font = new Font("Segoe UI", 10, FontStyle.Bold);
        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.SupportsTransparentBackColor | ControlStyles.ResizeRedraw, true);
        BackColor = Color.Transparent;
    }

    protected override void OnPaintBackground(PaintEventArgs e)
    {
        // ButtonBase pode deixar os cantos sem pintar no modo Flat + UserPaint.
        // Reproduza o fundo real do pai, inclusive painéis transparentes sobre cartões.
        if (Parent == null) { e.Graphics.Clear(Theme.Background); return; }
        var state = e.Graphics.Save();
        try
        {
            e.Graphics.TranslateTransform(-Left, -Top);
            using (var background = new PaintEventArgs(e.Graphics, Bounds))
            {
                InvokePaintBackground(Parent, background);
                InvokePaint(Parent, background);
            }
        }
        finally { e.Graphics.Restore(state); }
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Left) pressed = true;
        Invalidate();
        base.OnMouseDown(e);
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
        pressed = false;
        Invalidate();
        base.OnMouseUp(e);
    }

    protected override void OnMouseCaptureChanged(EventArgs e)
    {
        pressed = false;
        Invalidate();
        base.OnMouseCaptureChanged(e);
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Space) { pressed = true; Invalidate(); }
        base.OnKeyDown(e);
    }

    protected override void OnKeyUp(KeyEventArgs e)
    {
        pressed = false;
        Invalidate();
        base.OnKeyUp(e);
    }

    protected override void OnLostFocus(EventArgs e)
    {
        pressed = false;
        Invalidate();
        base.OnLostFocus(e);
    }

    protected override void OnMouseEnter(EventArgs e)
    {
        hover = true;
        Invalidate();
        base.OnMouseEnter(e);
    }

    protected override void OnMouseLeave(EventArgs e)
    {
        hover = false;
        pressed = false;
        Invalidate();
        base.OnMouseLeave(e);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        if (Width < 4 || Height < 4) return;
        // O renderizador Flat nem sempre chama OnPaintBackground antes de OnPaint.
        OnPaintBackground(e);
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        Color color = Primary ? (hover ? Color.FromArgb(0, 96, 89) : Theme.Accent) : (hover ? Color.FromArgb(230, 240, 240) : Color.FromArgb(242, 247, 247));
        if (pressed) color = Primary ? Color.FromArgb(0, 79, 74) : Color.FromArgb(211, 229, 228);
        if (!Enabled)
            color = Theme.Line;
        using (var path = Theme.Round(new Rectangle(1, 1, Width - 3, Height - 3), 9))
        {
            using (var brush = new SolidBrush(color))
                e.Graphics.FillPath(brush, path);
            using (var border = new Pen(Primary ? color : Color.FromArgb(203, 219, 223)))
                e.Graphics.DrawPath(border, path);
        }

        var textBounds = new Rectangle(8, pressed ? 1 : 0, Math.Max(1, Width - 16), Height);
        TextRenderer.DrawText(e.Graphics, Text, Font, textBounds, !Enabled ? Theme.Muted : (Primary ? Color.White : Theme.Ink), TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
        if (Focused && ShowFocusCues && Width > 16 && Height > 16)
            using (var focusPath = Theme.Round(new Rectangle(5, 5, Width - 11, Height - 11), 6))
            using (var focusPen = new Pen(Primary ? Color.White : Theme.Accent))
            {
                focusPen.DashStyle = DashStyle.Dot;
                e.Graphics.DrawPath(focusPen, focusPath);
            }
    }
}

// Superfície de entrada com contorno discreto e cantos consistentes.
public class ModernField : Panel
{
    public ModernField()
    {
        DoubleBuffered = true;
        BackColor = Color.Transparent;
        SetStyle(ControlStyles.ResizeRedraw, true);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        if (Width < 4 || Height < 4) return;
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        using (var path = Theme.Round(new Rectangle(1, 1, Width - 3, Height - 3), 8))
        using (var brush = new SolidBrush(Theme.Background))
        using (var pen = new Pen(ContainsFocus ? Theme.Accent : Theme.Line))
        {
            e.Graphics.FillPath(brush, path);
            e.Graphics.DrawPath(pen, path);
        }
    }

    protected override void OnEnter(EventArgs e) { base.OnEnter(e); Invalidate(); }
    protected override void OnLeave(EventArgs e) { base.OnLeave(e); Invalidate(); }
}

public class PrintPreview : PictureBox
{
    public PrintPreview()
    {
        BackColor = Color.FromArgb(247, 250, 251);
        SizeMode = PictureBoxSizeMode.Zoom;
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        if (Image != null)
            return;
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        using (var pen = new Pen(Color.FromArgb(193, 211, 215)))
        {
            pen.DashStyle = DashStyle.Dash;
            using (var p = Theme.Round(new Rectangle(1, 1, Width - 3, Height - 3), 10))
                e.Graphics.DrawPath(pen, p);
        }

        using (var font = new Font("Segoe UI", 11, FontStyle.Bold))
            TextRenderer.DrawText(e.Graphics, "Seu print aparece aqui", font, new Rectangle(0, Height / 2 - 23, Width, 25), Theme.Ink, TextFormatFlags.HorizontalCenter);
        using (var font = new Font("Segoe UI", 9))
            TextRenderer.DrawText(e.Graphics, "Importe uma imagem ou cole da área de transferência", font, new Rectangle(0, Height / 2 + 4, Width, 25), Theme.Muted, TextFormatFlags.HorizontalCenter);
    }
}
