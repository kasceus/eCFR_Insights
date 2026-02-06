using ecfrInsights.Data.Entities;
using ecfrInsights.Data.Interfaces;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Xml.Linq;

namespace ecfrInsights.Xml;

/// <summary>
/// Parses eCFR XML documents into CfrDocument and CfrReference entities.
/// Maps the hierarchical DIV structure to the CFR reference hierarchy.
/// </summary>
public static class CfrXmlParser
{

    /// <summary>
    /// Parse a downloaded title XML into a CfrDocument and its CfrHierarchies with proper hierarchical structure.
    /// Creates parent-child relationships where each reference only contains its own level's identifier.
    /// </summary>
    /// <param name="xml">The XML content of the title</param>
    /// <param name="titleNumber">The CFR title number</param>
    /// <returns>Tuple containing the CfrDocument, list of CfrHierarchies, document hash, and original XML</returns>
    public static (T Document, List<Y> References, string DocumentHash, string Xml) Parse<T, Y>(
        string xml, int titleNumber, ConcurrentBag<string>? hashList = null) where T : ICfrTitle, new()
        where Y : ICfrHierarchy, new()
    {
        var doc = XDocument.Parse(xml);

        // Calculate document hash
        string documentHash;
        using (var sha = SHA256.Create())
        {
            var bytes = Encoding.UTF8.GetBytes(xml);
            documentHash = Convert.ToHexString(sha.ComputeHash(bytes));
        }
        if (hashList != null && !hashList.IsEmpty && hashList.Contains(documentHash))
        {
            //return since this document is already parsed
            throw new InvalidOperationException($"Document hash matches existing hash: {documentHash} stopping");
        }
        else
        {
            hashList ??= new ConcurrentBag<string>();
            hashList.Add(documentHash);

        }
        // Find the title DIV1 node
        var titleDiv = doc.Descendants("DIV1").FirstOrDefault(d => (string?)d.Attribute("TYPE") == "TITLE");
        var titleName = titleDiv?.Element("HEAD")?.Value?.Trim() ?? $"Title {titleNumber}";

        var cfrDoc = new T
        {
            Number = titleNumber,
            Name = titleName,
            XmlDocumentHash = documentHash
        };

        var refs = new List<Y>();
        var referencesByNumber = new Dictionary<string, Y>();

        // Create the Title-level reference (TitleNumber only)
        var titleRef = new Y
        {
            TitleNumber = titleNumber,
            CfrReferenceNumber = titleNumber.ToString(),
            CfrReferenceTitle = titleName
        };
        refs.Add(titleRef);
        referencesByNumber[titleRef.CfrReferenceNumber] = titleRef;

        // Find all leaf sections (typically DIV8 TYPE="SECTION", but handle cases where structure is shallower)
        var sectionNodes = FindAllSections(doc);

        foreach (var sectionNode in sectionNodes)
        {
            var sectionRefs = ParseSectionHierarchy(sectionNode, titleNumber, referencesByNumber);
            refs.AddRange(sectionRefs);
        }

        cfrDoc.SectionCount = refs.Count;
        return (cfrDoc, refs, documentHash, xml);
    }

    /// <summary>
    /// Finds all section nodes in the document.
    /// Prioritizes DIV8 TYPE="SECTION", but also looks for other potential section containers.
    /// </summary>
    private static List<XElement> FindAllSections(XDocument doc)
    {
        var sections = new List<XElement>();

        // First, try to find explicit SECTION type DIVs
        sections.AddRange(doc.Descendants()
            .Where(e => e.Name.LocalName.StartsWith("DIV") &&
                        (string?)e.Attribute("TYPE") == "SECTION")
            .ToList());

        // If no explicit sections found, find leaf DIVs (deepest level nodes with content)
        if (sections.Count == 0 && doc.Root != null)
        {
            sections.AddRange(FindLeafDivs(doc.Root));
        }

        return sections;
    }

    /// <summary>
    /// Recursively finds leaf DIV elements (DIVs that don't contain other DIVs).
    /// These are treated as sections if they contain content.
    /// </summary>
    private static List<XElement> FindLeafDivs(XElement element, int minDepth = 7)
    {
        var leafDivs = new List<XElement>();

        var currentDivLevel = GetDivLevel(element);

        // Only consider DIVs at minDepth or deeper
        if (currentDivLevel >= minDepth && currentDivLevel <= 8)
        {
            var childDivs = element.Elements()
                .Where(e => e.Name.LocalName.StartsWith("DIV"))
                .ToList();

            if (childDivs.Count == 0)
            {
                // This is a leaf DIV
                leafDivs.Add(element);
            }
            else
            {
                // Recurse into child DIVs
                foreach (var childDiv in childDivs)
                {
                    leafDivs.AddRange(FindLeafDivs(childDiv, minDepth));
                }
            }
        }
        else if (currentDivLevel > 0)
        {
            // Recurse if not yet at the target depth
            foreach (var child in element.Elements().Where(e => e.Name.LocalName.StartsWith("DIV")))
            {
                leafDivs.AddRange(FindLeafDivs(child, minDepth));
            }
        }

        return leafDivs;
    }

