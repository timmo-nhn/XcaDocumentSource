using System.Xml;
using XcaXds.Commons.Commons;
using XcaXds.Commons.Models.Soap;
using XcaXds.Commons.Serializers;

namespace XcaXds.Tests;

public class UnitTests_Soap
{
    [Fact]
    public async Task SerializeSoapMessage()
    {
        var soapMessage = """
            <?xml version="1.0" encoding="UTF-8"?>
            <soapenv:Envelope xmlns:soapenv="http://www.w3.org/2003/05/soap-envelope">
              <soapenv:Header xmlns:wsa="http://www.w3.org/2005/08/addressing">
                <wsa:ReplyTo soapenv:mustUnderstand="1">
                  <wsa:Address>http://www.w3.org/2005/08/addressing/anonymous</wsa:Address>
                </wsa:ReplyTo>
                <wsa:To>https://frank-sykehus-t1.t-xcads.pjd.nhn.no/XCA/services/RespondingGatewayService</wsa:To>
                <wsa:MessageID>urn:uuid:0ab6fcb4-05b0-4506-99f8-820a0e1aee9b</wsa:MessageID>
                <wsa:Action>urn:ihe:iti:2007:CrossGatewayQuery</wsa:Action>
                <wsse:Security xmlns:wsse="http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd">
                  <saml:Assertion xmlns:saml="urn:oasis:names:tc:SAML:2.0:assertion" ID="_678328d4-a1c8-438f-bbcf-01c7cca0becc" IssueInstant="2025-08-08T12:36:54.561Z" Version="2.0">
                    <saml:Issuer>https://helseid-xdssaml.test.nhn.no</saml:Issuer>
                    <Signature xmlns="http://www.w3.org/2000/09/xmldsig#">
                      <SignedInfo>
                        <CanonicalizationMethod Algorithm="http://www.w3.org/2001/10/xml-exc-c14n#"/>
                        <SignatureMethod Algorithm="http://www.w3.org/2001/04/xmldsig-more#rsa-sha256"/>
                        <Reference URI="#_678328d4-a1c8-438f-bbcf-01c7cca0becc">
                          <Transforms>
                            <Transform Algorithm="http://www.w3.org/2000/09/xmldsig#enveloped-signature"/>
                            <Transform Algorithm="http://www.w3.org/2001/10/xml-exc-c14n#"/>
                          </Transforms>
                          <DigestMethod Algorithm="http://www.w3.org/2001/04/xmlenc#sha256"/>
                          <DigestValue>ES4B+YGED9IvWxFaeMaS7W1Qu/kbEkbG+Jbzq5z3m8w=</DigestValue>
                        </Reference>
                      </SignedInfo>
                      <SignatureValue>Os7kGX1v5lqL319Edw0/K8X/CtISajOMBLQdJ3VXDyGaPI3lY+CQRL5ww2TxOqy2ysl2BzE2rtm5ezUUpWGre89MnWDunjqmAISNs4bwXWaEikqgtp9PoKX2T1XnEr/KKtAT2JA8N840Gqo7eCO1So32kJn2O5uLwDMCwjHya9OE5JFRet5AC7qptWUTt1pONQf57mAb5KRzVRTs9fz4SzJDe62yj2G9Zq1uMnwYOYKUJXj0tXTo5CDIjMFg+/FPCDEtg2iIneXbKXzbIoX0Nl4HL0kMbAboDw5+NBt07Jp/nnEdi+PPmKpMlgL39ZsAazWLfWBv0PxgYFXdp11xYV7lbU0bAf5DNNcPQRtLOrsN+jDvf4QBGVs9Bi8LHkHCm9KhRSZESANAIi1P87CDmfHNfOfW97ogY+WqoifbS4npjW41/9tOoSaITWbynAhCC7PdYX/7O3ECTpmk5B5pjdq83k/8uaZY/ci+U0n61pRXtlLHGkazQasVMOBmhkhZ</SignatureValue>
                      <KeyInfo>
                        <X509Data>
                          <X509Certificate>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</X509Certificate>
                        </X509Data>
                      </KeyInfo>
                    </Signature>
                    <saml:Subject>
                      <saml:NameID>05898597468</saml:NameID>
                      <saml:SubjectConfirmation Method="urn:oasis:names:tc:SAML:2.0:cm:bearer"/>
                    </saml:Subject>
                    <saml:Conditions NotBefore="2025-08-08T12:36:54.561Z" NotOnOrAfter="2025-08-08T13:36:54.561Z">
                      <saml:AudienceRestriction>
                        <saml:Audience>nhn:dokumentdeling-saml</saml:Audience>
                      </saml:AudienceRestriction>
                    </saml:Conditions>
                    <saml:AttributeStatement>
                      <saml:Attribute Name="urn:oasis:names:tc:xspa:1.0:subject:subject-id">
                        <saml:AttributeValue>GRØNN VITS</saml:AttributeValue>
                      </saml:Attribute>
                      <saml:Attribute Name="urn:oasis:names:tc:xspa:1.0:subject:role">
                        <saml:AttributeValue>&lt;Role xmlns="urn:hl7-org:v3" xsi:type="CE" code="LE" codeSystem="urn:oid:2.16.578.1.12.4.1.1.9060" codeSystemName="Kategori helsepersonell" displayName="Lege"/></saml:AttributeValue>
                      </saml:Attribute>
                      <saml:Attribute Name="urn:no:ehelse:saml:1.0:subject:homeCommunityId">
                        <saml:AttributeValue>2.16.578.1.12.4.1.7.1.1</saml:AttributeValue>
                      </saml:Attribute>
                      <saml:Attribute Name="urn:oasis:names:tc:xspa:2.0:subject:npi">
                        <saml:AttributeValue>565501872</saml:AttributeValue>
                      </saml:Attribute>
                      <saml:Attribute Name="urn:oasis:names:tc:xspa:1.0:subject:purposeOfUse">
                        <saml:AttributeValue>&lt;PurposeOfUse xmlns="urn:hl7-org:v3" xsi:type="CE" code="TREAT" codeSystem="urn:oid:2.16.840.1.113883.1.11.20448" codeSystemName="PurposeOfUse (HL7)" displayName="Treatment"/></saml:AttributeValue>
                      </saml:Attribute>
                      <saml:Attribute Name="urn:oasis:names:tc:xacml:2.0:resource:resource-id">
                        <saml:AttributeValue>12119000465^^^&amp;amp;2.16.578.1.12.4.1.4.1&amp;amp;ISO</saml:AttributeValue>
                      </saml:Attribute>
                      <saml:Attribute Name="urn:no:ehelse:saml:1.0:subject:SecurityLevel">
                        <saml:AttributeValue>4</saml:AttributeValue>
                      </saml:Attribute>
                      <saml:Attribute Name="urn:no:ehelse:saml:1.0:subject:Scope">
                        <saml:AttributeValue>journaldokumenter_helsepersonell</saml:AttributeValue>
                      </saml:Attribute>
                      <saml:Attribute Name="urn:no:ehelse:saml:1.0:subject:Authentication_method">
                        <saml:AttributeValue a:nil="true"
                          xmlns:a="http://www.w3.org/2001/XMLSchema-instance"/>
                      </saml:Attribute>
                      <saml:Attribute Name="urn:oasis:names:tc:xacml:1.0:subject:subject-id">
                        <saml:AttributeValue>GRØNN VITS</saml:AttributeValue>
                      </saml:Attribute>
                      <saml:Attribute Name="urn:oasis:names:tc:xspa:1.0:subject:organization">
                        <saml:AttributeValue>STIFTELSEN BETANIEN HOSPITAL SKIEN</saml:AttributeValue>
                      </saml:Attribute>
                      <saml:Attribute Name="urn:oasis:names:tc:xspa:1.0:subject:organization-id">
                        <saml:AttributeValue>&lt;id xmlns="urn:hl7-org:v3" xsi:type="II" extension="981275721" root="urn:oid:2.16.578.1.12.4.1.4.101" assigningAuthorityName="https://www.brreg.no/" displayable="true"/></saml:AttributeValue>
                      </saml:Attribute>
                      <saml:Attribute Name="urn:oasis:names:tc:xspa:1.0:subject:child-organization">
                        <saml:AttributeValue>&lt;id xmlns="urn:hl7-org:v3" xsi:type="II" extension="873255102" root="urn:oid:2.16.578.1.12.4.1.4.101" assigningAuthorityName="https://www.brreg.no/" displayable="true"/></saml:AttributeValue>
                      </saml:Attribute>
                      <saml:Attribute Name="urn:oasis:names:tc:xacml:2.0:subject:role">
                        <saml:AttributeValue>&lt;Role xmlns="urn:hl7-org:v3" xsi:type="CE" code="LE" codeSystem="urn:oid:2.16.578.1.12.4.1.1.9060" codeSystemName="Kategori helsepersonell" displayName="Lege"/></saml:AttributeValue>
                      </saml:Attribute>
                      <saml:Attribute Name="urn:ihe:iti:xca:2010:homeCommunityId">
                        <saml:AttributeValue>2.16.578.1.12.4.1.7.1.1</saml:AttributeValue>
                      </saml:Attribute>
                      <saml:Attribute Name="urn:oasis:names:tc:xspa:1.0:subject:npi">
                        <saml:AttributeValue>565501872</saml:AttributeValue>
                      </saml:Attribute>
                      <saml:Attribute Name="urn:ihe:iti:xua:2017:subject:provider-identifier">
                        <saml:AttributeValue>&lt;id xmlns="urn:hl7-org:v3" type="II" extension="565501872" root="2.16.578.1.12.4.1.4.4" displayable="false"/></saml:AttributeValue>
                      </saml:Attribute>
                      <saml:Attribute Name="urn:oasis:names:tc:xacml:1.0:resource:resource-id">
                        <saml:AttributeValue>12119000465^^^&amp;amp;2.16.578.1.12.4.1.4.1&amp;amp;ISO</saml:AttributeValue>
                      </saml:Attribute>
                      <saml:Attribute Name="urn:no:ehelse:saml:1.0:subject:client_id">
                        <saml:AttributeValue>ea1b0c25-154d-483c-abeb-cdf2e09d0a45</saml:AttributeValue>
                      </saml:Attribute>
                      <saml:Attribute Name="urn:nhn:trust-framework:1.0:ext:subject:child-organization-name">
                        <saml:AttributeValue>BETANIEN HOSPITAL</saml:AttributeValue>
                      </saml:Attribute>
                      <saml:Attribute Name="urn:nhn:trust-framework:1.0:ext:resource:child-organization-name">
                        <saml:AttributeValue>BETANIEN HOSPITAL</saml:AttributeValue>
                      </saml:Attribute>
                      <saml:Attribute Name="urn:nhn:trust-framework:1.0:ext:resource:child-organization">
                        <saml:AttributeValue>&lt;id xmlns="urn:hl7-org:v3" xsi:type="II" extension="873255102" root="urn:oid:2.16.578.1.12.4.1.4.101" assigningAuthorityName="https://www.brreg.no/" displayable="true"/></saml:AttributeValue>
                      </saml:Attribute>
                      <saml:Attribute Name="urn:oasis:names:tc:xacml:2.0:action:purpose">
                        <saml:AttributeValue>&lt;Purpose xmlns="urn:hl7-org:v3" xsi:type="CE" code="TREAT" codeSystem="urn:oid:2.16.840.1.113883.1.11.20448" displayName="Treatment"/></saml:AttributeValue>
                      </saml:Attribute>
                      <saml:Attribute Name="urn:nhn:trust-framework:1.0:ext:care-relationship:healthcare-service">
                        <saml:AttributeValue>&lt;HealthcareService xmlns="urn:hl7-org:v3" xsi:type="CE" code="01" codeSystem="urn:oid:2.16.578.1.12.4.1.1.8666" displayName="Bedriftshelsetjeneste" assigningAuthorityName="https://volven.no/"/></saml:AttributeValue>
                      </saml:Attribute>
                      <saml:Attribute Name="urn:nhn:trust-framework:1.0:ext:care-relationship:purpose-of-use-details">
                        <saml:AttributeValue>&lt;purpose-of-use-details xmlns="urn:hl7-org:v3" xsi:type="CE" code="BEHANDLER" codeSystem="urn:AuditEventHL7Norway/CodeSystem/carerelation" displayName="Bruker har behandlingsansvar for pasienten" assigningAuthorityName="https://www.hl7.no/"/></saml:AttributeValue>
                      </saml:Attribute>
                      <saml:Attribute Name="urn:nhn:trust-framework:1.0:ext:care-relationship:decision-ref">
                        <saml:AttributeValue>&lt;decision-ref>&lt;id value="51c1b711-ce1c-4953-97b7-6945633c4cb8" />&lt;user-selected value="True" />&lt;/decision-ref></saml:AttributeValue>
                      </saml:Attribute>
                      <saml:Attribute Name="urn:ihe:iti:xua:2012:acp">
                        <saml:AttributeValue a:nil="true"
                          xmlns:a="http://www.w3.org/2001/XMLSchema-instance"/>
                      </saml:Attribute>
                      <saml:Attribute Name="urn:ihe:iti:bppc:2007:docid">
                        <saml:AttributeValue a:nil="true"
                          xmlns:a="http://www.w3.org/2001/XMLSchema-instance"/>
                      </saml:Attribute>
                    </saml:AttributeStatement>
                    <saml:AuthnStatement AuthnInstant="2025-08-08T12:33:04.000Z" SessionNotOnOrAfter="2025-08-08T13:36:54.000Z">
                      <saml:AuthnContext>
                        <saml:AuthnContextClassRef>urn:oasis:names:tc:SAML:2.0:ac:classes:X509</saml:AuthnContextClassRef>
                      </saml:AuthnContext>
                    </saml:AuthnStatement>
                  </saml:Assertion>
                  <u:Timestamp xmlns:u="http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd">
                    <u:Created>2025-08-08T12:36:54Z</u:Created>
                    <u:Expires>2025-08-08T12:51:54Z</u:Expires>
                  </u:Timestamp>
                </wsse:Security>
                <sense:context xmlns:sense="http://sense.ith-icoserve.com/context/"
                  xmlns:wsctx="http://docs.oasis-open.org/ws-caf/2005/10/wsctx">
                  <wsctx:context-identifier>urn:uuid:f9af742c-8cd7-442f-95c0-5da8e92c0dff</wsctx:context-identifier>
                  <wsctx:context-service>
                    <sense:service>XCA</sense:service>
                  </wsctx:context-service>
                </sense:context>
              </soapenv:Header>
              <soapenv:Body>
                <ns5:AdhocQueryRequest xmlns:ns5="urn:oasis:names:tc:ebxml-regrep:xsd:query:3.0"
                  xmlns:ns8="urn:ihe:iti:rmd:2017"
                  xmlns:ns7="urn:oasis:names:tc:ebxml-regrep:xsd:lcm:3.0"
                  xmlns:ns6="urn:ihe:rad:xdsi-b:2009"
                  xmlns:ns4="urn:oasis:names:tc:ebxml-regrep:xsd:rs:3.0"
                  xmlns:ns3="urn:ihe:iti:xds-b:2007"
                  xmlns:ns2="urn:oasis:names:tc:ebxml-regrep:xsd:rim:3.0">
                  <ns5:ResponseOption returnType="LeafClass" returnComposedObjects="true"/>
                  <ns2:AdhocQuery id="urn:uuid:14d4debf-8f97-4251-9a74-a90016b0af0d" home="urn:oid:2.16.578.1.12.4.5.100.1">
                    <ns2:Slot name="$XDSDocumentEntryStatus">
                      <ns2:ValueList>
                        <ns2:Value>('urn:oasis:names:tc:ebxml-regrep:StatusType:Approved')</ns2:Value>
                      </ns2:ValueList>
                    </ns2:Slot>
                    <ns2:Slot name="$XDSDocumentEntryConfidentialityCode">
                      <ns2:ValueList>
                        <ns2:Value>('L^^2.16.840.1.113883.5.25')</ns2:Value>
                        <ns2:Value>('M^^2.16.840.1.113883.5.25')</ns2:Value>
                        <ns2:Value>('N^^2.16.840.1.113883.5.25')</ns2:Value>
                        <ns2:Value>('U^^2.16.840.1.113883.5.25')</ns2:Value>
                        <ns2:Value>('N^^2.16.578.1.12.4.1.1.9603')</ns2:Value>
                      </ns2:ValueList>
                    </ns2:Slot>
                    <ns2:Slot name="$XDSDocumentEntryPatientId">
                      <ns2:ValueList>
                        <ns2:Value>'12119000465^^^&amp;2.16.578.1.12.4.1.4.1&amp;ISO'</ns2:Value>
                      </ns2:ValueList>
                    </ns2:Slot>
                  </ns2:AdhocQuery>
                </ns5:AdhocQueryRequest>
              </soapenv:Body>
            </soapenv:Envelope>
            """;

        var sxmls = new SoapXmlSerializer(Constants.XmlDefaultOptions.DefaultXmlWriterSettings);

        var soapEnvelope = sxmls.DeserializeSoapMessage<SoapEnvelope>(soapMessage);

        var xmlDocument = new XmlDocument();

        xmlDocument.LoadXml(soapMessage);

    }
}