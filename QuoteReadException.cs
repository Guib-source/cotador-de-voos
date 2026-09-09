using System;

/// <summary>
/// Dados de voo incompletos ou ambíguos. Permite tentar outro OCR sem
/// confundir uma falha de reconhecimento com um erro de programação.
/// </summary>
public sealed class QuoteReadException : Exception
{
    public QuoteReadException(string message) : base(message)
    {
    }

    public QuoteReadException(string message, Exception inner) : base(message, inner)
    {
    }
}
