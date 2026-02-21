using System;
using System.Collections.Generic;
using System.Linq;

namespace NeathCopy.Services
{
    /// <summary>
    /// Centralizes transfer lifecycle decisions so window Close/Hide does not leave orphan transfers.
    /// This is the first step toward a full "orchestrator" architecture (PauseAll/CancelAll, etc.).
    /// </summary>
    internal sealed class TransferOrchestrator
    {
        private readonly Func<IEnumerable<ContainerWindow>> getContainers;

        internal TransferOrchestrator(Func<IEnumerable<ContainerWindow>> getContainers)
        {
            this.getContainers = getContainers ?? (() => Enumerable.Empty<ContainerWindow>());
        }

        private IEnumerable<ContainerWindow> GetContainers()
        {
            var containers = getContainers();
            return containers ?? Enumerable.Empty<ContainerWindow>();
        }

        public bool HasAnyTransfers()
        {
            return GetContainers()
                .Where(c => c != null && c.IsLoaded)
                .SelectMany(c => c.VisualsCopys ?? Enumerable.Empty<NeathCopy.VisualCopy>())
                .Any();
        }

        public bool HasAnyActiveTransfers()
        {
            return GetContainers()
                .Where(c => c != null && c.IsLoaded)
                .SelectMany(c => c.VisualsCopys ?? Enumerable.Empty<NeathCopy.VisualCopy>())
                .Any(vc => vc.State == NeathCopy.VisualCopy.VisualCopyState.Runing || vc.State == NeathCopy.VisualCopy.VisualCopyState.Discovering);
        }

        public void CancelAllTransfers()
        {
            foreach (var container in GetContainers().ToList())
            {
                if (container == null || !container.IsLoaded)
                    continue;

                try { container.CancelAll(); } catch { }
            }
        }

        public void PauseAllTransfers()
        {
            foreach (var container in GetContainers().ToList())
            {
                if (container == null || !container.IsLoaded)
                    continue;

                try { container.PauseAll(); } catch { }
            }
        }

        public void ResumeAllTransfers()
        {
            foreach (var container in GetContainers().ToList())
            {
                if (container == null || !container.IsLoaded)
                    continue;

                try { container.ResumeAll(); } catch { }
            }
        }
    }
}
