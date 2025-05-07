using System;

#nullable enable
namespace CsInstall {
  /// <summary>
  /// An installer exception.
  /// </summary>
  public class InstallerException : Exception {
    /// <summary>
    /// The exit code to exit with.
    /// </summary>
    internal int ExitCode { get; set; }

    /// <summary>
    /// Whether to silently uninstall.
    /// </summary>
    internal bool Uninstall { get; set; }

    /// <remarks>
    /// Initializes an <see cref="InstallerException"/>.
    /// </remarks>
    /// <param name="message">The error message. This should end with
    /// punctuation.</param>
    /// <param name="exitCode"><inheritdoc cref="ExitCode"
    /// path="/summary"/></param>
    /// <param name="e">An exception to use as an inner exception.</param>
    /// <param name="uninstall"><inheritdoc cref="Uninstall"
    /// path="/summary"/></param>
    public InstallerException(string message, int exitCode = 1, Exception? e = null, bool uninstall = true) : base(message, e) {
      ExitCode = exitCode;
      Uninstall = uninstall;
    }
  }
}