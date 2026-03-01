using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace NeathCopyEngine.Helpers
{
    public class BulkObservableCollection<T> : ObservableCollection<T>
    {
        private bool suppressNotifications;

        public void AddRange(IEnumerable<T> items)
        {
            if (items == null)
                return;

            suppressNotifications = true;
            try
            {
                foreach (var item in items)
                    Items.Add(item);
            }
            finally
            {
                suppressNotifications = false;
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            }
        }

        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            if (suppressNotifications)
                return;

            base.OnCollectionChanged(e);
        }
    }
}
