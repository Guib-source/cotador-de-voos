using System;
using System.IO;
using System.Diagnostics;
using System.Threading.Tasks;

/// <summary>Executa o OCR local fora da thread da interface e remove o arquivo de resultado.</summary>
public static class OcrClient
{
    private const int OcrTimeoutMilliseconds = 60000;
    public static async Task<string> ReadAsync(string path, int scale)
    {
        string outputPath = Path.GetTempFileName();
        try
        {
            string script = Path.Combine(Path.GetDirectoryName(typeof(OcrClient).Assembly.Location), "Ocr.ps1");
            await Task.Run(() =>
            {
                var info = new ProcessStartInfo("powershell.exe", "-NoProfile -ExecutionPolicy Bypass -File \"" + script + "\" -ImagePath \"" + path + "\" -OutputPath \"" + outputPath + "\" -ScaleFactor " + scale)
                {
                    UseShellExecute = false,
                    CreateNoWindow = true
                };
                using (var process = Process.Start(info))
                {
                    if (!process.WaitForExit(OcrTimeoutMilliseconds))
                    {
                        process.Kill();
                        throw new Exception("A leitura demorou demais. Tente um print menor.");
                    }

                    if (process.ExitCode != 0)
                        throw new Exception("Falha no reconhecimento: " + File.ReadAllText(outputPath));
                }
            });
            string text = File.ReadAllText(outputPath);
            if (text.StartsWith("ERRO:"))
                throw new Exception(text);
            return text;
        }
        finally
        {
            File.Delete(outputPath);
        }
    }
}
