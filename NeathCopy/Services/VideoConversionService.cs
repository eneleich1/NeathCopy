using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NeathCopy.Services
{
    public class VideoConversionSettings
    {
        public string OutputFormat { get; set; }
        public string Bitrate { get; set; }
        public string Resolution { get; set; }
    }

    public class VideoConversionRequest
    {
        public string InputPath { get; set; }
        public string OutputPath { get; set; }
    }

    public class VideoConversionProgress
    {
        public string InputPath { get; set; }
        public string Status { get; set; }
        public double? Percent { get; set; }
        public string Message { get; set; }
    }

    public class VideoConversionService
    {
        private readonly string ffmpegPath;
        private readonly string ffprobePath;

        public VideoConversionService()
        {
            var baseDir = AppDomain.CurrentDomain.BaseDirectory;
            ffmpegPath = Path.Combine(baseDir, "Tools", "ffmpeg-8.0.1-full_build", "bin", "ffmpeg.exe");
            ffprobePath = Path.Combine(baseDir, "Tools", "ffmpeg-8.0.1-full_build", "bin", "ffprobe.exe");
        }

        public string FfmpegPath => ffmpegPath;
        public string FfprobePath => ffprobePath;
        public bool FfmpegExists => File.Exists(ffmpegPath);
        public bool FfprobeExists => File.Exists(ffprobePath);

        public async Task ConvertAsync(
            IReadOnlyList<VideoConversionRequest> requests,
            VideoConversionSettings settings,
            int maxParallel,
            CancellationToken token,
            Action<VideoConversionProgress> progressCallback,
            Action<string> logCallback)
        {
            if (!FfmpegExists)
                throw new FileNotFoundException("ffmpeg executable not found.", ffmpegPath);

            if (requests == null || requests.Count == 0)
                return;

            using (var semaphore = new SemaphoreSlim(Math.Max(1, maxParallel)))
            {
                var tasks = requests.Select(request =>
                    ConvertOneAsync(request, settings, semaphore, token, progressCallback, logCallback));

                await Task.WhenAll(tasks).ConfigureAwait(false);
            }
        }

        private async Task ConvertOneAsync(
            VideoConversionRequest request,
            VideoConversionSettings settings,
            SemaphoreSlim semaphore,
            CancellationToken token,
            Action<VideoConversionProgress> progressCallback,
            Action<string> logCallback)
        {
            await semaphore.WaitAsync(token).ConfigureAwait(false);
            try
            {
                progressCallback?.Invoke(new VideoConversionProgress
                {
                    InputPath = request.InputPath,
                    Status = "Converting"
                });

                double? durationSeconds = await GetDurationAsync(request.InputPath, logCallback).ConfigureAwait(false);

                var args = BuildArguments(request, settings);

                using (var process = new Process())
                {
                    process.StartInfo = new ProcessStartInfo
                    {
                        FileName = ffmpegPath,
                        Arguments = args,
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        CreateNoWindow = true
                    };

                    using (token.Register(() =>
                    {
                        try
                        {
                            if (!process.HasExited)
                                process.Kill();
                        }
                        catch
                        {
                        }
                    }))
                    {
                        process.OutputDataReceived += (sender, e) =>
                        {
                            if (string.IsNullOrWhiteSpace(e.Data))
                                return;

                            if (!TryHandleProgressLine(request.InputPath, e.Data, durationSeconds, progressCallback))
                                logCallback?.Invoke(e.Data);
                        };

                        process.ErrorDataReceived += (sender, e) =>
                        {
                            if (string.IsNullOrWhiteSpace(e.Data))
                                return;

                            logCallback?.Invoke(e.Data);
                        };

                        logCallback?.Invoke(string.Format("Starting conversion: {0}", request.InputPath));
                        process.Start();
                        process.BeginOutputReadLine();
                        process.BeginErrorReadLine();

                        await Task.Run(() => process.WaitForExit()).ConfigureAwait(false);

                        if (token.IsCancellationRequested)
                        {
                            progressCallback?.Invoke(new VideoConversionProgress
                            {
                                InputPath = request.InputPath,
                                Status = "Canceled"
                            });
                            return;
                        }

                        if (process.ExitCode == 0)
                        {
                            progressCallback?.Invoke(new VideoConversionProgress
                            {
                                InputPath = request.InputPath,
                                Status = "Completed",
                                Percent = 100
                            });
                        }
                        else
                        {
                            progressCallback?.Invoke(new VideoConversionProgress
                            {
                                InputPath = request.InputPath,
                                Status = "Failed",
                                Message = string.Format("ffmpeg exited with code {0}.", process.ExitCode)
                            });
                        }
                    }
                }
            }
            catch (OperationCanceledException)
            {
                progressCallback?.Invoke(new VideoConversionProgress
                {
                    InputPath = request.InputPath,
                    Status = "Canceled"
                });
            }
            catch (Exception ex)
            {
                logCallback?.Invoke(string.Format("Conversion failed: {0}", ex.Message));
                progressCallback?.Invoke(new VideoConversionProgress
                {
                    InputPath = request.InputPath,
                    Status = "Failed",
                    Message = ex.Message
                });
            }
            finally
            {
                semaphore.Release();
            }
        }

        private bool TryHandleProgressLine(
            string inputPath,
            string line,
            double? durationSeconds,
            Action<VideoConversionProgress> progressCallback)
        {
            var index = line.IndexOf('=');
            if (index <= 0)
                return false;

            var key = line.Substring(0, index).Trim();
            var value = line.Substring(index + 1).Trim();

            if (key.Equals("out_time_ms", StringComparison.OrdinalIgnoreCase))
            {
                if (durationSeconds.HasValue &&
                    double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var outTimeMs))
                {
                    var percent = Math.Min(100d, (outTimeMs / 1000000d) / durationSeconds.Value * 100d);
                    progressCallback?.Invoke(new VideoConversionProgress
                    {
                        InputPath = inputPath,
                        Percent = percent
                    });
                }
                return true;
            }

            if (key.Equals("progress", StringComparison.OrdinalIgnoreCase) && value.Equals("end", StringComparison.OrdinalIgnoreCase))
            {
                progressCallback?.Invoke(new VideoConversionProgress
                {
                    InputPath = inputPath,
                    Percent = 100
                });
                return true;
            }

            return false;
        }

        private string BuildArguments(VideoConversionRequest request, VideoConversionSettings settings)
        {
            var builder = new StringBuilder();
            builder.Append("-y -hide_banner -i ");
            builder.Append('"').Append(request.InputPath).Append('"').Append(' ');

            if (!string.IsNullOrWhiteSpace(settings.Bitrate))
            {
                builder.Append("-b:v ").Append(settings.Bitrate).Append(' ');
            }

            if (!string.IsNullOrWhiteSpace(settings.Resolution))
            {
                builder.Append("-s ").Append(settings.Resolution).Append(' ');
            }

            builder.Append("-progress pipe:1 -nostats ");
            builder.Append('"').Append(request.OutputPath).Append('"');

            return builder.ToString();
        }

        private async Task<double?> GetDurationAsync(string inputPath, Action<string> logCallback)
        {
            if (!FfprobeExists)
                return null;

            try
            {
                using (var process = new Process())
                {
                    process.StartInfo = new ProcessStartInfo
                    {
                        FileName = ffprobePath,
                        Arguments = string.Format("-v error -show_entries format=duration -of default=noprint_wrappers=1:nokey=1 \"{0}\"", inputPath),
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        CreateNoWindow = true
                    };

                    process.Start();
                    var output = await process.StandardOutput.ReadToEndAsync().ConfigureAwait(false);
                    var error = await process.StandardError.ReadToEndAsync().ConfigureAwait(false);
                    process.WaitForExit();

                    if (!string.IsNullOrWhiteSpace(error))
                        logCallback?.Invoke(error.Trim());

                    if (double.TryParse(output.Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out var seconds))
                        return seconds;
                }
            }
            catch (Exception ex)
            {
                logCallback?.Invoke(string.Format("ffprobe failed: {0}", ex.Message));
            }

            return null;
        }
    }
}