    /// <summary>
    /// Extracts the DIV level (1-8) from an element name.
    /// </summary>
    private static int GetDivLevel(XElement element)
    {
        var name = element.Name.LocalName;
        if (name.StartsWith("DIV") && int.TryParse(name.Substring(3), out var level))
        {
            return level;
        }
        return -1;
    }

    /// <summary>
    /// Parses a section node and creates the full hierarchical chain of CfrReference records.
    /// Each record in the hierarchy only contains the identifier for its specific level.
    /// </summary>
    private static List<Y> ParseSectionHierarchy<Y>(XElement sectionNode, int titleNumber, Dictionary<string, Y> referencesByNumber) where Y : ICfrHierarchy, new()
    {
        var results = new List<Y>();

        // Extract section title from HEAD element
        var head = sectionNode.Element("HEAD")?.Value?.Trim() ?? string.Empty;

        // Extract section number from N attribute or from HEAD
        var sectionNumber = sectionNode.Attribute("N")?.Value ?? ExtractNumberFromHead(head);

        // Extract reference content from P (paragraph) elements
        var referenceContent = ExtractParagraphContent(sectionNode);

        // Extract authority, source, and citation
        var authority = ExtractAuthority(sectionNode);
        var source = ExtractSource(sectionNode);
        var citation = ExtractCitation(sectionNode);

        // Build the hierarchy by walking ancestors, creating records at each level
        var hierarchy = BuildHierarchyChain<Y>(sectionNode, titleNumber, referencesByNumber, sectionNumber);

        // The last element in the hierarchy chain is the section itself
        if (hierarchy.Count > 0)
        {
            var sectionRef = hierarchy[^1];
            sectionRef.Section = sectionNumber;
            sectionRef.CfrReferenceTitle = head;
            sectionRef.ReferenceContent = referenceContent;
            sectionRef.Authority = authority;
            sectionRef.Source = source;
            sectionRef.Citation = citation;

            results.AddRange(hierarchy);
        }

        return results;
    }
    /// <summary>
    /// Builds the complete hierarchy chain by walking ancestors.
    /// Creates a CfrReference for each hierarchical level, each containing only its own identifier.
    /// Returns the chain from parent to child (leaf), ensuring all are stored in referencesByNumber.
    /// </summary>
    private static List<Y> BuildHierarchyChain<Y>(XElement element, int titleNumber, Dictionary<string, Y> referencesByNumber, string sectionNumber) where Y : ICfrHierarchy, new()
    {
        var chain = new List<Y>();
        var ancestors = element.Ancestors().Reverse().ToList();

        string parentRefNumber = titleNumber.ToString(); // Start with title reference

        foreach (var ancestor in ancestors)
        {
            var divLevel = GetDivLevel(ancestor);
            if (divLevel <= 1 || divLevel > 8) // Skip DIV1 (Title) and invalid levels
                continue;

            var head = ancestor.Element("HEAD")?.Value?.Trim() ?? string.Empty;
            var attrN = ancestor.Attribute("N")?.Value ?? string.Empty;

            // If N attribute missing, try to extract a label from the HEAD (handles letter or other non-digit labels)
            var identifier = !string.IsNullOrEmpty(attrN) ? attrN : ExtractLabelFromHead(head);
            if (string.IsNullOrEmpty(identifier))
                continue;

            // Build reference number for this level based on parent + current identifier
            var levelRefNumber = parentRefNumber + "-" + identifier;

            // Check if we already have this reference (avoid duplicates)
            if (referencesByNumber.TryGetValue(levelRefNumber, out var existingRef))
            {
                chain.Add(existingRef);
                parentRefNumber = levelRefNumber;
                continue;
            }

            var cref = new Y
            {
                TitleNumber = titleNumber,
                ParentCfrReferenceNumber = parentRefNumber,
                CfrReferenceNumber = levelRefNumber,
            };
            cref.CfrReferenceTitle = head;

            // Set the appropriate level field to only the label (not the full hierarchy)
            switch (divLevel)
            {
                case 2: // DIV2 - Subtitle
                    cref.Subtitle = identifier;
                    break;
                case 3: // DIV3 - Chapter
                    cref.Chapter = identifier;
                    break;
                case 4: // DIV4 - Subchapter
                    cref.Subchapter = identifier;
                    break;
                case 5: // DIV5 - Part
                    cref.Part = identifier;
                    break;
                case 6: // DIV6 - Subpart
                    cref.Subpart = identifier;
                    break;
                case 7: // DIV7 - Section (ancestor-level section container)
                    cref.Section = identifier;
                    break;
                case 8: // DIV8 - Appendix
                    cref.Appendix = identifier;
                    break;
            }

            chain.Add(cref);
            referencesByNumber[levelRefNumber] = cref;
            parentRefNumber = levelRefNumber;
        }

        // Finally, create the leaf section record. If we have a section number, append it to the parent chain.
        var sectionRef = new Y
        {
            TitleNumber = titleNumber,
            ParentCfrReferenceNumber = parentRefNumber,
        };

        if (!string.IsNullOrEmpty(sectionNumber))
        {
            // build full section reference number
            sectionRef.CfrReferenceNumber = parentRefNumber + "-" + sectionNumber;
        }
        else
        {
            // fallback to parentRefNumber to avoid null primary key
            sectionRef.CfrReferenceNumber = parentRefNumber;
        }

        // Avoid duplicate insert if already exists
        if (!referencesByNumber.ContainsKey(sectionRef.CfrReferenceNumber))
        {
            referencesByNumber[sectionRef.CfrReferenceNumber] = sectionRef;
        }
        else
        {
            sectionRef = referencesByNumber[sectionRef.CfrReferenceNumber];
        }

        chain.Add(sectionRef);

        return chain;
    }
    /// <summary>
    /// Extracts a short identifier/label from a HEAD when N attribute is missing.
    /// Captures leading token like "A", "1", "I", "App", or "A.100" style.
    /// </summary>
    private static string ExtractLabelFromHead(string head)
    {
        if (string.IsNullOrEmpty(head))
            return string.Empty;

        // Try after common words (e.g., "Subtitle", "Chapter", "Part", "Subpart", "Appendix")
        var tokens = head.Split(new[] { ' ', '\t', ':' }, StringSplitOptions.RemoveEmptyEntries);
        if (tokens.Length > 0)
        {
            // pick first token that looks like an identifier (letters/numbers/dot/hyphen)
            foreach (var t in tokens)
            {
                var m = System.Text.RegularExpressions.Regex.Match(t, @"^[A-Za-z0-9\.\-]+");
                if (m.Success)
                    return m.Value;
            }
        }

        // fallback: extract first matching run
        var fallback = System.Text.RegularExpressions.Regex.Match(head, @"[A-Za-z0-9\.\-]+");
        return fallback.Success ? fallback.Value : string.Empty;
    }

