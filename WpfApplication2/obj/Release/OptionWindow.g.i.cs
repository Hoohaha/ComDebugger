﻿#pragma checksum "..\..\OptionWindow.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "A88FAAA5D8C7D5E53415A344881C5EBC"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.0
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms.Integration;
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


namespace COM_DEBUGGER {
    
    
    /// <summary>
    /// OptionWindow
    /// </summary>
    public partial class OptionWindow : System.Windows.Window, System.Windows.Markup.IComponentConnector {
        
        
        #line 26 "..\..\OptionWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Label Label1;
        
        #line default
        #line hidden
        
        
        #line 27 "..\..\OptionWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBox path1;
        
        #line default
        #line hidden
        
        
        #line 29 "..\..\OptionWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBlock Notice1;
        
        #line default
        #line hidden
        
        
        #line 31 "..\..\OptionWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button Save_button;
        
        #line default
        #line hidden
        
        
        #line 32 "..\..\OptionWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button Cancle_button;
        
        #line default
        #line hidden
        
        
        #line 39 "..\..\OptionWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Label Label2;
        
        #line default
        #line hidden
        
        
        #line 40 "..\..\OptionWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBox path2;
        
        #line default
        #line hidden
        
        
        #line 42 "..\..\OptionWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBlock Notice2;
        
        #line default
        #line hidden
        
        
        #line 50 "..\..\OptionWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Label Label3;
        
        #line default
        #line hidden
        
        
        #line 51 "..\..\OptionWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBox path3;
        
        #line default
        #line hidden
        
        
        #line 53 "..\..\OptionWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBlock Notice3;
        
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
            System.Uri resourceLocater = new System.Uri("/Com Debugger;component/optionwindow.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\OptionWindow.xaml"
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
            this.Label1 = ((System.Windows.Controls.Label)(target));
            return;
            case 2:
            this.path1 = ((System.Windows.Controls.TextBox)(target));
            
            #line 27 "..\..\OptionWindow.xaml"
            this.path1.LostFocus += new System.Windows.RoutedEventHandler(this.Path_Judge_Event);
            
            #line default
            #line hidden
            return;
            case 3:
            this.Notice1 = ((System.Windows.Controls.TextBlock)(target));
            return;
            case 4:
            this.Save_button = ((System.Windows.Controls.Button)(target));
            
            #line 31 "..\..\OptionWindow.xaml"
            this.Save_button.Click += new System.Windows.RoutedEventHandler(this.Save_button_Click);
            
            #line default
            #line hidden
            return;
            case 5:
            this.Cancle_button = ((System.Windows.Controls.Button)(target));
            
            #line 32 "..\..\OptionWindow.xaml"
            this.Cancle_button.Click += new System.Windows.RoutedEventHandler(this.Cancle_button_Click);
            
            #line default
            #line hidden
            return;
            case 6:
            this.Label2 = ((System.Windows.Controls.Label)(target));
            return;
            case 7:
            this.path2 = ((System.Windows.Controls.TextBox)(target));
            
            #line 40 "..\..\OptionWindow.xaml"
            this.path2.LostFocus += new System.Windows.RoutedEventHandler(this.Path_Judge_Event);
            
            #line default
            #line hidden
            return;
            case 8:
            this.Notice2 = ((System.Windows.Controls.TextBlock)(target));
            return;
            case 9:
            this.Label3 = ((System.Windows.Controls.Label)(target));
            return;
            case 10:
            this.path3 = ((System.Windows.Controls.TextBox)(target));
            
            #line 51 "..\..\OptionWindow.xaml"
            this.path3.LostFocus += new System.Windows.RoutedEventHandler(this.Path_Judge_Event);
            
            #line default
            #line hidden
            return;
            case 11:
            this.Notice3 = ((System.Windows.Controls.TextBlock)(target));
            return;
            }
            this._contentLoaded = true;
        }
    }
}
