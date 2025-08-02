namespace CsInstall {
  /// <summary>
  /// An uninstaller task.
  /// </summary>
  public interface IUninstallerTask {
    /// <summary>
    /// Whether to skip invoking <see cref="Uninstall"/>.
    /// </summary>
    public bool SkipUninstall { get; set; }

    /// <summary>
    /// Performs any checks required before invoking <see cref="Uninstall"/>.
    /// </summary>
    /// <remarks>Set <see cref="SkipUninstall"/> to <c>true</c> to skip
    /// invoking <see cref="Uninstall"/>.</remarks>
    /// <exception cref="UninstallerException"></exception>
    public void Checks();

    /// <summary>
    /// Performs the uninstallation, unless <see cref="SkipUninstall"/> is
    /// <c>true</c>.
    /// </summary>
    /// <exception cref="UninstallerException"></exception>
    public void Uninstall();
  }
}