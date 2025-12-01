using Link.Foundation.Links.Notation;

namespace Linkzip.Services;

public class Pattern
{
    public string Type { get; set; } = string.Empty; // "exact", "prefix", "suffix"
    public string PatternText { get; set; } = string.Empty;
    public List<string> Items { get; set; } = new();
    public int Count { get; set; }
}

public class DeduplicationService
{
    /// <summary>
    /// Converts a Link object to its string representation by flattening nested values.
    /// </summary>
    private static string LinkToString(Link<string> link)
    {
        // Simple links with just an id
        if (link.Id != null && (link.Values == null || link.Values.Count == 0))
        {
            return link.Id;
        }

        // Complex links with values - flatten recursively
        var parts = new List<string>();

        void Flatten(Link<string> l)
        {
            if (l.Id != null && (l.Values == null || l.Values.Count == 0))
            {
                parts.Add(l.Id);
            }
            else if (l.Values != null && l.Values.Count > 0)
            {
                foreach (var value in l.Values)
                {
                    Flatten(value);
                }
            }
        }

        if (link.Values != null && link.Values.Count > 0)
        {
            foreach (var value in link.Values)
            {
                Flatten(value);
            }
            return string.Join(" ", parts);
        }

        return link.Id ?? string.Empty;
    }

    /// <summary>
    /// Finds common prefix or suffix pattern between two reference arrays.
    /// </summary>
    private static string? FindCommonPattern(string[] references1, string[] references2, string type)
    {
        var len1 = references1.Length;
        var len2 = references2.Length;

        const int MIN_REFERENCES_FOR_PATTERN = 2;
        if (len1 < MIN_REFERENCES_FOR_PATTERN || len2 < MIN_REFERENCES_FOR_PATTERN)
        {
            return null;
        }

        int matchLen = 0;

        if (type == "prefix")
        {
            var maxMatch = Math.Min(len1 - 1, len2 - 1);
            while (matchLen < maxMatch && references1[matchLen] == references2[matchLen])
            {
                matchLen++;
            }
            return matchLen > 0 ? string.Join(" ", references1.Take(matchLen)) : null;
        }
        else // suffix
        {
            var maxMatch = Math.Min(len1 - 1, len2 - 1);
            while (matchLen < maxMatch &&
                   references1[len1 - 1 - matchLen] == references2[len2 - 1 - matchLen])
            {
                matchLen++;
            }
            return matchLen > 0 ? string.Join(" ", references1.TakeLast(matchLen)) : null;
        }
    }

    /// <summary>
    /// Identifies deduplication patterns from an array of links.
    /// </summary>
    private static List<Pattern> FindPatterns(IList<Link<string>> links)
    {
        var patterns = new List<Pattern>();

        // Filter links with deduplicatable content (2+ references)
        var validLinks = links.Where(link =>
        {
            var content = LinkToString(link);
            return content.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).Length >= 2;
        }).ToList();

        // Identify structured links
        var structuredLinks = validLinks.Where(link =>
            link.Id == null && link.Values != null && link.Values.Count > 1 &&
            link.Values[0].Values != null && link.Values[0].Values.Count > 0
        ).ToList();

        // Count exact duplicates
        var exactCounts = new Dictionary<string, int>();
        foreach (var link in validLinks)
        {
            var key = LinkToString(link);
            exactCounts[key] = exactCounts.GetValueOrDefault(key, 0) + 1;
        }

        // Structured duplicates should become prefix patterns
        var structuredDuplicates = new HashSet<string>(
            structuredLinks
                .Select(LinkToString)
                .Where(content => exactCounts.GetValueOrDefault(content, 0) >= 2)
        );

        // Add exact patterns (excluding structured duplicates)
        foreach (var (content, count) in exactCounts)
        {
            if (count >= 2 && !structuredDuplicates.Contains(content))
            {
                patterns.Add(new Pattern
                {
                    Type = "exact",
                    PatternText = content,
                    Items = new List<string> { content },
                    Count = count
                });
            }
        }

        // Maps for prefix and suffix patterns
        var prefixMap = new Dictionary<string, HashSet<string>>();
        var suffixMap = new Dictionary<string, HashSet<string>>();

        // Handle structured duplicates as prefix patterns
        foreach (var content in structuredDuplicates)
        {
            var matchingLinks = structuredLinks.Where(link => LinkToString(link) == content).ToList();
            if (matchingLinks.Count >= 2 && matchingLinks[0].Values != null && matchingLinks[0].Values.Count > 1)
            {
                var firstPart = matchingLinks[0].Values[0];
                var prefix = LinkToString(firstPart);

                if (!string.IsNullOrEmpty(prefix))
                {
                    if (!prefixMap.ContainsKey(prefix))
                    {
                        prefixMap[prefix] = new HashSet<string>();
                    }
                    foreach (var link in matchingLinks)
                    {
                        prefixMap[prefix].Add(LinkToString(link));
                    }
                }
            }
        }

        // Find prefix and suffix patterns from non-structured links
        for (int i = 0; i < validLinks.Count; i++)
        {
            if (structuredLinks.Contains(validLinks[i])) continue;

            for (int j = i + 1; j < validLinks.Count; j++)
            {
                if (structuredLinks.Contains(validLinks[j])) continue;

                var content1 = LinkToString(validLinks[i]);
                var content2 = LinkToString(validLinks[j]);

                if (content1 == content2) continue;

                var references1 = content1.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                var references2 = content2.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                // Check for common prefix
                var prefix = FindCommonPattern(references1, references2, "prefix");
                if (prefix != null)
                {
                    if (!prefixMap.ContainsKey(prefix))
                    {
                        prefixMap[prefix] = new HashSet<string>();
                    }
                    prefixMap[prefix].Add(content1);
                    prefixMap[prefix].Add(content2);
                }

                // Check for common suffix
                var suffix = FindCommonPattern(references1, references2, "suffix");
                if (suffix != null)
                {
                    if (!suffixMap.ContainsKey(suffix))
                    {
                        suffixMap[suffix] = new HashSet<string>();
                    }
                    suffixMap[suffix].Add(content1);
                    suffixMap[suffix].Add(content2);
                }
            }
        }

