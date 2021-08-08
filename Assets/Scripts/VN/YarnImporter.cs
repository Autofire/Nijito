using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using UnityEngine;
using Yarn.Compiler;

namespace Dialogue
{
	public static class YarnImporter
	{

		public static YarnProgram FromString(string name, string contents)
		{
			try
			{
				IDictionary<string, Yarn.Compiler.StringInfo> stringTable;
				Yarn.Program compiledProgram;

				Compiler.CompileString(contents, name, out compiledProgram, out stringTable);

				return PrepareProgram(compiledProgram, stringTable, name);
			}
			catch (ParseException e)
			{
				Debug.LogError(e.Message);
				return null;
			}


		}

		public static YarnProgram FromFile(string path)
		{

			try
			{
				string fileName = Path.GetFileNameWithoutExtension(path);

				IDictionary<string, Yarn.Compiler.StringInfo> stringTable;
				Yarn.Program compiledProgram;

				Compiler.CompileFile(path, out compiledProgram, out stringTable);

				return PrepareProgram(compiledProgram, stringTable, fileName);
			}
			catch (ParseException e)
			{
				Debug.LogError(e.Message);
				return null;
			}

		}


		private static YarnProgram PrepareProgram(Yarn.Program compiledProgram, IDictionary<string, Yarn.Compiler.StringInfo> stringTable, string name)
		{
			// This code came out of Yarn's private scripts. For some reason,
			// they make the Compiler publicly accessable and then don't give you any way to
			// convert compiled programs into YarnPrograms, which the runner clearly needs to
			// function. I'm should make an issue on the Yarn Spinner repo about this...

			try
			{
				YarnProgram programContainer = ScriptableObject.CreateInstance<YarnProgram>(); // new YarnProgram();

				YarnTranslation[] localizations = new YarnTranslation[0];
				string baseLanguageID = CultureInfo.CurrentCulture.Name;

				// Create a container for storing the bytes
				using (var memoryStream = new MemoryStream())
				using (var outputStream = new Google.Protobuf.CodedOutputStream(memoryStream))
				{

					// Serialize the compiled program to memory
					compiledProgram.WriteTo(outputStream);
					outputStream.Flush();

					byte[] compiledBytes = memoryStream.ToArray();

					programContainer.compiledProgram = compiledBytes;

					// var outPath = Path.ChangeExtension(ctx.assetPath, ".yarnc");
					// File.WriteAllBytes(outPath, compiledBytes);
				}

				if (stringTable.Count > 0)
				{
					using (var memoryStream = new MemoryStream())
					using (var textWriter = new StreamWriter(memoryStream))
					{
						// Generate the localised .csv file

						// Use the invariant culture when writing the CSV
						var configuration = new CsvHelper.Configuration.Configuration(CultureInfo.InvariantCulture);

						var csv = new CsvHelper.CsvWriter(
							textWriter, // write into this stream
							configuration // use this configuration
							);

						var lines = stringTable.Select(x => new
						{
							id = x.Key,
							text = x.Value.text,
							file = x.Value.fileName,
							node = x.Value.nodeName,
							lineNumber = x.Value.lineNumber
						});

						csv.WriteRecords(lines);
						textWriter.Flush();
						memoryStream.Position = 0;

						using (var reader = new StreamReader(memoryStream))
						{
							var textAsset = new TextAsset(reader.ReadToEnd());
							textAsset.name = $"{name} ({baseLanguageID})";

							programContainer.baseLocalisationStringTable = textAsset;
							programContainer.localizations = localizations;
						}

						//stringIDs = lines.Select(l => l.id).ToArray();

					}
				}

				Debug.LogFormat("Yarn size: ", programContainer.GetProgram().CalculateSize());

				return programContainer;
			}
			catch (ParseException e)
			{
				Debug.LogError(e.Message);
				return null;
			}


		}
	}
}
