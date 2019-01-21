using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ISO639ToJson
{
	internal class WikiPageParser
	{
		internal IDictionary<string, string> ParseKeyValues(string wikiPageContent, LanguageNameMode mode)
		{
			var result = new SortedDictionary<string, string>();

			var tableIndex = 1;
			var languageCodeIndex = 1;
			var languageNameIndex = GetLanguageNameIndex(mode);
			var languageModeFallback = LanguageNameMode.English;
			var languageNameFallbackIndex = GetLanguageNameIndex(languageModeFallback);

			var doc = new HtmlDocument();
			doc.LoadHtml(wikiPageContent);

			var languagesTable = doc.DocumentNode.SelectSingleNode($"//table[{tableIndex}]");
			foreach (var languageRow in languagesTable.ChildNodes[1].ChildNodes.Where(node => node.Name == "tr")
				.Skip(1))
			{
				var languageCode = languageRow.ChildNodes[languageCodeIndex].InnerText.Trim().Substring(0, 3);
				var languageName = languageRow.ChildNodes[languageNameIndex].InnerText.Trim();
				if (mode != languageModeFallback && string.IsNullOrEmpty(languageName))
				{
					languageName = languageRow.ChildNodes[languageNameFallbackIndex].InnerText.Trim();
				}

				languageName = CleanString(languageName);

				if (string.IsNullOrEmpty(languageCode))
				{
					Console.WriteLine($"The code for language '{languageName}' is empty an won't be added.");
					continue;
				}

				result.Add(languageCode, languageName);
			}

			return result;
		}

		private static int GetLanguageNameIndex(LanguageNameMode nameMode)
		{
			int languageNameIndex;
			switch (nameMode)
			{
				case LanguageNameMode.Native:
					languageNameIndex = 15;
					break;

				case LanguageNameMode.English:
					languageNameIndex = 9;
					break;

				default:
					throw new NotSupportedException(
						$"The specified nameMode={nameMode} is not supported. Supported values are {nameof(LanguageNameMode.Native)} and {nameof(LanguageNameMode.English)}");
			}

			return languageNameIndex;
		}

		private static string CleanString(string name)
		{
			name = name.Replace("&#160;", "; ");
			return name;
		}
	}
}