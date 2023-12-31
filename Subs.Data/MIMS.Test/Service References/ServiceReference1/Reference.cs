﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace MIMS.Test.ServiceReference1 {
    
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ServiceModel.ServiceContractAttribute(ConfigurationName="ServiceReference1.ServiceSoap")]
    public interface ServiceSoap {
        
        // CODEGEN: Generating message contract since message AuthorizeRequest has headers
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/Authorize", ReplyAction="*")]
        [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults=true)]
        MIMS.Test.ServiceReference1.AuthorizeResponse Authorize(MIMS.Test.ServiceReference1.AuthorizeRequest request);
        
        // CODEGEN: Generating message contract since message AuthorizationsRequest has headers
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/Authorizations", ReplyAction="*")]
        [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults=true)]
        MIMS.Test.ServiceReference1.AuthorizationsResponse Authorizations(MIMS.Test.ServiceReference1.AuthorizationsRequest request);
        
        // CODEGEN: Generating message contract since message TestRequest has headers
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/Test", ReplyAction="*")]
        [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults=true)]
        MIMS.Test.ServiceReference1.TestResponse Test(MIMS.Test.ServiceReference1.TestRequest request);
        
        // CODEGEN: Generating message contract since message ReceiverSubscriptionCheckRequest has headers
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/ReceiverSubscriptionCheck", ReplyAction="*")]
        [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults=true)]
        MIMS.Test.ServiceReference1.ReceiverSubscriptionCheckResponse ReceiverSubscriptionCheck(MIMS.Test.ServiceReference1.ReceiverSubscriptionCheckRequest request);
        
        // CODEGEN: Generating message contract since message FindEMailByCustomerIdRequest has headers
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/FindEMailByCustomerId", ReplyAction="*")]
        [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults=true)]
        MIMS.Test.ServiceReference1.FindEMailByCustomerIdResponse FindEMailByCustomerId(MIMS.Test.ServiceReference1.FindEMailByCustomerIdRequest request);
        
        // CODEGEN: Generating message contract since message FindCustomerIdByEmailRequest has headers
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/FindCustomerIdByEmail", ReplyAction="*")]
        [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults=true)]
        MIMS.Test.ServiceReference1.FindCustomerIdByEmailResponse FindCustomerIdByEmail(MIMS.Test.ServiceReference1.FindCustomerIdByEmailRequest request);
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.7.2612.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace="http://tempuri.org/")]
    public partial class AuthorizationHeader : object, System.ComponentModel.INotifyPropertyChanged {
        
        private string typeField;
        
        private string sourceField;
        
        private System.Xml.XmlAttribute[] anyAttrField;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Order=0)]
        public string Type {
            get {
                return this.typeField;
            }
            set {
                this.typeField = value;
                this.RaisePropertyChanged("Type");
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Order=1)]
        public string Source {
            get {
                return this.sourceField;
            }
            set {
                this.sourceField = value;
                this.RaisePropertyChanged("Source");
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAnyAttributeAttribute()]
        public System.Xml.XmlAttribute[] AnyAttr {
            get {
                return this.anyAttrField;
            }
            set {
                this.anyAttrField = value;
                this.RaisePropertyChanged("AnyAttr");
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
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.7.2612.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace="http://tempuri.org/")]
    public partial class AuthorizationResult : object, System.ComponentModel.INotifyPropertyChanged {
        
        private int yearField;
        
        private int monthField;
        
        private int customerIdField;
        
        private int productIdField;
        
        private int seatsField;
        
        private System.DateTime expirationDateField;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Order=0)]
        public int Year {
            get {
                return this.yearField;
            }
            set {
                this.yearField = value;
                this.RaisePropertyChanged("Year");
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Order=1)]
        public int Month {
            get {
                return this.monthField;
            }
            set {
                this.monthField = value;
                this.RaisePropertyChanged("Month");
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Order=2)]
        public int CustomerId {
            get {
                return this.customerIdField;
            }
            set {
                this.customerIdField = value;
                this.RaisePropertyChanged("CustomerId");
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Order=3)]
        public int ProductId {
            get {
                return this.productIdField;
            }
            set {
                this.productIdField = value;
                this.RaisePropertyChanged("ProductId");
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Order=4)]
        public int Seats {
            get {
                return this.seatsField;
            }
            set {
                this.seatsField = value;
                this.RaisePropertyChanged("Seats");
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Order=5)]
        public System.DateTime ExpirationDate {
            get {
                return this.expirationDateField;
            }
            set {
                this.expirationDateField = value;
                this.RaisePropertyChanged("ExpirationDate");
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
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.7.2612.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace="http://tempuri.org/")]
    public partial class SeatResult : object, System.ComponentModel.INotifyPropertyChanged {
        
        private int seatsField;
        
        private string reasonField;
        
        private System.DateTime expirationDateField;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Order=0)]
        public int Seats {
            get {
                return this.seatsField;
            }
            set {
                this.seatsField = value;
                this.RaisePropertyChanged("Seats");
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Order=1)]
        public string Reason {
            get {
                return this.reasonField;
            }
            set {
                this.reasonField = value;
                this.RaisePropertyChanged("Reason");
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Order=2)]
        public System.DateTime ExpirationDate {
            get {
                return this.expirationDateField;
            }
            set {
                this.expirationDateField = value;
                this.RaisePropertyChanged("ExpirationDate");
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
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
    [System.ServiceModel.MessageContractAttribute(WrapperName="Authorize", WrapperNamespace="http://tempuri.org/", IsWrapped=true)]
    public partial class AuthorizeRequest {
        
        [System.ServiceModel.MessageHeaderAttribute(Namespace="http://tempuri.org/")]
        public MIMS.Test.ServiceReference1.AuthorizationHeader AuthorizationHeader;
        
        [System.ServiceModel.MessageBodyMemberAttribute(Namespace="http://tempuri.org/", Order=0)]
        public int pProductId;
        
        [System.ServiceModel.MessageBodyMemberAttribute(Namespace="http://tempuri.org/", Order=1)]
        public int pReceiverId;
        
        public AuthorizeRequest() {
        }
        
        public AuthorizeRequest(MIMS.Test.ServiceReference1.AuthorizationHeader AuthorizationHeader, int pProductId, int pReceiverId) {
            this.AuthorizationHeader = AuthorizationHeader;
            this.pProductId = pProductId;
            this.pReceiverId = pReceiverId;
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
    [System.ServiceModel.MessageContractAttribute(WrapperName="AuthorizeResponse", WrapperNamespace="http://tempuri.org/", IsWrapped=true)]
    public partial class AuthorizeResponse {
        
        [System.ServiceModel.MessageBodyMemberAttribute(Namespace="http://tempuri.org/", Order=0)]
        public MIMS.Test.ServiceReference1.SeatResult AuthorizeResult;
        
        public AuthorizeResponse() {
        }
        
        public AuthorizeResponse(MIMS.Test.ServiceReference1.SeatResult AuthorizeResult) {
            this.AuthorizeResult = AuthorizeResult;
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
    [System.ServiceModel.MessageContractAttribute(WrapperName="Authorizations", WrapperNamespace="http://tempuri.org/", IsWrapped=true)]
    public partial class AuthorizationsRequest {
        
        [System.ServiceModel.MessageHeaderAttribute(Namespace="http://tempuri.org/")]
        public MIMS.Test.ServiceReference1.AuthorizationHeader AuthorizationHeader;
        
        public AuthorizationsRequest() {
        }
        
        public AuthorizationsRequest(MIMS.Test.ServiceReference1.AuthorizationHeader AuthorizationHeader) {
            this.AuthorizationHeader = AuthorizationHeader;
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
    [System.ServiceModel.MessageContractAttribute(WrapperName="AuthorizationsResponse", WrapperNamespace="http://tempuri.org/", IsWrapped=true)]
    public partial class AuthorizationsResponse {
        
        [System.ServiceModel.MessageBodyMemberAttribute(Namespace="http://tempuri.org/", Order=0)]
        public MIMS.Test.ServiceReference1.AuthorizationResult[] AuthorizationsResult;
        
        public AuthorizationsResponse() {
        }
        
        public AuthorizationsResponse(MIMS.Test.ServiceReference1.AuthorizationResult[] AuthorizationsResult) {
            this.AuthorizationsResult = AuthorizationsResult;
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
    [System.ServiceModel.MessageContractAttribute(WrapperName="Test", WrapperNamespace="http://tempuri.org/", IsWrapped=true)]
    public partial class TestRequest {
        
        [System.ServiceModel.MessageHeaderAttribute(Namespace="http://tempuri.org/")]
        public MIMS.Test.ServiceReference1.AuthorizationHeader AuthorizationHeader;
        
        [System.ServiceModel.MessageBodyMemberAttribute(Namespace="http://tempuri.org/", Order=0)]
        public int ReceiverId;
        
        [System.ServiceModel.MessageBodyMemberAttribute(Namespace="http://tempuri.org/", Order=1)]
        public int ProductId;
        
        public TestRequest() {
        }
        
        public TestRequest(MIMS.Test.ServiceReference1.AuthorizationHeader AuthorizationHeader, int ReceiverId, int ProductId) {
            this.AuthorizationHeader = AuthorizationHeader;
            this.ReceiverId = ReceiverId;
            this.ProductId = ProductId;
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
    [System.ServiceModel.MessageContractAttribute(WrapperName="TestResponse", WrapperNamespace="http://tempuri.org/", IsWrapped=true)]
    public partial class TestResponse {
        
        [System.ServiceModel.MessageBodyMemberAttribute(Namespace="http://tempuri.org/", Order=0)]
        public string TestResult;
        
        public TestResponse() {
        }
        
        public TestResponse(string TestResult) {
            this.TestResult = TestResult;
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
    [System.ServiceModel.MessageContractAttribute(WrapperName="ReceiverSubscriptionCheck", WrapperNamespace="http://tempuri.org/", IsWrapped=true)]
    public partial class ReceiverSubscriptionCheckRequest {
        
        [System.ServiceModel.MessageHeaderAttribute(Namespace="http://tempuri.org/")]
        public MIMS.Test.ServiceReference1.AuthorizationHeader AuthorizationHeader;
        
        [System.ServiceModel.MessageBodyMemberAttribute(Namespace="http://tempuri.org/", Order=0)]
        public int ReceiverId;
        
        [System.ServiceModel.MessageBodyMemberAttribute(Namespace="http://tempuri.org/", Order=1)]
        public int ProductId;
        
        public ReceiverSubscriptionCheckRequest() {
        }
        
        public ReceiverSubscriptionCheckRequest(MIMS.Test.ServiceReference1.AuthorizationHeader AuthorizationHeader, int ReceiverId, int ProductId) {
            this.AuthorizationHeader = AuthorizationHeader;
            this.ReceiverId = ReceiverId;
            this.ProductId = ProductId;
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
    [System.ServiceModel.MessageContractAttribute(WrapperName="ReceiverSubscriptionCheckResponse", WrapperNamespace="http://tempuri.org/", IsWrapped=true)]
    public partial class ReceiverSubscriptionCheckResponse {
        
        [System.ServiceModel.MessageBodyMemberAttribute(Namespace="http://tempuri.org/", Order=0)]
        [System.Xml.Serialization.XmlElementAttribute(IsNullable=true)]
        public System.Nullable<System.DateTime> ReceiverSubscriptionCheckResult;
        
        public ReceiverSubscriptionCheckResponse() {
        }
        
        public ReceiverSubscriptionCheckResponse(System.Nullable<System.DateTime> ReceiverSubscriptionCheckResult) {
            this.ReceiverSubscriptionCheckResult = ReceiverSubscriptionCheckResult;
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
    [System.ServiceModel.MessageContractAttribute(WrapperName="FindEMailByCustomerId", WrapperNamespace="http://tempuri.org/", IsWrapped=true)]
    public partial class FindEMailByCustomerIdRequest {
        
        [System.ServiceModel.MessageHeaderAttribute(Namespace="http://tempuri.org/")]
        public MIMS.Test.ServiceReference1.AuthorizationHeader AuthorizationHeader;
        
        [System.ServiceModel.MessageBodyMemberAttribute(Namespace="http://tempuri.org/", Order=0)]
        public int pCustomerId;
        
        public FindEMailByCustomerIdRequest() {
        }
        
        public FindEMailByCustomerIdRequest(MIMS.Test.ServiceReference1.AuthorizationHeader AuthorizationHeader, int pCustomerId) {
            this.AuthorizationHeader = AuthorizationHeader;
            this.pCustomerId = pCustomerId;
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
    [System.ServiceModel.MessageContractAttribute(WrapperName="FindEMailByCustomerIdResponse", WrapperNamespace="http://tempuri.org/", IsWrapped=true)]
    public partial class FindEMailByCustomerIdResponse {
        
        [System.ServiceModel.MessageBodyMemberAttribute(Namespace="http://tempuri.org/", Order=0)]
        public bool FindEMailByCustomerIdResult;
        
        [System.ServiceModel.MessageBodyMemberAttribute(Namespace="http://tempuri.org/", Order=1)]
        public string pEmailAddress;
        
        public FindEMailByCustomerIdResponse() {
        }
        
        public FindEMailByCustomerIdResponse(bool FindEMailByCustomerIdResult, string pEmailAddress) {
            this.FindEMailByCustomerIdResult = FindEMailByCustomerIdResult;
            this.pEmailAddress = pEmailAddress;
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
    [System.ServiceModel.MessageContractAttribute(WrapperName="FindCustomerIdByEmail", WrapperNamespace="http://tempuri.org/", IsWrapped=true)]
    public partial class FindCustomerIdByEmailRequest {
        
        [System.ServiceModel.MessageHeaderAttribute(Namespace="http://tempuri.org/")]
        public MIMS.Test.ServiceReference1.AuthorizationHeader AuthorizationHeader;
        
        [System.ServiceModel.MessageBodyMemberAttribute(Namespace="http://tempuri.org/", Order=0)]
        public string EmailAddress;
        
        public FindCustomerIdByEmailRequest() {
        }
        
        public FindCustomerIdByEmailRequest(MIMS.Test.ServiceReference1.AuthorizationHeader AuthorizationHeader, string EmailAddress) {
            this.AuthorizationHeader = AuthorizationHeader;
            this.EmailAddress = EmailAddress;
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
    [System.ServiceModel.MessageContractAttribute(WrapperName="FindCustomerIdByEmailResponse", WrapperNamespace="http://tempuri.org/", IsWrapped=true)]
    public partial class FindCustomerIdByEmailResponse {
        
        [System.ServiceModel.MessageBodyMemberAttribute(Namespace="http://tempuri.org/", Order=0)]
        public bool FindCustomerIdByEmailResult;
        
        [System.ServiceModel.MessageBodyMemberAttribute(Namespace="http://tempuri.org/", Order=1)]
        public int CustomerId;
        
        public FindCustomerIdByEmailResponse() {
        }
        
        public FindCustomerIdByEmailResponse(bool FindCustomerIdByEmailResult, int CustomerId) {
            this.FindCustomerIdByEmailResult = FindCustomerIdByEmailResult;
            this.CustomerId = CustomerId;
        }
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public interface ServiceSoapChannel : MIMS.Test.ServiceReference1.ServiceSoap, System.ServiceModel.IClientChannel {
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public partial class ServiceSoapClient : System.ServiceModel.ClientBase<MIMS.Test.ServiceReference1.ServiceSoap>, MIMS.Test.ServiceReference1.ServiceSoap {
        
        public ServiceSoapClient() {
        }
        
        public ServiceSoapClient(string endpointConfigurationName) : 
                base(endpointConfigurationName) {
        }
        
        public ServiceSoapClient(string endpointConfigurationName, string remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public ServiceSoapClient(string endpointConfigurationName, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public ServiceSoapClient(System.ServiceModel.Channels.Binding binding, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(binding, remoteAddress) {
        }
        
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
        MIMS.Test.ServiceReference1.AuthorizeResponse MIMS.Test.ServiceReference1.ServiceSoap.Authorize(MIMS.Test.ServiceReference1.AuthorizeRequest request) {
            return base.Channel.Authorize(request);
        }
        
        public MIMS.Test.ServiceReference1.SeatResult Authorize(MIMS.Test.ServiceReference1.AuthorizationHeader AuthorizationHeader, int pProductId, int pReceiverId) {
            MIMS.Test.ServiceReference1.AuthorizeRequest inValue = new MIMS.Test.ServiceReference1.AuthorizeRequest();
            inValue.AuthorizationHeader = AuthorizationHeader;
            inValue.pProductId = pProductId;
            inValue.pReceiverId = pReceiverId;
            MIMS.Test.ServiceReference1.AuthorizeResponse retVal = ((MIMS.Test.ServiceReference1.ServiceSoap)(this)).Authorize(inValue);
            return retVal.AuthorizeResult;
        }
        
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
        MIMS.Test.ServiceReference1.AuthorizationsResponse MIMS.Test.ServiceReference1.ServiceSoap.Authorizations(MIMS.Test.ServiceReference1.AuthorizationsRequest request) {
            return base.Channel.Authorizations(request);
        }
        
        public MIMS.Test.ServiceReference1.AuthorizationResult[] Authorizations(MIMS.Test.ServiceReference1.AuthorizationHeader AuthorizationHeader) {
            MIMS.Test.ServiceReference1.AuthorizationsRequest inValue = new MIMS.Test.ServiceReference1.AuthorizationsRequest();
            inValue.AuthorizationHeader = AuthorizationHeader;
            MIMS.Test.ServiceReference1.AuthorizationsResponse retVal = ((MIMS.Test.ServiceReference1.ServiceSoap)(this)).Authorizations(inValue);
            return retVal.AuthorizationsResult;
        }
        
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
        MIMS.Test.ServiceReference1.TestResponse MIMS.Test.ServiceReference1.ServiceSoap.Test(MIMS.Test.ServiceReference1.TestRequest request) {
            return base.Channel.Test(request);
        }
        
        public string Test(MIMS.Test.ServiceReference1.AuthorizationHeader AuthorizationHeader, int ReceiverId, int ProductId) {
            MIMS.Test.ServiceReference1.TestRequest inValue = new MIMS.Test.ServiceReference1.TestRequest();
            inValue.AuthorizationHeader = AuthorizationHeader;
            inValue.ReceiverId = ReceiverId;
            inValue.ProductId = ProductId;
            MIMS.Test.ServiceReference1.TestResponse retVal = ((MIMS.Test.ServiceReference1.ServiceSoap)(this)).Test(inValue);
            return retVal.TestResult;
        }
        
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
        MIMS.Test.ServiceReference1.ReceiverSubscriptionCheckResponse MIMS.Test.ServiceReference1.ServiceSoap.ReceiverSubscriptionCheck(MIMS.Test.ServiceReference1.ReceiverSubscriptionCheckRequest request) {
            return base.Channel.ReceiverSubscriptionCheck(request);
        }
        
        public System.Nullable<System.DateTime> ReceiverSubscriptionCheck(MIMS.Test.ServiceReference1.AuthorizationHeader AuthorizationHeader, int ReceiverId, int ProductId) {
            MIMS.Test.ServiceReference1.ReceiverSubscriptionCheckRequest inValue = new MIMS.Test.ServiceReference1.ReceiverSubscriptionCheckRequest();
            inValue.AuthorizationHeader = AuthorizationHeader;
            inValue.ReceiverId = ReceiverId;
            inValue.ProductId = ProductId;
            MIMS.Test.ServiceReference1.ReceiverSubscriptionCheckResponse retVal = ((MIMS.Test.ServiceReference1.ServiceSoap)(this)).ReceiverSubscriptionCheck(inValue);
            return retVal.ReceiverSubscriptionCheckResult;
        }
        
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
        MIMS.Test.ServiceReference1.FindEMailByCustomerIdResponse MIMS.Test.ServiceReference1.ServiceSoap.FindEMailByCustomerId(MIMS.Test.ServiceReference1.FindEMailByCustomerIdRequest request) {
            return base.Channel.FindEMailByCustomerId(request);
        }
        
        public bool FindEMailByCustomerId(MIMS.Test.ServiceReference1.AuthorizationHeader AuthorizationHeader, int pCustomerId, out string pEmailAddress) {
            MIMS.Test.ServiceReference1.FindEMailByCustomerIdRequest inValue = new MIMS.Test.ServiceReference1.FindEMailByCustomerIdRequest();
            inValue.AuthorizationHeader = AuthorizationHeader;
            inValue.pCustomerId = pCustomerId;
            MIMS.Test.ServiceReference1.FindEMailByCustomerIdResponse retVal = ((MIMS.Test.ServiceReference1.ServiceSoap)(this)).FindEMailByCustomerId(inValue);
            pEmailAddress = retVal.pEmailAddress;
            return retVal.FindEMailByCustomerIdResult;
        }
        
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
        MIMS.Test.ServiceReference1.FindCustomerIdByEmailResponse MIMS.Test.ServiceReference1.ServiceSoap.FindCustomerIdByEmail(MIMS.Test.ServiceReference1.FindCustomerIdByEmailRequest request) {
            return base.Channel.FindCustomerIdByEmail(request);
        }
        
        public bool FindCustomerIdByEmail(MIMS.Test.ServiceReference1.AuthorizationHeader AuthorizationHeader, string EmailAddress, out int CustomerId) {
            MIMS.Test.ServiceReference1.FindCustomerIdByEmailRequest inValue = new MIMS.Test.ServiceReference1.FindCustomerIdByEmailRequest();
            inValue.AuthorizationHeader = AuthorizationHeader;
            inValue.EmailAddress = EmailAddress;
            MIMS.Test.ServiceReference1.FindCustomerIdByEmailResponse retVal = ((MIMS.Test.ServiceReference1.ServiceSoap)(this)).FindCustomerIdByEmail(inValue);
            CustomerId = retVal.CustomerId;
            return retVal.FindCustomerIdByEmailResult;
        }
    }
}
