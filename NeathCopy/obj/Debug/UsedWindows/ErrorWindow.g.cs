﻿#pragma checksum "..\..\..\UsedWindows\ErrorWindow.xaml" "{8829d00f-11b8-4213-878b-770e8597ac16}" "B16D3313E0DD914047FEF7F4DAF6346A48240404E8589A394955FAB86EBB50EA"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using NeathCopy.UsedWindows;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Media.TextFormatting;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Shell;


namespace NeathCopy.UsedWindows {
    
    
    /// <summary>
    /// ErrorWindow
    /// </summary>
    public partial class ErrorWindow : System.Windows.Window, System.Windows.Markup.IComponentConnector {
        
        
        #line 23 "..\..\..\UsedWindows\ErrorWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.RichTextBox info_tb;
        
        #line default
        #line hidden
        
        
        #line 26 "..\..\..\UsedWindows\ErrorWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button try_bt;
        
        #line default
        #line hidden
        
        
        #line 28 "..\..\..\UsedWindows\ErrorWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button skipAll_button;
        
        #line default
        #line hidden
        
        
        #line 30 "..\..\..\UsedWindows\ErrorWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button skip_button;
        
        #line default
        #line hidden
        
        
        #line 32 "..\..\..\UsedWindows\ErrorWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button cancel_button;
        
        #line default
        #line hidden
        
        private bool _contentLoaded;
        
        /// <summary>
        /// InitializeComponent
        /// </summary>
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        public void InitializeComponent() {
            if (_contentLoaded) {
                return;
            }
            _contentLoaded = true;
            System.Uri resourceLocater = new System.Uri("/NeathCopy;component/usedwindows/errorwindow.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\..\UsedWindows\ErrorWindow.xaml"
            System.Windows.Application.LoadComponent(this, resourceLocater);
            
            #line default
            #line hidden
        }
        
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        void System.Windows.Markup.IComponentConnector.Connect(int connectionId, object target) {
            switch (connectionId)
            {
            case 1:
            
            #line 6 "..\..\..\UsedWindows\ErrorWindow.xaml"
            ((NeathCopy.UsedWindows.ErrorWindow)(target)).Closing += new System.ComponentModel.CancelEventHandler(this.Window_Closing);
            
            #line default
            #line hidden
            return;
            case 2:
            this.info_tb = ((System.Windows.Controls.RichTextBox)(target));
            return;
            case 3:
            this.try_bt = ((System.Windows.Controls.Button)(target));
            
            #line 26 "..\..\..\UsedWindows\ErrorWindow.xaml"
            this.try_bt.Click += new System.Windows.RoutedEventHandler(this.try_bt_Click);
            
            #line default
            #line hidden
            return;
            case 4:
            this.skipAll_button = ((System.Windows.Controls.Button)(target));
            
            #line 28 "..\..\..\UsedWindows\ErrorWindow.xaml"
            this.skipAll_button.Click += new System.Windows.RoutedEventHandler(this.skipAll_button_Click);
            
            #line default
            #line hidden
            return;
            case 5:
            this.skip_button = ((System.Windows.Controls.Button)(target));
            
            #line 30 "..\..\..\UsedWindows\ErrorWindow.xaml"
            this.skip_button.Click += new System.Windows.RoutedEventHandler(this.skip_button_Click);
            
            #line default
            #line hidden
            return;
            case 6:
            this.cancel_button = ((System.Windows.Controls.Button)(target));
            
            #line 32 "..\..\..\UsedWindows\ErrorWindow.xaml"
            this.cancel_button.Click += new System.Windows.RoutedEventHandler(this.cancel_button_Click);
            
            #line default
            #line hidden
            return;
            }
            this._contentLoaded = true;
        }
    }
}

