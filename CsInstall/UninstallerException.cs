using System;

#nullable enable
namespace CsInstall {
  /// <summary>
  /// An uninstaller exception.
  /// </summary>
  public class UninstallerException : Exception {
    /// <summary>
    /// The exit code to exit with.
    /// </summary>
    internal int ExitCode { get; set; }

    /// <summary>
    /// Initializes an <see cref="UninstallerException"/>.
    /// </summary>
    /// <param name="message">The error message. This should end with
    /// punctuation.</param>
    /// <param name="exitCode"><inheritdoc cref="ExitCode"
    /// path="/summary"/></param>
    /// <param name="e">An exception to use as an inner exception.</param>
    public UninstallerException(string message, int exitCode = 1, Exception? e = null) : base(message, e) {
      ExitCode = exitCode;
    }
  }
}