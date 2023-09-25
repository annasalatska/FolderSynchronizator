# FolderSynchronizator
Test project for Veeam company

The Synchronization Library is a command-line utility that allows you to synchronize files and directories between a source directory tree and a destination directory tree at specified time intervals. This utility uses the `SynchronizationLibrary` to perform the synchronization and provides logging capabilities to track the synchronization process.

## Usage

To use the Synchronization Library, follow the instructions below:

### Prerequisites

- .NET Framework or .NET Core installed on your system.

### Instructions

1. Clone or download this repository to your local machine.
   
2. Build solution in visual studioo.

3. Open a terminal or command prompt.

4. Navigate to the directory where you have placed the Synchronization Library executable (e.g., `DirSync.exe`).

6. Run the utility with the following command:

   ```
   DirSync.exe <source directory tree> <destination directory tree> <time interval to update> <Log file>
   ```

   - `<source directory tree>`: The path to the source directory tree that you want to synchronize.
   - `<destination directory tree>`: The path to the destination directory tree where files from the source will be synchronized.
   - `<time interval to update>`: The time interval, in milliseconds, at which the synchronization process should occur.
   - `<Log file>`: The path to the log file where synchronization activity will be recorded.

   Make sure to enclose file paths in quotes if they contain spaces.

7. The utility will start the synchronization process and display status messages in the console.

8. To stop synchronization, press `Enter`.

### Example

Here's an example command to synchronize a source directory to a destination directory every 5000 milliseconds (5 seconds) and log the activity to a file:

```
DirSync.exe "C:\SourceFolder" "D:\DestinationFolder" 5000 "C:\Logs\sync.log"
```

### Logging

The utility creates a log file if it doesn't already exist and appends synchronization activity to it. If the log file doesn't exist, the utility will create it before starting synchronization.
