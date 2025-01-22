//namespace XcaDocumentSource.Models.Soap;


//// NOTE: Generated code may require at least .NET Framework 4.5 or .NET Core/Standard 2.0.
///// <remarks/>
//[System.SerializableAttribute()]
//[System.ComponentModel.DesignerCategoryAttribute("code")]
//[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.w3.org/2003/05/soap-envelope")]
//[System.Xml.Serialization.XmlRootAttribute(Namespace = "http://www.w3.org/2003/05/soap-envelope", IsNullable = false)]
//public partial class Envelope
//{

//    private EnvelopeHeader headerField;

//    private EnvelopeBody bodyField;

//    /// <remarks/>
//    public EnvelopeHeader Header
//    {
//        get
//        {
//            return this.headerField;
//        }
//        set
//        {
//            this.headerField = value;
//        }
//    }

//    /// <remarks/>
//    public EnvelopeBody Body
//    {
//        get
//        {
//            return this.bodyField;
//        }
//        set
//        {
//            this.bodyField = value;
//        }
//    }
//}

///// <remarks/>
//[System.SerializableAttribute()]
//[System.ComponentModel.DesignerCategoryAttribute("code")]
//[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.w3.org/2003/05/soap-envelope")]
//public partial class EnvelopeHeader
//{

//    private Security securityField;

//    private Action actionField;

//    private string messageIDField;

//    private string toField;

//    private ReplyTo replyToField;

//    /// <remarks/>
//    [System.Xml.Serialization.XmlElementAttribute(Namespace = "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd" +
//        "")]
//    public Security Security
//    {
//        get
//        {
//            return this.securityField;
//        }
//        set
//        {
//            this.securityField = value;
//        }
//    }

//    /// <remarks/>
//    [System.Xml.Serialization.XmlElementAttribute(Namespace = "http://www.w3.org/2005/08/addressing")]
//    public Action Action
//    {
//        get
//        {
//            return this.actionField;
//        }
//        set
//        {
//            this.actionField = value;
//        }
//    }

//    /// <remarks/>
//    [System.Xml.Serialization.XmlElementAttribute(Namespace = "http://www.w3.org/2005/08/addressing")]
//    public string MessageID
//    {
//        get
//        {
//            return this.messageIDField;
//        }
//        set
//        {
//            this.messageIDField = value;
//        }
//    }

//    /// <remarks/>
//    [System.Xml.Serialization.XmlElementAttribute(Namespace = "http://www.w3.org/2005/08/addressing")]
//    public string To
//    {
//        get
//        {
//            return this.toField;
//        }
//        set
//        {
//            this.toField = value;
//        }
//    }

//    /// <remarks/>
//    [System.Xml.Serialization.XmlElementAttribute(Namespace = "http://www.w3.org/2005/08/addressing")]
//    public ReplyTo ReplyTo
//    {
//        get
//        {
//            return this.replyToField;
//        }
//        set
//        {
//            this.replyToField = value;
//        }
//    }
//}

///// <remarks/>
//[System.SerializableAttribute()]
//[System.ComponentModel.DesignerCategoryAttribute("code")]
//[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd" +
//    "")]
//[System.Xml.Serialization.XmlRootAttribute(Namespace = "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd" +
//    "", IsNullable = false)]
//public partial class Security
//{

//    private Timestamp timestampField;

//    private bool mustUnderstandField;

//    /// <remarks/>
//    [System.Xml.Serialization.XmlElementAttribute(Namespace = "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xs" +
//        "d")]
//    public Timestamp Timestamp
//    {
//        get
//        {
//            return this.timestampField;
//        }
//        set
//        {
//            this.timestampField = value;
//        }
//    }

//    /// <remarks/>
//    [System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Qualified, Namespace = "http://www.w3.org/2003/05/soap-envelope")]
//    public bool mustUnderstand
//    {
//        get
//        {
//            return this.mustUnderstandField;
//        }
//        set
//        {
//            this.mustUnderstandField = value;
//        }
//    }
//}

