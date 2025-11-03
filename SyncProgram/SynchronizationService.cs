using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyncProgram
{
    // Handles all file system operations, synchronization logic and logging.
    public class SynchronizationService
    {
        private readonly string _logFilePath;
        public SynchronizationService(string logFilePath)
        {
            _logFilePath = logFilePath;
        }

        // Writes the logs to the console and log file.
        public void Log(string message)
        {
            string timestampedMessage = $"[{DateTime.Now:HH:mm:ss}] {message}";

            Console.WriteLine(timestampedMessage);

            try
            {
                File.AppendAllText(_logFilePath, timestampedMessage + Environment.NewLine);
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"[LOGGING ERROR] Could not write to log file: {ex.Message}");
                Console.ResetColor();
            }
        }

        // Main logic to ensure the replica folder exactly matches the source folder (one-way sync)
        public void SynchronizeFolders(string source, string replica)
        {
            if (!Directory.Exists(replica))
            {
                Directory.CreateDirectory(replica);
                Log($"Created replica directory: {replica}");
            }

            Log("Phase 1: Copying new and updating modified files/folders.");
            CopySourceToReplica(source, replica);

            Log("Phase 2: Removing files/folders that no longer exist in Source.");
            RemoveExtraFromReplica(source, replica);
        }

        // Recursively copies new/modified files and creates necessary directories from source to replica.
        private void CopySourceToReplica(string sourceDir, string replicaDir)
        {
            foreach (var sourceFile in Directory.GetFiles(sourceDir))
            {
                var fileName = Path.GetFileName(sourceFile);
                var replicaFile = Path.Combine(replicaDir, fileName);

                try
                {
                    if (!File.Exists(replicaFile))
                    {
                        // Case 1: File creation (exists in source, not in replica)
                        File.Copy(sourceFile, replicaFile, false); // Copy without overwrite initially
                        Log($"CREATE/COPY: {sourceFile}");
                    }
                    else
                    {
                        // Case 2: Check for updates (exists in both)
                        var sourceInfo = new FileInfo(sourceFile);
                        var replicaInfo = new FileInfo(replicaFile);

                        if (sourceInfo.Length != replicaInfo.Length || sourceInfo.LastWriteTimeUtc > replicaInfo.LastWriteTimeUtc)
                        {
                            File.Copy(sourceFile, replicaFile, true); // Overwrite = true
                            Log($"UPDATE/COPY: {sourceFile}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log($"ERROR: Could not copy file {sourceFile}. {ex.Message}");
                }
            }

            // Recursively call for subdirectories
            foreach (var sourceSubDir in Directory.GetDirectories(sourceDir))
            {
                var dirName = Path.GetFileName(sourceSubDir);
                var replicaSubDir = Path.Combine(replicaDir, dirName);

                if (!Directory.Exists(replicaSubDir))
                {

                    Directory.CreateDirectory(replicaSubDir);
                    Log($"CREATE FOLDER: {sourceSubDir}");
                }

                CopySourceToReplica(sourceSubDir, replicaSubDir);
            }
        }

        // Recursively deletes files and directories from replica that do not exist in the source.
        private void RemoveExtraFromReplica(string sourceDir, string replicaDir)
        {
            // Remove extra files from replica
            foreach (var replicaFile in Directory.GetFiles(replicaDir))
            {
                var fileName = Path.GetFileName(replicaFile);
                var sourceFile = Path.Combine(sourceDir, fileName);

                if (!File.Exists(sourceFile))
                {
                    // Case 3: File removal (exists in replica, not in source)
                    try
                    {
                        File.Delete(replicaFile);
                        Log($"DELETE FILE: {replicaFile}");
                    }
                    catch (Exception ex)
                    {
                        Log($"ERROR: Could not delete file {replicaFile}. {ex.Message}");
                    }
                }
            }

            // Remove extra directories from replica
            foreach (var replicaSubDir in Directory.GetDirectories(replicaDir))
            {
                var dirName = Path.GetFileName(replicaSubDir);
                var sourceSubDir = Path.Combine(sourceDir, dirName);

                RemoveExtraFromReplica(sourceSubDir, replicaSubDir);

                if (!Directory.Exists(sourceSubDir))
                {
                    // Case 4: Folder removal (exists in replica, not in source)
                    try
                    {
                        Directory.Delete(replicaSubDir, true);
                        Log($"DELETE FOLDER: {replicaSubDir}");
                    }
                    catch (Exception ex)
                    {
                        Log($"ERROR: Could not delete folder {replicaSubDir}. {ex.Message}");
                    }
                }
            }
        }

    }
}
