﻿#pragma checksum "..\..\..\UsedWindows\InformationWindow.xaml" "{8829d00f-11b8-4213-878b-770e8597ac16}" "6FE355D23502FEE6CDFE60358042232B5A89C9BA2CAD7F8592E8B47F26D0AECD"
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
    /// InformationWindow
    /// </summary>
    public partial class InformationWindow : System.Windows.Window, System.Windows.Markup.IComponentConnector {
        
        
        #line 21 "..\..\..\UsedWindows\InformationWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.RichTextBox message_rb;
        
        #line default
        #line hidden
        
        
        #line 23 "..\..\..\UsedWindows\InformationWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.ListView diskSpace_listview;
        
        #line default
        #line hidden
        
        
        #line 49 "..\..\..\UsedWindows\InformationWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button try_button;
        
        #line default
        #line hidden
        
        
        #line 50 "..\..\..\UsedWindows\InformationWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button ignore_button;
        
        #line default
        #line hidden
        
        
        #line 51 "..\..\..\UsedWindows\InformationWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button fit_button;
        
        #line default
        #line hidden
        
        
        #line 52 "..\..\..\UsedWindows\InformationWindow.xaml"
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
            System.Uri resourceLocater = new System.Uri("/NeathCopy;component/usedwindows/informationwindow.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\..\UsedWindows\InformationWindow.xaml"
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
            
            #line 6 "..\..\..\UsedWindows\InformationWindow.xaml"
            ((NeathCopy.UsedWindows.InformationWindow)(target)).Closing += new System.ComponentModel.CancelEventHandler(this.Window_Closing);
            
            #line default
            #line hidden
            return;
            case 2:
            this.message_rb = ((System.Windows.Controls.RichTextBox)(target));
            return;
            case 3:
            this.diskSpace_listview = ((System.Windows.Controls.ListView)(target));
            return;
            case 4:
            this.try_button = ((System.Windows.Controls.Button)(target));
            
            #line 49 "..\..\..\UsedWindows\InformationWindow.xaml"
            this.try_button.Click += new System.Windows.RoutedEventHandler(this.try_button_Click);
            
            #line default
            #line hidden
            return;
            case 5:
            this.ignore_button = ((System.Windows.Controls.Button)(target));
            
            #line 50 "..\..\..\UsedWindows\InformationWindow.xaml"
            this.ignore_button.Click += new System.Windows.RoutedEventHandler(this.ignore_button_Click);
            
            #line default
            #line hidden
            return;
            case 6:
            this.fit_button = ((System.Windows.Controls.Button)(target));
            
            #line 51 "..\..\..\UsedWindows\InformationWindow.xaml"
            this.fit_button.Click += new System.Windows.RoutedEventHandler(this.fit_button_Click);
            
            #line default
            #line hidden
            return;
            case 7:
            this.cancel_button = ((System.Windows.Controls.Button)(target));
            
            #line 52 "..\..\..\UsedWindows\InformationWindow.xaml"
            this.cancel_button.Click += new System.Windows.RoutedEventHandler(this.cancel_button_Click);
            
            #line default
            #line hidden
            return;
            }
            this._contentLoaded = true;
        }
    }
}

