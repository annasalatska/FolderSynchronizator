using System.Collections;

namespace SynchronizationLibrary
{
    //two folders synchronization class
    public class Sync
    {
        #region CONSTRUCTORS

        public Sync(string sourceDirectory, string destinationDirectory) 
        {
            this.SourceDirectory = new DirectoryInfo(sourceDirectory);
            this.DestinationDirectory = new DirectoryInfo(destinationDirectory);
        }

        #endregion

        #region PROPERTIES

        /// Set this property to log the synchronization progress by this class to the given delegate. 
        /// For example, to log to the console, set this property to Console.Writeline
        public virtual Action<string> Log { get; set; }

        /// Get or set the source folder to synchronize
        public virtual DirectoryInfo SourceDirectory { get; set; }

        /// Get or set the target folder where all files will be synchronized
        public virtual DirectoryInfo DestinationDirectory { get; set; }


        #endregion

        #region METHODS

        /// Performs one-way synchronization from source directory tree to destination directory tree
        public virtual SyncResults Start()
        {
            SyncResults results = new SyncResults();

            if (Validate(this.SourceDirectory.FullName, this.DestinationDirectory.FullName))
            {
                // recursively process directories
                ProcessDirectory(this.SourceDirectory.FullName, this.DestinationDirectory.FullName, ref results);

            }

            return results;
        }


        /// Robustly deletes a directory including all subdirectories and contents
        public virtual void DeleteDirectory(DirectoryInfo directory)
        {
            // make sure all files are not read-only
            FileInfo[] files = directory.GetFiles("*.*", SearchOption.AllDirectories);
            foreach (FileInfo fileInfo in files)
            {
                if (fileInfo.IsReadOnly)
                {
                    fileInfo.IsReadOnly = false;
                }
            }

            // make sure all subdirectories are not read-only
            DirectoryInfo[] directories = directory.GetDirectories("*.*", SearchOption.AllDirectories);
            foreach (DirectoryInfo subdir in directories)
            {
                if ((subdir.Attributes & FileAttributes.ReadOnly) > 0)
                {
                    subdir.Attributes &= ~FileAttributes.ReadOnly;
                }
            }

            // make sure top level directory is not read-only
            if ((directory.Attributes & FileAttributes.ReadOnly) > 0)
            {
                directory.Attributes &= ~FileAttributes.ReadOnly;
            }
            directory.Delete(true);
        }

        /// Gets list of files in specified directory
        public virtual FileInfo[] GetFiles(DirectoryInfo directoryInfo, ref SyncResults results)
        {
            // get all files
            List<FileInfo> fileList = new List<FileInfo>(directoryInfo.GetFiles());
            return fileList.ToArray();
        }

        /// Gets list of subdirectories of specified directory, optionally filtered by specified input parameters
        public virtual DirectoryInfo[] GetDirectories(DirectoryInfo directoryInfo, ref SyncResults results)
        {
            // get all directories
            List<DirectoryInfo> directoryList = new List<DirectoryInfo>(directoryInfo.GetDirectories());

            return directoryList.ToArray();
        }      
        /// Trace message method
        public void Trace(string message, params object[] args)
        {
            if (this.Log != null)
                this.Log.Invoke(String.Format(message, args));
        }
        #endregion

        #region PRIVATES

        /// Validate source folder and create in case doesn't exist
        private bool Validate(string srcDir, string destDir)
        {

            string fullSrcDir = Path.GetFullPath(srcDir);
            string fullDestDir = Path.GetFullPath(destDir);
            if (destDir.StartsWith(fullSrcDir) || srcDir.StartsWith(fullDestDir))
            {
                Trace("Error: source directory {0} and destination directory {1} cannot contain each other", fullSrcDir, fullDestDir);
                return false;
            }

            // ensure source directory exists
            if (!Directory.Exists(srcDir))
            {
                this.Trace("Error: source directory {0} not found", srcDir);
                return false;
            }
            return true;
        }

