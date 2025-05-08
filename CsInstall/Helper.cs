using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;

#nullable enable
namespace CsInstall {
  /// <summary>
  /// Installer helper methods.
  /// </summary>
  public static class Helper {
    private const string thisAssembly = "{0}.dll";

    private const string comspec = "%COMSPEC%";
    private const string comspecArgs = "/c";
    private const string comspecSilent = "> NUL";
    private const string comspecStderrRedirect = "2>&1";

    private const string powerShell = "powershell";
    private const string powerShellParameters = "-NoProfile -ExecutionPolicy Bypass -Command";
    private const string windowsPowerShell = "pwsh";
    private const string powerShellSilent = "> $null";

    private const string softwareKey = @"SOFTWARE";
    private const string installerKey = @"Microsoft\Windows\CurrentVersion\Uninstall\{0}.{1}";
    private const int noModify = 1;
    private const int noRepair = 1;

    private const string wherePwsh = $"where pwsh {comspecSilent}";
    private const string deleteInstallLocation = "timeout /t 2 && rmdir /s /q \"{0}\"";

    private static string? comspecCommand;
    private static string? powerShellCommand;

    /// <summary>
    /// <c>%COMSPEC%</c>, expanded.
    /// </summary>
    private static string Comspec {
      get {
        comspecCommand ??= Environment.ExpandEnvironmentVariables(comspec);

        return comspecCommand;
      }
    }

    /// <summary>
    /// The best PowerShell command for this system.
    /// </summary>
    private static string PowerShell {
      get {
        if (powerShellCommand is null) {
          ProcessStartInfo processStartInfo;
          Process? process;

          processStartInfo = new() {
            FileName = Comspec,
            Arguments = $"{comspecArgs} {wherePwsh} {comspecSilent} {comspecStderrRedirect}",
            UseShellExecute = false
          };

          process = Process.Start(processStartInfo);
          process.WaitForExit();

          powerShellCommand = process.ExitCode.Equals(0)
            ? windowsPowerShell
            : powerShell;
        }

        return powerShellCommand;
      }
    }

    /// <summary>
    /// Returns <paramref name="path"/> with <see
    /// cref="Installer.WorkingDirectory"/><c>\</c> prepended.
    /// </summary>
    /// <param name="installer">The installer.</param>
    /// <param name="path">The path to which to prepend.</param>
    /// <returns><paramref name="path"/> with <see
    /// cref="Installer.WorkingDirectory"/><c>\</c> prepended.</returns>
    public static string File(this Installer installer, string path) => $@"{installer.WorkingDirectory}\{path}";

    /// <inheritdoc cref="File" />
    public static string Directory(this Installer installer, string path) => File(installer, path);

    /// <summary>
    /// <inheritdoc cref="File.Exists" path="/summary"/>
    /// </summary>
    /// <param name="installer">The installer.</param>
    /// <param name="path"><inheritdoc cref="File.Exists"
    /// path="/param[@name='path']"/></param>
    /// <returns><inheritdoc cref="File.Exists" path="/returns"/></returns>
    public static bool FileExists(this Installer installer, string path) => System.IO.File.Exists(File(installer, path));

    /// <summary>
    /// <inheritdoc cref="Directory.Exists" path="/summary"/>
    /// </summary>
    /// <param name="installer">The installer.</param>
    /// <param name="path"><inheritdoc cref="Directory.Exists"
    /// path="/param[@name='path']"/></param>
    /// <returns><inheritdoc cref="Directory.Exists" path="/returns"/></returns>
    public static bool DirectoryExists(this Installer installer, string path) => System.IO.Directory.Exists(Directory(installer, path));

    /// <summary>
    /// <inheritdoc cref="File.Copy(string, string, bool)"/>
    /// </summary>
    /// <param name="installer">The installer.</param>
    /// <param name="sourceFileName"><inheritdoc cref="File.Copy(string,
    /// string, bool)" path="/param[@name='sourceFileName']"/></param>
    /// <param name="destFileName"><inheritdoc cref="File.Copy(string,
    /// string, bool)" path="/param[@name='destFileName']"/></param>
    /// <param name="overwrite"><inheritdoc cref="File.Copy(string,
    /// string, bool)" path="/param[@name='overwrite']"/></param>
    public static void CopyFile(this Installer installer, string sourceFileName, string destFileName, bool overwrite = false) => System.IO.File.Copy(File(installer, sourceFileName), destFileName, overwrite);