        // Convert maps to patterns
        void AddPatternsFromMap(Dictionary<string, HashSet<string>> map, string type)
        {
            foreach (var (pattern, items) in map)
            {
                var itemArray = items.ToList();
                var count = itemArray.Sum(item =>
                    validLinks.Count(link => LinkToString(link) == item));

                if (count >= 2)
                {
                    patterns.Add(new Pattern
                    {
                        Type = type,
                        PatternText = pattern,
                        Items = itemArray,
                        Count = count
                    });
                }
            }
        }

        AddPatternsFromMap(prefixMap, "prefix");
        AddPatternsFromMap(suffixMap, "suffix");

        return patterns;
    }

    /// <summary>
    /// Selects the best patterns based on scoring.
    /// </summary>
    private static List<Pattern> SelectBestPatterns(List<Pattern> patterns, double topPercentage)
    {
        // Sort by score (count * pattern length)
        var sorted = patterns.OrderByDescending(p =>
        {
            var score = p.Count * p.PatternText.Split(' ').Length;
            return (score, p.Count);
        }).ToList();

        // Remove overlapping patterns
        var selected = new List<Pattern>();
        var usedItems = new HashSet<string>();

        foreach (var pattern in sorted)
        {
            var hasOverlap = pattern.Items.Any(item => usedItems.Contains(item));

            if (!hasOverlap)
            {
                selected.Add(pattern);
                foreach (var item in pattern.Items)
                {
                    usedItems.Add(item);
                }

                if (selected.Count >= Math.Max(1, (int)Math.Ceiling(patterns.Count * topPercentage)))
                {
                    break;
                }
            }
        }

        return selected;
    }

    /// <summary>
    /// Creates a reference definition link.
    /// </summary>
    private static Link<string> CreateReference(int refId, string[] references)
    {
        var valueLinks = references.Select(reference => new Link<string>(reference)).ToList();
        return new Link<string>(refId.ToString(), valueLinks);
    }

    /// <summary>
    /// Creates a compound link from multiple parts.
    /// </summary>
    private static Link<string> CreateCompoundLink(List<Link<string>> parts)
    {
        return new Link<string>(null, parts);
    }

    /// <summary>
    /// Applies selected patterns to links.
    /// </summary>
    private static IList<Link<string>> ApplyPatterns(IList<Link<string>> links, List<Pattern> patterns)
    {
        var replacements = new Dictionary<string, (int RefId, Pattern Pattern)>();
        int nextRefId = 1;

        foreach (var pattern in patterns)
        {
            foreach (var item in pattern.Items)
            {
                replacements[item] = (nextRefId, pattern);
            }
            nextRefId++;
        }

        var result = new List<Link<string>>();
        var definedPatterns = new HashSet<int>();

        foreach (var link in links)
        {
            var content = LinkToString(link);

            if (!replacements.TryGetValue(content, out var replacement))
            {
                result.Add(link);
                continue;
            }

            var (refId, pattern) = replacement;

            // Define reference if not already defined
            if (!definedPatterns.Contains(refId))
            {
                var references = pattern.PatternText.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                result.Add(CreateReference(refId, references));
                definedPatterns.Add(refId);
            }

            // Use the reference
            var refLink = new Link<string>(refId.ToString());

            if (pattern.Type == "exact")
            {
                result.Add(refLink);
            }
            else if (pattern.Type == "prefix")
            {
                var suffix = content.Substring(pattern.PatternText.Length).Trim();
                if (!string.IsNullOrEmpty(suffix))
                {
                    var suffixReferences = suffix.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
                        .Select(reference => new Link<string>(reference)).ToList();
                    var parts = new List<Link<string>> { refLink };
                    parts.AddRange(suffixReferences);
                    result.Add(CreateCompoundLink(parts));
                }
                else
                {
                    result.Add(refLink);
                }
            }
            else if (pattern.Type == "suffix")
            {
                var prefix = content.Substring(0, content.Length - pattern.PatternText.Length).Trim();
                if (!string.IsNullOrEmpty(prefix))
                {
                    var prefixReferences = prefix.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
                        .Select(reference => new Link<string>(reference)).ToList();
                    prefixReferences.Add(refLink);
                    result.Add(CreateCompoundLink(prefixReferences));
                }
                else
                {
                    result.Add(refLink);
                }
            }
        }

        return result;
    }

    /// <summary>
    /// Main deduplication function.
    /// </summary>
    public (string Output, int PatternsApplied) Deduplicate(string input, double topPercentage = 0.2)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return (input, 0);
        }

        var parser = new Parser();
        IList<Link<string>> links;

        try
        {
            links = parser.Parse(input);
        }
        catch
        {
            return (input, 0);
        }

        var patterns = FindPatterns(links);
        var selectedPatterns = SelectBestPatterns(patterns, topPercentage);

        if (selectedPatterns.Count == 0)
        {
            return (links.Format(), 0);
        }

        var deduplicatedLinks = ApplyPatterns(links, selectedPatterns);
        return (deduplicatedLinks.Format(), selectedPatterns.Count);
    }
}
