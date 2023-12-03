using Newtonsoft.Json;
using System.Collections.Concurrent;
using Wipedir.CommandLine;

namespace Wipedir.Executor;
internal static class WipedirTemplateExecutor
{
    internal static readonly string m_TemplatePath = Path.Combine(Environment.CurrentDirectory, "Templates");
    private static readonly ConcurrentDictionary<string, CommandLineArguments> m_Templates = new();

    #region [RunWithTemplate]
    internal static bool RunWithTemplate(string templateName)
    {
        ArgumentNullException.ThrowIfNullOrEmpty(templateName);
        ArgumentNullException.ThrowIfNullOrWhiteSpace(templateName);

        _ = __LoadTemplates();

        if (!m_Templates.TryGetValue(templateName, out CommandLineArguments args))
        {
            return false;
        }

        ArgumentNullException.ThrowIfNull(args);

        new WipedirExecutor(args).Run();
        return true;
    }
    #endregion

    #region [__LoadTemplates]
    private static bool __LoadTemplates()
    {
        if (!Directory.Exists(m_TemplatePath))
            return false;

        var files = Directory.GetFiles(m_TemplatePath).Where(x => x.EndsWith(".json")).ToList();

        if (files.Count == 0) return false;

        _ = Parallel.ForEach(files, (file) =>
        {
            try
            {
                var fileContent = File.ReadAllText(file) ?? string.Empty;
                var parsed = __ParseTemplate(fileContent);
                var templateName = file.Replace(".json", string.Empty)
                                       .Split('\\')
                                       .LastOrDefault();
                _ = m_Templates.TryAdd(templateName, parsed);
            }
            catch
            {
                // Catch and continue
            }
        });

        return true;
    }
    #endregion

    #region [__ParseTemplate]
    private static CommandLineArguments __ParseTemplate(string templateJson)
    {
        ArgumentNullException.ThrowIfNullOrWhiteSpace(templateJson);
        ArgumentNullException.ThrowIfNullOrEmpty(templateJson);

        var parsedObject = JsonConvert.DeserializeObject<CommandLineArguments>(templateJson);

        ArgumentNullException.ThrowIfNull(parsedObject);

        return parsedObject;
    }
    #endregion
}