    /// <summary>
    /// Extracts the section number from the HEAD element text.
    /// Looks for patterns like "§ 1.1" or similar.
    /// </summary>
    private static string ExtractNumberFromHead(string head)
    {
        if (string.IsNullOrEmpty(head))
            return string.Empty;

        // Look for section symbol
        var idx = head.IndexOf('§');
        if (idx >= 0)
        {
            var after = head.Substring(idx + 1).Trim();
            var parts = after.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length > 0)
                return parts[0].Trim();
        }

        // Try to extract leading number pattern
        var match = System.Text.RegularExpressions.Regex.Match(head, @"^[\d\.]+");
        if (match.Success)
            return match.Value;

        return string.Empty;
    }

    /// <summary>
    /// Extracts all paragraph content from P elements within a section.
    /// Concatenates multiple paragraphs with appropriate spacing.
    /// </summary>
    private static string ExtractParagraphContent(XElement element)
    {
        var paragraphs = element.Elements("P")
            .Select(p => CleanTextContent(p.Value))
            .Where(p => !string.IsNullOrWhiteSpace(p))
            .ToList();

        return string.Join("\n\n", paragraphs);
    }

    /// <summary>
    /// Extracts the Authority field from AUTH/HED/PSPACE elements.
    /// </summary>
    private static string ExtractAuthority(XElement element)
    {
        var auth = element.Element("AUTH");
        if (auth != null)
        {
            var pspace = auth.Element("PSPACE");
            if (pspace != null)
                return CleanTextContent(pspace.Value);
        }

        // Also check ancestor elements for authority
        var authNode = element.Ancestors()
            .Select(a => a.Element("AUTH"))
            .FirstOrDefault(a => a != null);

        if (authNode != null)
        {
            var pspace = authNode.Element("PSPACE");
            if (pspace != null)
                return CleanTextContent(pspace.Value);
        }

        return string.Empty;
    }

    /// <summary>
    /// Extracts the Source field from SOURCE/HED/PSPACE elements.
    /// </summary>
    private static string ExtractSource(XElement element)
    {
        var source = element.Element("SOURCE");
        if (source != null)
        {
            var pspace = source.Element("PSPACE");
            if (pspace != null)
                return CleanTextContent(pspace.Value);
        }

        // Also check ancestor elements for source
        var sourceNode = element.Ancestors()
            .Select(a => a.Element("SOURCE"))
            .FirstOrDefault(a => a != null);

        if (sourceNode != null)
        {
            var pspace = sourceNode.Element("PSPACE");
            if (pspace != null)
                return CleanTextContent(pspace.Value);
        }

        return string.Empty;
    }

    /// <summary>
    /// Extracts the Citation field from CITA elements.
    /// </summary>
    private static string ExtractCitation(XElement element)
    {
        var cita = element.Element("CITA");
        if (cita != null)
            return CleanTextContent(cita.Value);

        return string.Empty;
    }

    /// <summary>
    /// Cleans up text content by removing excess whitespace and newlines.
    /// </summary>
    private static string CleanTextContent(string text)
    {
        if (string.IsNullOrEmpty(text))
            return string.Empty;

        // Replace multiple whitespace/newlines with single space
        var cleaned = System.Text.RegularExpressions.Regex.Replace(text, @"\s+", " ");
        return cleaned.Trim();
    }
}
