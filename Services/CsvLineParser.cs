using System.Text;

namespace ECommerceCsvImport.Services;

public static class CsvLineParser
{
    public static IReadOnlyList<string> Parse(string line)
    {
        if (string.IsNullOrEmpty(line))
        {
            return new List<string>();
        }

        var result = new List<string>();
        var currentField = new StringBuilder();
        var inQuotes = false;
        var fieldWasQuoted = false;

        for (int i = 0; i < line.Length; i++)
        {
            char currentChar = line[i];

            if (currentChar == '"')
            {
                // проверка на экранированную кавычку (двойная кавычка)
                if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                {
                    currentField.Append('"');
                    i++;
                }
                else
                {
                    inQuotes = !inQuotes;
                    fieldWasQuoted = true;
                }
            }
            else if (currentChar == ',' && !inQuotes)
            {
                // конец поля
                result.Add(ProcessField(currentField.ToString(), fieldWasQuoted));
                currentField.Clear();
                fieldWasQuoted = false;
            }
            else
            {
                currentField.Append(currentChar);
            }
        }

        // добавляем последнее поле
        result.Add(ProcessField(currentField.ToString(), fieldWasQuoted));

        return result;
    }

    private static string ProcessField(string field, bool wasQuoted)
    {
        return wasQuoted ? field : field.Trim();
    }
}
