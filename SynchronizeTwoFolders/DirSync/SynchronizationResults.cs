using System;

namespace SynchronizationLibrary
{
    public class SyncResults
    {
        /// Get or set the number of files copied.
        public int FilesCopied { get; set; }

        /// Get or set the number of files already up to date.
        public int FilesUpToDate { get; set; }

        /// Get or set the number of files delete
        public int FilesDeleted { get; set; }

        /// Get or set the number of files not synchronized.
        public int FilesIgnored { get; set; }

        /// Get or set the number of new folders created.
        public int DirectoriesCreated { get; set; }

        /// Get or set the number of folders removed.
        public int DirectoriesDeleted { get; set; }

        /// Get or set the number of folder not synchronized and ignored.
        public int DirectoriesIgnored { get; set; }
    }
}
