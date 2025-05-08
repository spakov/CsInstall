using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

#nullable enable
namespace CsInstall {
  /// <summary>
  /// An installer.
  /// </summary>
  public class Installer {
    private readonly List<string> installOptions = new() {
      "--install",
      "--i",
      "-i",
      "/i"
    };

    private readonly List<string> uninstallOptions = new() {
      "--uninstall",
      "--u",
      "-u",
      "/u"
    };

    private readonly List<string> silentOptions = new() {
      "--silent",
      "--s",
      "-s",
      "/s",
      "--q",
      "-q",
      "/q"
    };

    private readonly List<string> versionOptions = new() {
      "--version",
      "--v",
      "-v",
      "/v"
    };

    private readonly List<string> helpOptions = new() {
      "--help",
      "--h",
      "-h",
      "/h",
      "--?",
      "-?",
      "/?"
    };

    private readonly bool install;
    private readonly bool uninstall;
    private readonly bool silent;
    private readonly bool version;
    private readonly bool help;

    /// <summary>
    /// The installer definition to use.
    /// </summary>
    /// <remarks>Required.</remarks>
    public Definition? Definition { get; set; }

    /// <summary>
    /// The namespace containing embedded resources.
    /// </summary>
    /// <remarks>Required.</remarks>
    public string? Resources { get; set; }

    /// <summary>
    /// The package installation location.
    /// </summary>
    /// <remarks>Required.</remarks>
    public string? InstallLocation { get; set; }

    /// <summary>
    /// The package's short name.
    /// </summary>
    /// <remarks>Required.</remarks>
    public string? ShortName { get; set; }

    /// <summary>
    /// The package's display name.
    /// </summary>
    /// <remarks>Required.</remarks>
    public string? DisplayName { get; set; }

    /// <summary>
    /// The package's publisher.
    /// </summary>
    /// <remarks>Required.</remarks>
    public string? Publisher { get; set; }

    /// <summary>
    /// The package's version.
    /// </summary>
    /// <remarks>Required.</remarks>
    public string? Version { get; set; }

    /// <summary>
    /// The package's URL.
    /// </summary>
    /// <remarks>Optional.</remarks>
    public string? Url { get; set; }

    /// <summary>
    /// The package's icon, which is an embedded resource.
    /// </summary>
    /// <remarks>Optional.</remarks>
    public string? Icon { get; set; }

    /// <summary>
    /// Accepted command-line parameters that map to the install action.
    /// </summary>
    /// <remarks>The first member of the list is displayed in help.</remarks>
    public List<string> InstallOptions => installOptions;

    /// <summary>
    /// Accepted command-line parameters that map to the uninstall action.
    /// </summary>
    /// <remarks>The first member of the list is displayed in help.</remarks>
    public List<string> UninstallOptions => uninstallOptions;

    /// <summary>
    /// Accepted command-line parameters that map to the silent option.
    /// </summary>
    /// <remarks>The first member of the list is displayed in help.</remarks>
    public List<string> SilentOptions => silentOptions;

    /// <summary>
    /// Accepted command-line parameters that map to the version action.
    /// </summary>
    /// <remarks>The first member of the list is displayed in help.</remarks>
    public List<string> VersionOptions => versionOptions;

    /// <summary>
    /// Accepted command-line parameters that map to the help action.
    /// </summary>
    /// <remarks>The first member of the list is displayed in help.</remarks>
    public List<string> HelpOptions => helpOptions;

    /// <summary>
    /// The installer working directory.
    /// </summary>
    public string? WorkingDirectory { get; private set; }

    /// <summary>
    /// Whether to operate in silent mode.
    /// </summary>
    public bool Silent { get; private set; }

    /// <summary>
    /// The name of the installer executable, with no extension.
    /// </summary>
    internal string InstallerName { get; private set; }

    /// <summary>
    /// The name of the installer executable, with extension.
    /// </summary>
    internal string InstallerFile { get; private set; }

    /// <summary>
    /// The uninstall command.
    /// </summary>
    internal string UninstallCommand { get; private set; }

