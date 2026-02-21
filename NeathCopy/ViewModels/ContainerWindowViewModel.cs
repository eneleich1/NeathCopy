using NeathCopy.Module2_Configuration;
using NeathCopyEngine.CopyHandlers;
using NeathCopyEngine.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Threading;

namespace NeathCopy.ViewModels
{
    public class ContainerWindowViewModel : ViewModelBase
    {
        private static readonly Mutex mut = new Mutex();
        private readonly Dispatcher dispatcher;
        private readonly Action closeIfEmpty;
        private readonly Action hideWindow;
        private readonly Func<bool> shouldKeepPlaceholder;

        public ObservableCollection<VisualCopy> VisualsCopys { get; }

        public ContainerWindowViewModel(Dispatcher dispatcher, Action closeIfEmpty, Action hideWindow, Func<bool> shouldKeepPlaceholder)
        {
            this.dispatcher = dispatcher;
            this.closeIfEmpty = closeIfEmpty;
            this.hideWindow = hideWindow;
            this.shouldKeepPlaceholder = shouldKeepPlaceholder;
            VisualsCopys = new ObservableCollection<VisualCopy>();
        }

        public VisualCopy AddNew()
        {
            try
            {
                var vc = new VisualCopy();
                vc.Id = VisualsCopysHandler.VisualsCopys.Count() + 1;

                vc.BreakInqueve += Vc_BreakInqueve;
                vc.AfterCancel += Vc_AfterCancel;
                vc.Finish += Vc_Finish;

                ManipulateList(vc, ListManipulation.Add);
                return vc;
            }
            catch (Exception ex)
            {
                MessageBox.Show(Error.GetErrorLog(ex.Message, "NeathCopy", "ContainerWindowViewModel", "AddNew"));
                return null;
            }
        }

        public IEnumerable<VisualCopy> GetVisualsCopys()
        {
            return VisualsCopys.ToList();
        }

        public void Remove(VisualCopy vc, Action closeWindowIfEmpty)
        {
            try
            {
                ManipulateList(vc, ListManipulation.Remove);
                var keep = shouldKeepPlaceholder != null && shouldKeepPlaceholder();
                if (keep && VisualsCopys.Count == 0)
                {
                    VisualCopy placeholder = null;
                    dispatcher.Invoke(new Action(() =>
                    {
                        placeholder = AddNew();
                    }));
                    if (placeholder != null)
                        LogPlaceholder("ResidentMode placeholder created after last VC removed.", null);

                    if (hideWindow != null)
                        dispatcher.Invoke(hideWindow);
                }
                else if (VisualsCopys.Count == 0)
                {
                    var closeAction = closeWindowIfEmpty ?? closeIfEmpty;
                    if (closeAction != null)
                        dispatcher.Invoke(closeAction);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(Error.GetErrorLog(ex.Message, "NeathCopy", "ContainerWindowViewModel", "Remove"));
            }
        }

        public void CancelAll(Action closeWindowIfEmpty)
        {
            try
            {
                foreach (var vc in GetVisualsCopys())
                {
                    vc.Cancel("User Cancell All Copys");
                    Remove(vc, closeWindowIfEmpty);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(Error.GetErrorLog(ex.Message, "NeathCopy", "ContainerWindowViewModel", "CancelAll"));
            }
        }

        public void PauseAll()
        {
            try
            {
                foreach (var vc in GetVisualsCopys())
                    vc.Pause();
            }
            catch (Exception ex)
            {
                MessageBox.Show(Error.GetErrorLog(ex.Message, "NeathCopy", "ContainerWindowViewModel", "PauseAll"));
            }
        }

        public void ResumeAll()
        {
            try
            {
                foreach (var vc in GetVisualsCopys())
                    vc.Resume();
            }
            catch (Exception ex)
            {
                MessageBox.Show(Error.GetErrorLog(ex.Message, "NeathCopy", "ContainerWindowViewModel", "ResumeAll"));
            }
        }

        private enum ListManipulation { Add, Remove }

        private void ManipulateList(VisualCopy vc, ListManipulation listManipulation)
        {
            try
            {
                mut.WaitOne();

                if (listManipulation == ListManipulation.Add)
                {
                    dispatcher.Invoke(() => VisualsCopys.Add(vc));
                }
                else if (listManipulation == ListManipulation.Remove)
                {
                    dispatcher.Invoke(() => VisualsCopys.Remove(vc));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(Error.GetErrorLog(ex.Message, "NeathCopy", "ContainerWindowViewModel", "ManipulateList"));
            }
            finally
            {
                mut.ReleaseMutex();
            }
        }

        private void Vc_BreakInqueve(VisualCopy sender)
        {
            try
            {
                Configuration.Main.RemoveFromQueve(sender, false);
            }
            catch (Exception ex)
            {
                MessageBox.Show(Error.GetErrorLog(ex.Message, "NeathCopy", "ContainerWindowViewModel", "Vc_BreakInqueve"));
            }
        }

        private void Vc_Finish(VisualCopy sender)
        {
            try
            {
                Configuration.Main.PLaySoundAfterOperation(Configuration.Main.PlaySound_After_Finish,
                    Configuration.Main.FinishOperation_Sound);

                Remove(sender, null);
                Configuration.Main.RemoveFromQueve(sender, true);
            }
            catch (Exception ex)
            {
                MessageBox.Show(Error.GetErrorLog(ex.Message, "NeathCopy", "ContainerWindowViewModel", "Vc_Finish"));
            }
        }

        private void Vc_AfterCancel(VisualCopy sender)
        {
            try
            {
                Configuration.Main.PLaySoundAfterOperation(Configuration.Main.PlaySound_After_Cancel,
                    Configuration.Main.Cancell_Sound);

                Remove(sender, null);
                Configuration.Main.RemoveFromQueve(sender, true);
            }
            catch (Exception ex)
            {
                MessageBox.Show(Error.GetErrorLog(ex.Message, "NeathCopy", "ContainerWindowViewModel", "Vc_AfterCancel"));
            }
        }

        private void LogPlaceholder(string message, Exception ex)
        {
            try
            {
                var logsDir = RegisterAccess.Acces.GetLogsDir();
                if (string.IsNullOrWhiteSpace(logsDir))
                    logsDir = AppDomain.CurrentDomain.BaseDirectory;

                var path = Path.Combine(logsDir, "ContainerWindow.log");
                var line = string.Format("[{0:yyyy-MM-dd HH:mm:ss}] {1}", DateTime.Now, message);
                if (ex != null)
                    line = line + Environment.NewLine + ex.ToString();

                File.AppendAllText(path, line + Environment.NewLine);
            }
            catch (Exception logEx)
            {
                System.Diagnostics.Trace.WriteLine("Container placeholder log failure: " + logEx);
            }
        }
    }
}
