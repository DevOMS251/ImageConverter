using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace ImageConverter
{
    public partial class MainWindow : Window
    {
        private const int IconsPerRow = 4; // 행당 아이콘 개수

        public MainWindow()
        {
            InitializeComponent();
            Settings Setting = JsonMgr.GetSettingsFromJson();
            TS_SelectFolderPath.Text = Setting.FolderPath;
            TI_IconSize.Text = Setting.ImageSize.ToString();
        }

        private void DisplayIconsInWrapPanel(string folderPath)
        {
            try
            {
                if (!Directory.Exists(folderPath))
                {
                    StateMsg.Text = "Folder does not exist.";
                    return;
                }

                // .ico 파일 가져오기 및 정렬
                string[] icoFiles = Directory.GetFiles(folderPath, "*.ico");
                var sortedFiles = icoFiles
                    .OrderByDescending(file => File.GetCreationTime(file))
                    .ToArray();

                // Grid 초기화
                IconGrid.Children.Clear();
                IconGrid.RowDefinitions.Clear();
                IconGrid.ColumnDefinitions.Clear();

                if (sortedFiles.Length > 0)
                {
                    // 행과 열 정의
                    int totalRows = (int)Math.Ceiling((double)sortedFiles.Length / IconsPerRow);
                    for (int i = 0; i < totalRows; i++)
                        IconGrid.RowDefinitions.Add(new RowDefinition());

                    for (int i = 0; i < IconsPerRow; i++)
                        IconGrid.ColumnDefinitions.Add(new ColumnDefinition());

                    // 아이콘 추가
                    for (int i = 0; i < sortedFiles.Length; i++)
                    {
                        var iconImage = new Image
                        {
                            Source = new BitmapImage(new Uri(sortedFiles[i])),
                            Width = (IconGrid.ActualWidth / IconsPerRow) - 10,
                            Height = IconGrid.ActualWidth / IconsPerRow
                        };

                        Grid.SetRow(iconImage, i / IconsPerRow);
                        Grid.SetColumn(iconImage, i % IconsPerRow);
                        IconGrid.Children.Add(iconImage);
                    }
                }
                else
                {
                    // 표시할 이미지가 없을 때 텍스트 추가
                    var noImagesText = new TextBlock
                    {
                        Text = "표시할 이미지가 없습니다.",
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center,
                        FontSize = 16,
                        Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Gray)
                    };
                    IconGrid.Children.Add(noImagesText);
                }
            }
            catch (Exception ex)
            {
                StateMsg.Text = ex.Message;
            }
        }


        private string GenerateUniqueFileName(string folderPath, string baseFileName, string extension)
        {
            string outputFilePath = Path.Combine(folderPath, $"{baseFileName}{extension}");
            int count = 1;

            while (File.Exists(outputFilePath))
            {
                outputFilePath = Path.Combine(folderPath, $"{baseFileName} ({count}){extension}");
                count++;
            }

            return outputFilePath;
        }

        private void ProcessDroppedFile(string filePath, byte imageSize)
        {
            FileInfo inputFI = new FileInfo(filePath);
            string extension = Path.GetExtension(filePath).ToLower();

            if (extension != ".jpg" && extension != ".png" && extension != ".bmp")
            {
                StateMsg.Text = "Only image files (.jpg, .png, .bmp) are supported.";
                return;
            }

            string outputFilePath = GenerateUniqueFileName(TS_SelectFolderPath.Text, Path.GetFileNameWithoutExtension(inputFI.Name), ".ico");

            // 변환 및 결과 표시
            double formattedValue = Converter.ConvertToIco(imageSize, inputFI.FullName, outputFilePath) / 100000.0;
            StateMsg.Text = $"Created File Name: {Path.GetFileName(outputFilePath)} / Created Time: {formattedValue:F6}";

            DroppedImage.Source = new BitmapImage(new Uri(filePath));
            DroppedFilePath.Text = filePath;

            DisplayIconsInWrapPanel(TS_SelectFolderPath.Text);
            JsonMgr.SaveSettingsToJson(TS_SelectFolderPath.Text, Convert.ToInt32(TI_IconSize.Text));
        }

        private void Border_Drop(object sender, DragEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TS_SelectFolderPath.Text))
            {
                StateMsg.Text = "You must select a folder.";
                return;
            }

            if (string.IsNullOrWhiteSpace(TI_IconSize.Text) || !byte.TryParse(TI_IconSize.Text, out byte imageSize))
            {
                StateMsg.Text = "Enter a valid number for the icon size.";
                return;
            }

            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (files?.Length > 0)
                {
                    foreach (var file in files)
                    {
                        ProcessDroppedFile(file, imageSize);
                    }
                }
            }
        }

        private void SelectFolder(object sender, RoutedEventArgs e)
        {
            var fbd = new System.Windows.Forms.FolderBrowserDialog();
            if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                DisplayIconsInWrapPanel(fbd.SelectedPath);
                TS_SelectFolderPath.Text = fbd.SelectedPath;
                JsonMgr.SaveSettingsToJson(fbd.SelectedPath, Convert.ToInt32(TI_IconSize.Text));
            }
        }

        private void IconGrid_Loaded(object sender, RoutedEventArgs e)
        {
            DisplayIconsInWrapPanel(TS_SelectFolderPath.Text);
        }
    }
}
