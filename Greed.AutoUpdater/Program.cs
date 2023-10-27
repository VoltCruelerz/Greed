using System.Diagnostics;
using System.Text;

var sb = new StringBuilder();
try
{
    // Parse Input
    sb.AppendLine("Starting auto-updater...");
    if (args.Length != 1) throw new ArgumentException("Missing parameter 'process_id'");
    sb.AppendLine("Argument: " + args[0]);

    int processId = 0;
    if (!int.TryParse(args[0], out processId)) throw new ArgumentException("Invalid process ID.");

    // Get the Greed process
    Process process = Process.GetProcessById(processId) ?? throw new Exception("Process not found!");

    // Wait for it to finish
    sb.AppendLine($"Waiting for process {processId} to complete...");
    process.WaitForExit();

    // Read the directory
    var curDir = System.AppDomain.CurrentDomain.BaseDirectory;
    var allFiles = Directory.GetFiles(curDir).ToList();
    sb.AppendLine("- Found Files...");
    allFiles.ForEach(f => sb.AppendLine(Path.GetFileName(f)));
    var old = allFiles
        .Where(f => !f.EndsWith(".tmp") && !f.EndsWith(".dll.config") && !f.EndsWith(".json") && Path.GetFileName(f) != "Greed.AutoUpdater.exe")
        .ToList();
    var tmp = allFiles
        .Where(f => f.EndsWith(".tmp") && !f.EndsWith(".dll.config") && !f.EndsWith(".json") && Path.GetFileName(f) != "Greed.AutoUpdater.exe.tmp")
        .ToList();

    // Purge everthing that doesn't end in .tmp
    sb.AppendLine("Purging Old...");
    old.ForEach(f =>
    {
        sb.AppendLine("- Deleting " + f);
        var i = 0;
        var maxRetries = 5;
        do
        {
            try
            {
                File.Delete(f);
                break;
            }
            catch (Exception ex)
            {
                sb.AppendLine(ex.ToString());
                sb.AppendLine("Sleeping...");
                File.AppendAllText("updater.log", sb.ToString());
                sb.Clear();
                Thread.Sleep(1000);
                sb.AppendLine($"Retry [{i}/{maxRetries}]");
            }
        } while (i++ <= maxRetries);
    });

    // Rename the .tmp files to be live.
    tmp.ForEach(f =>
    {
        sb.AppendLine("- Activating " + f);
        File.Move(f, f.Replace(".tmp", ""));
    });

    Process.Start(Path.Combine(curDir, "Greed.exe"));
}
catch (Exception ex)
{
    sb.AppendLine(ex.ToString());
}
finally
{
    sb.AppendLine();
    File.AppendAllText("updater.log", sb.ToString());
}