using System;
using System.Windows.Forms;

// Ponto de entrada STA: necessário para a área de transferência do Windows.
internal static class Program
{
    [STAThread]
    public static void Main()
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        Application.Run(new MainForm());
    }
}
