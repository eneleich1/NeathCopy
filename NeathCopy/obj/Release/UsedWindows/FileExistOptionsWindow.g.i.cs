﻿#pragma checksum "..\..\..\UsedWindows\FileExistOptionsWindow.xaml" "{8829d00f-11b8-4213-878b-770e8597ac16}" "919231D8CE5CA45430428090E3814829D80E56D3AD415D1D716A10A415C87309"
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
    /// FileExistOptionsWindow
    /// </summary>
    public partial class FileExistOptionsWindow : System.Windows.Window, System.Windows.Markup.IComponentConnector {
        
        
        #line 21 "..\..\..\UsedWindows\FileExistOptionsWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.RichTextBox info_tb;
        
        #line default
        #line hidden
        
        
        #line 24 "..\..\..\UsedWindows\FileExistOptionsWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.ComboBox option_cb;
        
        #line default
        #line hidden
        
        
        #line 32 "..\..\..\UsedWindows\FileExistOptionsWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button ok_button;
        
        #line default
        #line hidden
        
        
        #line 33 "..\..\..\UsedWindows\FileExistOptionsWindow.xaml"
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
            System.Uri resourceLocater = new System.Uri("/NeathCopy;component/usedwindows/fileexistoptionswindow.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\..\UsedWindows\FileExistOptionsWindow.xaml"
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
            
            #line 7 "..\..\..\UsedWindows\FileExistOptionsWindow.xaml"
            ((NeathCopy.UsedWindows.FileExistOptionsWindow)(target)).Closing += new System.ComponentModel.CancelEventHandler(this.Window_Closing);
            
            #line default
            #line hidden
            return;
            case 2:
            this.info_tb = ((System.Windows.Controls.RichTextBox)(target));
            return;
            case 3:
            this.option_cb = ((System.Windows.Controls.ComboBox)(target));
            return;
            case 4:
            this.ok_button = ((System.Windows.Controls.Button)(target));
            
            #line 32 "..\..\..\UsedWindows\FileExistOptionsWindow.xaml"
            this.ok_button.Click += new System.Windows.RoutedEventHandler(this.ok_button_Click);
            
            #line default
            #line hidden
            return;
            case 5:
            this.cancel_button = ((System.Windows.Controls.Button)(target));
            
            #line 33 "..\..\..\UsedWindows\FileExistOptionsWindow.xaml"
            this.cancel_button.Click += new System.Windows.RoutedEventHandler(this.cancel_button_Click);
            
            #line default
            #line hidden
            return;
            }
            this._contentLoaded = true;
        }
    }
}

