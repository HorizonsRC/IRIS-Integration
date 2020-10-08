﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.1008
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Harness.FinancialsService {
    using System.Runtime.Serialization;
    using System;
    
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.CollectionDataContractAttribute(Name="TimeRecordingData", Namespace="http://www.datacom.co.nz/IRIS/Financials/", ItemName="TimeRecordingEntry")]
    [System.SerializableAttribute()]
    public class TimeRecordingData : System.Collections.Generic.List<Harness.FinancialsService.TimeRecordingEntry> {
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="TimeRecordingEntry", Namespace="http://www.datacom.co.nz/IRIS/Financials/")]
    [System.SerializableAttribute()]
    public partial class TimeRecordingEntry : object, System.Runtime.Serialization.IExtensibleDataObject, System.ComponentModel.INotifyPropertyChanged {
        
        [System.NonSerializedAttribute()]
        private System.Runtime.Serialization.ExtensionDataObject extensionDataField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private string CommentsField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private System.DateTime DateField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private string FINProjectCodeField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private decimal HoursField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private bool IsBillableField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private string TimeCodeField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private string UsernameField;
        
        [global::System.ComponentModel.BrowsableAttribute(false)]
        public System.Runtime.Serialization.ExtensionDataObject ExtensionData {
            get {
                return this.extensionDataField;
            }
            set {
                this.extensionDataField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string Comments {
            get {
                return this.CommentsField;
            }
            set {
                if ((object.ReferenceEquals(this.CommentsField, value) != true)) {
                    this.CommentsField = value;
                    this.RaisePropertyChanged("Comments");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public System.DateTime Date {
            get {
                return this.DateField;
            }
            set {
                if ((this.DateField.Equals(value) != true)) {
                    this.DateField = value;
                    this.RaisePropertyChanged("Date");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string FINProjectCode {
            get {
                return this.FINProjectCodeField;
            }
            set {
                if ((object.ReferenceEquals(this.FINProjectCodeField, value) != true)) {
                    this.FINProjectCodeField = value;
                    this.RaisePropertyChanged("FINProjectCode");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public decimal Hours {
            get {
                return this.HoursField;
            }
            set {
                if ((this.HoursField.Equals(value) != true)) {
                    this.HoursField = value;
                    this.RaisePropertyChanged("Hours");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public bool IsBillable {
            get {
                return this.IsBillableField;
            }
            set {
                if ((this.IsBillableField.Equals(value) != true)) {
                    this.IsBillableField = value;
                    this.RaisePropertyChanged("IsBillable");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string TimeCode {
            get {
                return this.TimeCodeField;
            }
            set {
                if ((object.ReferenceEquals(this.TimeCodeField, value) != true)) {
                    this.TimeCodeField = value;
                    this.RaisePropertyChanged("TimeCode");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string Username {
            get {
                return this.UsernameField;
            }
            set {
                if ((object.ReferenceEquals(this.UsernameField, value) != true)) {
                    this.UsernameField = value;
                    this.RaisePropertyChanged("Username");
                }
            }
        }
        
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        
        protected void RaisePropertyChanged(string propertyName) {
            System.ComponentModel.PropertyChangedEventHandler propertyChanged = this.PropertyChanged;
            if ((propertyChanged != null)) {
                propertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
            }
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="IRISServiceFaultContract", Namespace="http://www.datacom.co.nz/IRIS/")]
    [System.SerializableAttribute()]
    public partial class IRISServiceFaultContract : object, System.Runtime.Serialization.IExtensibleDataObject, System.ComponentModel.INotifyPropertyChanged {
        
        [System.NonSerializedAttribute()]
        private System.Runtime.Serialization.ExtensionDataObject extensionDataField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private int FaultCodeField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private string FaultMessageField;
        
        [global::System.ComponentModel.BrowsableAttribute(false)]
        public System.Runtime.Serialization.ExtensionDataObject ExtensionData {
            get {
                return this.extensionDataField;
            }
            set {
                this.extensionDataField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public int FaultCode {
            get {
                return this.FaultCodeField;
            }
            set {
                if ((this.FaultCodeField.Equals(value) != true)) {
                    this.FaultCodeField = value;
                    this.RaisePropertyChanged("FaultCode");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string FaultMessage {
            get {
                return this.FaultMessageField;
            }
            set {
                if ((object.ReferenceEquals(this.FaultMessageField, value) != true)) {
                    this.FaultMessageField = value;
                    this.RaisePropertyChanged("FaultMessage");
                }
            }
        }
        
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        
        protected void RaisePropertyChanged(string propertyName) {
            System.ComponentModel.PropertyChangedEventHandler propertyChanged = this.PropertyChanged;
            if ((propertyChanged != null)) {
                propertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
            }
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.CollectionDataContractAttribute(Name="Parameters", Namespace="http://www.datacom.co.nz/IRIS/Financials/", ItemName="Parameter", KeyName="Name", ValueName="Value")]
    [System.SerializableAttribute()]
    public class Parameters : System.Collections.Generic.Dictionary<string, string> {
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ServiceModel.ServiceContractAttribute(Namespace="http://www.datacom.co.nz/IRIS/Financials/", ConfigurationName="FinancialsService.IFinancialsService")]
    public interface IFinancialsService {
        
        [System.ServiceModel.OperationContractAttribute(Action="http://www.datacom.co.nz/IRIS/Financials/IFinancialsService/GetTimeRecordingData", ReplyAction="http://www.datacom.co.nz/IRIS/Financials/IFinancialsService/GetTimeRecordingDataR" +
            "esponse")]
        [System.ServiceModel.FaultContractAttribute(typeof(Harness.FinancialsService.IRISServiceFaultContract), Action="http://www.datacom.co.nz/IRIS/Financials/IFinancialsService/GetTimeRecordingDataI" +
            "RISServiceFaultContractFault", Name="IRISServiceFaultContract", Namespace="http://www.datacom.co.nz/IRIS/")]
        Harness.FinancialsService.TimeRecordingData GetTimeRecordingData(System.DateTime SinceDateTime, string AccountName);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://www.datacom.co.nz/IRIS/Financials/IFinancialsService/GetIRISData", ReplyAction="http://www.datacom.co.nz/IRIS/Financials/IFinancialsService/GetIRISDataResponse")]
        [System.ServiceModel.FaultContractAttribute(typeof(Harness.FinancialsService.IRISServiceFaultContract), Action="http://www.datacom.co.nz/IRIS/Financials/IFinancialsService/GetIRISDataIRISServic" +
            "eFaultContractFault", Name="IRISServiceFaultContract", Namespace="http://www.datacom.co.nz/IRIS/")]
        string GetIRISData(string NamedQuery, Harness.FinancialsService.Parameters Parameters);
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public interface IFinancialsServiceChannel : Harness.FinancialsService.IFinancialsService, System.ServiceModel.IClientChannel {
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public partial class FinancialsServiceClient : System.ServiceModel.ClientBase<Harness.FinancialsService.IFinancialsService>, Harness.FinancialsService.IFinancialsService {
        
        public FinancialsServiceClient() {
        }
        
        public FinancialsServiceClient(string endpointConfigurationName) : 
                base(endpointConfigurationName) {
        }
        
        public FinancialsServiceClient(string endpointConfigurationName, string remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public FinancialsServiceClient(string endpointConfigurationName, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public FinancialsServiceClient(System.ServiceModel.Channels.Binding binding, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(binding, remoteAddress) {
        }
        
        public Harness.FinancialsService.TimeRecordingData GetTimeRecordingData(System.DateTime SinceDateTime, string AccountName) {
            return base.Channel.GetTimeRecordingData(SinceDateTime, AccountName);
        }
        
        public string GetIRISData(string NamedQuery, Harness.FinancialsService.Parameters Parameters) {
            return base.Channel.GetIRISData(NamedQuery, Parameters);
        }
    }
}