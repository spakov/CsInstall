namespace CsInstall {
  /// <summary>
  /// An installer task.
  /// </summary>
  public interface IInstallerTask {
    /// <summary>
    /// Whether to skip invoking <see cref="Install"/>.
    /// </summary>
    public bool SkipInstall { get; set; }

    /// <summary>
    /// Performs any checks required before invoking <see cref="Install"/>.
    /// </summary>
    /// <remarks>Set <see cref="SkipInstall"/> to <c>true</c> to skip invoking
    /// <see cref="Install"/>.</remarks>
    /// <exception cref="InstallerException"></exception>
    public void Checks();

    /// <summary>
    /// Performs the installation, unless <see cref="SkipInstall"/> is
    /// <c>true</c>.
    /// </summary>
    /// <exception cref="InstallerException"></exception>
    public void Install();
  }
}