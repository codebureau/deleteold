namespace CodeBureau.DeleteOld
{
    /// <summary>
    /// Options for the DeleteOld utility.
    /// </summary>
    public class DeleteOldOptions
    {
        /// <summary>
        /// Path to search for files to delete.
        /// </summary>
        public string Path { get; set; } = System.Environment.CurrentDirectory;

        /// <summary>
        /// File filter pattern (e.g., "*.log").
        /// </summary>
        public string Filter { get; set; } = "*.*";

        /// <summary>
        /// Age of files to delete (in units specified by TimeFrame).
        /// </summary>
        public int Age { get; set; }

        /// <summary>
        /// Time frame unit: s (second), n (minute), h (hour), d (day), m (month), y (year).
        /// </summary>
        public string TimeFrame { get; set; } = "d";

        /// <summary>
        /// Whether to recurse into subdirectories.
        /// </summary>
        public bool RecurseSubfolders { get; set; }

        /// <summary>
        /// Whether to remove empty folders after deletion.
        /// </summary>
        public bool RemoveEmptyFolders { get; set; }

        /// <summary>
        /// Whether to suppress output.
        /// </summary>
        public bool QuietMode { get; set; }

        /// <summary>
        /// Whether to simulate only (do not actually delete).
        /// </summary>
        public bool SimulateOnly { get; set; }
    }
}
