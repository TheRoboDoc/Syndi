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

        public static void CreateFile(string fileDir)
        {
            FileInfo fileInfo = new(fileDir);

            fileInfo.Create().Dispose();
        }

        public struct ChannelSetting
        {
            public List<string> RSSLinks;
        }

        public struct Messages
        {
            public List<string> messages;
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
                CreateFile(path);
            }

            string jsonString = File.ReadAllText(path);

            ChannelSetting? setting = new ChannelSetting();

            if (!string.IsNullOrEmpty(jsonString))
            {
                setting = JsonConvert.DeserializeObject<ChannelSetting>(jsonString);
            }

            return setting;
        }

        public static void WriteChannelSettings(ChannelSetting settings, string guildID, string channelID)
        {
            ChannelSetting? setting;

            string path = $"{IDToPath(guildID, channelID)}/settings.json";

            FileInfo fileInfo = new(path);

            try
            {
                setting = ReadChannelSettings(guildID, channelID);
                fileInfo.Delete();
            }
            catch
            {
                setting = new ChannelSetting();
            }

            if (setting == null)
            {
                return;
            }

            foreach (string RSSLink in settings.RSSLinks)
            {
                setting.Value.RSSLinks?.Add(RSSLink);
            }

            using StreamWriter fileStream = File.CreateText(path);

            JsonSerializerSettings serializerSettings = new()
            {
                Formatting = Formatting.Indented
            };

            string json = JsonConvert.SerializeObject(setting, serializerSettings);

            fileStream.Write(json);
        }

        public static Messages? ReadSentMessages(string guildID, string channelID)
        {
            string path = $"{IDToPath(guildID, channelID)}/messages.json";

            if (!FileExists(path))
            {
                CreateFile(path);
            }

            string jsonString = File.ReadAllText(path);

            Messages? setting = new Messages();

            if (!string.IsNullOrEmpty(jsonString))
            {
                setting = JsonConvert.DeserializeObject<Messages>(jsonString);
            }

            return setting;
        }

        public static void WriteSentMessage(Messages messages, string guildID, string channelID)
        {
            Messages? message;

            string path = $"{IDToPath(guildID, channelID)}/settings.json";

            FileInfo fileInfo = new(path);

            try
            {
                message = ReadSentMessages(guildID, channelID);
                fileInfo.Delete();
            }
            catch
            {
                message = new Messages();
            }

            if (message == null)
            {
                return;
            }

            foreach (string messageEntry in messages.messages)
            {
                message.Value.messages?.Add(messageEntry);
            }

            using StreamWriter fileStream = File.CreateText(path);

            JsonSerializerSettings serializerSettings = new()
            {
                Formatting = Formatting.Indented
            };

            string json = JsonConvert.SerializeObject(message, serializerSettings);

            fileStream.Write(json);
        }
    }
}
