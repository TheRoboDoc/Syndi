using Newtonsoft.Json;
using System.Reflection;

namespace Syndi
{
    /// <summary>
    ///     Class responsible for managment of files
    /// </summary>
    public static class FileManager
    {
        /// <summary>
        ///     Class responsible for managment of files
        /// </summary>
        public readonly struct Paths
        {
            public static readonly string basePath = AppDomain.CurrentDomain.BaseDirectory;
            public static readonly string dataPath = $@"{basePath}/Data";
        }

        /// <summary>
        ///     Checks that the directory exists
        /// </summary>
        /// 
        /// <returns>
        ///     A list of all directories created
        /// </returns>
        public static async Task<List<string>> DirCheck()
        {
            List<string> list = [];

            await Task.Run(() =>
            {
                Paths paths = new();

                foreach (FieldInfo field in typeof(Paths).GetFields())
                {
                    string? path = field.GetValue(paths)?.ToString();

                    if (string.IsNullOrEmpty(path))
                    {
                        continue;
                    }

                    DirectoryInfo directoryInfo = new(path);

                    if (!directoryInfo.Exists)
                    {
                        directoryInfo.Create();
                        list.Add(field.Name);
                    }
                }
            });

            return list;
        }

        /// <summary>
        ///     Checks if file exists
        /// </summary>
        /// 
        /// <param name="fileDir">
        ///     File location
        /// </param>
        /// 
        /// <returns>
        ///     <list type="table">
        ///         <item>
        ///             <c>True</c>: File exists
        ///         </item>
        ///         
        ///         <item>
        ///             <c>False</c>: File doesn't exist
        ///         </item>
        ///     </list>
        /// </returns>
        public static bool FileExists(string fileDir)
        {
            FileInfo fileInfo = new(fileDir);

            return fileInfo.Exists;
        }

        public static bool DirectoryExists(string directoryDir)
        {
            DirectoryInfo directoryInfo = new(directoryDir);

            return directoryInfo.Exists;
        }

        public static void CreateDirectory(string path)
        {
            DirectoryInfo directoryInfo = new(path);

            directoryInfo.Create();
        }

        public static void CreateFile(string fileDir)
        {
            FileInfo fileInfo = new(fileDir);

            fileInfo.Create().Dispose();
        }

        public struct ChannelSetting
        {
            public List<string> RSSLinks { get; set; }
        }

        public struct Messages
        {
            public List<string> MessagesList { get; set; }
        }

        private static string IDToPath(string guildID, string channelID)
        {
            return $@"{Paths.dataPath}/{guildID}/{channelID}";
        }

        public static ChannelSetting? ReadChannelSettings(string guildID, string channelID)
        {
            string path = $"{IDToPath(guildID, channelID)}/settings.json";

            if (!FileExists(path))
            {
                CreateDirectory(IDToPath(guildID, channelID));
                CreateFile(path);
            }

            string jsonString = File.ReadAllText(path);

            ChannelSetting? setting = null;

            if (!string.IsNullOrEmpty(jsonString))
            {
                setting = JsonConvert.DeserializeObject<ChannelSetting>(jsonString);
            }
            else
            {
                ChannelSetting newSettings = new()
                {
                    RSSLinks = []
                };

                setting = newSettings;
            }

            return setting;
        }

        public static void WriteChannelSettings(ChannelSetting settings, string guildID, string channelID)
        {
            string path = $"{IDToPath(guildID, channelID)}/settings.json";

            CreateDirectory(IDToPath(guildID, channelID));

            using StreamWriter fileStream = File.CreateText(path);

            JsonSerializerSettings serializerSettings = new()
            {
                Formatting = Formatting.Indented
            };

            string json = JsonConvert.SerializeObject(settings, serializerSettings);

            fileStream.Write(json);
        }
    }
}