///// <remarks/>
//[System.SerializableAttribute()]
//[System.ComponentModel.DesignerCategoryAttribute("code")]
//[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xs" +
//    "d")]
//[System.Xml.Serialization.XmlRootAttribute(Namespace = "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xs" +
//    "d", IsNullable = false)]
//public partial class Timestamp
//{

//    private System.DateTime createdField;

//    private System.DateTime expiresField;

//    private string idField;

//    /// <remarks/>
//    public System.DateTime Created
//    {
//        get
//        {
//            return this.createdField;
//        }
//        set
//        {
//            this.createdField = value;
//        }
//    }

//    /// <remarks/>
//    public System.DateTime Expires
//    {
//        get
//        {
//            return this.expiresField;
//        }
//        set
//        {
//            this.expiresField = value;
//        }
//    }

//    /// <remarks/>
//    [System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Qualified)]
//    public string Id
//    {
//        get
//        {
//            return this.idField;
//        }
//        set
//        {
//            this.idField = value;
//        }
//    }
//}

///// <remarks/>
//[System.SerializableAttribute()]
//[System.ComponentModel.DesignerCategoryAttribute("code")]
//[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.w3.org/2005/08/addressing")]
//[System.Xml.Serialization.XmlRootAttribute(Namespace = "http://www.w3.org/2005/08/addressing", IsNullable = false)]
//public partial class Action
//{

//    private bool mustUnderstandField;

//    private string valueField;

//    /// <remarks/>
//    [System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Qualified, Namespace = "http://www.w3.org/2003/05/soap-envelope")]
//    public bool mustUnderstand
//    {
//        get
//        {
//            return this.mustUnderstandField;
//        }
//        set
//        {
//            this.mustUnderstandField = value;
//        }
//    }

//    /// <remarks/>
//    [System.Xml.Serialization.XmlTextAttribute()]
//    public string Value
//    {
//        get
//        {
//            return this.valueField;
//        }
//        set
//        {
//            this.valueField = value;
//        }
//    }
//}

///// <remarks/>
//[System.SerializableAttribute()]
//[System.ComponentModel.DesignerCategoryAttribute("code")]
//[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.w3.org/2005/08/addressing")]
//[System.Xml.Serialization.XmlRootAttribute(Namespace = "http://www.w3.org/2005/08/addressing", IsNullable = false)]
//public partial class ReplyTo
//{

//    private string addressField;

//    private bool mustUnderstandField;

//    /// <remarks/>
//    public string Address
//    {
//        get
//        {
//            return this.addressField;
//        }
//        set
//        {
//            this.addressField = value;
//        }
//    }

//    /// <remarks/>
//    [System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Qualified, Namespace = "http://www.w3.org/2003/05/soap-envelope")]
//    public bool mustUnderstand
//    {
//        get
//        {
//            return this.mustUnderstandField;
//        }
//        set
//        {
//            this.mustUnderstandField = value;
//        }
//    }
//}

///// <remarks/>
//[System.SerializableAttribute()]
//[System.ComponentModel.DesignerCategoryAttribute("code")]
//[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.w3.org/2003/05/soap-envelope")]
//public partial class EnvelopeBody
//{

//    private AdhocQueryRequest adhocQueryRequestField;

//    /// <remarks/>
//    [System.Xml.Serialization.XmlElementAttribute(Namespace = "urn:oasis:names:tc:ebxml-regrep:xsd:query:3.0")]
//    public AdhocQueryRequest AdhocQueryRequest
//    {
//        get
//        {
//            return this.adhocQueryRequestField;
//        }
//        set
//        {
//            this.adhocQueryRequestField = value;
//        }
//    }
//}

