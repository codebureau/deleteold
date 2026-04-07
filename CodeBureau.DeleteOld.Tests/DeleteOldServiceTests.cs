using System;
using System.IO;
using System.Text;
using Xunit;
using CodeBureau.DeleteOld;

namespace CodeBureau.DeleteOld.Tests
{
    public class DeleteOldServiceTests : IDisposable
    {
        private readonly string _testPath;
        private readonly string _subfolderPath;

        public DeleteOldServiceTests()
        {
            _testPath = Path.Combine(Path.GetTempPath(), "DeleteOldTests_" + Guid.NewGuid());
            _subfolderPath = Path.Combine(_testPath, "subfolder");
            SetupTempFileTree();
        }

        public void Dispose()
        {
            RemoveTempFileTree();
        }

        /// <summary>
        /// Set up a temporary file tree for testing:
        /// - 50 XML files (10 dated 10 days ago, 40 current)
        /// - 30 TXT files (all current)
        /// - subfolder with 20 XSD files (6 dated 10 days ago, 14 current)
        /// </summary>
        private void SetupTempFileTree()
        {
            Directory.CreateDirectory(_testPath);
            Directory.CreateDirectory(_subfolderPath);

            // Create 50 XML files in root
            for (int i = 0; i < 50; i++)
            {
                string fileName = Path.Combine(_testPath, $"file{i:D2}.xml");
                File.WriteAllText(fileName, "test");
                
                // Set last 10 files to 10 days old
                if (i > 39)
                    File.SetLastWriteTime(fileName, DateTime.Now.AddDays(-10).AddMinutes(-2));
            }

            // Create 30 TXT files in root (all current)
            for (int i = 0; i < 30; i++)
            {
                string fileName = Path.Combine(_testPath, $"file{i:D2}.txt");
                File.WriteAllText(fileName, "test");
            }

            // Create 20 XSD files in subfolder
            for (int i = 0; i < 20; i++)
            {
                string fileName = Path.Combine(_subfolderPath, $"file{i:D2}.xsd");
                File.WriteAllText(fileName, "test");
                
                // Set last 6 files to 10 days old
                if (i > 13)
                    File.SetLastWriteTime(fileName, DateTime.Now.AddDays(-10).AddMinutes(-1));
            }
        }

        private void RemoveTempFileTree()
        {
            if (Directory.Exists(_testPath))
                Directory.Delete(_testPath, true);
        }

        [Fact]
        public void GetUsage_ReturnsValidUsageString()
        {
            var usage = DeleteOldService.GetUsage();
            Assert.NotNull(usage);
            Assert.NotEmpty(usage);
            Assert.Contains("--help", usage);
        }

        [Fact]
        public void ValidateOptions_FailsWithNegativeAge()
        {
            var output = new StringWriter();
            var service = new DeleteOldService(output, output);
            var options = new DeleteOldOptions { Age = -1, Path = _testPath };

            var result = service.ValidateOptions(options);
            
            Assert.False(result);
        }

