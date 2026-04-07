using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Text;

namespace CodeBureau.DeleteOld
{
    /// <summary>
    /// Core service for deleting old files.
    /// </summary>
    public class DeleteOldService
    {
        private readonly TextWriter _output;
        private readonly TextWriter _error;

        public DeleteOldService(TextWriter? output = null, TextWriter? error = null)
        {
            _output = output ?? Console.Out;
            _error = error ?? Console.Error;
        }

        /// <summary>
        /// Validate options for completeness.
        /// </summary>
        public bool ValidateOptions(DeleteOldOptions options)
        {
            if (options.Age < 0)
            {
                _error.WriteLine("Age must be >= 0");
                return false;
            }

            if (!Directory.Exists(options.Path))
            {
                _error.WriteLine($"Directory does not exist: {options.Path}");
                return false;
            }

            if (!Regex.IsMatch(options.TimeFrame, @"^[sndmhy]$"))
            {
                _error.WriteLine("TimeFrame must be one of: s, n, h, d, m, y");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Execute the delete operation.
        /// </summary>
        public int Execute(DeleteOldOptions options)
        {
            if (!ValidateOptions(options))
                return -1;

            var runTime = DateTime.Now;
            var deleteOlderThan = CalculateDeleteAge(runTime, options.Age, options.TimeFrame);

            if (!options.QuietMode)
            {
                if (options.SimulateOnly)
                    _output.WriteLine($"SIMULATION ONLY:\nDeleting {options.Filter} from {options.Path} older than {deleteOlderThan:dd/MM/yyyy hh:mm:ss}");
                else
                    _output.WriteLine($"Deleting {options.Filter} from {options.Path} older than {deleteOlderThan:dd/MM/yyyy hh:mm:ss}");
            }

            try
            {
                var directory = new DirectoryInfo(options.Path);
                var rootPath = Path.GetFullPath(options.Path);
                DeleteFilesFromFolder(directory, options, deleteOlderThan, rootPath);
                return 0;
            }
            catch (Exception ex)
            {
                if (!options.QuietMode)
                    _error.WriteLine($"Error: {ex.Message}");
                return -1;
            }
        }

        /// <summary>
        /// Calculate the age threshold.
        /// </summary>
        private DateTime CalculateDeleteAge(DateTime runTime, int age, string timeFrame)
        {
            return timeFrame switch
            {
                "s" => runTime.AddSeconds(-age),
                "n" => runTime.AddMinutes(-age),
                "h" => runTime.AddHours(-age),
                "d" => runTime.AddDays(-age),
                "m" => runTime.AddMonths(-age),
                "y" => runTime.AddYears(-age),
                _ => runTime.AddDays(-age)
            };
        }

        /// <summary>
        /// Recursively delete files and optionally remove empty folders (except root).
        /// </summary>
        private void DeleteFilesFromFolder(DirectoryInfo folder, DeleteOldOptions options, DateTime deleteOlderThan, string rootPath)
        {
            // Delete files matching the filter and age
            foreach (var file in folder.GetFiles(options.Filter))
            {
                if (file.LastWriteTime < deleteOlderThan)
                {
                    try
                    {
                        if (!options.QuietMode)
                            _output.WriteLine($"Deleting {file.FullName}");

                        if (!options.SimulateOnly)
                            file.Delete();
                    }
                    catch (Exception ex)
                    {
                        if (!options.QuietMode)
                            _error.WriteLine($"Could not delete file {file.FullName}: {ex.Message}");
                    }
                }
            }

            // Recurse into subdirectories if requested
            if (options.RecurseSubfolders)
            {
                foreach (var subFolder in folder.GetDirectories())
                {
                    DeleteFilesFromFolder(subFolder, options, deleteOlderThan, rootPath);
                }
            }

            // Remove empty folders if requested (but NOT the root path)
            var currentPath = Path.GetFullPath(folder.FullName);
            if (options.RemoveEmptyFolders && 
                currentPath != rootPath && 
                folder.GetFiles().Length == 0 && 
                folder.GetDirectories().Length == 0)
            {
                try
                {
                    if (!options.QuietMode)
                        _output.WriteLine($"Deleting empty folder {folder.FullName}");

                    if (!options.SimulateOnly)
                        folder.Delete(true);
                }
                catch (Exception ex)
                {
                    if (!options.QuietMode)
                        _error.WriteLine($"Could not delete folder {folder.FullName}: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Get usage help text.
        /// </summary>
        public static string GetUsage()
        {
            var sb = new StringBuilder();
            sb.AppendLine();
            sb.AppendLine("Deletes files older than a specified age");
            sb.AppendLine();
            sb.AppendLine("Usage: DeleteOld [OPTIONS]");
            sb.AppendLine();
            sb.AppendLine("Options:");
            sb.AppendLine("  -p, --path <PATH>           Path to search (default: current directory)");
            sb.AppendLine("  -f, --filter <FILTER>       File filter pattern (default: *)");
            sb.AppendLine("  -a, --age <AGE>             Age of files to delete (required)");
            sb.AppendLine("  -t, --timeframe <TIMEFRAME> Time unit: s|n|h|d|m|y (default: d)");
            sb.AppendLine("                              s=second, n=minute, h=hour, d=day, m=month, y=year");
            sb.AppendLine("  -s, --recurse               Recurse into subdirectories (default: false)");
            sb.AppendLine("  -r, --remove-empty          Remove empty folders (default: false)");
            sb.AppendLine("  -q, --quiet                 Suppress all output (default: false)");
            sb.AppendLine("  -n, --simulate              Simulate only (don't actually delete) (default: false)");
            sb.AppendLine("  -h, --help                  Show this help message");
            sb.AppendLine();
            sb.AppendLine("Example:");
            sb.AppendLine("  DeleteOld --path \"C:\\temp\" --filter \"*.log\" --age 30 --timeframe d --recurse --remove-empty");
            sb.AppendLine();

            return sb.ToString();
        }
    }
}
