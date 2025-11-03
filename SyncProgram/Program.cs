using SyncProgram;

try
{
    // 1. Argument Parsing and Validation
    if (args.Length != 4)
    {
        Console.WriteLine("Error: Incorrect number of arguments.");
        Console.WriteLine("Usage: SyncProgram.exe <SourceFolderPath> <ReplicaFolderPath> <IntervalSeconds> <LogFilePath>");
        Environment.Exit(1);
    }

    string sourcePath = args[0];
    string replicaPath = args[1];
    string intervalArg = args[2];
    string logFilePath = args[3];

    if (!Directory.Exists(sourcePath))
    {
        Console.WriteLine($"Error: Source folder not found at {sourcePath}");
        Environment.Exit(1);
    }

    if (!int.TryParse(intervalArg, out int intervalSeconds) || intervalSeconds <= 0)
    {
        Console.WriteLine($"Error: Invalid synchronization interval specified. Must be a positive integer in seconds.");
        Environment.Exit(1);
    }

    var syncService = new SynchronizationService(logFilePath);
    var intervalMilliseconds = intervalSeconds * 1000;

    syncService.Log($"Starting Folder Synchronization Service.");
    syncService.Log($"Source: {sourcePath}");
    syncService.Log($"Replica: {replicaPath}");
    syncService.Log($"Interval: {intervalSeconds} seconds");
    syncService.Log($"Log File: {logFilePath}\n");

    // 2. Periodic Execution Loop
    while (true)
    {
        syncService.Log($"--- Synchronization started at {DateTime.Now:dd-MM-yyyy HH:mm:ss} ---");

        syncService.SynchronizeFolders(sourcePath, replicaPath);

        syncService.Log($"--- Synchronization finished ---\n");

        await Task.Delay(intervalMilliseconds);
    }
}
catch (Exception ex)
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine($"[FATAL ERROR] The application encountered a critical error: {ex.Message}");
    Console.ResetColor();

    Environment.Exit(1);
}