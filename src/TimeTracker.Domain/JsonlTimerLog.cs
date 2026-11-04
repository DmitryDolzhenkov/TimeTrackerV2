using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace TimeTracker.Domain;

/// <summary>The Timer Log stored as an append-only JSONL file (ADR-0001).</summary>
public sealed class JsonlTimerLog(string path)
{
    private const byte NewLine = (byte)'\n';

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        RespectRequiredConstructorParameters = true,
        RespectNullableAnnotations = true,
        Converters = { new JsonStringEnumConverter(allowIntegerValues: false) },
    };

    public IReadOnlyList<Transition> ReadAll()
    {
        if (!File.Exists(path))
        {
            return [];
        }

        return File.ReadLines(path).Select(Parse).OfType<Transition>().ToList();
    }

    public void Append(Transition transition)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(path))!);

        using var stream = new FileStream(path, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read);

        // A crash can leave a torn last line; start on a fresh line so the new entry stays readable.
        var prefix = EndsWithoutNewLine(stream) ? "\n" : "";
        var line = prefix + JsonSerializer.Serialize(transition, JsonOptions) + "\n";

        stream.Seek(0, SeekOrigin.End);
        stream.Write(new UTF8Encoding(encoderShouldEmitUTF8Identifier: false).GetBytes(line));
        stream.Flush(flushToDisk: true);
    }

    private static bool EndsWithoutNewLine(FileStream stream)
    {
        if (stream.Length == 0)
        {
            return false;
        }

        stream.Seek(-1, SeekOrigin.End);
        return stream.ReadByte() != NewLine;
    }

    // A line that cannot be parsed is a torn write: it stays in the file and is skipped.
    private static Transition? Parse(string line)
    {
        try
        {
            return JsonSerializer.Deserialize<Transition>(line, JsonOptions);
        }
        catch (JsonException)
        {
            return null;
        }
    }
}
