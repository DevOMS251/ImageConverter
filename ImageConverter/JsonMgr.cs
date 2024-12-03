using Newtonsoft.Json;
using System.IO;

namespace ImageConverter
{
    public class Settings
    {
        public string FolderPath { get; set; } = "C:\\";    // 폴더 경로 기본값
        public int ImageSize { get; set; } = 32;        // 이미지 크기 기본값
    }

    public static class JsonMgr
    {
        private const string JsonFilePath = "settings.json"; // JSON 파일 경로

        /// <summary>
        /// JSON 파일에서 설정 값을 불러옵니다.
        /// 파일이 없으면 기본 설정으로 새 파일을 생성합니다.
        /// </summary>
        /// <returns>Settings 객체를 반환</returns>
        public static Settings GetSettingsFromJson()
        {
            if (File.Exists(JsonFilePath))
            {
                var jsonContent = File.ReadAllText(JsonFilePath);
                return JsonConvert.DeserializeObject<Settings>(jsonContent);
            }
            else
            {
                // 파일이 없는 경우 기본 설정 파일 생성
                var defaultSettings = new Settings();
                SaveSettingsToJson(defaultSettings.FolderPath, defaultSettings.ImageSize);
                return defaultSettings;
            }
        }

        /// <summary>
        /// 설정 값을 JSON 파일에 저장합니다.
        /// </summary>
        /// <param name="folderPath">폴더 경로</param>
        /// <param name="imageSize">이미지 크기</param>
        public static void SaveSettingsToJson(string folderPath, int imageSize)
        {
            var settings = new Settings
            {
                FolderPath = folderPath,
                ImageSize = imageSize
            };

            string jsonContent = JsonConvert.SerializeObject(settings, Formatting.Indented);
            File.WriteAllText(JsonFilePath, jsonContent);
        }
    }
}