    /// <summary>
    /// The silent uninstall command.
    /// </summary>
    internal string SilentUninstallCommand { get; private set; }

    /// <summary>
    /// Initializes the installer.
    /// </summary>
    /// <param name="args">Command-line parameters.</param>
    public Installer(string[] args) {
      install = false;
      uninstall = false;
      silent = false;
      version = false;
      help = false;

      foreach (string arg in args) {
        foreach (string installOption in installOptions) {
          if (arg.ToLower().StartsWith(installOption)) install = true;
        }

        foreach (string uninstallOption in uninstallOptions) {
          if (arg.ToLower().StartsWith(uninstallOption)) uninstall = true;
        }

        foreach (string silentOption in silentOptions) {
          if (arg.ToLower().StartsWith(silentOption)) silent = true;
        }

        foreach (string versionOption in versionOptions) {
          if (arg.ToLower().StartsWith(versionOption)) version = true;
        }

        foreach (string helpOption in helpOptions) {
          if (arg.ToLower().StartsWith(helpOption)) help = true;
        }
      }

      if (!install && !uninstall) install = true;

      InstallerName = Process.GetCurrentProcess().ProcessName;
      InstallerFile = $"{InstallerName}.exe";
      UninstallCommand = $@"{InstallerFile} --uninstall";
      SilentUninstallCommand = $"{UninstallCommand} --silent";
    }

    /// <summary>
    /// Executes the action.
    /// </summary>
    /// <returns>0 on success or non-zero on failure.</returns>
    public int Execute() {
      if (ShortName is null) {
        throw new ArgumentNullException("ShortName");
      }

      if (DisplayName is null) {
        throw new ArgumentNullException("DisplayName");
      }

      if (Publisher is null) {
        throw new ArgumentNullException("Publisher");
      }

      if (Version is null) {
        throw new ArgumentNullException("Version");
      }

      if (help) {
        Console.Error.WriteLine(
          "Description:\r\n" +
          $"  {DisplayName} installer.\r\n" +
          "\r\n" +
          "Usage:\r\n" +
          $"  {InstallerName} [options]\r\n" +
          "\r\n" +
          "Options:\r\n" +
          $"  {installOptions[0]}\tInstall {DisplayName} (default).\r\n" +
          $"  {uninstallOptions[0]}\tUninstall {DisplayName}.\r\n" +
          $"  {silentOptions[0]}\tProduce no output during installation/uninstallation.\r\n" +
          $"  {versionOptions[0]}\tPrint the installer version.\r\n" +
          $"  {helpOptions[0]}\tShow help and usage information."
        );

        return 0;
      }

      if (version) {
        if (!silent) Console.WriteLine(Version);

        return 0;
      }

      if (install && uninstall) {
        if (!silent) Console.Error.WriteLine($"Cannot specify both {installOptions[0]} and {uninstallOptions[0]}");

        return 1;
      }

      Silent = silent;

      return install
        ? Install()
        : Uninstall();
    }

    /// <summary>
    /// Logs a message, unless silent mode is active.
    /// </summary>
    /// <param name="message">The message to log.</param>
    public void Log(string message) {
      if (!Silent) Console.WriteLine(message);
    }

    /// <summary>
    /// Logs an error, unless silent mode is active.
    /// </summary>
    /// <param name="error">The error to log.</param>
    public void Error(string error) {
      if (!Silent) Console.Error.WriteLine(error);
    }

