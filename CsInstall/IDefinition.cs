using System.Collections.Generic;

namespace CsInstall {
  /// <summary>
  /// An installer definition.
  /// </summary>
  public interface IDefinition {
    /// <summary>
    /// The <see cref="CsInstall.Installer"/> associated with this <see
    /// cref="IDefinition"/>.
    /// </summary>
    public Installer Installer { get; }

    /// <summary>
    /// The <see cref="IInstallerTask"/>s to invoke.
    /// </summary>
    public List<IInstallerTask> InstallerTasks { get; }

    /// <summary>
    /// The <see cref="IUninstallerTask"/>s to invoke.
    /// </summary>
    public List<IUninstallerTask> UninstallerTasks { get; }

    /// <summary>
    /// Whether <see cref="UninstallerTasks"/> need resources to be extracted.
    /// </summary>
    public bool UninstallerTasksNeedResources { get; }

    /// <summary>
    /// Performs any checks required before invoking <see
    /// cref="InstallerTasks"/> or <see cref="UninstallerTasks"/>.
    /// </summary>
    /// <returns><c>true</c> if the tasks can proceed or <c>false</c> if they
    /// cannot.</returns>
    public bool Checks();

    /// <summary>
    /// Performs any checks required before invoking <see
    /// cref="InstallerTasks"/>.
    /// </summary>
    /// <exception cref="InstallerException"></exception>
    public void InstallChecks();

    /// <summary>
    /// Performs any checks required before invoking <see
    /// cref="UninstallerTasks"/>.
    /// </summary>
    /// <exception cref="UninstallerException"></exception>
    public void UninstallChecks();
  }
}
