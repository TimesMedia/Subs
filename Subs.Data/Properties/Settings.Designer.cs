﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Subs.Data.Properties {
    
    
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "16.10.0.0")]
    internal sealed partial class Settings : global::System.Configuration.ApplicationSettingsBase {
        
        private static Settings defaultInstance = ((Settings)(global::System.Configuration.ApplicationSettingsBase.Synchronized(new Settings())));
        
        public static Settings Default {
            get {
                return defaultInstance;
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.SpecialSettingAttribute(global::System.Configuration.SpecialSetting.ConnectionString)]
        [global::System.Configuration.DefaultSettingValueAttribute("Data Source=PKLWEBDB01\\mssql2016std;Initial Catalog=MIMS3;Integrated Security=Tru" +
            "e;Enlist=False;Pooling=True;Max Pool Size=10;Connect Timeout=100")]
        public string MIMSConnectionString {
            get {
                return ((string)(this["MIMSConnectionString"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.SpecialSettingAttribute(global::System.Configuration.SpecialSetting.ConnectionString)]
        [global::System.Configuration.DefaultSettingValueAttribute("Data Source=PKLWEBDB01\\mssql2016std;Initial Catalog=MIMS3;Integrated Security=Tru" +
            "e;Enlist=False;Max Pool Size=10;Connect Timeout=100")]
        public string MIMS3ConnectionString {
            get {
                return ((string)(this["MIMS3ConnectionString"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.SpecialSettingAttribute(global::System.Configuration.SpecialSetting.ConnectionString)]
        [global::System.Configuration.DefaultSettingValueAttribute("Data Source=pklwebdb01;Initial Catalog=SUBSDW;Integrated Security=True")]
        public string SUBSDWConnectionString {
            get {
                return ((string)(this["SUBSDWConnectionString"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.SpecialSettingAttribute(global::System.Configuration.SpecialSetting.ConnectionString)]
        [global::System.Configuration.DefaultSettingValueAttribute("Data Source=PKLWEBDB01;Initial Catalog=MIMS3;Integrated Security=True;Connect Tim" +
            "eout=30;Encrypt=False;TrustServerCertificate=False")]
        public string MIMS3ConnectionString1 {
            get {
                return ((string)(this["MIMS3ConnectionString1"]));
            }
        }
    }
}