    /// <summary>
    /// <inheritdoc cref="RecursiveDirectoryCopy"/>
    /// </summary>
    /// <param name="installer">The installer.</param>
    /// <param name="sourceDirectory"><inheritdoc cref="RecursiveDirectoryCopy"
    /// path="/param[@name='sourceDirectory']"/></param>
    /// <param name="destDirectory"><inheritdoc cref="RecursiveDirectoryCopy"
    /// path="/param[@name='destDirectory']"/></param>
    public static void CopyDirectory(this Installer installer, string sourceDirectory, string destDirectory) => RecursiveDirectoryCopy(new(Directory(installer, sourceDirectory)), new(destDirectory));

    /// <summary>
    /// Runs a <c>%COMSPEC%</c> command.
    /// </summary>
    /// <remarks>This is an extension method to <see
    /// cref="Installer"/>.</remarks>
    /// <param name="installer">The installer.</param>
    /// <param name="command">The command to run.</param>
    /// <param name="forceSilent">Whether to redirect all output to
    /// <c>NUL</c>.</param>
    /// <returns>The exit code of <paramref name="command"/>.</returns>
    /// <exception cref="InstallerException"></exception>
    public static int ComspecRun(this Installer installer, string command, bool forceSilent = false) {
      forceSilent = installer.Silent | forceSilent;

      ProcessStartInfo processStartInfo;
      Process process;

      processStartInfo = new() {
        FileName = Comspec,
        Arguments = $"{comspecArgs} {command}{(forceSilent ? $" {comspecSilent}" : string.Empty)}",
        UseShellExecute = false
      };

      if (forceSilent) {
        processStartInfo.RedirectStandardOutput = true;
        processStartInfo.RedirectStandardError = true;
      }

      process = Process.Start(processStartInfo);

      if (process is null) {
        throw new InstallerException($"Could not run {command} in {comspec}.");
      }

      process.WaitForExit();

      return process.ExitCode;
    }

    /// <summary>
    /// <inheritdoc cref="ComspecRun(Installer, string, bool)" path="/summary"/>
    /// </summary>
    /// <remarks>
    /// <para>Be sure to trim whitespace in <paramref name="stdout"/>, if
    /// necessary.</para>
    /// <para>This is an extension method to <see cref="Installer"/>.</para>
    /// </remarks>
    /// <param name="installer">The installer.</param>
    /// <param name="command"><inheritdoc cref="ComspecRun(Installer, string,
    /// bool)" path="/param[@name='command']"/></param>
    /// <param name="stdout">Write standard output here.</param>
    /// <returns><inheritdoc cref="ComspecRun(Installer, string, bool)"
    /// path="/returns"/></returns>
    /// <exception cref="InstallerException"></exception>
    public static int ComspecRun(this Installer installer, string command, out string stdout) {
      ProcessStartInfo processStartInfo;
      Process process;

      processStartInfo = new() {
        FileName = Comspec,
        Arguments = $"{comspecArgs} {command}",
        RedirectStandardOutput = true,
        UseShellExecute = false
      };

      if (installer.Silent) {
        processStartInfo.RedirectStandardError = true;
      }

      process = Process.Start(processStartInfo);

      if (process is null) {
        throw new InstallerException($"Could not run {command} in {comspec}.");
      }

      stdout = process.StandardOutput.ReadToEnd().Trim();

      process.WaitForExit();

      return process.ExitCode;
    }

    /// <summary>
    /// Runs a PowerShell command.
    /// </summary>
    /// <remarks>This is an extension method to <see
    /// cref="Installer"/>.</remarks>
    /// <param name="installer">The installer.</param>
    /// <param name="command">The command to run.</param>
    /// <param name="forceSilent">Whether to redirect all output to
    /// <c>$null</c>.</param>
    /// <returns>The exit code of <paramref name="command"/>.</returns>
    /// <exception cref="InstallerException"></exception>
    public static int PowerShellRun(this Installer installer, string command, bool forceSilent = false) {
      forceSilent = installer.Silent | forceSilent;

      ProcessStartInfo processStartInfo;
      Process process;

      processStartInfo = new() {
        FileName = PowerShell,
        Arguments = $"{powerShellParameters} {command}{(forceSilent ? $" {powerShellSilent}" : string.Empty)}",
        UseShellExecute = false
      };

      if (forceSilent) {
        processStartInfo.RedirectStandardOutput = true;
        processStartInfo.RedirectStandardError = true;
      }

      process = Process.Start(processStartInfo);

      if (process is null) {
        throw new InstallerException($"Could not run {command} in {PowerShell}.");
      }

      process.WaitForExit();

      return process.ExitCode;
    }

