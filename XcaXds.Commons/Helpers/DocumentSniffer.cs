using System.Buffers;
using System.Text;
using System.Xml;

namespace XcaXds.Commons.Helpers; 

public static class DocumentSniffer
{
	public enum DocumentKind
	{
		Unknown = 0,
		Xml,
		ClinicalDocumentXml
	}

	/// <summary>
	/// Sniffs the beginning of a byte[] to determine if it's XML and whether it's a CDA ClinicalDocument.
	/// Does not parse a full XML document.
	/// </summary>
	public static DocumentKind DetectKind(ReadOnlySpan<byte> data)
	{
		if (data.IsEmpty) return DocumentKind.Unknown;

		// Decode only a small prefix. XML prolog + root tag should be near the start.
		const int maxPrefixBytes = 32 * 1024; // plenty for prolog + namespaces + comments
		var slice = data.Length > maxPrefixBytes ? data[..maxPrefixBytes] : data;

		// Convert prefix to string. Use strict UTF8; if it throws, it's probably not XML text.
		string prefix;
		try
		{
			prefix = Encoding.UTF8.GetString(slice);
		}
		catch
		{
			return DocumentKind.Unknown;
		}

		if (string.IsNullOrWhiteSpace(prefix))
			return DocumentKind.Unknown;

		// Quickly skip BOM/whitespace/comments/prolog/doctype until first start element.
		if (!TryFindRootElementName(prefix.AsSpan(), out var rootName))
			return DocumentKind.Unknown;

		// rootName is like "ClinicalDocument" or "cda:ClinicalDocument"
		// Compare local-name only (ignore prefix).
		var localName = GetLocalName(rootName);

		if (localName.Equals("ClinicalDocument".AsSpan(), StringComparison.Ordinal))
			return DocumentKind.ClinicalDocumentXml;

		if (IsWellFormedXml(data)) // Extra safe check 
		{
			return DocumentKind.Xml;
		}
		else
		{
			return DocumentKind.Unknown;
		}
	}

	private static ReadOnlySpan<char> GetLocalName(ReadOnlySpan<char> name)
	{
		var colon = name.IndexOf(':');
		return colon >= 0 ? name[(colon + 1)..] : name;
	}

	/// <summary>
	/// Finds the first element name in an XML text prefix, without full parsing.
	/// Skips whitespace, BOM, processing instructions, comments, and doctype.
	/// </summary>
	private static bool TryFindRootElementName(ReadOnlySpan<char> s, out ReadOnlySpan<char> elementName)
	{
		elementName = default;

		int i = 0;

		// Trim leading whitespace
		while (i < s.Length && char.IsWhiteSpace(s[i])) i++;

		// Skip UTF-8 BOM if it got decoded as U+FEFF
		if (i < s.Length && s[i] == '\uFEFF') i++;

		// Scan for the first '<' that begins an element start tag
		while (i < s.Length)
		{
			// Find next '<'
			while (i < s.Length && s[i] != '<') i++;
			if (i >= s.Length) return false;
			i++; // after '<'
			if (i >= s.Length) return false;

			// Handle things we want to skip: <?xml ...?>, <!-- -->, <!DOCTYPE ...>, <![CDATA[...]]>
			if (s[i] == '?')
			{
				// Processing instruction
				i++;
				if (!SkipUntil(s, ref i, "?>")) return false;
				continue;
			}

			if (s[i] == '!')
			{
				i++;
				if (StartsWith(s, i, "--"))
				{
					i += 2;
					if (!SkipUntil(s, ref i, "-->")) return false;
					continue;
				}

				if (StartsWithIgnoreCase(s, i, "DOCTYPE"))
				{
					// Skip doctype (simple skip until next '>' in prefix; good enough for sniffing)
					if (!SkipUntilChar(s, ref i, '>')) return false;
					continue;
				}

				if (StartsWith(s, i, "[CDATA["))
				{
					i += 7;
					if (!SkipUntil(s, ref i, "]]>")) return false;
					continue;
				}

				// Some other declaration - skip to '>'
				if (!SkipUntilChar(s, ref i, '>')) return false;
				continue;
			}

			if (s[i] == '/')
			{
				// End tag before any start tag? Not a valid root start.
				return false;
			}

			// This should be a start tag. Read the element name.
			int start = i;

			// Element name: letter/_/: then letters/digits/._:- etc. We'll stop at whitespace, '/', or '>'.
			while (i < s.Length)
			{
				var ch = s[i];
				if (char.IsWhiteSpace(ch) || ch == '/' || ch == '>')
					break;
				i++;
			}

			if (i == start) return false;

			elementName = s.Slice(start, i - start);
			return true;
		}

		return false;
	}

	private static bool SkipUntil(ReadOnlySpan<char> s, ref int i, string terminator)
	{
		int idx = s.Slice(i).IndexOf(terminator.AsSpan(), StringComparison.Ordinal);
		if (idx < 0) return false;
		i += idx + terminator.Length;
		return true;
	}

	private static bool SkipUntilChar(ReadOnlySpan<char> s, ref int i, char terminator)
	{
		int idx = s.Slice(i).IndexOf(terminator);
		if (idx < 0) return false;
		i += idx + 1;
		return true;
	}

	private static bool StartsWith(ReadOnlySpan<char> s, int i, string value)
	{
		var v = value.AsSpan();
		return i + v.Length <= s.Length && s.Slice(i, v.Length).SequenceEqual(v);
	}

	private static bool StartsWithIgnoreCase(ReadOnlySpan<char> s, int i, string value)
	{
		var v = value.AsSpan();
		if (i + v.Length > s.Length) return false;
		return s.Slice(i, v.Length).Equals(v, StringComparison.OrdinalIgnoreCase);
	}

	public static bool IsWellFormedXml(ReadOnlySpan<byte> data)
	{
		try
		{
			using var ms = new MemoryStream(data.ToArray());
			var settings = new XmlReaderSettings
			{
				DtdProcessing = DtdProcessing.Prohibit,
				XmlResolver = null
			};

			using var reader = XmlReader.Create(ms, settings);
			while (reader.Read()) { }
			return true;
		}
		catch
		{
			return false;
		}
	}

}
