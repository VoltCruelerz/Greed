using System.Diagnostics;
using System.Text;

var sb = new StringBuilder();
try
{
    sb.AppendLine("Starting auto-updater...");
    if (args.Length != 1)
    {
        throw new ArgumentException("Missing parameter 'process_id'");
    }
    sb.AppendLine("Argument: " + args[0]);

    int processId = 0;
    if (!int.TryParse(args[0], out processId))
    {
        throw new ArgumentException("Invalid process ID.");
    }

    sb.AppendLine("Searching for process with id " + processId);
    Process process = Process.GetProcessById(processId);
    sb.AppendLine("Got process: " + process?.Id);

    if (process == null)
    {
        throw new Exception("Process not found!");
    }

    sb.AppendLine($"Waiting for process {processId} to complete...");
    process.WaitForExit();

    string greedName = "Greed.exe";
    string updatedGreedName = "Greed_New.exe";

    sb.AppendLine("Initiating Greed Update...");
    if (File.Exists(greedName))
    {
        File.Delete(greedName);
        sb.AppendLine($"Deleted {greedName}.");
    }

    File.Copy(updatedGreedName, greedName);
    sb.AppendLine($"Copied {updatedGreedName} to {greedName}.");

    Process.Start(greedName);
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