using Link.Foundation.Links.Notation;

namespace Linkzip.Services;

public class ZipperService
{
    private readonly DeduplicationService _deduplicationService;

    public ZipperService(DeduplicationService deduplicationService)
    {
        _deduplicationService = deduplicationService;
    }

    /// <summary>
    /// Converts text to links notation and applies compression.
    /// </summary>
    public (string LinksNotation, int PatternsApplied) Zip(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return (string.Empty, 0);
        }

        // Convert text to links notation
        var parser = new Parser();
        IList<Link<string>> links;

        try
        {
            // Try to parse as links notation first
            links = parser.Parse(text);
        }
        catch
        {
            // If parsing fails, convert plain text to links notation
            // Split by lines and words
            var words = text.Split(new[] { ' ', '\n', '\r', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            links = words.Select(word => new Link<string>(word)).ToList();
        }

        // Format as links notation
        var linksNotation = links.Format();

        // Apply deduplication/compression
        var (compressed, patternsApplied) = _deduplicationService.Deduplicate(linksNotation);

        return (compressed, patternsApplied);
    }

    /// <summary>
    /// Converts links notation back to text.
    /// </summary>
    public string Unzip(string linksNotation)
    {
        if (string.IsNullOrWhiteSpace(linksNotation))
        {
            return string.Empty;
        }

        var parser = new Parser();
        IList<Link<string>> links;

        try
        {
            links = parser.Parse(linksNotation);
        }
        catch
        {
            // If parsing fails, return the input as-is
            return linksNotation;
        }

        // Convert links to text by extracting all reference values
        var text = ExtractText(links);
        return text;
    }

    private string ExtractText(IList<Link<string>> links)
    {
        var references = new List<string>();

        foreach (var link in links)
        {
            ExtractReferences(link, references);
        }

        return string.Join(" ", references);
    }

    private void ExtractReferences(Link<string> link, List<string> references)
    {
        // If link has an ID and no values, it's a simple reference
        if (link.Id != null && (link.Values == null || link.Values.Count == 0))
        {
            references.Add(link.Id);
        }
        // If link has values, recursively extract from them
        else if (link.Values != null && link.Values.Count > 0)
        {
            foreach (var value in link.Values)
            {
                ExtractReferences(value, references);
            }
        }
    }
}
