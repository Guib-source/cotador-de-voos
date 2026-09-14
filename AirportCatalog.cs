using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

public sealed class AirportChoice
{
    public string Code, City, Name;
    public override string ToString() { return Code + " — " + City + " · " + Name; }
}

public static partial class AirportCatalog
{
    static string Fold(string text)
    {
        var value = new StringBuilder();
        foreach (char c in (text ?? "").Normalize(NormalizationForm.FormD))
            if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                value.Append(char.ToUpperInvariant(c));
        return value.ToString().Trim();
    }
    public static List<AirportChoice> Search(string query)
    {
        string folded = Fold(query);
        string[] words = folded.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        return Quote.Cities.Select(pair => new AirportChoice {
            Code = pair.Key, City = pair.Value, Name = Names.ContainsKey(pair.Key) ? Names[pair.Key] : pair.Value
        }).Where(a => words.All(word => Fold(a.ToString()).Contains(word)))
          .OrderBy(a => a.Code == folded ? 0 : (Fold(a.City) == folded ? 1 : 2))
          .ThenBy(a => a.City).ThenBy(a => a.Code).ToList();
    }
}