    /// <summary>
    /// Installs the package.
    /// </summary>
    /// <returns>An exit code.</returns>
    private int Install() {
      if (Definition is null) {
        throw new ArgumentNullException("Definition");
      }

      if (Resources is null) {
        throw new ArgumentNullException("Resources");
      }

      InstallLocation = InstallLocation is null
        ? throw new ArgumentNullException("InstallLocation")
        : Environment.ExpandEnvironmentVariables(InstallLocation);

      if (!Definition.Checks()) return 1;

      try {
        Definition.InstallChecks();
      } catch (InstallerException e) {
        PrintInstallerException(e, abort: true);

        return e.ExitCode;
      }

      Log($"Installing {DisplayName}...");

      WorkingDirectory = Helper.ExtractResources(Resources);

      foreach (IInstallerTask installerTask in Definition.InstallerTasks) {
        try {
          installerTask.Checks();
        } catch (InstallerException e) {
          PrintInstallerException(e, abort: true);

          return e.Uninstall
            ? Uninstall(silent: true)
            : e.ExitCode;
        }

        if (!installerTask.SkipInstall) {
          try {
            installerTask.Install();
          } catch (InstallerException e) {
            PrintInstallerException(e);

            return e.Uninstall
              ? Uninstall(silent: true)
              : e.ExitCode;
          }
        }
      }

      try {
        this.RegisterInstallation();
      } catch (InstallerException e) {
        PrintInstallerException(e, abort: true);

        return e.Uninstall
          ? Uninstall(silent: true)
          : e.ExitCode;
      }

      Directory.Delete(WorkingDirectory, true);

      Log("Installation complete.");

      return 0;
    }

    /// <summary>
    /// Uninstalls the package.
    /// </summary>
    /// <param name="silent">Whether to force a silent uninstall.</param>
    /// <returns>An exit code.</returns>
    private int Uninstall(bool silent = false) {
      if (silent) Silent = true;

      if (Definition == null) {
        throw new ArgumentNullException("Definition");
      }

      if (Resources is null) {
        throw new ArgumentNullException("Resources");
      }

      if (InstallLocation is null) {
        throw new ArgumentNullException("InstallLocation");
      }

      if (!Definition.Checks()) return 1;

      try {
        Definition.UninstallChecks();
      } catch (UninstallerException e) {
        PrintUninstallerException(e, abort: true);

        return e.ExitCode;
      }

      Log($"Uninstalling {DisplayName}...");

      if (Definition.UninstallerTasksNeedResources && WorkingDirectory is null) {
        WorkingDirectory = Helper.ExtractResources(Resources);
      }

      foreach (IUninstallerTask uninstallerTask in Definition.UninstallerTasks) {
        try {
          uninstallerTask.Checks();
        } catch (UninstallerException e) {
          PrintUninstallerException(e, abort: true);

          return e.ExitCode;
        }

        if (!uninstallerTask.SkipUninstall) {
          try {
            uninstallerTask.Uninstall();
          } catch (UninstallerException e) {
            PrintUninstallerException(e);

            return e.ExitCode;
          }
        }
      }

      if (WorkingDirectory is not null) {
        Directory.Delete(WorkingDirectory, true);
      }

      try {
        this.UnregisterInstallation();
        this.DeleteInstallLocation();
      } catch (UninstallerException e) {
        PrintUninstallerException(e, abort: true);

        return e.ExitCode;
      }

      Log("Uninstallation complete.");

      return 0;
    }

    /// <summary>
    /// Prints <paramref name="e"/>.
    /// </summary>
    /// <param name="e">The <see cref="InstallerException"/> to print.</param>
    /// <param name="abort">Whether to treat this as aborting the
    /// installation.</param>
    private void PrintInstallerException(InstallerException e, bool abort = false) {
      string message = e.Message;
      Exception? innerException = e.InnerException;

      while (innerException is not null) {
        message += $" {innerException.Message}";
        innerException = innerException.InnerException;
      }

      Error(message);
      Error(abort ? "Aborting installation!" : "Installation failed!");
    }

    /// <summary>
    /// Prints <paramref name="e"/>.
    /// </summary>
    /// <param name="e">The <see cref="UninstallerException"/> to print.</param>
    /// <param name="abort">Whether to treat this as aborting the
    /// uninstallation.</param>
    private void PrintUninstallerException(UninstallerException e, bool abort = false) {
      string message = e.Message;
      Exception? innerException = e.InnerException;

      while (innerException is not null) {
        message += $" {innerException.Message}";
        innerException = innerException.InnerException;
      }

      Error(message);
      Error(abort ? "Aborting uninstallation!" : "Uninstallation failed!");
    }
  }
}