///// <remarks/>
//[System.SerializableAttribute()]
//[System.ComponentModel.DesignerCategoryAttribute("code")]
//[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "urn:oasis:names:tc:ebxml-regrep:xsd:query:3.0")]
//[System.Xml.Serialization.XmlRootAttribute(Namespace = "urn:oasis:names:tc:ebxml-regrep:xsd:query:3.0", IsNullable = false)]
//public partial class AdhocQueryRequest
//{

//    private AdhocQueryRequestResponseOption responseOptionField;

//    private AdhocQuery adhocQueryField;

//    /// <remarks/>
//    public AdhocQueryRequestResponseOption ResponseOption
//    {
//        get
//        {
//            return this.responseOptionField;
//        }
//        set
//        {
//            this.responseOptionField = value;
//        }
//    }

//    /// <remarks/>
//    [System.Xml.Serialization.XmlElementAttribute(Namespace = "urn:oasis:names:tc:ebxml-regrep:xsd:rim:3.0")]
//    public AdhocQuery AdhocQuery
//    {
//        get
//        {
//            return this.adhocQueryField;
//        }
//        set
//        {
//            this.adhocQueryField = value;
//        }
//    }
//}

///// <remarks/>
//[System.SerializableAttribute()]
//[System.ComponentModel.DesignerCategoryAttribute("code")]
//[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "urn:oasis:names:tc:ebxml-regrep:xsd:query:3.0")]
//public partial class AdhocQueryRequestResponseOption
//{

//    private string returnTypeField;

//    private bool returnComposedObjectsField;

//    /// <remarks/>
//    [System.Xml.Serialization.XmlAttributeAttribute()]
//    public string returnType
//    {
//        get
//        {
//            return this.returnTypeField;
//        }
//        set
//        {
//            this.returnTypeField = value;
//        }
//    }

//    /// <remarks/>
//    [System.Xml.Serialization.XmlAttributeAttribute()]
//    public bool returnComposedObjects
//    {
//        get
//        {
//            return this.returnComposedObjectsField;
//        }
//        set
//        {
//            this.returnComposedObjectsField = value;
//        }
//    }
//}

///// <remarks/>
//[System.SerializableAttribute()]
//[System.ComponentModel.DesignerCategoryAttribute("code")]
//[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "urn:oasis:names:tc:ebxml-regrep:xsd:rim:3.0")]
//[System.Xml.Serialization.XmlRootAttribute(Namespace = "urn:oasis:names:tc:ebxml-regrep:xsd:rim:3.0", IsNullable = false)]
//public partial class AdhocQuery
//{

//    private AdhocQuerySlot[] slotField;

//    private string idField;

//    /// <remarks/>
//    [System.Xml.Serialization.XmlElementAttribute("Slot")]
//    public AdhocQuerySlot[] Slot
//    {
//        get
//        {
//            return this.slotField;
//        }
//        set
//        {
//            this.slotField = value;
//        }
//    }

//    /// <remarks/>
//    [System.Xml.Serialization.XmlAttributeAttribute()]
//    public string id
//    {
//        get
//        {
//            return this.idField;
//        }
//        set
//        {
//            this.idField = value;
//        }
//    }
//}

///// <remarks/>
//[System.SerializableAttribute()]
//[System.ComponentModel.DesignerCategoryAttribute("code")]
//[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "urn:oasis:names:tc:ebxml-regrep:xsd:rim:3.0")]
//public partial class AdhocQuerySlot
//{

//    private string[] valueListField;

//    private string nameField;

//    /// <remarks/>
//    [System.Xml.Serialization.XmlArrayItemAttribute("Value", IsNullable = false)]
//    public string[] ValueList
//    {
//        get
//        {
//            return this.valueListField;
//        }
//        set
//        {
//            this.valueListField = value;
//        }
//    }

//    /// <remarks/>
//    [System.Xml.Serialization.XmlAttributeAttribute()]
//    public string name
//    {
//        get
//        {
//            return this.nameField;
//        }
//        set
//        {
//            this.nameField = value;
//        }
//    }
//}