    /// <summary>
    /// <inheritdoc cref="PowerShellRun(Installer, string, bool)"
    /// path="/summary"/>
    /// </summary>
    /// <remarks>
    /// <para>Be sure to trim whitespace in <paramref name="stdout"/>, if
    /// necessary.</para>
    /// <para>This is an extension method to <see cref="Installer"/>.</para>
    /// </remarks>
    /// <param name="installer">Unused.</param>
    /// <param name="command"><inheritdoc cref="PowerShellRun(Installer, string,
    /// bool)" path="/param[@name='command']"/></param>
    /// <param name="stdout">Write standard output here.</param>
    /// <returns><inheritdoc cref="PowerShellRun(Installer, string, bool)"
    /// path="/returns"/></returns>
    /// <exception cref="InstallerException"></exception>
    public static int PowerShellRun(this Installer installer, string command, out string stdout) {
      ProcessStartInfo processStartInfo;
      Process process;

      processStartInfo = new() {
        FileName = PowerShell,
        Arguments = $"{powerShellParameters} {command}",
        RedirectStandardOutput = true,
        UseShellExecute = false
      };

      if (installer.Silent) {
        processStartInfo.RedirectStandardError = true;
      }

      process = Process.Start(processStartInfo);

      if (process is null) {
        throw new InstallerException($"Could not run {command} in {PowerShell}.");
      }

      stdout = process.StandardOutput.ReadToEnd().Trim();

      process.WaitForExit();

      return process.ExitCode;
    }

    /// <summary>
    /// Extracts embedded resources.
    /// </summary>
    /// <returns>The working directory.</returns>
    /// <param name="resources"><inheritdoc
    /// cref="Installer.Resources" path="/summary"/></param>
    /// <exception cref="ArgumentException"></exception>
    internal static string ExtractResources(string resources) {
      Assembly assembly = Assembly.GetEntryAssembly();
      Regex resourceRegex = new($@"{Regex.Escape($"{assembly.GetName().Name}.{resources}.")}(.*)");

      string workingDirectory = CreateTemporaryDirectory();

      foreach (string resource in assembly.GetManifestResourceNames()) {
        using (StreamReader resourceReader = new(assembly.GetManifestResourceStream(resource))) {
          string filename;

          Match match = resourceRegex!.Match(resource);

          try {
            filename = $@"{workingDirectory}\{match.Groups[1].Value}";
          } catch (IndexOutOfRangeException) {
            throw new ArgumentException("Unable to locate Resources namespace.", "Resources");
          }

          using (FileStream fileStream = System.IO.File.Create(filename)) {
            resourceReader.BaseStream.CopyTo(fileStream);
          }
        }
      }

      return workingDirectory;
    }

