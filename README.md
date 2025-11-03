# One-Way Folder Synchronization Service

This project implements a one-way folder synchronization tool in C# as a persistent console application. It ensures that a Replica directory is maintained as a full, identical copy of a Source directory, running its synchronization cycle periodically based on user-defined settings.


## Key Features

The program meets all specified requirements:

One-Way Synchronization: The content flow is strictly Source to Replica. Any changes in the Replica that do not exist in the Source are deleted to maintain an exact mirror.

Periodic Execution: The synchronization process runs continuously at a user-defined time interval.

Command-Line Configuration: All configuration parameters are provided via command-line arguments.

Comprehensive Logging: All file system operations (create, update, delete) are logged to both the console (for real-time monitoring) and a dedicated log file (for persistent history).

No Third-Party Sync Libraries: The core synchronization logic is implemented using only built-in .NET System.IO functions (File.Copy, Directory.Delete, etc.).

## Design Decisions

To ensure the application is reliable and professional, the following design choices were made:

1. File Comparison

Instead of relying solely on the file timestamp, the synchronization logic checks for two conditions before overwriting a Replica file:

File Size Check: If the size of the Source and Replica files differ, an update is necessary.

Last Write Time Check: If sizes are the same, the Source must have a newer LastWriteTimeUtc timestamp to trigger an update.

2. Synchronization Integrity (Two-Phase Sync)

The synchronization process is divided into two distinct phases to ensure the Replica is a true mirror:

Phase 1 (Copy/Update): Handles creation of new items and overwriting of modified items (Source -> Replica).

Phase 2 (Removal): Handles deletion of items in the Replica (ensuring the mirror is perfect).

3. Error Handling and Stability

Critical file operations (File.Copy, File.Delete) are wrapped in try...catch blocks. If an operation fails (e.g., due to file permissions or the file being in use), the error is logged, but the synchronization process continues for other files, preventing a full application crash.


## How to Run

Prerequisites:

- .NET SDK (Version 6.0 or higher is recommended).

- A Source folder and a Log folder must exist before execution.



The program requires four arguments in this specific order:

- dotnet run -- SourceFolderPath ReplicaFolderPath IntervalSeconds LogFilePath

Example:
dotnet run -- "C:\Data\Source" "C:\Data\Replica" 60 "C:\Logs\sync_history.txt"
