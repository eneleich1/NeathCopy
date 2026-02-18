using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace NeathCopy.Helpers
{
    public static class RichTextBoxBinding
    {
        public static readonly DependencyProperty BindableDocumentProperty =
            DependencyProperty.RegisterAttached(
                "BindableDocument",
                typeof(FlowDocument),
                typeof(RichTextBoxBinding),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnBindableDocumentChanged));

        public static FlowDocument GetBindableDocument(DependencyObject obj)
        {
            return (FlowDocument)obj.GetValue(BindableDocumentProperty);
        }

        public static void SetBindableDocument(DependencyObject obj, FlowDocument value)
        {
            obj.SetValue(BindableDocumentProperty, value);
        }

        private static void OnBindableDocumentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var rtb = d as RichTextBox;
            if (rtb == null) return;
            rtb.Document = e.NewValue as FlowDocument ?? new FlowDocument();
        }
    }
}