    /// <summary>
    /// Registers the installation in Windows.
    /// </summary>
    /// <remarks>This is an extension method to <see
    /// cref="Installer"/>.</remarks>
    /// <param name="installer">The installer.</param>
    /// <exception cref="InstallerException"></exception>
    internal static void RegisterInstallation(this Installer installer) {
      string installerKey = string.Format(
        Helper.installerKey,
        installer.Publisher,
        installer.ShortName
      );

      try {
        System.IO.Directory.CreateDirectory(installer.InstallLocation);

        // Copy the installer
        System.IO.File.Copy(
          $@"{AppDomain.CurrentDomain.BaseDirectory}\{installer.InstallerFile}",
          $@"{installer.InstallLocation}\{installer.InstallerFile}",
          overwrite: true
        );

        Assembly library = Assembly.GetExecutingAssembly();

        // Copy this library
        System.IO.File.Copy(
          $@"{AppDomain.CurrentDomain.BaseDirectory}\{string.Format(thisAssembly, library.GetName().Name)}",
          $@"{installer.InstallLocation}\{string.Format(thisAssembly, library.GetName().Name)}",
          overwrite: true
        );

        if (installer.Icon is not null) {
          System.IO.File.Copy(
            $@"{installer.WorkingDirectory}\{installer.Icon}",
            $@"{installer.InstallLocation}\{installer.Icon}",
            overwrite: true
          );
        }
      } catch (Exception e) {
        throw new InstallerException($"Unable to install {installer.DisplayName} to {installer.InstallLocation}.", e: e);
      }

      try {
        RegistryKey softwareRegistryKey = Registry.LocalMachine.OpenSubKey(softwareKey, writable: true)!;
        RegistryKey registryKey = softwareRegistryKey.CreateSubKey(installerKey);

        registryKey.SetValue("InstallLocation", installer.InstallLocation);
        registryKey.SetValue("DisplayName", installer.DisplayName);
        registryKey.SetValue("Publisher", installer.Publisher);
        registryKey.SetValue("DisplayVersion", installer.Version);

        if (installer.Url is not null) {
          registryKey.SetValue("URLInfoAbout", installer.Url);
        }

        if (installer.Icon is not null) {
          registryKey.SetValue("DisplayIcon", $@"{installer.InstallLocation}\{installer.Icon}");
        }

        registryKey.SetValue("InstallDate", DateTime.Now.ToString("yyyyMMdd"));
        registryKey.SetValue("UninstallString", $@"{installer.InstallLocation}\{installer.UninstallCommand}");
        registryKey.SetValue("QuietUninstallString", $@"{installer.InstallLocation}\{installer.SilentUninstallCommand}");
        registryKey.SetValue("NoModify", noModify);
        registryKey.SetValue("NoRepair", noRepair);

        registryKey.Close();
        softwareRegistryKey.Close();
      } catch (Exception e) {
        throw new InstallerException($"Unable to register installation in registry.", e: e);
      }
    }

    /// <summary>
    /// Unregisters the installation in Windows.
    /// </summary>
    /// <remarks>This is an extension method to <see
    /// cref="Installer"/>.</remarks>
    /// <exception cref="InstallerException"></exception>
    internal static void UnregisterInstallation(this Installer installer) {
      string installerKey = string.Format(
        Helper.installerKey,
        installer.Publisher,
        installer.ShortName
      );

      try {
        RegistryKey softwareRegistryKey = Registry.LocalMachine.OpenSubKey(softwareKey, writable: true)!;

        softwareRegistryKey.DeleteSubKeyTree(installerKey, throwOnMissingSubKey: false);
        softwareRegistryKey.Close();
      } catch (Exception e) {
        throw new UninstallerException($"Unable to unregister installation in registry.", e: e);
      }
    }

    /// <summary>
    /// Deletes the install location.
    /// </summary>
    /// <remarks>
    /// <para>This is an extension method to <see cref="Installer"/>.</para>
    /// <para>Leaves two seconds before %COMSPEC% deletes the installer
    /// directory.</para>
    /// </remarks>
    /// <exception cref="InstallerException"></exception>
    internal static void DeleteInstallLocation(this Installer installer) {
      ProcessStartInfo processStartInfo = new() {
        FileName = Comspec,
        Arguments = $"{comspecArgs} {string.Format(deleteInstallLocation, installer.InstallLocation)}",
        UseShellExecute = true,
        WindowStyle = ProcessWindowStyle.Hidden
      };

      Process.Start(processStartInfo);
    }

    /// <summary>
    /// Creates a unique temporary directory.
    /// </summary>
    /// <returns>The name of the directory.</returns>
    private static string CreateTemporaryDirectory() {
      string directory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());

      if (System.IO.Directory.Exists(directory)) {
        return CreateTemporaryDirectory();
      } else {
        System.IO.Directory.CreateDirectory(directory);
        return directory;
      }
    }

    /// <summary>
    /// Recursively copies the directory <paramref name="sourceDirectory"/> to
    /// <paramref name="destDirectory"/>.
    /// </summary>
    /// <param name="sourceDirectory">The source directory.</param>
    /// <param name="destDirectory">The destination directory.</param>
    private static void RecursiveDirectoryCopy(DirectoryInfo sourceDirectory, DirectoryInfo destDirectory) {
      System.IO.Directory.CreateDirectory(destDirectory.FullName);

      foreach (FileInfo file in sourceDirectory.GetFiles()) {
        file.CopyTo(Path.Combine(destDirectory.FullName, file.Name), true);
      }

      foreach (DirectoryInfo subdirectory in sourceDirectory.GetDirectories()) {
        DirectoryInfo destSubdirectory = destDirectory.CreateSubdirectory(subdirectory.Name);
        RecursiveDirectoryCopy(subdirectory, destSubdirectory);
      }
    }
  }
}