using System;

namespace NeathCopy.Services.AppControl
{
    internal interface IAppController
    {
        void Start();
        void ApplyIntegrationState();
        void RequestShowMainWindow(string reason = null);
        void RequestHideToTray(string reason = null);
        void RequestExit(string reason = null);
        bool HasAnyTransfers();
        bool HasAnyActiveTransfers();
        void CancelAllTransfers();
        void PauseAllTransfers();
        void ResumeAllTransfers();
        void RegisterContainer(ContainerWindow container);
        void UnregisterContainer(ContainerWindow container);
    }
}