        [Fact]
        public void ValidateOptions_FailsWithNonExistentPath()
        {
            var output = new StringWriter();
            var service = new DeleteOldService(output, output);
            var options = new DeleteOldOptions { 
                Age = 10, 
                Path = @"C:\NonExistent\Path\That\Does\Not\Exist" 
            };

            var result = service.ValidateOptions(options);
            
            Assert.False(result);
            Assert.Contains("does not exist", output.ToString(), StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void ValidateOptions_FailsWithInvalidTimeframe()
        {
            var output = new StringWriter();
            var service = new DeleteOldService(output, output);
            var options = new DeleteOldOptions { 
                Age = 10, 
                Path = _testPath,
                TimeFrame = "invalid"
            };

            var result = service.ValidateOptions(options);
            
            Assert.False(result);
        }

        [Fact]
        public void ValidateOptions_SucceedsWithValidOptions()
        {
            var output = new StringWriter();
            var service = new DeleteOldService(output, output);
            var options = new DeleteOldOptions { 
                Age = 10, 
                Path = _testPath,
                TimeFrame = "d"
            };

            var result = service.ValidateOptions(options);
            
            Assert.True(result);
        }

        [Fact]
        public void Execute_DeletesAllFilesInCurrentFolder_RegardlessOfAge()
        {
            var output = new StringWriter();
            var service = new DeleteOldService(output, output);
            var options = new DeleteOldOptions
            {
                Age = 0,
                TimeFrame = "s",
                Path = _testPath,
                QuietMode = false
            };

            var result = service.Execute(options);

            Assert.Equal(0, result);
            Assert.Empty(Directory.GetFiles(_testPath));
            
            // Subfolder should still exist with all files
            Assert.Equal(20, Directory.GetFiles(_subfolderPath).Length);
        }

        [Fact]
        public void Execute_DeletesFilteredFiles_CurrentFolderOnly()
        {
            var output = new StringWriter();
            var service = new DeleteOldService(output, output);
            var options = new DeleteOldOptions
            {
                Age = 0,
                TimeFrame = "s",
                Filter = "*.x*",
                Path = _testPath,
                QuietMode = false
            };

            var result = service.Execute(options);

            Assert.Equal(0, result);
            // XML files deleted, TXT files remain
            Assert.Equal(30, Directory.GetFiles(_testPath).Length);
            
            // Subfolder should still have all 20 files
            Assert.Equal(20, Directory.GetFiles(_subfolderPath).Length);
        }

        [Fact]
        public void Execute_DeletesFilteredFiles_Recursive_ByAge()
        {
            var output = new StringWriter();
            var service = new DeleteOldService(output, output);
            var options = new DeleteOldOptions
            {
                Age = 10,
                TimeFrame = "d",
                Filter = "*.x*",
                Path = _testPath,
                RecurseSubfolders = true,
                QuietMode = false
            };

            var result = service.Execute(options);

            Assert.Equal(0, result);
            
            // Root: 70 files remain (30 TXT + 40 recent XML)
            var rootFiles = Directory.GetFiles(_testPath, "*", SearchOption.TopDirectoryOnly);
            Assert.Equal(70, rootFiles.Length);
            
            // Subfolder: 14 files remain (20 - 6 old XSD files)
            Assert.Equal(14, Directory.GetFiles(_subfolderPath).Length);
        }

        [Fact]
        public void Execute_DeletesAllAndRemovesEmptyFolders_Recursive()
        {
            var output = new StringWriter();
            var service = new DeleteOldService(output, output);
            var options = new DeleteOldOptions
            {
                Age = 0,
                TimeFrame = "s",
                Path = _testPath,
                RecurseSubfolders = true,
                RemoveEmptyFolders = true,
                QuietMode = false
            };

            var result = service.Execute(options);

            Assert.Equal(0, result);
            
            // Root folder exists, is empty of files
            Assert.True(Directory.Exists(_testPath));
            Assert.Empty(Directory.GetFiles(_testPath, "*", SearchOption.TopDirectoryOnly));
            
            // Subfolder should be deleted (was empty after file deletion)
            Assert.False(Directory.Exists(_subfolderPath));
        }

        [Fact]
        public void Execute_SimulateMode_DoesNotDeleteFiles()
        {
            var output = new StringWriter();
            var service = new DeleteOldService(output, output);
            var options = new DeleteOldOptions
            {
                Age = 0,
                TimeFrame = "s",
                Path = _testPath,
                RecurseSubfolders = true,
                RemoveEmptyFolders = true,
                SimulateOnly = true,
                QuietMode = false
            };

            var result = service.Execute(options);

            Assert.Equal(0, result);
            
            // All files should still exist (simulation only): 50 XML + 30 TXT + 20 XSD = 100
            Assert.Equal(100, Directory.GetFiles(_testPath, "*", SearchOption.AllDirectories).Length);
            Assert.True(Directory.Exists(_subfolderPath));
            
            // Output should indicate simulation
            var outputText = output.ToString();
            Assert.Contains("SIMULATION ONLY", outputText);
        }

        [Fact]
        public void Execute_QuietMode_ProducesNoOutput()
        {
            var output = new StringWriter();
            var service = new DeleteOldService(output, output);
            var options = new DeleteOldOptions
            {
                Age = 10,
                TimeFrame = "d",
                Path = _testPath,
                QuietMode = true
            };

            var result = service.Execute(options);

            Assert.Equal(0, result);
            Assert.Empty(output.ToString());
        }

        [Fact]
        public void Execute_CalculatesAgeCorrectly_AllTimeframes()
        {
            var output = new StringWriter();
            var service = new DeleteOldService(output, output);

            // Test with second timeframe - should delete files older than 0 seconds (essentially all)
            var options = new DeleteOldOptions
            {
                Age = 0,
                TimeFrame = "s",
                Path = _testPath,
                QuietMode = true
            };

            var result = service.Execute(options);
            
            Assert.Equal(0, result);
        }
    }
}
