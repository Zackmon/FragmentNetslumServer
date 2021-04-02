using Serilog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;

namespace FragmentServerWV
{

    /// <summary>
    /// A simple key-value pair configuration class that also supports line comments
    /// </summary>
    /// <remarks>
    /// INI lite
    /// </remarks>
    public sealed class SimpleConfiguration
    {

        private readonly Dictionary<string, string> configurationValues;
        private readonly ILogger logger;


        /// <summary>
        /// Gets a <see cref="ReadOnlyDictionary{TKey, TValue}"/> that represents the loaded configuration
        /// </summary>
        public ReadOnlyDictionary<string, string> Values => new ReadOnlyDictionary<string, string>(configurationValues);

        /// <summary>
        /// Creates a new instance of the Configuration class
        /// </summary>
        public SimpleConfiguration(ILogger logger)
        {
            configurationValues = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            logger.Information("Loading Settings...");
            if (File.Exists("settings.txt"))
            {
                logger.Information("Settings file found, importing...");
                var lines = File.ReadAllLines("settings.txt");
                foreach (var line in lines)
                {
                    logger.Debug(line);
                    var clean = line.Trim();
                    if (!string.IsNullOrWhiteSpace(clean) && !clean.StartsWith("#"))
                    {
                        var parts = line.Split('=');
                        var key = parts[0];
                        var value = string.Join('=', lines[1..]);
                        logger.Information($"{key}: {value}");
                        configurationValues.Add(key.ToLowerInvariant(), value);
                    }
                }
                    
            }

            this.logger = logger;
        }

        /// <summary>
        /// Retrieves a value if it exists, otherwise returns <paramref name="default"/>
        /// </summary>
        /// <param name="name">The name of the configuration value</param>
        /// <param name="default">The value to return if <paramref name="name"/> doesn't exist</param>
        /// <returns><see cref="string"/></returns>
        public string Get(string name, string @default = "") => configurationValues.ContainsKey(name) ? configurationValues[name] : @default;

    }

}
