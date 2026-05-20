using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

const string appName = "Project Update Cloner";

try
{
    var payloadDirectory = Path.Combine(
        Path.GetTempPath(),
        "ProjectUpdateClonerSetup-" + Guid.NewGuid().ToString("N"));

    Directory.CreateDirectory(payloadDirectory);

    var assembly = Assembly.GetExecutingAssembly();
    ExtractResource(assembly, "payload.ProjectUpdateCloner.exe", Path.Combine(payloadDirectory, "ProjectUpdateCloner.exe"));
    ExtractResource(assembly, "payload.setup.cmd", Path.Combine(payloadDirectory, "setup.cmd"));
    ExtractResource(assembly, "payload.setup.ps1", Path.Combine(payloadDirectory, "setup.ps1"));
    ExtractResource(assembly, "payload.uninstall.ps1", Path.Combine(payloadDirectory, "uninstall.ps1"));

    var setupPath = Path.Combine(payloadDirectory, "setup.cmd");
    var process = Process.Start(new ProcessStartInfo
    {
        FileName = setupPath,
        WorkingDirectory = payloadDirectory,
        UseShellExecute = true,
        WindowStyle = ProcessWindowStyle.Hidden
    });

    if (process is null)
    {
        throw new InvalidOperationException("The installer could not start.");
    }

    process.WaitForExit();

    if (process.ExitCode != 0)
    {
        throw new InvalidOperationException($"The installer finished with exit code {process.ExitCode}.");
    }

    TryDeleteDirectory(payloadDirectory);
}
catch (Exception ex)
{
    MessageBox.Show(
        ex.Message,
        $"{appName} Setup",
        MessageBoxButtons.OK,
        MessageBoxIcon.Error);
}

static void ExtractResource(Assembly assembly, string resourceName, string destinationPath)
{
    using var stream = assembly.GetManifestResourceStream(resourceName)
        ?? throw new InvalidOperationException($"Installer payload is missing: {resourceName}");

    using var file = File.Create(destinationPath);
    stream.CopyTo(file);
}

static void TryDeleteDirectory(string path)
{
    try
    {
        Directory.Delete(path, recursive: true);
    }
    catch
    {
        // Temp cleanup is best effort because antivirus or Explorer can briefly hold files.
    }
}
