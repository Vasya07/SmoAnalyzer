using System.Collections.Generic;

namespace SmoAnalyzer
{
    /// <summary>Результат расчёта СМО: список пар «показатель — значение».</summary>
    public class SmoResult
    {
        public string Title { get; set; } = "";
        public List<KeyValuePair<string, string>> Rows { get; } = new();

        public void Add(string name, double value, string fmt = "F6", string unit = "")
        {
            Rows.Add(new KeyValuePair<string, string>(
                name,
                string.IsNullOrEmpty(unit)
                    ? value.ToString(fmt)
                    : $"{value.ToString(fmt)} {unit}"));
        }

        public void AddText(string name, string value)
            => Rows.Add(new KeyValuePair<string, string>(name, value));
    }
}