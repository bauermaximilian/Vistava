// SPDX-License-Identifier: GPL-3.0-or-later

using System.Text.Json;
using Microsoft.Extensions.FileProviders;
using Vistava.Service.Common;

namespace Vistava.Service.Services;

public class SourcesInfoProvider(IFileProvider fileProvider)
{
    private const string SourceDefinitionSuffix = ".source.json";

    private readonly JsonSerializerOptions sourceDefinitionSerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = true
    };

    public async Task<SourceInformation> GetSourceInformationAsync(CancellationToken token)
    {
        var sourceInfo = new SourceInformation(new Dictionary<string, SourceConfiguration>(), []);

        foreach (var fileEntry in fileProvider.GetDirectoryContents("Sources"))
        {
            token.ThrowIfCancellationRequested();

            var fileName = fileEntry.Name.ToLower();
            int sourceDefinitionSuffixIndex = fileName.LastIndexOf(SourceDefinitionSuffix);
            if (fileName.EndsWith(SourceDefinitionSuffix))
            {
                var sourceName = fileName[..sourceDefinitionSuffixIndex];
                if (sourceInfo.Sources.ContainsKey(sourceName))
                {
                    sourceInfo.Errors.Add($"The source '{sourceName}' was defined more than " +
                        "once - only the first encounter will be loaded.");
                }

                SourceConfiguration? sourceConfiguration = null;
                try
                {
                    using var fileEntryStream = fileEntry.CreateReadStream();
                    try
                    {
                        sourceConfiguration = await JsonSerializer.DeserializeAsync<SourceConfiguration>(
                            fileEntryStream, sourceDefinitionSerializerOptions, token) ??
                            throw new InvalidOperationException("The structure must not be null.");
                    }
                    catch (Exception exc)
                    {
                        throw new InvalidOperationException($"The configuration was invalid JSON: {exc.Message}");
                    }

                    if (string.IsNullOrWhiteSpace(sourceConfiguration.ModuleFileName))
                    {
                        throw new InvalidOperationException(
                            "The configuration did not provide a valid module file name.");
                    }

                    var moduleFileInfo = fileProvider.GetFileInfo("Sources/" + sourceConfiguration.ModuleFileName);
                    if (!moduleFileInfo.Exists || moduleFileInfo.IsDirectory)
                    {
                        throw new InvalidOperationException("The module root file wasn't found.");
                    }

                    if (!sourceConfiguration.ModuleFileName.StartsWith(sourceName + "/"))
                    {
                        throw new InvalidOperationException("The module root file must reside in a subdirectory " +
                            "with the same name as the module identifier.");
                    }

                    sourceInfo.Sources[sourceName] = sourceConfiguration;
                }
                catch (Exception exc)
                {
                    sourceInfo.Errors.Add($"The extension \"{sourceName}\" couldn't be loaded. " + exc.Message);
                }
            }
        }
        return sourceInfo;
    }
}