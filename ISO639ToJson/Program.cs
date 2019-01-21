using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace ISO639ToJson
{
	public class Program
	{
		private const string WIKI_ISO_639_PAGE_URL = "https://en.wikipedia.org/wiki/List_of_ISO_639-2_codes";
		private const string EXPORT_FILE_NAME = "languages.json";
		
		private const LanguageNameMode DEFAULT_LANGUAGE_NAME_MODE = LanguageNameMode.Native;

		public static void Main(string[] args)
		{
			try
			{
				if (args.Length != 0 && args.Length != 1)
				{
					Console.WriteLine($"Specify 0 or 1 parameters. '{nameof(LanguageNameMode)}': {string.Join(", ", Enum.GetValues(typeof(LanguageNameMode)).OfType<LanguageNameMode>())}");
					throw new NotSupportedException();
				}

				Console.WriteLine($"Start importing ISO-639 from {WIKI_ISO_639_PAGE_URL}");

				// Retrieve the Wiki page
				string pageContent;
				try
				{
					pageContent = new HttpPageGetter().Get(WIKI_ISO_639_PAGE_URL);
				}
				catch (Exception ex)
				{
					Console.WriteLine($"An error occured while getting the content of the page {WIKI_ISO_639_PAGE_URL}: {ex.Message}");
					throw;
				}

				// Parse the wiki page to get key=languageCode, value=languageName
				IDictionary<string, string> keyValues;
				try
				{
					var languageNameMode = GetLanguageMode(args.Length > 0 ? args[0] : null, DEFAULT_LANGUAGE_NAME_MODE);
					keyValues = new WikiPageParser().ParseKeyValues(pageContent, languageNameMode);
				}
				catch (Exception ex)
				{
					Console.WriteLine($"Failed to parse values: {ex.Message}");
					throw;
				}

				// Serialize to JSON
				string jsonString;
				try
				{
					jsonString = JsonConvert.SerializeObject(keyValues, Formatting.Indented);
				}
				catch (Exception ex)
				{
					Console.WriteLine($"Failed to serialize json string: {ex.Message}");
					throw;
				}

				// Save JSON to file
				var saveFilePath = Path.Combine(Directory.GetCurrentDirectory(), EXPORT_FILE_NAME);
				try
				{
					File.WriteAllText(saveFilePath, jsonString);
					Console.WriteLine($"Json file has been written to '{saveFilePath}'");
				}
				catch (Exception ex)
				{
					Console.WriteLine($"Failed to write json to file '{saveFilePath}': {ex.Message}");
					throw;
				}
			}
			catch
			{
				// Empty catch to avoid exiting application before final message.
				// All exceptions have been handled above.
			}

			Console.WriteLine("Press any key to exit");
			Console.ReadKey();
		}

		private static TMode GetLanguageMode<TMode>(string modeString, TMode defaultValue) 
			where TMode : struct
		{
			if (string.IsNullOrEmpty(modeString))
			{
				var defaultLanguageNameMode = defaultValue;
				Console.WriteLine($"No mode has been specified for {typeof(TMode).Name}; using default={defaultLanguageNameMode}. To specify a command line parameter: {string.Join(", ", Enum.GetValues(typeof(TMode))).Select(mode => $"'{mode}'")}.");
				return defaultLanguageNameMode;
			}

			var languageImportMode = Enum.Parse<TMode>(modeString);
			Console.WriteLine($"Exporting with specified {typeof(TMode).Name} '{languageImportMode}'");

			return languageImportMode;
		}
	}
}
