using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using YoutubeExplode;
using YoutubeExplode.Videos.Streams;

namespace YouTubeDownloader
{
    public partial class Form1 : Form
    {
        private YoutubeClient youtube => new YoutubeClient(); // Lazy init, nhanh hơn khi khởi động
        private StreamManifest streamManifest;
        private string videoTitle = "";
        private string videoId = "";
        private CancellationTokenSource cts = new CancellationTokenSource(); // Token để dừng tải

        public Form1()
        {
            InitializeComponent();
        }

        private async void btnGetInfo_Click(object sender, EventArgs e)
        {
            try
            {
                lblStatus.Text = "Đang phân tích...";
                cmbStreams.Items.Clear();

                var video = await youtube.Videos.GetAsync(txtUrl.Text);
                videoTitle = video.Title;
                videoId = video.Id;

                streamManifest = await youtube.Videos.Streams.GetManifestAsync(video.Id);

                var videoOnlyStreams = streamManifest.GetVideoOnlyStreams()
                    .OrderByDescending(s => s.VideoQuality.MaxHeight)
                    .ToList();

                if (videoOnlyStreams.Count == 0)
                {
                    MessageBox.Show("Không tìm thấy chất lượng video.");
                    lblStatus.Text = "Không có video.";
                    return;
                }

                foreach (var stream in videoOnlyStreams)
                {
                    cmbStreams.Items.Add($"{stream.VideoQuality.Label} - {stream.Container.Name} - {stream.Size.MegaBytes:N2} MB");
                }

                cmbStreams.SelectedIndex = 0;
                lblStatus.Text = "Chọn chất lượng và tải.";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi: {ex.Message}");
                lblStatus.Text = "Lỗi phân tích video.";
            }
        }

        private async void btnDownload_Click(object sender, EventArgs e)
        {
            string videoPath = "";
            string audioPath = "";

            try
            {
                if (cmbStreams.SelectedIndex < 0)
                {
                    MessageBox.Show("Vui lòng chọn chất lượng.");
                    return;
                }

                var saveFileDialog = new SaveFileDialog();
                saveFileDialog.FileName = SanitizeFileName(videoTitle) + ".mp4";
                saveFileDialog.Filter = "MP4 Video (*.mp4)|*.mp4";
                saveFileDialog.Title = "Chọn nơi lưu và đặt tên video";

                if (saveFileDialog.ShowDialog() != DialogResult.OK)
                    return;

                string outputPath = saveFileDialog.FileName;
                string folderPath = Path.GetDirectoryName(outputPath);
                string fileNameWithoutExt = Path.GetFileNameWithoutExtension(outputPath);

                lblStatus.Text = "Đang chuẩn bị tải...";
                progressBar.Value = 0;

                var selectedVideo = streamManifest.GetVideoOnlyStreams()
                    .OrderByDescending(s => s.VideoQuality.MaxHeight)
                    .ElementAt(cmbStreams.SelectedIndex);

                var audioStream = streamManifest.GetAudioOnlyStreams()
                    .OrderByDescending(s => s.Bitrate)
                    .FirstOrDefault();

                if (audioStream == null)
                {
                    MessageBox.Show("Không tìm thấy audio cho video này.");
                    return;
                }

                videoPath = Path.Combine(folderPath, $"{fileNameWithoutExt}_video.{selectedVideo.Container.Name}");
                audioPath = Path.Combine(folderPath, $"{fileNameWithoutExt}_audio.{audioStream.Container.Name}");

                var videoProgress = new Progress<double>(p =>
                {
                    progressBar.Value = (int)(p * 50);
                    lblStatus.Text = $"Đang tải video... {progressBar.Value}%";
                });

                var audioProgress = new Progress<double>(p =>
                {
                    progressBar.Value = 50 + (int)(p * 50);
                    lblStatus.Text = $"Đang tải audio... {progressBar.Value}%";
                });

                cts = new CancellationTokenSource();

                await youtube.Videos.Streams.DownloadAsync(selectedVideo, videoPath, videoProgress, cts.Token);
                await youtube.Videos.Streams.DownloadAsync(audioStream, audioPath, audioProgress, cts.Token);

                lblStatus.Text = "Đang ghép video + audio...";
                await MergeVideoAndAudio(videoPath, audioPath, outputPath);

                lblStatus.Text = "Tải hoàn tất!";
            }
            catch (OperationCanceledException)
            {
                lblStatus.Text = "Đã hủy tải.";
                MessageBox.Show("Đã hủy quá trình tải.");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải: {ex.Message}");
                lblStatus.Text = "Tải thất bại.";
            }
            finally
            {
                try
                {
                    if (File.Exists(videoPath)) File.Delete(videoPath);
                    if (File.Exists(audioPath)) File.Delete(audioPath);
                }
                catch { }
            }
        }

        private async Task MergeVideoAndAudio(string videoPath, string audioPath, string outputPath)
        {
            await Task.Run(() =>
            {
                string ffmpegPath = ExtractFFmpeg();

                if (!File.Exists(ffmpegPath))
                    throw new FileNotFoundException("Không tìm thấy ffmpeg.exe");

                var startInfo = new ProcessStartInfo
                {
                    FileName = ffmpegPath,
                    Arguments = $"-y -i \"{videoPath}\" -i \"{audioPath}\" -c:v copy -c:a aac \"{outputPath}\"",
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };

                using (var process = new Process { StartInfo = startInfo })
                {
                    process.Start();

                    string errorOutput = process.StandardError.ReadToEnd();
                    string output = process.StandardOutput.ReadToEnd();

                    if (!process.WaitForExit(20000))
                    {
                        process.Kill();
                        throw new Exception("Ghép video quá lâu. Có thể ffmpeg bị treo.");
                    }

                    if (process.ExitCode != 0)
                    {
                        throw new Exception("Ghép video thất bại.");
                    }
                }
            });
        }

        private string SanitizeFileName(string name)
        {
            foreach (char c in Path.GetInvalidFileNameChars())
                name = name.Replace(c, '_');
            return name;
        }

        private string ExtractFFmpeg()
        {
            string resourceName = "YoutubeDownloader.ffmpeg.exe";
            string outputPath = Path.Combine(Path.GetTempPath(), "ffmpeg_temp.exe");

            if (!File.Exists(outputPath))
            {
                using (var resource = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName))
                {
                    if (resource == null)
                        throw new Exception("Không tìm thấy tài nguyên nhúng ffmpeg.exe");

                    using (var file = new FileStream(outputPath, FileMode.Create, FileAccess.Write))
                    {
                        resource.CopyTo(file);
                    }
                }
            }

            return outputPath;
        }
        private void btnCancel_Click(object sender, EventArgs e)
        {
            cts?.Cancel();
            lblStatus.Text = "Đang hủy...";
        }
    }
}
