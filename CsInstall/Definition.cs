using System.Collections.Generic;

namespace CsInstall {
  /// <summary>
  /// An installer definition.
  /// </summary>
  public abstract class Definition {
    /// <summary>
    /// The <see cref="CsInstall.Installer"/> associated with this <see
    /// cref="Definition"/>.
    /// </summary>
    public Installer Installer { get; private set; }

    /// <summary>
    /// The <see cref="IInstallerTask"/>s to invoke.
    /// </summary>
    /// <remarks>Defaults to none.</remarks>
    public List<IInstallerTask> InstallerTasks { get; } = new();

    /// <summary>
    /// The <see cref="IUninstallerTask"/>s to invoke.
    /// </summary>
    /// <remarks>Defaults to none.</remarks>
    public List<IUninstallerTask> UninstallerTasks { get; } = new();

    /// <summary>
    /// Whether <see cref="UninstallerTasks"/> need resources to be extracted.
    /// </summary>
    /// <remarks>Defaults to <c>false</c>.</remarks>
    public bool UninstallerTasksNeedResources { get; set; } = false;

    /// <summary>
    /// Initializes a <see cref="Definition"/>.
    /// </summary>
    protected Definition(Installer installer) {
      Installer = installer;
    }

    /// <summary>
    /// Performs any checks required before invoking <see
    /// cref="InstallerTasks"/> or <see cref="UninstallerTasks"/>.
    /// </summary>
    /// <returns><c>true</c> if the tasks can proceed or <c>false</c> if they
    /// cannot.</returns>
    public abstract bool Checks();

    /// <summary>
    /// Performs any checks required before invoking <see
    /// cref="InstallerTasks"/>.
    /// </summary>
    /// <exception cref="InstallerException"></exception>
    public abstract void InstallChecks();

    /// <summary>
    /// Performs any checks required before invoking <see
    /// cref="UninstallerTasks"/>.
    /// </summary>
    /// <exception cref="UninstallerException"></exception>
    public abstract void UninstallChecks();
  }
}