        /// Recursively performs one-way synchronization from a single source to destination directory
        private bool ProcessDirectory(string srcDir, string destDir, ref SyncResults results)
        {
            DirectoryInfo diSrc = new DirectoryInfo(srcDir);
            DirectoryInfo diDest = new DirectoryInfo(destDir);

            // create destination directory if it doesn't exist
            if (!diDest.Exists)
            {
                try
                {
                    Trace("Creating directory: {0}", diDest.FullName);

                    // create the destination directory
                    diDest.Create();
                    results.DirectoriesCreated++;
                }
                catch (Exception ex)
                {
                    Trace("Error: failed to create directory {0}. {1}", destDir, ex.Message);
                    return false;
                }
            }

            // get list of selected files from source directory
            FileInfo[] fiSrc = GetFiles(diSrc, ref results);
            // get list of files in destination directory
            FileInfo[] fiDest = GetFiles(diDest, ref results);

            // put the source files and destination files into hash tables                     
            Hashtable hashSrc = new Hashtable(fiSrc.Length);
            foreach (FileInfo srcFile in fiSrc)
            {
                hashSrc.Add(srcFile.Name, srcFile);
            }
            Hashtable hashDest = new Hashtable(fiDest.Length);
            foreach (FileInfo destFile in fiDest)
            {
                hashDest.Add(destFile.Name, destFile);
            }

            // make sure all the selected source files exist in destination
            foreach (FileInfo srcFile in fiSrc)
            {
                bool isUpToDate = false;

                // look up in hash table to see if file exists in destination
                FileInfo destFile = (FileInfo)hashDest[srcFile.Name];
                // if file exists and length, write time and attributes match, it's up to date
                if ((destFile != null) && (srcFile.Length == destFile.Length) &&
                    (srcFile.LastWriteTime == destFile.LastWriteTime) &&
                    (srcFile.Attributes == destFile.Attributes))
                {
                    isUpToDate = true;
                    results.FilesUpToDate++;
                }

                // if the file doesn't exist or is different, copy the source file to destination
                if (!isUpToDate)
                {
                    string destPath = Path.Combine(destDir, srcFile.Name);
                    // make sure destination is not read-only
                    if (destFile != null && destFile.IsReadOnly)
                    {
                        destFile.IsReadOnly = false;
                    }

                    try
                    {
                        Trace("Copying: {0} -> {1}", srcFile.FullName, Path.GetFullPath(destPath));

                        // copy the file
                        srcFile.CopyTo(destPath, true);
                        // set attributes appropriately
                        File.SetAttributes(destPath, srcFile.Attributes);
                        results.FilesCopied++;
                    }
                    catch (Exception ex)
                    {
                        Trace("Error: failed to copy file from {0} to {1}. {2}", srcFile.FullName, destPath, ex.Message);
                        return false;
                    }
                }
            }

            // delete extra files in destination directory if specified
            
            foreach (FileInfo destFile in fiDest)
            {
                FileInfo srcFile = (FileInfo)hashSrc[destFile.Name];
                if (srcFile == null)
                {
                    try
                    {

                        Trace("Deleting: {0} ", destFile.FullName);

                        destFile.IsReadOnly = false;
                        // delete the file
                        destFile.Delete();
                        results.FilesDeleted++;
                    }
                    catch (Exception ex)
                    {
                        Trace("Error: failed to delete file from {0}. {1}", destFile.FullName, ex.Message);
                        return false;
                    }
                }
               
            }

            // Get list of selected subdirectories in source directory
            DirectoryInfo[] diSrcSubdirs = GetDirectories(diSrc, ref results);
            // Get list of subdirectories in destination directory
            DirectoryInfo[] diDestSubdirs = GetDirectories(diDest, ref results);

            // add selected source subdirectories to hash table, and recursively process them
            Hashtable hashSrcSubdirs = new Hashtable(diSrcSubdirs.Length);
            foreach (DirectoryInfo diSrcSubdir in diSrcSubdirs)
            {
                hashSrcSubdirs.Add(diSrcSubdir.Name, diSrcSubdir);
                // recurse into this directory
                if (!ProcessDirectory(diSrcSubdir.FullName, Path.Combine(destDir, diSrcSubdir.Name), ref results))
                    return false;
            }


            foreach (DirectoryInfo diDestSubdir in diDestSubdirs)
            {
                // does this destination subdirectory exist in the source subdirs?
                if (!hashSrcSubdirs.ContainsKey(diDestSubdir.Name))
                {

                    try
                    {
                        Trace("Deleting directory: {0} ", diDestSubdir.FullName);

                        // delete directory
                        DeleteDirectory(diDestSubdir);
                        results.DirectoriesDeleted++;
                    }
                    catch (Exception ex)
                    {
                        Trace("Error: failed to delete directory {0}. {1}", diDestSubdir.FullName, ex.Message);
                        return false;
                    }
                }
            }
            
            return true;
        }
        #endregion
    }
}
