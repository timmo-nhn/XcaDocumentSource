namespace XcaXds.Tests;
using XcaXds.Commons.Models;
using XcaXds.Commons.Models.ClinicalDocument;
using XcaXds.Commons.Services;

public class UnitTest1
{
    [Fact]
    public async Task Test1()
    {
        string cdaTestString = """
<ClinicalDocument xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns="urn:hl7-org:v3"
  xmlns:cda="urn:hl7-org:v3" xmlns:sdtc="urn:hl7-org:sdtc">
    <realmCode code="US" /> "�� ��"
  <typeId root="2.16.840.1.113883.1.3" extension="POCD_HD0 00040" />
    <templateId root="2.16.840.1.113883.10.20.22.1.1" extension="2014-06-09" />
    <templateId root="2.16.840.1.113883.10.20.22.1.13" />
  <id root="04fc2b90-10e0-11e2-892e-0800200c9a66" />
  <code codeSystem="2.16.840.1.113883.6.1" codeSystemName="LO IN" code="18761-7"
    displayName="Transfer summary note" />
  <title>Transfer Summary</title>
  <effectiveTime value="201309210500-0800" />
  <confidentialityCode code="N" codeSystem="2.16.840.1.113883.5.25" displayName="normal" />
  <languageCode code="eng" />
  <setId extension="sTT988" root="2.16.840.1.113883.19.5.99999.19" />
  <versionNumber value="1" />
  <recordTarget>
    <patientRole>
      <id extension="444222222" root="2.16.840.1.113883.4.1" />
            <addr use="HP">
                <streetAddressLine>2222 Home Street</streetAddressLine>
        <city>Beaverton</city>
        <state>OR</state>
        <postalCode>97867</postalCode>
        <country>US</country>
              </addr>
      <telecom value="tel:+1(555)555-2003" use="HP" />
            <patient>
                <name use="L">
          <given>Eve</given>
                    <family qualifier="SP">Betterhalf</family>
        </name>
                <name use="SRCH">
          <given>Eve</given>
                    <family qualifier="BR">Everywoman</family>
        </name>
        <administrativeGenderCode code="F" displayName="Female" codeSystem="2.16.840.1.113883.5.1"
          codeSystemName="AdministrativeGender" />
                <birthTime value="19450501" />
        <maritalStatusCode code="M" displayName="Married" codeSystem="2.16.840.1.113883.5.2"
          codeSystemName="MaritalStatusCode" />
        <religiousAffiliationCode code="1013" displayName="Christian (non-Catholic, non-specific)"
          codeSystem="2.16.840.1.113883.5.1076" codeSystemName="HL7 Religious Affiliation" />
                <raceCode code="2106-3" displayName="White" codeSystem="2.16.840.1.113883.6.238"
          codeSystemName="Race &amp; Ethnicity - CDC" />
                <sdtc:raceCode code="2076-8" displayName="Native Hawaiian or Other Pacific Islander"
          codeSystem="2.16.840.1.113883.6.238" codeSystemName="Race &amp; Ethnicity - CDC" />
        <ethnicGroupCode code="2186-5" displayName="Not Hispanic or Latino" codeSystem="2.16.840.1.113883.6.238"
          codeSystemName="Race &amp; Ethnicity - CDC" />
        <guardian>
          <code code="POWATT" displayName="Power of Attorney" codeSystem="2.16.840.1.113883.1.11.19830"
            codeSystemName="ResponsibleParty" />
          <addr use="HP">
            <streetAddressLine>2222 Home Street</streetAddressLine>
            <city>Beaverton</city>
            <state>OR</state>
            <postalCode>97867</postalCode>
            <country>US</country>
          </addr>
          <telecom value="tel:+1(555)555-2008" use="MC" />
          <guardianPerson>
            <name>
              <given>Boris</given>
              <given qualifier="CL">Bo</given>
              <family>Betterhalf</family>
            </name>
          </guardianPerson>
        </guardian>
        <birthplace>
          <place>
            <addr>
              <streetAddressLine>4444 Home Street</streetAddressLine>
              <city>Beaverton</city>
              <state>OR</state>
              <postalCode>97867</postalCode>
              <country>US</country>
            </addr>
          </place>
        </birthplace>
        <languageCommunication>
          <languageCode code="eng" />
                    <modeCode code="ESP" displayName="Expressed spoken" codeSystem="2.16.840.1.113883.5.60"
            codeSystemName="LanguageAbilityMode" />
          <proficiencyLevelCode code="G" displayName="Good" codeSystem="2.16.840.1.113883.5.61"
            codeSystemName="LanguageAbilityProficiency" />
                    <preferenceInd value="true" />
        </languageCommunication>
      </patient>
      <providerOrganization>
        <id extension="219BX" root="2.16.840.1.113883.4.6" />
        <name>The DoctorsTogether Physician Group</name>
        <telecom use="WP" value="tel: +1(555)555-5000" />
        <addr>
          <streetAddressLine>1007 Health Drive</streetAddressLine>
          <city>Portland</city>
          <state>OR</state>
          <postalCode>99123</postalCode>
          <country>US</country>
        </addr>
      </providerOrganization>
    </patientRole>
  </recordTarget>
    <author typeCode="AUT">
    <templateId root="2.16.840.1.113883.10.20.22.4.119" />
    <time value="20130730" />
    <assignedAuthor>
      <id extension="5555555555" root="2.16.840.1.113883.4.6" />
      <code code="207QA0505X" displayName="Allopathic &amp; Osteopathic Physicians; Family Medicine, Adult Medicine" codeSystem="2.16.840.1.113883.6.101"
        codeSystemName="Healthcare Provider Taxonomy (HIPAA)" />
      <addr>
        <streetAddressLine>1004 Healthcare Drive </streetAddressLine>
        <city>Portland</city>
        <state>OR</state>
        <postalCode>99123</postalCode>
        <country>US</country>
      </addr>
      <telecom use="WP" value="tel:+1(555)555-1004" />
      <assignedPerson>
        <name>
          <given>Patricia</given>
          <given qualifier="CL">Patty</given>
          <family>Primary</family>
          <suffix qualifier="AC">M.D.</suffix>
        </name>
      </assignedPerson>
    </assignedAuthor>
  </author>
    <dataEnterer>
    <assignedEntity>
      <id extension="333777777" root="2.16.840.1.113883.4.6" />
      <addr>
        <streetAddressLine>1007 Healthcare Drive</streetAddressLine>
        <city>Portland</city>
        <state>OR</state>
        <postalCode>99123</postalCode>
        <country>US</country>
      </addr>
      <telecom use="WP" value="tel:+1(555)555-1050" />
      <assignedPerson>
        <name>
          <given>Ellen</given>
          <family>Enter</family>
        </name>
      </assignedPerson>
    </assignedEntity>
  </dataEnterer>
    <informant>
    <assignedEntity>
      <id extension="888888888" root="2.16.840.1.113883.19.5" />
      <addr>
        <streetAddressLine>1007 Healthcare Drive</streetAddressLine>
        <city>Portland</city>
        <state>OR</state>
        <postalCode>99123</postalCode>
        <country>US</country>
      </addr>
      <telecom use="WP" value="tel:+1(555)555-1003" />
      <assignedPerson>
        <name>
          <given>Harold</given>
          <family>Hippocrates</family>
          <suffix qualifier="AC">D.O.</suffix>
        </name>
      </assignedPerson>
    </assignedEntity>
  </informant>
  <informant>
    <relatedEntity classCode="PRS">
            <code code="SPS" displayName="SPOUSE" codeSystem="2.16.840.1.113883.1.11.19563"
        codeSystemName="Personal Relationship Role Type Value Set" />
      <relatedPerson>
        <name>
          <given>Rose</given>
          <family>Everyman</family>
        </name>
      </relatedPerson>
    </relatedEntity>
  </informant>
    <custodian>
    <assignedCustodian>
      <representedCustodianOrganization>
        <id extension="321CX" root="2.16.840.1.113883.4.6" />
        <name>Good Health HIE</name>
        <telecom use="WP" value="tel:+1(555)555-1009" />
        <addr use="WP">
          <streetAddressLine>1009 Healthcare Drive </streetAddressLine>
          <city>Portland</city>
          <state>OR</state>
          <postalCode>99123</postalCode>
          <country>US</country>
        </addr>
      </representedCustodianOrganization>
    </assignedCustodian>
  </custodian>
    <informationRecipient>
    <intendedRecipient>
      <informationRecipient>
        <name>
          <given>Sara</given>
          <family>Specialize</family>
          <suffix qualifier="AC">M.D.</suffix>
        </name>
      </informationRecipient>
      <receivedOrganization>
        <name>The DoctorsApart Physician Group</name>
      </receivedOrganization>
    </intendedRecipient>
  </informationRecipient>
    <legalAuthenticator>
    <time value="20130915223615-0800" />
    <signatureCode code="S" />
    <assignedEntity>
      <id extension="5555555555" root="2.16.840.1.113883.4.6" />
      <code code="207QA0505X" displayName="Allopathic &amp; Osteopathic Physicians; Family Medicine, Adult Medicine" codeSystem="2.16.840.1.113883.6.101"
        codeSystemName="Healthcare Provider Taxonomy (HIPAA)" />
      <addr>
        <streetAddressLine>1004 Healthcare Drive </streetAddressLine>
        <city>Portland</city>
        <state>OR</state>
        <postalCode>99123</postalCode>
        <country>US</country>
      </addr>
      <telecom use="WP" value="tel:+1(555)555-1004" />
      <assignedPerson>
        <name>
          <given>Patricia</given>
          <given qualifier="CL">Patty</given>
          <family>Primary</family>
          <suffix qualifier="AC">M.D.</suffix>
        </name>
      </assignedPerson>
    </assignedEntity>
  </legalAuthenticator>
    <authenticator>
    <time value="201209151030-0800" />
    <signatureCode code="S" />
    <assignedEntity>
      <id extension="5555555555" root="2.16.840.1.113883.4.6" />
      <code code="207QA0505X" displayName="Allopathic &amp; Osteopathic Physicians; Family Medicine, Adult Medicine" codeSystem="2.16.840.1.113883.6.101"
        codeSystemName="Healthcare Provider Taxonomy (HIPAA)" />
      <addr>
        <streetAddressLine>1004 Healthcare Drive </streetAddressLine>
        <city>Portland</city>
        <state>OR</state>
        <postalCode>99123</postalCode>
        <country>US</country>
      </addr>
      <telecom use="WP" value="tel:+1(555)555-1004" />
      <assignedPerson>
        <name>
          <given>Patricia</given>
          <given qualifier="CL">Patty</given>
          <family>Primary</family>
          <suffix qualifier="AC">M.D.</suffix>
        </name>
      </assignedPerson>
    </assignedEntity>
  </authenticator>
    <participant typeCode="CALLBCK">
    <time value="20050329224411-0500" />
    <associatedEntity classCode="ASSIGNED">
      <id extension="99999999" root="2.16.840.1.113883.4.6" />
      <code code="200000000X" codeSystem="2.16.840.1.113883.6.101" displayName="Allopathic &amp; Osteopathic Physicians" />
      <addr>
        <streetAddressLine>1002 Healthcare Drive </streetAddressLine>
        <city>Ann Arbor</city>
        <state>MI</state>
        <postalCode>97857</postalCode>
        <country>US</country>
      </addr>
      <telecom use="WP" value="tel:555-555-1002" />
      <associatedPerson>
        <name>
          <given>Henry</given>
          <family>Seven</family>
          <suffix>DO</suffix>
        </name>
      </associatedPerson>
    </associatedEntity>
  </participant>
    <participant typeCode="IND">
    <functionCode code="407543004" displayName="Primary caregiver" codeSystem="2.16.840.1.113883.6.96"
      codeSystemName="SNOMED-CT" />
        <associatedEntity classCode="CAREGIVER">
      <code code="MTH" codeSystem="2.16.840.1.113883.5.111" displayName="mother" />
      <addr>
        <streetAddressLine>17 Daws Rd.</streetAddressLine>
        <city>Ann Arbor</city>
        <state>MI</state>
        <postalCode>97857</postalCode>
        <country>US</country>
      </addr>
      <telecom value="tel: 1+(555)555-1212" use="WP" />
      <associatedPerson>
        <name>
          <prefix>Mrs.</prefix>
          <given>Martha</given>
          <family>Jones</family>
        </name>
      </associatedPerson>
    </associatedEntity>
  </participant>
  <documentationOf typeCode="DOC">
    <serviceEvent classCode="PCPR">
      <effectiveTime>
        <low value="20130601" />
        <high value="20130815" />
      </effectiveTime>
      <performer typeCode="PRF">
        <functionCode code="PCP" codeSystem="2.16.840.1.113883.5.88" codeSystemName="ParticipationFunction"
          displayName="primary care physician">
          <originalText>Primary Care Provider</originalText>
        </functionCode>
        <assignedEntity>
          <id extension="5555555555" root="2.16.840.1.113883.4.6" />
          <code code="207QA0505X" displayName="Allopathic &amp; Osteopathic Physicians; Family Medicine, Adult Medicine" codeSystem="2.16.840.1.113883.6.101"
            codeSystemName="Healthcare Provider Taxonomy (HIPAA)" />
          <addr>
            <streetAddressLine>1004 Healthcare Drive </streetAddressLine>
            <city>Portland</city>
            <state>OR</state>
            <postalCode>99123</postalCode>
            <country>US</country>
          </addr>
          <telecom use="WP" value="tel:+1(555)555-1004" />
          <assignedPerson>
            <name>
              <given>Patricia</given>
              <given qualifier="CL">Patty</given>
              <family>Primary</family>
              <suffix qualifier="AC">M.D.</suffix>
            </name>
          </assignedPerson>
          <representedOrganization>
            <id extension="219BX" root="1.1.1.1.1.1.1.1.2" />
            <name>Good Health Hospital</name>
            <telecom use="WP" value="tel:+1(555)555-5000" />
            <addr>
              <streetAddressLine>1004 Health Drive</streetAddressLine>
              <city>Portland</city>
              <state>OR</state>
              <postalCode>99123</postalCode>
              <country>US</country>
            </addr>
          </representedOrganization>
        </assignedEntity>
      </performer>
      <performer typeCode="PRF">
        <assignedEntity>
          <id extension="5555555555" root="2.16.840.1.113883.4.6" />
          <code code="207RN0300X" displayName="Allopathic &amp; Osteopathic Physicians; Internal Medicine, Nephrology" codeSystem="2.16.840.1.113883.6.101"
            codeSystemName="Healthcare Provider Taxonomy (HIPAA)" />
          <addr>
            <streetAddressLine>1004 Healthcare Drive</streetAddressLine>
            <city>Ann Arbor</city>
            <state>MA</state>
            <postalCode>99123</postalCode>
            <country>US</country>
          </addr>
          <telecom use="WP" value="tel:+1(555)555-1038" />
          <assignedPerson>
            <name>
              <given>Rory</given>
              <given qualifier="CL">Renal</given>
              <family>Primary</family>
              <suffix qualifier="AC">M.D.</suffix>
            </name>
          </assignedPerson>
          <representedOrganization>
            <id extension="219BX" root="1.1.1.1.1.1.1.1.2" />
            <name>Good Health Hospital</name>
            <telecom use="WP" value="tel: +1(555)555-1039" />
            <addr>
              <streetAddressLine>1036 Health Drive</streetAddressLine>
              <city>>Ann Arbor</city>
              <state>MA</state>
              <postalCode>99123</postalCode>
              <country>US</country>
            </addr>
          </representedOrganization>
        </assignedEntity>
      </performer>
    </serviceEvent>
  </documentationOf>
    <component>
    <structuredBody>
            <component>
        <section>
                    <templateId root="2.16.840.1.113883.10.20.22.2.6.1" extension="2014-06-09" />
          <code code="48765-2" codeSystem="2.16.840.1.113883.6.1" />
          <title>ALLERGIES AND ADVERSE REACTIONS</title>
          <text>
            <table border="1" width="100%">
              <thead>
                <tr>
                  <th>Substance</th>
                  <th>Reaction</th>
                </tr>
              </thead>
              <tbody>
                <tr>
                  <td>
                    <content ID="substance1">Penicillin</content>
                  </td>
                  <td>
                    <content ID="reaction1">Nausea</content>
                  </td>
                </tr>
                <tr>
                  <td>
                    <content ID="substance2">Codeine</content>
                  </td>
                  <td>
                    <content ID="reaction2">Wheezing</content>
                  </td>
                </tr>
              </tbody>
            </table>
          </text>
          <entry typeCode="DRIV">
            <act classCode="ACT" moodCode="EVN">
                            <templateId root="2.16.840.1.113883.10.20.22.4.30" extension="2014-06-09" />
              <id root="36e3e930-7b14-11db-9fe1-0800200c9a66" />
              <code code="CONC" codeSystem="2.16.840.1.113883.5.6" />
                                          <statusCode code="active" />
              <effectiveTime>
                                                <low value="199805011145-0800" />
              </effectiveTime>
              <author typeCode="AUT">
                <templateId root="2.16.840.1.113883.10.20.22.4.119" />
                                <time value="199805011145-0800" />
                <assignedAuthor>
                  <id extension="555555555" root="2.16.840.1.113883.4.6" />
                  <code code="207QA0505X" displayName="Allopathic &amp; Osteopathic Physicians; Family Medicine, Adult Medicine" codeSystem="2.16.840.1.113883.6.101"
                    codeSystemName="Healthcare Provider Taxonomy (HIPAA)" />
                </assignedAuthor>
              </author>
              <entryRelationship typeCode="SUBJ">
                <observation classCode="OBS" moodCode="EVN">
                                    <templateId root="2.16.840.1.113883.10.20.22.4.7" extension="2014-06-09" />
                  <id root="4adc1020-7b14-11db-9fe1-0800200c9a66" />
                  <code code="ASSERTION" codeSystem="2.16.840.1.113883.5.4" />
                  <text>
                    <reference value="#allergytype1" />
                  </text>
                                    <statusCode code="completed" />
                  <effectiveTime>
                                                            <low value="19980501" />
                                      </effectiveTime>
                  <value xsi:type="CD" code="419199007" displayName="Allergy to substance"
                    codeSystem="2.16.840.1.113883.6.96" codeSystemName="SNOMED CT" />
                  <author typeCode="AUT">
                    <templateId root="2.16.840.1.113883.10.20.22.4.119" />
                    <time value="199805011145-0800" />
                    <assignedAuthor>
                      <id extension="222223333" root="2.16.840.1.113883.4.6" />
                      <code code="207KA0200X" displayName="Allopathic &amp; Osteopathic Physicians; Allergy &amp; Immunology, Allergy" codeSystem="2.16.840.1.113883.6.101"
                        codeSystemName="Healthcare Provider Taxonomy (HIPAA)" />
                    </assignedAuthor>
                  </author>
                  <participant typeCode="CSM">
                    <participantRole classCode="MANU">
                      <playingEntity classCode="MMAT">
                        <code code="70618" displayName="Penicillin" codeSystem="2.16.840.1.113883.6.88"
                          codeSystemName="RxNorm" />
                      </playingEntity>
                    </participantRole>
                  </participant>
                  <entryRelationship typeCode="MFST" inversionInd="true">
                    <observation classCode="OBS" moodCode="EVN">
                                            <templateId root="2.16.840.1.113883.10.20.22.4.9" extension="2014-06-09" />
                      <id root="4adc1020-7b14-11db-9fe1-0800200c9a64" />
                      <code code="ASSERTION" codeSystem="2.16.840.1.113883.5.4" />
                      <text>
                        <reference value="#reaction1" />
                      </text>
                      <statusCode code="completed" />
                      <effectiveTime>
                        <low value="200802260805-0800" />
                        <high value="200802281205-0800" />
                      </effectiveTime>
                      <value xsi:type="CD" code="422587007" codeSystem="2.16.840.1.113883.6.96" displayName="Nausea" />
                      <entryRelationship typeCode="SUBJ" inversionInd="true">
                        <observation classCode="OBS" moodCode="EVN">
                                                                              <templateId root="2.16.840.1.113883.10.20.22.4.8" extension="2014-06-09" />
                          <code code="SEV" displayName="Severity Observation" codeSystem="2.16.840.1.113883.5.4"
                            codeSystemName="ActCode" />
                          <statusCode code="completed" />
                          <value xsi:type="CD" code="255604002" displayName="Mild" codeSystem="2.16.840.1.113883.6.96"
                            codeSystemName="SNOMED CT" />
                        </observation>
                      </entryRelationship>
                    </observation>
                  </entryRelationship>
                </observation>
              </entryRelationship>
              <entryRelationship typeCode="SUBJ" inversionInd="true">
                <observation classCode="OBS" moodCode="EVN">
                                                      <templateId root="2.16.840.1.113883.10.20.22.4.8" extension="2014-06-09" />
                  <code code="SEV" displayName="Severity Observation" codeSystem="2.16.840.1.113883.5.4"
                    codeSystemName="ActCode" />
                  <text>
                    <reference value="#allergyseverity1" />
                  </text>
                  <statusCode code="completed" />
                  <value xsi:type="CD" code="371924009" displayName="Moderate to severe"
                    codeSystem="2.16.840.1.113883.6.96" codeSystemName="SNOMED CT" />
                </observation>
              </entryRelationship>
            </act>
          </entry>
          <entry typeCode="DRIV">
            <act classCode="ACT" moodCode="EVN">
                            <templateId root="2.16.840.1.113883.10.20.22.4.30" extension="2014-06-09" />
              <id root="b03805bd-2eb6-4ab8-a9ff-473c6653971a" />
              <code code="CONC" codeSystem="2.16.840.1.113883.5.6" />
                                          <statusCode code="active" />
              <effectiveTime>
                                                <low value="199805011145-0800" />
              </effectiveTime>
              <author typeCode="AUT">
                <templateId root="2.16.840.1.113883.10.20.22.4.119" />
                                <time value="199805011145-0800" />
                <assignedAuthor>
                  <id extension="555555555" root="2.16.840.1.113883.4.6" />
                  <code code="207QA0505X" displayName="Allopathic &amp; Osteopathic Physicians; Family Medicine, Adult Medicine" codeSystem="2.16.840.1.113883.6.101"
                    codeSystemName="Healthcare Provider Taxonomy (HIPAA)" />
                </assignedAuthor>
              </author>
              <entryRelationship typeCode="SUBJ">
                <observation classCode="OBS" moodCode="EVN">
                                    <templateId root="2.16.840.1.113883.10.20.22.4.7" extension="2014-06-09" />
                  <id root="901db0f8-9355-4794-81cd-fd951ef07917" />
                  <code code="ASSERTION" codeSystem="2.16.840.1.113883.5.4" />
                  <text>
                    <reference value="#allergytype2" />
                  </text>
                                    <statusCode code="completed" />
                  <effectiveTime>
                                        <low nullFlavor="UNK" />
                                      </effectiveTime>
                  <value xsi:type="CD" code="419199007" displayName="Allergy to substance"
                    codeSystem="2.16.840.1.113883.6.96" codeSystemName="SNOMED CT" />
                  <author typeCode="AUT">
                    <templateId root="2.16.840.1.113883.10.20.22.4.119" />
                    <time value="201010110915-0800" />
                    <assignedAuthor>
                      <id extension="222223333" root="2.16.840.1.113883.4.6" />
                      <code code="207KA0200X" displayName="Allopathic &amp; Osteopathic Physicians; Allergy &amp; Immunology, Allergy" codeSystem="2.16.840.1.113883.6.101"
                        codeSystemName="Healthcare Provider Taxonomy (HIPAA)" />
                    </assignedAuthor>
                  </author>
                  <participant typeCode="CSM">
                    <participantRole classCode="MANU">
                      <playingEntity classCode="MMAT">
                        <code code="2670" displayName="codeine" codeSystem="2.16.840.1.113883.6.88"
                          codeSystemName="RxNorm" />
                      </playingEntity>
                    </participantRole>
                  </participant>
                  <entryRelationship typeCode="MFST" inversionInd="true">
                    <observation classCode="OBS" moodCode="EVN">
                                            <templateId root="2.16.840.1.113883.10.20.22.4.9" extension="2014-06-09" />
                      <id root="38c63dea-1a43-4f84-ab71-1ffd04f6a1dd" />
                      <code code="ASSERTION" codeSystem="2.16.840.1.113883.5.4" />
                      <text>
                        <reference value="#reaction2" />
                      </text>
                      <statusCode code="completed" />
                      <effectiveTime>
                        <low nullFlavor="UNK" />
                      </effectiveTime>
                      <value xsi:type="CD" code="56018004" displayName="Wheezing" codeSystem="2.16.840.1.113883.6.96"
                        codeSystemName="SNOMED CT" />
                      <entryRelationship typeCode="SUBJ" inversionInd="true">
                        <observation classCode="OBS" moodCode="EVN">
                                                                              <templateId root="2.16.840.1.113883.10.20.22.4.8" extension="2014-06-09" />
                          <code code="SEV" displayName="Severity Observation" codeSystem="2.16.840.1.113883.5.4"
                            codeSystemName="ActCode" />
                          <text>
                            <reference value="#reactionseverity2" />
                          </text>
                          <statusCode code="completed" />
                          <value xsi:type="CD" code="6736007" displayName="Moderate" codeSystem="2.16.840.1.113883.6.96"
                            codeSystemName="SNOMED CT" />
                        </observation>
                      </entryRelationship>
                    </observation>
                  </entryRelationship>
                  <entryRelationship typeCode="SUBJ" inversionInd="true">
                    <observation classCode="OBS" moodCode="EVN">
                                                                  <templateId root="2.16.840.1.113883.10.20.22.4.8" extension="2014-06-09" />
                      <code code="SEV" displayName="Severity Observation" codeSystem="2.16.840.1.113883.5.4"
                        codeSystemName="ActCode" />
                      <text>
                        <reference value="#allergyseverity2" />
                      </text>
                      <statusCode code="completed" />
                      <value xsi:type="CD" code="255604002" displayName="Mild" codeSystem="2.16.840.1.113883.6.96"
                        codeSystemName="SNOMED CT" />
                    </observation>
                  </entryRelationship>
                </observation>
              </entryRelationship>
            </act>
          </entry>
        </section>
      </component>
            <component>
        <section>
                    <templateId root="2.16.840.1.113883.10.20.22.2.21.1" extension="2014-06-09" />
          <code code="42348-3" codeSystem="2.16.840.1.113883.6.1" displayName="Advance directives" />
          <title>ADVANCE DIRECTIVES</title>
          <text>
            <table border="1" width="100%">
              <thead>
                <tr>
                  <th>Directive</th>
                  <th>Description</th>
                  <th>Verification</th>
                  <th>Supporting Document(s)</th>
                </tr>
              </thead>
              <tbody>
                <tr>
                  <td>Resuscitation status</td>
                  <td>
                    <content ID="AD1">Do not resuscitate</content>
                  </td>
                  <td>Dr. Patricia Primary, Feb 19, 2011</td>
                  <td>
                    <linkHtml href="AdvanceDirective.b50b7910.pdf">Advance directive</linkHtml>
                  </td>
                </tr>
              </tbody>
            </table>
          </text>
          <entry>
            <organizer classCode="CLUSTER" moodCode="EVN">
                            <templateId root="2.16.840.1.113883.10.20.22.4.108" />
              <id root="af6ebdf2-d996-11e2-a5b8-f23c91aec05e" />
              <code code="45473-6" codeSystem="2.16.840.1.113883.6.1" codeSystemName="LOINC" />
              <statusCode code="completed" />
              <author>
                <templateId root="2.16.840.1.113883.10.20.22.4.119" />
                <time value="201308011235-0800" />
                <assignedAuthor>
                  <id root="20cf14fb-b65c-4c8c-a54d-b0cca834c18c" />
                  <code code="163W00000X" displayName="Nursing Service Providers; Registered Nurse" codeSystem="2.16.840.1.113883.6.101"
                    codeSystemName="Healthcare Provider Taxonomy (HIPAA)" />
                  <assignedPerson>
                    <name>
                      <given>Nurse</given>
                      <family>Nightingale</family>
                      <suffix>RN</suffix>
                    </name>
                  </assignedPerson>
                  <representedOrganization classCode="ORG">
                    <id root="2.16.840.1.113883.19.5" />
                    <name>Good Health Hospital</name>
                  </representedOrganization>
                </assignedAuthor>
              </author>
              <component>
                <observation classCode="OBS" moodCode="EVN">
                                    <templateId root="2.16.840.1.113883.10.20.22.4.48" extension="2014-06-09" />
                  <id root="9b54c3c9-1673-49c7-aef9-b037ed72ed27" />
                  <code code="75278-2" codeSystem="2.16.840.1.113883.6.1" codeSystemName="LOINC" />
                  <statusCode code="completed" />
                  <effectiveTime>
                    <low value="20110219" />
                    <high nullFlavor="NA" />
                  </effectiveTime>
                  <value xsi:type="CD" code="304253006" displayName="Not for resuscitation"
                    codeSystem="2.16.840.1.113883.6.96" />
                  <author>
                    <templateId root="2.16.840.1.113883.10.20.22.4.119" />
                    <time value="201308011235-0800" />
                    <assignedAuthor>
                      <id root="20cf14fb-b65c-4c8c-a54d-b0cca834c18c" />
                      <code code="163W00000X" displayName="Nursing Service Providers; Registered Nurse" codeSystem="2.16.840.1.113883.6.101"
                        codeSystemName="Healthcare Provider Taxonomy (HIPAA)" />
                      <assignedPerson>
                        <name>
                          <given>Nurse</given>
                          <family>Nightingale</family>
                          <suffix>RN</suffix>
                        </name>
                      </assignedPerson>
                      <representedOrganization classCode="ORG">
                        <id root="2.16.840.1.113883.19.5" />
                        <name>Good Health Hospital</name>
                      </representedOrganization>
                    </assignedAuthor>
                  </author>
                  <participant typeCode="VRF">
                    <time value="201102019" />
                    <participantRole>
                      <id root="20cf14fb-b65c-4c8c-a54d-b0cca834c18c" />
                      <code code="207QA0505X" displayName="Allopathic &amp; Osteopathic Physicians; Family Medicine, Adult Medicine" codeSystem="2.16.840.1.113883.6.101"
                        codeSystemName="Healthcare Provider Taxonomy (HIPAA)" />
                      <playingEntity>
                        <name>
                          <prefix>Dr.</prefix>
                          <family>Patricia</family>
                          <given>Primary</given>
                        </name>
                      </playingEntity>
                    </participantRole>
                  </participant>
                  <participant typeCode="CST">
                    <participantRole classCode="AGNT">
                      <addr>
                        <streetAddressLine>1004 Health Drive</streetAddressLine>
                        <city>Portland</city>
                        <state>OR</state>
                        <postalCode>99123</postalCode>
                        <country>US</country>
                      </addr>
                      <telecom use="WP" value="tel:+1(555)555-1004" />
                      <playingEntity>
                        <name>
                          <prefix>Dr.</prefix>
                          <family>Patricia</family>
                          <given>Primary</given>
                        </name>
                      </playingEntity>
                    </participantRole>
                  </participant>
                  <reference typeCode="REFR">
                    <seperatableInd value="false" />
                    <externalDocument>
                      <id root="b50b7910-7ffb-4f4c-bbe4-177ed68cbbf3" />
                      <text mediaType="application/pdf">
                        <reference value="AdvanceDirective.b50b7910.pdf" />
                      </text>
                    </externalDocument>
                  </reference>
                </observation>
              </component>
            </organizer>
          </entry>
        </section>
      </component>
            <component>
        <section>
          <templateId root="2.16.840.1.113883.10.20.22.2.8" />
          <code codeSystem="2.16.840.1.113883.6.1" codeSystemName="LOINC" code="51848-0" displayName="Evaluation note" />
          <title>ASSESSMENT</title>
          <text>
            <list listType="ordered">
              <item>Flank pain.</item>
              <item>Pain on deep palpation of lower back.</item>
              <item>Other chronic diagnoses as noted above, currently stable.</item>
            </list>
          </text>
        </section>
      </component>
            <component>
        <section>
          <templateId root="2.16.840.1.113883.10.20.22.2.22.1" extension="2014-06-09" />
                    <code code="46240-8" codeSystem="2.16.840.1.113883.6.1" codeSystemName="LOINC"
            displayName="History of encounters" />
          <title>ENCOUNTERS</title>
          <text>
            <table border="1" width="100%">
              <thead>
                <tr>
                  <th>Encounter</th>
                  <th>Performer</th>
                  <th>Location</th>
                  <th>Date</th>
                </tr>
              </thead>
              <tbody>
                <tr>
                  <td>
                    <content ID="Encounter1" />Check-up, Inpatient Medical Ward</td>
                  <td>Amanda Assigned, General Physician</td>
                  <td>Good Health Clinic</td>
                  <td>February 12, 2013</td>
                </tr>
              </tbody>
            </table>
          </text>
          <entry typeCode="DRIV">
            <encounter classCode="ENC" moodCode="EVN">
              <templateId root="2.16.840.1.113883.10.20.22.4.49" extension="2014-06-09" />
                            <id root="2a620155-9d11-439e-92b3-5d9815ff4de8" />
              <code code="99241" displayName="Office consultation for a new or established patient, which requires these 3 key components: A problem focused history; A problem focused examination; and Straightforward medical decision making. Counseling and/or coordination of care with other physicians, other qualified health care professionals, or agencies are provided consistent with the nature of the problem(s) and the patient's and/or family's needs. Usually, the presenting problem(s) are self limited or minor. Typically, 15 minutes are spent face-to-face with the patient and/or family." codeSystemName="CPT"
                codeSystem="2.16.840.1.113883.6.12" codeSystemVersion="4">
                <originalText>Checkup Examination<reference value="#Encounter1" />
                </originalText>
                <translation code="AMB" codeSystem="2.16.840.1.113883.5.4" displayName="Ambulatory"
                  codeSystemName="HL7 ActEncounterCode" />
              </code>
              <effectiveTime value="200130212" />
              <performer>
                <assignedEntity>
                  <id />
                  <code code="59058001" codeSystem="2.16.840.1.113883.6.96" codeSystemName="SNOMED CT"
                    displayName="General physician" />
                  <addr>
                    <streetAddressLine>102 Health Drive</streetAddressLine>
                    <city>Ann Arbor</city>
                    <state>MI</state>
                    <postalCode>99099</postalCode>
                    <country>US</country>
                  </addr>
                  <telecom nullFlavor="UNK" />
                  <assignedPerson>
                    <name>
                      <given>Amanda</given>
                      <family>Assigned</family>
                    </name>
                  </assignedPerson>
                  <representedOrganization>
                    <id root="2.16.840.1.113883.19.5.9999.1394" />
                    <name>Good Health Clinic</name>
                    <telecom nullFlavor="UNK" />
                    <addr nullFlavor="UNK" />
                  </representedOrganization>
                </assignedEntity>
              </performer>
              <participant typeCode="LOC">
                <participantRole classCode="SDLOC">
                  <templateId root="2.16.840.1.113883.10.20.22.4.32" />
                                    <code code="1060-3" codeSystem="2.16.840.1.113883.6.259"
                    codeSystemName="HL7 HealthcareServiceLocation" displayName="Medical Ward" />
                  <addr>
                    <streetAddressLine>17 Daws Rd.</streetAddressLine>
                    <city>Blue Bell</city>
                    <state>MA</state>
                    <postalCode>02368</postalCode>
                    <country>US</country>
                  </addr>
                  <telecom nullFlavor="UNK" />
                  <playingEntity classCode="PLC">
                    <name>Good Health Clinic</name>
                  </playingEntity>
                </participantRole>
              </participant>
                            <entryRelationship typeCode="RSON">
                <observation classCode="OBS" moodCode="EVN">
                  <templateId root="2.16.840.1.113883.10.20.22.4.19" extension="2014-06-09" />
                                    <id root="db734647-fc99-424c-a864-7e3cda82e703" />
                  <code code="ASSERTION" codeSystem="2.16.840.1.113883.5.4" />
                  <statusCode code="completed" />
                  <value xsi:type="CD" code="32398004" displayName="Bronchitis" codeSystem="2.16.840.1.113883.6.96" />
                </observation>
              </entryRelationship>
            </encounter>
          </entry>
        </section>
      </component>
            <component>
        <section>
                    <templateId root="2.16.840.1.113883.10.20.22.2.15" extension="2014-06-09" />
          <code code="10157-6" codeSystem="2.16.840.1.113883.6.1" />
          <title>FAMILY HISTORY</title>
          <text>
            <paragraph>Father (deceased)</paragraph>
            <table border="1" width="100%">
              <thead>
                <tr>
                  <th>Diagnosis</th>
                  <th>Age At Onset</th>
                </tr>
              </thead>
              <tbody>
                <tr>
                  <td>Myocardial Infarction (cause of death)</td>
                  <td>57</td>
                </tr>
                <tr>
                  <td>Diabetes</td>
                  <td>40</td>
                </tr>
              </tbody>
            </table>
          </text>
          <entry typeCode="DRIV">
            <organizer moodCode="EVN" classCode="CLUSTER">
                            <templateId root="2.16.840.1.113883.10.20.22.4.45" extension="2014-06-09" />
              <id root="d42ebf70-5c89-11db-b0de-0855200c9a66" />
              <statusCode code="completed" />
              <subject>
                <relatedSubject classCode="PRS">
                  <code code="FTH" displayName="father" codeSystemName="FamilyRelationshipRoleType"
                    codeSystem="2.16.840.1.113883.5.111">
                    <translation code="9947008" displayName="Biological father" codeSystemName="SNOMED"
                      codeSystem="2.16.840.1.113883.6.96" />
                  </code>
                  <subject>
                    <sdtc:id root="2.16.840.1.113883.19.5.99999.2" extension="99999999" />
                    <administrativeGenderCode code="M" codeSystem="2.16.840.1.113883.1.11.1" displayName="Male" />
                    <birthTime value="1910" />
                                                          </subject>
                </relatedSubject>
              </subject>
              <component>
                <observation classCode="OBS" moodCode="EVN">
                                    <templateId root="2.16.840.1.113883.10.20.22.4.46" extension="2014-06-09" />
                  <id root="d42ebf70-5c89-11db-b0de-0800200c9a66" />
                  <code code="64572001" displayName="Disease" codeSystemName="SNOMED CT"
                    codeSystem="2.16.840.1.113883.6.96" />
                  <statusCode code="completed" />
                  <effectiveTime value="1967" />
                  <value xsi:type="CD" code="22298006" codeSystem="2.16.840.1.113883.6.96"
                    displayName="Myocardial infarction" />
                  <entryRelationship typeCode="CAUS">
                    <observation classCode="OBS" moodCode="EVN">
                                            <templateId root="2.16.840.1.113883.10.20.22.4.47" />
                      <id root="6898fae0-5c8a-11db-b0de-0800200c9a66" />
                      <code code="ASSERTION" codeSystem="2.16.840.1.113883.5.4" />
                      <statusCode code="completed" />
                      <value xsi:type="CD" code="419099009" codeSystem="2.16.840.1.113883.6.96" displayName="Dead" />
                    </observation>
                  </entryRelationship>
                  <entryRelationship typeCode="SUBJ" inversionInd="true">
                    <observation classCode="OBS" moodCode="EVN">
                                            <templateId root="2.16.840.1.113883.10.20.22.4.31" />
                      <code code="445518008" codeSystem="2.16.840.1.113883.6.96" displayName="Age At Onset" />
                      <statusCode code="completed" />
                      <value xsi:type="PQ" value="57" unit="a" />
                    </observation>
                  </entryRelationship>
                </observation>
              </component>
              <component>
                <observation classCode="OBS" moodCode="EVN">
                                    <templateId root="2.16.840.1.113883.10.20.22.4.46" extension="2014-06-09" />
                  <id root="5bfe3ec0-5c8b-11db-b0de-0800200c9a66" />
                  <code code="64572001" displayName="Disease" codeSystemName="SNOMED CT"
                    codeSystem="2.16.840.1.113883.6.96" />
                  <statusCode code="completed" />
                  <effectiveTime value="1950" />
                  <value xsi:type="CD" code="44054006" codeSystem="2.16.840.1.113883.6.96"
                    displayName="Diabetes mellitus type 2" />
                  <entryRelationship typeCode="SUBJ" inversionInd="true">
                    <observation classCode="OBS" moodCode="EVN">
                                            <templateId root="2.16.840.1.113883.10.20.22.4.31" />
                      <code code="445518008" codeSystem="2.16.840.1.113883.6.96" displayName="Age At Onset" />
                      <statusCode code="completed" />
                      <value xsi:type="PQ" value="40" unit="a" />
                    </observation>
                  </entryRelationship>
                </observation>
              </component>
            </organizer>
          </entry>
        </section>
      </component>
            <component>
        <section>
          <templateId root="2.16.840.1.113883.10.20.22.2.14" extension="2014-06-09" />
                    <code code="47420-5" codeSystem="2.16.840.1.113883.6.1" codeSystemName="LOINC" displayName="Functional status assessment note" />
          <title>FUNCTIONAL STATUS</title>
          <text>
            <table border="1" width="100%">
              <thead>
                <tr>
                  <th>Functional Category</th>
                  <th>Effective Dates</th>
                  <th>Results of Evaluation</th>
                </tr>
              </thead>
              <tbody>
                <tr ID="FUNC1">
                  <td>Functional Assessment</td>
                  <td>March 11, 2013</td>
                  <td>Independent Walking</td>
                </tr>
                <tr>
                  <td>ADL/IADL: Bathing</td>
                  <td>March 11,2013</td>
                  <td>Independent</td>
                </tr>
              </tbody>
            </table>
          </text>
          <entry>
            <organizer classCode="CLUSTER" moodCode="EVN">
                            <templateId root="2.16.840.1.113883.10.20.22.4.66" extension="2014-06-09" />
              <id root="a7bc1062-8649-42a0-833d-eed65bd017c9" />
              <code code="d5" displayName="Self-Care" codeSystem="2.16.840.1.113883.6.254" codeSystemName="ICF" />
              <statusCode code="completed" />
              <author typeCode="AUT">
                <templateId root="2.16.840.1.113883.10.20.22.4.119" />
                <time value="201307061145-0800" />
                <assignedAuthor>
                  <id extension="555555555" root="2.16.840.1.113883.4.6" />
                  <code code="207QA0505X" displayName="Allopathic &amp; Osteopathic Physicians; Family Medicine, Adult Medicine" codeSystem="2.16.840.1.113883.6.101"
                    codeSystemName="Healthcare Provider Taxonomy (HIPAA)" />
                </assignedAuthor>
              </author>
              <component>
                <observation classCode="OBS" moodCode="EVN">
                                    <templateId root="2.16.840.1.113883.10.20.22.4.67" extension="2014-06-09" />
                  <id root="b63a8636-cfff-4461-b018-40ba58ba8b32" />
                  <code code="54522-8" displayName="Functional status" codeSystem="2.16.840.1.113883.6.1"
                    codeSystemName="LOINC" />
                  <text>
                    <reference value="#FUNC1" />
                  </text>
                  <statusCode code="completed" />
                  <effectiveTime value="20130311" />
                  <value xsi:type="CD" code="165245003" displayName="Independent walking"
                    codeSystem="2.16.840.1.113883.6.96" codeSystemName="SNOMED CT" />
                  <author typeCode="AUT">
                    <templateId root="2.16.840.1.113883.10.20.22.4.119" />
                    <time value="201307061145-0800" />
                    <assignedAuthor>
                      <id extension="555555555" root="2.16.840.1.113883.4.6" />
                      <code code="207QA0505X" displayName="Allopathic &amp; Osteopathic Physicians; Family Medicine, Adult Medicine" codeSystem="2.16.840.1.113883.6.101"
                        codeSystemName="Healthcare Provider Taxonomy (HIPAA)" />
                    </assignedAuthor>
                  </author>
                  <entryRelationship typeCode="COMP">
                    <supply classCode="SPLY" moodCode="EVN">
                      <templateId root="2.16.840.1.113883.10.20.22.4.50" extension="2014-06-09" />
                                            <id root="2413773c-2372-4299-bbe6-5b0f60664446" />
                      <statusCode code="completed" />
                      <effectiveTime xsi:type="IVL_TS">
                        <high value="20130311" />
                      </effectiveTime>
                      <quantity value="2" />
                      <participant typeCode="PRD">
                        <participantRole classCode="MANU">
                          <templateId root="2.16.840.1.113883.10.20.22.4.37" />
                                                    <id root="742aee30-21c5-11e1-bfc2-0800200c9a66" />
                          <playingDevice>
                            <code code="87405001" codeSystem="2.16.840.1.113883.6.96"
                              displayName="cane, device (physical object)" />
                          </playingDevice>
                          <scopingEntity>
                            <id root="eb936010-7b17-11db-9fe1-0800200c9b65" />
                          </scopingEntity>
                        </participantRole>
                      </participant>
                    </supply>
                  </entryRelationship>
                </observation>
              </component>
              <component>
                <observation classCode="OBS" moodCode="EVN">
                                    <templateId root="2.16.840.1.113883.10.20.22.4.128" />
                  <id root="c6b5a04b-2bf4-49d1-8336-636a3813df0a" />
                  <code code="46008-9 " displayName="Bathing" codeSystem="2.16.840.1.113883.6.1" codeSystemName="LOINC" />
                  <statusCode code="completed" />
                  <effectiveTime value="200130311" />
                  <value xsi:type="CD" code="371153006" displayName="Independent" codeSystem="2.16.840.1.113883.6.96"
                    codeSystemName="SNOMED CT" />
                  <author typeCode="AUT">
                    <templateId root="2.16.840.1.113883.10.20.22.4.119" />
                    <time value="201307061148-0800" />
                    <assignedAuthor>
                      <id extension="555555555" root="2.16.840.1.113883.4.6" />
                      <code code="207QA0505X" displayName="Allopathic &amp; Osteopathic Physicians; Family Medicine, Adult Medicine" codeSystem="2.16.840.1.113883.6.101"
                        codeSystemName="Healthcare Provider Taxonomy (HIPAA)" />
                    </assignedAuthor>
                  </author>
                </observation>
              </component>
            </organizer>
          </entry>
        </section>
      </component>
            <component>
        <section>
          <templateId root="2.16.840.1.113883.10.20.2.5" />
          <code code="10210-3" codeSystem="2.16.840.1.113883.6.1" codeSystemName="LOINC" displayName="Physical findings of General status Narrative" />
          <title>GENERAL STATUS</title>
          <text>
            <paragraph>Alert and in good spirits, mild distress.</paragraph>
          </text>
        </section>
      </component>
            <component>
        <section>
          <templateId root="2.16.840.1.113883.10.20.22.2.20" extension="2014-06-09" />
                    <code codeSystem="2.16.840.1.113883.6.1" codeSystemName="LOINC" code="11348-0"
            displayName="HISTORY OF PAST ILLNESS" />
          <title>HISTORY OF PAST MEDICAL/SURGICAL ILLNESS</title>
          <text>
            <paragraph>In 2011, the patient experienced a minor stroke, which caused temporary paralysis on her left
              side. She was monitored in hospital for three weeks and recovered. She has been taking warfarin since then
              and is expected continue on with close monitoring.</paragraph>
            <paragraph>She has had type II diabetes, poorly controlled for many years. Since the diagnosis, her kidney
              functions are compromised and she is predisposed to developing peripheral neuropathy.
              occlusion.</paragraph>
            <paragraph>Two weeks prior to this current hospital admission, she was also diagnosed with
              hypercholesterolemia. She is currently taking Lipitor to manage this.</paragraph>
          </text>
        </section>
      </component>
            <component>
        <section>
          <templateId root="1.3.6.1.4.1.19376.1.5.3.1.3.4" />
          <code codeSystem="2.16.840.1.113883.6.1" codeSystemName="LOINC" code="10164-2"
            displayName="HISTORY OF PRESENT ILLNESS" />
          <title>HISTORY OF PRESENT ILLNESS</title>
          <text>
            <paragraph> This is a 68-year-old white woman who went to the emergency room with sudden onset of severe
              left flank and left lower quadrant abdominal pain associated with gross hematuria. The patient had a CT
              stone profile which showed no evidence of renal calculi. She was referred for urologic
              evaluation.</paragraph>
            <paragraph>When seen in our office, the patient continued to have mild left flank pain and no difficultly
              voiding. Urinalysis showed 1+ occult blood. Intravenous pyelogram was done which demonstrated a low-lying
              malrotated right kidney. There was no evidence of renal or ureteral calculi or hydronephrosis. Urine
              cytology was negative for malignant cells. The patient subsequently had a CT renal scan with contrast.
              This showed what appeared to be an infarction of an area of the lower pole of the left kidney. It was
              suggested that a renal MRI be done for further delineation of this problem. She had a right kidney which
              was malrotated but was otherwise normal. The patient is admitted at this time for complete urologic
              evaluation. </paragraph>
          </text>
        </section>
      </component>
            <component>
        <section>
          <templateId root="2.16.840.1.113883.10.20.22.2.64" extension="2014-06-09" />
          <code codeSystem="2.16.840.1.113883.6.1" codeSystemName="LOINC" code="8648-8"
            displayName="Hospital Course Narrative" />
          <title>Hospital Course of Care</title>
          <text>
            <paragraph> This is a 68-year-old white woman who went to the emergency room with sudden onset of severe
              left flank and left lower quadrant abdominal pain associated with gross hematuria. The patient had a CT
              stone profile which showed no evidence of renal calculi. </paragraph>
          </text>
        </section>
      </component>
            <component>
        <section>
                    <templateId root="2.16.840.1.113883.10.20.22.2.24" extension="2014-06-09" />
          <code code="C-CDAV2-DDN" displayName="Discharge diagnosis narritive" codeSystem="2.16.840.1.113883.6.1"
            codeSystemName="LOINC" />
          <title>Hospital Discharge Diagnosis</title>
          <text>.Kidney Malrotation. Discharged August 1, 2013</text>
          <entry>
            <act classCode="ACT" moodCode="EVN">
                            <templateId root="2.16.840.1.113883.10.20.22.4.33" extension="2014-06-09" />
              <id root="5a784260-6856-4f38-9638-80c751aff2fb" />
              <code code="11535-2" codeSystem="2.16.840.1.113883.6.1" codeSystemName="LOINC"
                displayName="HOSPITAL DISCHARGE DIAGNOSIS" />
              <statusCode code="active" />
              <effectiveTime>
                <low value="201308011900-0800" />
              </effectiveTime>
              <entryRelationship typeCode="SUBJ" inversionInd="false">
                <observation classCode="OBS" moodCode="EVN">
                                    <templateId root="2.16.840.1.113883.10.20.22.4.4" extension="2014-06-09" />
                  <id root="ab1791b0-5c71-11db-b0de-0800200c9a66" />
                  <code code="409586006" codeSystem="2.16.840.1.113883.6.96" displayName="Complaint" />
                  <text>
                    <reference value="#problem2" />
                  </text>
                  <statusCode code="completed" />
                  <effectiveTime>
                    <low value="20130212" />
                  </effectiveTime>
                  <value xsi:type="CD" code="49008000" codeSystem="2.16.840.1.113883.6.96"
                    displayName="Malrotation of kidney" />
                  <author>
                    <templateId root="2.16.840.1.113883.10.20.22.4.119" />
                    <time value="200130311" />
                    <assignedAuthor>
                      <id extension="KP00017" root="2.16.840.1.113883.19.5" />
                      <addr>
                        <streetAddressLine>1003 Health Care Drive</streetAddressLine>
                        <city>Ann Arbor</city>
                        <state>MI</state>
                        <postalCode>02368</postalCode>
                        <country>US</country>
                      </addr>
                      <telecom use="WP" value="tel:+1(555)555-1003" />
                      <assignedPerson>
                        <name>
                          <given>Assigned</given>
                          <family>Amanda</family>
                        </name>
                      </assignedPerson>
                    </assignedAuthor>
                  </author>
                </observation>
              </entryRelationship>
            </act>
          </entry>
        </section>
      </component>
            <component>
        <section>
                    <templateId root="2.16.840.1.113883.10.20.22.2.2.1" extension="2014-06-09" />
          <code code="11369-6" codeSystem="2.16.840.1.113883.6.1" codeSystemName="LOINC"
            displayName="History of immunizations" />
          <title>IMMUNIZATIONS</title>
          <text>
            <content ID="immunSect" />
            <table border="1" width="100%">
              <thead>
                <tr>
                  <th>Vaccine</th>
                  <th>Date</th>
                  <th>Status</th>
                  <th>Series number</th>
                </tr>
              </thead>
              <tbody>
                <tr>
                  <td>
                    <content ID="immi1" /> Influenza, seasonal, IM </td>
                  <td>Nov 1999</td>
                  <td>Completed</td>
                  <td>N/A</td>
                </tr>
                <tr>
                  <td>
                    <content ID="immi2" /> Influenza, seasonal, IM </td>
                  <td>Dec 1998</td>
                  <td>Completed</td>
                  <td>N/A</td>
                </tr>
                <tr>
                  <td>
                    <content ID="immi3" /> Pneumococcal polysaccharide vaccine, IM </td>
                  <td>Dec 1998</td>
                  <td>Completed</td>
                  <td>N/A</td>
                </tr>
                <tr>
                  <td>
                    <content ID="immi4" /> Tetanus and diphtheria toxoids, IM </td>
                  <td>1997</td>
                  <td>Refused</td>
                  <td>N/A</td>
                </tr>
                <tr>
                  <td>Hepatitis B</td>
                  <td>Aug 1, 2012</td>
                  <td>Completed</td>
                  <td>3rd</td>
                </tr>
              </tbody>
            </table>
          </text>
          <entry typeCode="DRIV">
            <substanceAdministration classCode="SBADM" moodCode="EVN" negationInd="false">
                            <templateId root="2.16.840.1.113883.10.20.22.4.52" extension="2014-06-09" />
              <id root="e6f1ba43-c0ed-4b9b-9f12-f435d8ad8f92" />
              <text>
                <reference value="#immun1" />
              </text>
              <statusCode code="completed" />
              <effectiveTime value="199911" />
              <routeCode code="C28161" codeSystem="2.16.840.1.113883.3.26.1.1"
                codeSystemName="National Cancer Institute (NCI) Thesaurus" displayName="Intramuscular Route of Administration" />
              <doseQuantity value="50" unit="ug" />
              <consumable>
                <manufacturedProduct classCode="MANU">
                                    <templateId root="2.16.840.1.113883.10.20.22.4.54" extension="2014-06-09" />
                  <manufacturedMaterial>
                    <code code="88" codeSystem="2.16.840.1.113883.12.292" displayName="influenza virus vaccine, unspecified formulation"
                      codeSystemName="CVX">
                      <originalText>Influenza, seasonal, IM</originalText>
                      <translation code="141" displayName="Influenza, seasonal, injectable" codeSystemName="CVX"
                        codeSystem="2.16.840.1.113883.12.292" />
                    </code>
                    <lotNumberText>1</lotNumberText>
                  </manufacturedMaterial>
                  <manufacturerOrganization>
                    <name>Health LS - Immuno Inc.</name>
                  </manufacturerOrganization>
                </manufacturedProduct>
              </consumable>
              <performer>
                <assignedEntity>
                  <id root="2.16.840.1.113883.19.5.9999.456" extension="2981824" />
                  <addr>
                    <streetAddressLine>102 Health Drive</streetAddressLine>
                    <city>Ann Arbor</city>
                    <state>MI</state>
                    <postalCode>99099</postalCode>
                    <country>US</country>
                  </addr>
                  <telecom nullFlavor="UNK" />
                  <assignedPerson>
                    <name>
                      <given>Amanda</given>
                      <family>Assigned</family>
                    </name>
                  </assignedPerson>
                  <representedOrganization>
                    <id root="2.16.840.1.113883.19.5.9999.1394" />
                    <name>Good Health Clinic</name>
                    <telecom nullFlavor="UNK" />
                    <addr nullFlavor="UNK" />
                  </representedOrganization>
                </assignedEntity>
              </performer>
              <author typeCode="AUT">
                <templateId root="2.16.840.1.113883.10.20.22.4.119" />
                <time value="20130730" />
                <assignedAuthor>
                  <id extension="5555555555" root="2.16.840.1.113883.4.6" />
                  <code code="207QA0505X" displayName="Allopathic &amp; Osteopathic Physicians; Family Medicine, Adult Medicine" codeSystem="2.16.840.1.113883.6.101"
                    codeSystemName="Healthcare Provider Taxonomy (HIPAA)" />
                  <addr>
                    <streetAddressLine>1004 Healthcare Drive </streetAddressLine>
                    <city>Portland</city>
                    <state>OR</state>
                    <postalCode>99123</postalCode>
                    <country>US</country>
                  </addr>
                  <telecom use="WP" value="tel:+1(555)555-1004" />
                  <assignedPerson>
                    <name>
                      <given>Patricia</given>
                      <given qualifier="CL">Patty</given>
                      <family>Primary</family>
                      <suffix qualifier="AC">M.D.</suffix>
                    </name>
                  </assignedPerson>
                </assignedAuthor>
              </author>
              <entryRelationship typeCode="SUBJ" inversionInd="false">
                <act classCode="ACT" moodCode="INT">
                                    <templateId root="2.16.840.1.113883.10.20.22.4.20" extension="2014-06-09" />
                  <code code="171044003" codeSystem="2.16.840.1.113883.6.96" displayName="Immunization education" />
                  <text>
                    <reference value="#immunSect" /> Possible flu-like symptoms for three days. </text>
                  <statusCode code="completed" />
                </act>
              </entryRelationship>
            </substanceAdministration>
          </entry>
          <entry typeCode="DRIV">
            <substanceAdministration classCode="SBADM" moodCode="EVN" negationInd="true">
                            <templateId root="2.16.840.1.113883.10.20.22.4.52" extension="2014-06-09" />
              <id root="e6f1ba43-c0ed-4b9b-9f12-f435d8ad8f92" />
              <text>
                <reference value="#immun2" />
              </text>
              <statusCode code="completed" />
              <effectiveTime value="19981215" />
              <routeCode code="C28161" codeSystem="2.16.840.1.113883.3.26.1.1"
                codeSystemName="National Cancer Institute (NCI) Thesaurus" displayName="Intramuscular Route of Administration" />
              <doseQuantity value="50" unit="ug" />
              <consumable>
                <manufacturedProduct classCode="MANU">
                                    <templateId root="2.16.840.1.113883.10.20.22.4.54" extension="2014-06-09" />
                  <manufacturedMaterial>
                    <code code="88" codeSystem="2.16.840.1.113883.12.292" displayName="influenza virus vaccine, unspecified formulation"
                      codeSystemName="CVX">
                      <originalText>
                        <reference value="#immi2" />
                      </originalText>
                      <translation code="141" displayName="Influenza, seasonal, injectable" codeSystemName="CVX"
                        codeSystem="2.16.840.1.113883.12.292" />
                    </code>
                    <lotNumberText>1</lotNumberText>
                  </manufacturedMaterial>
                  <manufacturerOrganization>
                    <name>Health LS - Immuno Inc.</name>
                  </manufacturerOrganization>
                </manufacturedProduct>
              </consumable>
              <performer>
                <assignedEntity>
                  <id root="2.16.840.1.113883.19.5.9999.456" extension="2981824" />
                  <addr>
                    <streetAddressLine>102 Health Drive</streetAddressLine>
                    <city>Ann Arbor</city>
                    <state>MI</state>
                    <postalCode>99099</postalCode>
                    <country>US</country>
                  </addr>
                  <telecom nullFlavor="UNK" />
                  <assignedPerson>
                    <name>
                      <given>Amanda</given>
                      <family>Assigned</family>
                    </name>
                  </assignedPerson>
                  <representedOrganization>
                    <id root="2.16.840.1.113883.19.5.9999.1394" />
                    <name>Good Health Clinic</name>
                    <telecom nullFlavor="UNK" />
                    <addr nullFlavor="UNK" />
                  </representedOrganization>
                </assignedEntity>
              </performer>
              <author typeCode="AUT">
                <templateId root="2.16.840.1.113883.10.20.22.4.119" />
                <time value="20130730" />
                <assignedAuthor>
                  <id extension="5555555555" root="2.16.840.1.113883.4.6" />
                  <code code="207QA0505X" displayName="Allopathic &amp; Osteopathic Physicians; Family Medicine, Adult Medicine" codeSystem="2.16.840.1.113883.6.101"
                    codeSystemName="Healthcare Provider Taxonomy (HIPAA)" />
                  <addr>
                    <streetAddressLine>1004 Healthcare Drive </streetAddressLine>
                    <city>Portland</city>
                    <state>OR</state>
                    <postalCode>99123</postalCode>
                    <country>US</country>
                  </addr>
                  <telecom use="WP" value="tel:+1(555)555-1004" />
                  <assignedPerson>
                    <name>
                      <given>Patricia</given>
                      <given qualifier="CL">Patty</given>
                      <family>Primary</family>
                      <suffix qualifier="AC">M.D.</suffix>
                    </name>
                  </assignedPerson>
                </assignedAuthor>
              </author>
              <entryRelationship typeCode="SUBJ" inversionInd="true">
                <act classCode="ACT" moodCode="INT">
                                    <templateId root="2.16.840.1.113883.10.20.22.4.20" extension="2014-06-09" />
                  <code code="171044003" codeSystem="2.16.840.1.113883.6.96" displayName="Immunization education" />
                  <text>
                    <reference value="#immunSect" /> Possible flu-like symptoms for three days. </text>
                  <statusCode code="completed" />
                </act>
              </entryRelationship>
            </substanceAdministration>
          </entry>
          <entry typeCode="DRIV">
            <substanceAdministration classCode="SBADM" moodCode="EVN" negationInd="false">
                            <templateId root="2.16.840.1.113883.10.20.22.4.52" extension="2014-06-09" />
              <id root="e6f1ba43-c0ed-4b9b-9f12-f435d8ad8f92" />
              <text>
                <reference value="#immun3" />
              </text>
              <statusCode code="completed" />
              <effectiveTime value="19981215" />
              <routeCode code="C28161" codeSystem="2.16.840.1.113883.3.26.1.1"
                codeSystemName="National Cancer Institute (NCI) Thesaurus" displayName="Intramuscular Route of Administration" />
              <doseQuantity value="50" unit="ug" />
              <consumable>
                <manufacturedProduct classCode="MANU">
                                    <templateId root="2.16.840.1.113883.10.20.22.4.54" extension="2014-06-09" />
                  <manufacturedMaterial>
                    <code code="33" codeSystem="2.16.840.1.113883.12.292"
                      displayName="pneumococcal polysaccharide vaccine, 23 valent" codeSystemName="CVX">
                      <originalText>
                        <reference value="#immi3" />
                      </originalText>
                      <translation code="109" displayName="pneumococcal vaccine, unspecified formulation" codeSystemName="CVX"
                        codeSystem="2.16.840.1.113883.12.292" />
                    </code>
                    <lotNumberText>1</lotNumberText>
                  </manufacturedMaterial>
                  <manufacturerOrganization>
                    <name>Health LS - Immuno Inc.</name>
                  </manufacturerOrganization>
                </manufacturedProduct>
              </consumable>
              <performer>
                <assignedEntity>
                  <id root="2.16.840.1.113883.19.5.9999.456" extension="2981824" />
                  <addr>
                    <streetAddressLine>102 Health Drive</streetAddressLine>
                    <city>Ann Arbor</city>
                    <state>MI</state>
                    <postalCode>99099</postalCode>
                    <country>US</country>
                  </addr>
                  <telecom nullFlavor="UNK" />
                  <assignedPerson>
                    <name>
                      <given>Amanda</given>
                      <family>Assigned</family>
                    </name>
                  </assignedPerson>
                  <representedOrganization>
                    <id root="2.16.840.1.113883.19.5.9999.1394" />
                    <name>Good Health Clinic</name>
                    <telecom nullFlavor="UNK" />
                    <addr nullFlavor="UNK" />
                  </representedOrganization>
                </assignedEntity>
              </performer>
              <author typeCode="AUT">
                <templateId root="2.16.840.1.113883.10.20.22.4.119" />
                <time value="20130730" />
                <assignedAuthor>
                  <id extension="5555555555" root="2.16.840.1.113883.4.6" />
                  <code code="207QA0505X" displayName="Allopathic &amp; Osteopathic Physicians; Family Medicine, Adult Medicine" codeSystem="2.16.840.1.113883.6.101"
                    codeSystemName="Healthcare Provider Taxonomy (HIPAA)" />
                  <addr>
                    <streetAddressLine>1004 Healthcare Drive </streetAddressLine>
                    <city>Portland</city>
                    <state>OR</state>
                    <postalCode>99123</postalCode>
                    <country>US</country>
                  </addr>
                  <telecom use="WP" value="tel:+1(555)555-1004" />
                  <assignedPerson>
                    <name>
                      <given>Patricia</given>
                      <given qualifier="CL">Patty</given>
                      <family>Primary</family>
                      <suffix qualifier="AC">M.D.</suffix>
                    </name>
                  </assignedPerson>
                </assignedAuthor>
              </author>
            </substanceAdministration>
          </entry>
          <entry typeCode="DRIV">
            <substanceAdministration classCode="SBADM" moodCode="EVN" negationInd="true">
                            <templateId root="2.16.840.1.113883.10.20.22.4.52" extension="2014-06-09" />
              <id root="e6f1ba43-c0ed-4b9b-9f12-f435d8ad8f92" />
              <text>
                <reference value="#immun4" />
              </text>
              <statusCode code="completed" />
              <effectiveTime value="19981215" />
              <routeCode code="C28161" codeSystem="2.16.840.1.113883.3.26.1.1"
                codeSystemName="National Cancer Institute (NCI) Thesaurus" displayName="Intramuscular Route of Administration" />
              <doseQuantity value="50" unit="ug" />
              <consumable>
                <manufacturedProduct classCode="MANU">
                                    <templateId root="2.16.840.1.113883.10.20.22.4.54" extension="2014-06-09" />
                  <manufacturedMaterial>
                    <code code="103" codeSystem="2.16.840.1.113883.12.292"
                      displayName="meningococcal C conjugate vaccine" codeSystemName="CVX">
                      <originalText>
                        <reference value="#immi4" />
                      </originalText>
                      <translation code="09" displayName="tetanus and diphtheria toxoids, adsorbed, preservative free, for adult use"
                        codeSystemName="CVX" codeSystem="2.16.840.1.113883.12.292" />
                    </code>
                    <lotNumberText>1</lotNumberText>
                  </manufacturedMaterial>
                  <manufacturerOrganization>
                    <name>Health LS - Immuno Inc.</name>
                  </manufacturerOrganization>
                </manufacturedProduct>
              </consumable>
              <performer>
                <assignedEntity>
                  <id root="2.16.840.1.113883.19.5.9999.456" extension="2981824" />
                  <addr>
                    <streetAddressLine>102 Health Drive</streetAddressLine>
                    <city>Ann Arbor</city>
                    <state>MI</state>
                    <postalCode>99099</postalCode>
                    <country>US</country>
                  </addr>
                  <telecom nullFlavor="UNK" />
                  <assignedPerson>
                    <name>
                      <given>Amanda</given>
                      <family>Assigned</family>
                    </name>
                  </assignedPerson>
                  <representedOrganization>
                    <id root="2.16.840.1.113883.19.5.9999.1394" />
                    <name>Good Health Clinic</name>
                    <telecom nullFlavor="UNK" />
                    <addr nullFlavor="UNK" />
                  </representedOrganization>
                </assignedEntity>
              </performer>
              <author typeCode="AUT">
                <templateId root="2.16.840.1.113883.10.20.22.4.119" />
                <time value="20130730" />
                <assignedAuthor>
                  <id extension="5555555555" root="2.16.840.1.113883.4.6" />
                  <code code="207QA0505X" displayName="Allopathic &amp; Osteopathic Physicians; Family Medicine, Adult Medicine" codeSystem="2.16.840.1.113883.6.101"
                    codeSystemName="Healthcare Provider Taxonomy (HIPAA)" />
                  <addr>
                    <streetAddressLine>1004 Healthcare Drive </streetAddressLine>
                    <city>Portland</city>
                    <state>OR</state>
                    <postalCode>99123</postalCode>
                    <country>US</country>
                  </addr>
                  <telecom use="WP" value="tel:+1(555)555-1004" />
                  <assignedPerson>
                    <name>
                      <given>Patricia</given>
                      <given qualifier="CL">Patty</given>
                      <family>Primary</family>
                      <suffix qualifier="AC">M.D.</suffix>
                    </name>
                  </assignedPerson>
                </assignedAuthor>
              </author>
              <entryRelationship typeCode="RSON">
                <observation classCode="OBS" moodCode="EVN">
                                    <templateId root="2.16.840.1.113883.10.20.22.4.53" />
                  <id root="2a620155-9d11-439e-92b3-5d9815ff4dd8" />
                  <code displayName="patient objection" code="PATOBJ" codeSystemName="HL7 ActNoImmunizationReason"
                    codeSystem="2.16.840.1.113883.5.8" />
                  <statusCode code="completed" />
                </observation>
              </entryRelationship>
            </substanceAdministration>
          </entry>
          <entry typeCode="DRIV">
            <substanceAdministration classCode="SBADM" moodCode="EVN">
                            <templateId root="2.16.840.1.113883.10.20.22.4.52" extension="2014-06-09" />
              <id root="de10790f-1496-4719-8fe6-f1b87b6219f7" />
              <statusCode code="completed" />
              <effectiveTime value="20130801" />
              <routeCode code="C28161" codeSystem="2.16.840.1.113883.3.26.1.1"
                codeSystemName="National Cancer Institute (NCI) Thesaurus" displayName="Intramuscular Route of Administration" />
              <consumable>
                <manufacturedProduct classCode="MANU">
                                    <templateId root="2.16.840.1.113883.10.20.22.4.54" extension="2014-06-09" />
                  <manufacturedMaterial>
                    <code code="45" codeSystem="2.16.840.1.113883.12.292" displayName="hepatitis B vaccine, unspecified formulation"
                      codeSystemName="CVX" />
                    <lotNumberText>1</lotNumberText>
                  </manufacturedMaterial>
                </manufacturedProduct>
              </consumable>
              <performer>
                <assignedEntity>
                  <id root="2.16.840.1.113883.19.5.9999.456" extension="2981824" />
                  <addr>
                    <streetAddressLine>102 Health Drive</streetAddressLine>
                    <city>Ann Arbor</city>
                    <state>MI</state>
                    <postalCode>99099</postalCode>
                    <country>US</country>
                  </addr>
                  <telecom nullFlavor="UNK" />
                  <assignedPerson>
                    <name>
                      <given>Amanda</given>
                      <family>Assigned</family>
                    </name>
                  </assignedPerson>
                  <representedOrganization>
                    <id root="2.16.840.1.113883.19.5.9999.1394" />
                    <name>Good Health Clinic</name>
                    <telecom nullFlavor="UNK" />
                    <addr nullFlavor="UNK" />
                  </representedOrganization>
                </assignedEntity>
              </performer>
              <author typeCode="AUT">
                <templateId root="2.16.840.1.113883.10.20.22.4.119" />
                <time value="20130730" />
                <assignedAuthor>
                  <id extension="5555555555" root="2.16.840.1.113883.4.6" />
                  <code code="207QA0505X" displayName="Allopathic &amp; Osteopathic Physicians; Family Medicine, Adult Medicine" codeSystem="2.16.840.1.113883.6.101"
                    codeSystemName="Healthcare Provider Taxonomy (HIPAA)" />
                  <addr>
                    <streetAddressLine>1004 Healthcare Drive </streetAddressLine>
                    <city>Portland</city>
                    <state>OR</state>
                    <postalCode>99123</postalCode>
                    <country>US</country>
                  </addr>
                  <telecom use="WP" value="tel:+1(555)555-1004" />
                  <assignedPerson>
                    <name>
                      <given>Patricia</given>
                      <given qualifier="CL">Patty</given>
                      <family>Primary</family>
                      <suffix qualifier="AC">M.D.</suffix>
                    </name>
                  </assignedPerson>
                </assignedAuthor>
              </author>
              <entryRelationship typeCode="COMP" inversionInd="true">
                <sequenceNumber value="3" />
                <act classCode="ACT" moodCode="EVN">
                                    <templateId root="2.16.840.1.113883.10.20.22.4.118" />
                  <id root="3f998a26-1b8b-4cbf-8e91-16fbb7a7080f" />
                  <code code="416118004" codeSystem="2.16.840.1.113883.6.96" displayName="administration" />
                  <statusCode code="completed" />
                </act>
              </entryRelationship>
            </substanceAdministration>
          </entry>
        </section>
      </component>
            <component>
        <section>
                    <templateId root="2.16.840.1.113883.10.20.22.2.23" extension="2014-06-09" />
          <code code="46264-8" codeSystem="2.16.840.1.113883.6.1" />
          <title>MEDICAL EQUIPMENT</title>
          <text>
            <content styleCode="Bold">Medical Equipment</content>
            <list>
              <item>Implanted Devices: Cardiac PaceMaker July 3, 2012</item>
              <item>Implanted Devices: Upper GI Prosthesis, January 3, 2013</item>
              <item>Cane, February 2, 2003</item>
            </list>
            <content ID="Eqpt1">Biliary Stent, May 5, 2013</content>
          </text>
          <entry>
                        <organizer classCode="CLUSTER" moodCode="EVN">
              <templateId root="2.16.840.1.113883.10.20.22.4.135" />
                            <id root="3e414708-0e61-4d48-8863-484a2d473a02" />
              <code code="40388003" codeSystem="2.16.840.1.113883.6.96" codeSystemName="SNOMED CT" displayName="Implant">
                <originalText>Implants</originalText>
              </code>
              <statusCode code="completed" />
              <effectiveTime xsi:type="IVL_TS">
                <low value="20070103" />
                <high nullFlavor="UNK" />
              </effectiveTime>
              <component>
                <supply classCode="SPLY" moodCode="EVN">
                  <templateId root="2.16.840.1.113883.10.20.22.4.50" extension="2014-06-09" />
                                    <id root="39b5f1b4-a8e1-4ad7-8849-0deab10c97b1" />
                  <statusCode code="completed" />
                  <effectiveTime xsi:type="IVL_TS">
                    <high value="20120703" />
                  </effectiveTime>
                  <quantity value="1" />
                  <participant typeCode="PRD">
                    <participantRole classCode="MANU">
                      <templateId root="2.16.840.1.113883.10.20.22.4.37" />
                                            <id root="24993f33-6222-41ce-add6-37a9d3da6acb" />
                      <playingDevice>
                        <code code="14106009" displayName="cardiac pacemaker, device (physical object)"
                          codeSystem="2.16.840.1.113883.6.96" codeSystemName="SNOMED CT">
                          <originalText>Cardiac Pacemaker</originalText>
                        </code>
                      </playingDevice>
                      <scopingEntity>
                        <id root="eb936010-7b17-11db-9fe1-0800200c9b65" />
                        <desc>Good Health Durable Medical Equipment</desc>
                      </scopingEntity>
                    </participantRole>
                  </participant>
                </supply>
              </component>
              <component>
                <supply classCode="SPLY" moodCode="EVN">
                  <templateId root="2.16.840.1.113883.10.20.22.4.50" extension="2014-06-09" />
                                    <id root="39b5f1b4-a8e1-4ad7-8849-0deab10c97b1" />
                  <statusCode code="completed" />
                  <effectiveTime xsi:type="IVL_TS">
                    <high value="20130103" />
                  </effectiveTime>
                  <quantity value="1" />
                  <participant typeCode="PRD">
                    <participantRole classCode="MANU">
                      <templateId root="2.16.840.1.113883.10.20.22.4.37" />
                                            <id root="24993f33-6222-41ce-add6-37a9d3da6acb" />
                      <playingDevice>
                        <code code="303406003" displayName="upper gastrointestinal prosthesis (physical object)"
                          codeSystem="2.16.840.1.113883.6.96" codeSystemName="SNOMED CT">
                          <originalText>Upper GI Prosthesis</originalText>
                        </code>
                      </playingDevice>
                      <scopingEntity>
                        <id root="eb936010-7b17-11db-9fe1-0800200c9b65" />
                        <desc>Good Health Durable Medical Equipment</desc>
                      </scopingEntity>
                    </participantRole>
                  </participant>
                </supply>
              </component>
            </organizer>
          </entry>
          <entry>
            <supply classCode="SPLY" moodCode="EVN">
              <templateId root="2.16.840.1.113883.10.20.22.4.50" extension="2014-06-09" />
                            <id root="cf75f5be-1da0-4256-8276-94b7fc73f9f9" />
              <statusCode code="completed" />
              <effectiveTime xsi:type="IVL_TS">
                <high value="20030202" />
              </effectiveTime>
              <quantity value="1" />
              <participant typeCode="PRD">
                <participantRole classCode="MANU">
                  <templateId root="2.16.840.1.113883.10.20.22.4.37" />
                                    <id root="24993f33-6222-41ce-add6-37a9d3da6acb" />
                  <playingDevice>
                    <code code="87405001" displayName="cane" codeSystem="2.16.840.1.113883.6.96"
                      codeSystemName="SNOMED CT">
                      <originalText>Upper GI Prosthesis</originalText>
                    </code>
                  </playingDevice>
                  <scopingEntity>
                    <id root="eb936010-7b17-11db-9fe1-0800200c9b65" />
                    <desc>Good Health Durable Medical Equipment</desc>
                  </scopingEntity>
                </participantRole>
              </participant>
            </supply>
          </entry>
          <entry>
            <procedure classCode="PROC" moodCode="EVN">
                            <templateId root="2.16.840.1.113883.10.20.22.4.14" extension="2014-06-09" />
              <id root="d5b614bd-01ce-410d-8726-e1fd01dcc72a" />
              <code code="103716009" codeSystem="2.16.840.1.113883.6.96" codeSystemName="SNOMED CT"
                displayName="Placement of stent">
                <originalText>
                  <reference value="#Proc1" />
                </originalText>
              </code>
              <statusCode code="completed" />
              <effectiveTime value="20130512" />
              <targetSiteCode code="28273000" displayName="Bile duct structure" codeSystem="2.16.840.1.113883.6.96"
                codeSystemName="SNOMED CT" />
              <specimen typeCode="SPC">
                <specimenRole classCode="SPEC">
                  <id root="a6d7b927-2b70-43c7-bdf3-0e7c4133062c" />
                  <specimenPlayingEntity>
                    <code code="57259009" codeSystem="2.16.840.1.113883.6.96" displayName="Gallbladder bile" />
                  </specimenPlayingEntity>
                </specimenRole>
              </specimen>
              <performer>
                <assignedEntity>
                  <id root="2.16.840.1.113883.19" extension="1234" />
                  <addr>
                    <streetAddressLine>1004 Health Care Drive</streetAddressLine>
                    <city>Ann Arbor</city>
                    <state>MI</state>
                    <postalCode>02368</postalCode>
                    <country>US</country>
                  </addr>
                  <telecom use="WP" value="tel: +1(555)-555-5004" />
                  <representedOrganization>
                    <id root="2.16.840.1.113883.19.5" />
                    <name>Community Health and Hospitals</name>
                    <telecom use="WP" value="tel:+1(555)-555-5005" />
                    <addr>
                      <streetAddressLine>1003 Health Care Drive</streetAddressLine>
                      <city>Ann Arbor</city>
                      <state>MI</state>
                      <postalCode>02368</postalCode>
                      <country>US</country>
                    </addr>
                  </representedOrganization>
                </assignedEntity>
              </performer>
            </procedure>
          </entry>
        </section>
      </component>
            <component>
        <section>
          <templateId root="2.16.840.1.113883.10.20.22.2.56" />
                    <code code="10190-7" codeSystem="2.16.840.1.113883.6.1" codeSystemName="LOINC" displayName="Mental status Narrative" />
          <title>MENTAL STATUS</title>
          <text>
            <table border="1" width="100%">
              <thead>
                <tr>
                  <th>Mental Status Findings</th>
                  <th>Effective Dates</th>
                  <th>Condition Status</th>
                </tr>
              </thead>
              <tbody>
                <tr>
                  <td>Mental Function</td>
                  <td>March 11, 2013</td>
                  <td>Impaired</td>
                </tr>
                <tr>
                  <td>Mental Function</td>
                  <td>March 11, 2013</td>
                  <td>Agressive Behavior</td>
                </tr>
                <tr>
                  <td>Mental Function</td>
                  <td>March 11, 2013</td>
                  <td>Difficulty understanding own emotions</td>
                </tr>
                <tr>
                  <td>Mental Function</td>
                  <td>March 11, 2013</td>
                  <td>Difficulty communicating Thoughts </td>
                </tr>
              </tbody>
            </table>
          </text>
          <entry>
            <observation classCode="OBS" moodCode="EVN">
                            <templateId root="2.16.840.1.113883.10.20.22.4.74" extension="2014-06-09" />
              <id root="c12ecaaf-53f8-4593-8f79-359aeaa3948b" />
              <code code="75275-8" displayName="Cognitive function [Interpretation]" codeSystem="2.16.840.1.113883.6.1"
                codeSystemName="LOINC" />
              <statusCode code="completed" />
              <effectiveTime value="20130311" />
              <value xsi:type="CD" code="11163003" displayName="Intact" codeSystem="2.16.840.1.113883.6.96"
                codeSystemName="SNOMED CT"> </value>
              <author>
                <templateId root="2.16.840.1.113883.10.20.22.4.119" />
                <time value="200130311" />
                <assignedAuthor>
                  <id extension="KP00017" root="2.16.840.1.113883.19.5" />
                  <addr>
                    <streetAddressLine>1003 Health Care Drive</streetAddressLine>
                    <city>Ann Arbor</city>
                    <state>MI</state>
                    <postalCode>02368</postalCode>
                    <country>US</country>
                  </addr>
                  <telecom use="WP" value="tel:+1(555)555-1003" />
                  <assignedPerson>
                    <name>
                      <given>Assigned</given>
                      <family>Amanda</family>
                    </name>
                  </assignedPerson>
                </assignedAuthor>
              </author>
            </observation>
          </entry>
          <entry>
            <observation classCode="OBS" moodCode="EVN">
                            <templateId root="2.16.840.1.113883.10.20.22.4.74" extension="2014-06-09" />
              <id root="c6b5a04b-2bf4-49d1-8336-636a3813df0a" />
              <code code="75275-8" displayName="Cognitive function [Interpretation]" codeSystem="2.16.840.1.113883.6.1"
                codeSystemName="LOINC" />
              <statusCode code="completed" />
              <effectiveTime value="20130311" />
              <value xsi:type="CD" code="61372001" displayName="Aggressive behavior" codeSystem="2.16.840.1.113883.6.96"
                codeSystemName="SNOMED CT"> </value>
              <author>
                <templateId root="2.16.840.1.113883.10.20.22.4.119" />
                <time value="200130311" />
                <assignedAuthor>
                  <id extension="KP00017" root="2.16.840.1.113883.19.5" />
                  <addr>
                    <streetAddressLine>1003 Health Care Drive</streetAddressLine>
                    <city>Ann Arbor</city>
                    <state>MI</state>
                    <postalCode>02368</postalCode>
                    <country>US</country>
                  </addr>
                  <telecom use="WP" value="tel:+1(555)555-1003" />
                  <assignedPerson>
                    <name>
                      <given>Assigned</given>
                      <family>Amanda</family>
                    </name>
                  </assignedPerson>
                </assignedAuthor>
              </author>
            </observation>
          </entry>
          <entry>
            <organizer classCode="CLUSTER" moodCode="EVN">
                            <templateId root="2.16.840.1.113883.10.20.22.4.75" extension="2014-06-09" />
              <id root="a7bc1062-8649-42a0-833d-ekd65bd013c9" />
              <code code="d3" displayName="Communication" codeSystem="2.16.840.1.113883.6.254" codeSystemName="ICF" />
              <statusCode code="completed" />
              <component>
                <observation classCode="OBS" moodCode="EVN">
                                    <templateId root="2.16.840.1.113883.10.20.22.4.74" extension="2014-06-09" />
                  <id root="c6b5a04b-2bf4-49d1-8336-636a3813df0a" />
                  <code code="75275-8" displayName="Cognitive function [Interpretation]" codeSystem="2.16.840.1.113883.6.1"
                    codeSystemName="LOINC" />
                  <statusCode code="completed" />
                  <effectiveTime value="20130311" />
                  <value xsi:type="CD" code="286569004" displayName="Difficulty understanding own emotions"
                    codeSystem="2.16.840.1.113883.6.96" codeSystemName="SNOMED CT"> </value>
                  <author>
                    <templateId root="2.16.840.1.113883.10.20.22.4.119" />
                    <time value="200130311" />
                    <assignedAuthor>
                      <id extension="KP00017" root="2.16.840.1.113883.19.5" />
                      <addr>
                        <streetAddressLine>1003 Health Care Drive</streetAddressLine>
                        <city>Ann Arbor</city>
                        <state>MI</state>
                        <postalCode>02368</postalCode>
                        <country>US</country>
                      </addr>
                      <telecom use="WP" value="tel:+1(555)555-1003" />
                      <assignedPerson>
                        <name>
                          <given>Assigned</given>
                          <family>Amanda</family>
                        </name>
                      </assignedPerson>
                    </assignedAuthor>
                  </author>
                </observation>
              </component>
              <component>
                <observation classCode="OBS" moodCode="EVN">
                                    <templateId root="2.16.840.1.113883.10.20.22.4.74" extension="2014-06-09" />
                  <id root="c6b5a04b-2bf4-49d1-8336-636a3813df0a" />
                  <code code="75275-8" displayName="Cognitive function [Interpretation]" codeSystem="2.16.840.1.113883.6.1"
                    codeSystemName="LOINC" />
                  <statusCode code="completed" />
                  <effectiveTime value="20130311" />
                  <value xsi:type="CD" code="288760009" displayName="Difficulty communicating thoughts"
                    codeSystem="2.16.840.1.113883.6.96" codeSystemName="SNOMED CT"> </value>
                  <author>
                    <templateId root="2.16.840.1.113883.10.20.22.4.119" />
                    <time value="200130311" />
                    <assignedAuthor>
                      <id extension="KP00017" root="2.16.840.1.113883.19.5" />
                      <addr>
                        <streetAddressLine>1003 Health Care Drive</streetAddressLine>
                        <city>Ann Arbor</city>
                        <state>MI</state>
                        <postalCode>02368</postalCode>
                        <country>US</country>
                      </addr>
                      <telecom use="WP" value="tel:+1(555)555-1003" />
                      <assignedPerson>
                        <name>
                          <given>Assigned</given>
                          <family>Amanda</family>
                        </name>
                      </assignedPerson>
                    </assignedAuthor>
                  </author>
                </observation>
              </component>
            </organizer>
          </entry>
        </section>
      </component>
            <component>
        <section>
          <templateId root="2.16.840.1.113883.10.20.22.2.57" />
          <code code="61144-2" codeSystem="2.16.840.1.113883.6.1" codeSystemName="LOINC"
            displayName="Diet and Nutrition" />
          <title>NUTRITION SECTION</title>
          <text>
            <paragraph>Nutritional Status: well nourished</paragraph>
            <paragraph>Diet: Low sodium diet, excessive carbohydrate </paragraph>
          </text>
          <entry>
            <observation classCode="OBS" moodCode="EVN">
                            <templateId root="2.16.840.1.113883.10.20.22.4.124" />
              <id root="c12ecaaf-53f8-4593-8f79-359aeaa3948b" />
              <code code="75305-3" displayName="Nutritional status" codeSystem="2.16.840.1.113883.6.1"
                codeSystemName="LOINC">
                <originalText>Nutritional Status</originalText>
              </code>
              <statusCode code="completed" />
              <effectiveTime value="20130512" />
              <value xsi:type="CD" code="248324001" codeSystem="2.16.840.1.113883.6.96" codeSystemName="SNOMED-CT"
                displayName="Well nourished" />
              <entryRelationship typeCode="SUBJ">
                <observation classCode="OBS" moodCode="EVN">
                                    <templateId root="2.16.840.1.113883.10.20.22.4.138" />
                  <id root="ab1791b0-5c71-11db-b0de-0800200c9a66" />
                  <code code="75303-8" displayName="Nutrition assessment Narrative" codeSystem="2.16.840.1.113883.6.1"
                    codeSystemName="LOINC" />
                  <statusCode code="completed" />
                  <effectiveTime value="20130512" />
                  <value xsi:type="CD" code="386619000" displayName="Low sodium diet (finding)"
                    codeSystem="2.16.840.1.113883.6.96" codeSystemName="SNOMED CT"> </value>
                  <author typeCode="AUT">
                    <templateId root="2.16.840.1.113883.10.20.22.4.119" />
                    <time value="20130730" />
                    <assignedAuthor>
                      <id extension="5555555555" root="2.16.840.1.113883.4.6" />
                      <code code="207QA0505X" displayName="Allopathic &amp; Osteopathic Physicians; Family Medicine, Adult Medicine" codeSystem="2.16.840.1.113883.6.101"
                        codeSystemName="Healthcare Provider Taxonomy (HIPAA)" />
                      <addr>
                        <streetAddressLine>1004 Healthcare Drive </streetAddressLine>
                        <city>Portland</city>
                        <state>OR</state>
                        <postalCode>99123</postalCode>
                        <country>US</country>
                      </addr>
                      <telecom use="WP" value="tel:+1(555)555-1004" />
                      <assignedPerson>
                        <name>
                          <given>Patricia</given>
                          <given qualifier="CL">Patty</given>
                          <family>Primary</family>
                          <suffix qualifier="AC">M.D.</suffix>
                        </name>
                      </assignedPerson>
                    </assignedAuthor>
                  </author>
                </observation>
              </entryRelationship>
              <entryRelationship typeCode="SUBJ">
                <observation classCode="OBS" moodCode="EVN">
                                    <templateId root="2.16.840.1.113883.10.20.22.4.138" />
                  <id root="ab1791b0-5c71-11db-b0de-0800200c9a66" />
                  <code code="75303-8" displayName="Nutrition assessment Narrative" codeSystem="2.16.840.1.113883.6.1"
                    codeSystemName="LOINC" />
                  <statusCode code="completed" />
                  <effectiveTime value="20130512" />
                  <value xsi:type="CD" code="430186007" displayName="excessive dietary carbohydrate intake(finding)"
                    codeSystem="2.16.840.1.113883.6.96" codeSystemName="SNOMED CT"> </value>
                  <author typeCode="AUT">
                    <templateId root="2.16.840.1.113883.10.20.22.4.119" />
                    <time value="20130730" />
                    <assignedAuthor>
                      <id extension="5555555555" root="2.16.840.1.113883.4.6" />
                      <code code="207QA0505X" displayName="Allopathic &amp; Osteopathic Physicians; Family Medicine, Adult Medicine" codeSystem="2.16.840.1.113883.6.101"
                        codeSystemName="Healthcare Provider Taxonomy (HIPAA)" />
                      <addr>
                        <streetAddressLine>1004 Healthcare Drive </streetAddressLine>
                        <city>Portland</city>
                        <state>OR</state>
                        <postalCode>99123</postalCode>
                        <country>US</country>
                      </addr>
                      <telecom use="WP" value="tel:+1(555)555-1004" />
                      <assignedPerson>
                        <name>
                          <given>Patricia</given>
                          <given qualifier="CL">Patty</given>
                          <family>Primary</family>
                          <suffix qualifier="AC">M.D.</suffix>
                        </name>
                      </assignedPerson>
                    </assignedAuthor>
                  </author>
                </observation>
              </entryRelationship>
            </observation>
          </entry>
        </section>
      </component>
            <component>
        <section>
                    <templateId root="2.16.840.1.113883.10.20.22.2.1.1" extension="2014-06-09" />
          <code code="10160-0" codeSystem="2.16.840.1.113883.6.1" codeSystemName="LOINC"
            displayName="HISTORY OF MEDICATION USE" />
          <title>MEDICATIONS</title>
          <text>
            <table border="1" width="100%">
              <thead>
                <tr>
                  <th>Current Medication</th>
                  <th>Directions</th>
                  <th>Start Date</th>
                  <th>Status</th>
                  <th>Indications</th>
                  <th>Monitored by</th>
                </tr>
              </thead>
              <tbody>
                <tr>
                  <td>Proventil 0.09 MG/ACTUAT inhalant solution</td>
                  <td>2 puffs q6 hours PRN wheezing</td>
                  <td>Jan 3, 2013</td>
                  <td>Active</td>
                  <td>Asthma</td>
                  <td>Penny Puffer, MD</td>
                </tr>
                <tr>
                  <td>Atenolol 25 MG Oral Tablet</td>
                  <td>1 every 12 hours Orally</td>
                  <td>Mar 18, 2013</td>
                  <td>Active</td>
                  <td>Hypertension</td>
                  <td />
                </tr>
              </tbody>
            </table>
          </text>
          <entry typeCode="DRIV">
            <substanceAdministration classCode="SBADM" moodCode="EVN">
                            <templateId root="2.16.840.1.113883.10.20.22.4.16" extension="2014-06-09" />
              <id root="cdbd33f0-6cde-11db-9fe1-0800200c9a66" />
              <statusCode code="active" />
              <effectiveTime xsi:type="IVL_TS">
                <low value="20130103" />
              </effectiveTime>
              <effectiveTime xsi:type="PIVL_TS" institutionSpecified="true" operator="A">
                <period value="6" unit="h" />
              </effectiveTime>
              <routeCode code="C38216" codeSystem="2.16.840.1.113883.3.26.1.1" codeSystemName="NCI Thesaurus"
                displayName="Inhalation Route of Administration" />
              <doseQuantity value="2" />
              <administrationUnitCode code="PUFF" displayName="Puff" codeSystem="2.16.840.1.113883.5.85"
                codeSystemName="orderableDrugForm" />
              <consumable>
                <manufacturedProduct classCode="MANU">
                                    <templateId root="2.16.840.1.113883.10.20.22.4.23" extension="2014-06-09" />
                  <id root="2a620155-9d11-439e-92b3-5d9815ff4ee8" />
                  <manufacturedMaterial>
                    <code code="573621" displayName="albuterol 0.09 MG/ACTUAT [Proventil]"
                      codeSystem="2.16.840.1.113883.6.88" codeSystemName="RxNorm" />
                  </manufacturedMaterial>
                  <manufacturerOrganization>
                    <name>Medication Factory Inc.</name>
                  </manufacturerOrganization>
                </manufacturedProduct>
              </consumable>
              <performer>
                <assignedEntity>
                  <id extension="5555555555" root="2.16.840.1.113883.4.6" />
                  <addr nullFlavor="UNK" />
                  <telecom use="WP" value="tel: +1(555)555-1004" />
                  <representedOrganization>
                    <id root="2.16.840.1.113883.19.5.9999.1393" />
                    <name>Community Health and Hospitals</name>
                    <telecom use="WP" value="tel: +1(555)555-5000" />
                    <addr nullFlavor="UNK" />
                  </representedOrganization>
                </assignedEntity>
              </performer>
              <participant typeCode="CSM">
                <participantRole classCode="MANU">
                                    <templateId root="2.16.840.1.113883.10.20.22.4.24" />
                  <code code="412307009" displayName="Drug vehicle" codeSystem="2.16.840.1.113883.6.96" />
                  <playingEntity classCode="MMAT">
                    <code code="324049" displayName="Aerosol" codeSystem="2.16.840.1.113883.6.88"
                      codeSystemName="RxNorm" />
                    <name>Aerosol</name>
                  </playingEntity>
                </participantRole>
              </participant>
              <entryRelationship typeCode="RSON">
                <observation classCode="OBS" moodCode="EVN">
                                    <templateId root="2.16.840.1.113883.10.20.22.4.19" extension="2014-06-09" />
                  <id root="db734647-fc99-424c-a864-7e3cda82e703" extension="45665" />
                  <code code="404684003" displayName="Clinical finding" codeSystem="2.16.840.1.113883.6.96"
                    codeSystemName="SNOMED CT" />
                  <statusCode code="completed" />
                  <effectiveTime>
                    <low value="20130103" />
                  </effectiveTime>
                  <value xsi:type="CD" code="195967001" displayName="Asthma" codeSystem="2.16.840.1.113883.6.96" />
                </observation>
              </entryRelationship>
              <entryRelationship typeCode="COMP">
                                <act classCode="ACT" moodCode="INT">
                  <templateId root="2.16.840.1.113883.10.20.22.4.123" />
                  <id root="2a620155-9d11-439e-92b3-5d9815ff4ee8" />
                  <code code="395170001" displayName="Medication monitoring"
                    codeSystem="2.16.840.1.113883.6.96" codeSystemName="SNOMED-CT" />
                  <statusCode code="completed" />
                  <effectiveTime xsi:type="IVL_TS">
                    <low value="20130615" />
                    <high value="20130715" />
                  </effectiveTime>
                  <participant typeCode="RESP">
                    <participantRole classCode="ASSIGNED">
                      <id root="2a620155-9d11-439e-92b3-5d9815ff4ee5" />
                      <playingEntity classCode="PSN">
                        <name>
                          <given>Puffer</given>
                          <family>Penny</family>
                          <prefix>DR</prefix>
                        </name>
                      </playingEntity>
                    </participantRole>
                  </participant>
                </act>
              </entryRelationship>
            </substanceAdministration>
          </entry>
          <entry typeCode="DRIV">
            <substanceAdministration classCode="SBADM" moodCode="EVN">
                            <templateId root="2.16.840.1.113883.10.20.22.4.16" extension="2014-06-09" />
              <id root="6c844c75-aa34-411c-b7bd-5e4a9f206e29" />
              <statusCode code="active" />
              <effectiveTime xsi:type="IVL_TS">
                <low value="20120318" />
              </effectiveTime>
              <effectiveTime xsi:type="PIVL_TS" institutionSpecified="true" operator="A">
                <period value="12" unit="h" />
              </effectiveTime>
              <routeCode code="C38288" codeSystem="2.16.840.1.113883.3.26.1.1" codeSystemName="NCI Thesaurus"
                displayName="Oral Route of Administration" />
              <doseQuantity value="1" />
              <consumable>
                <manufacturedProduct classCode="MANU">
                                    <templateId root="2.16.840.1.113883.10.20.22.4.23" extension="2014-06-09" />
                  <id root="2a620155-9d11-439e-92b3-5d9815ff4ee8" />
                  <manufacturedMaterial>
                    <code code="197380" displayName="atenolol 25 MG Oral Tablet" codeSystem="2.16.840.1.113883.6.88"
                      codeSystemName="RxNorm" />
                  </manufacturedMaterial>
                </manufacturedProduct>
              </consumable>
              <entryRelationship typeCode="RSON">
                <observation classCode="OBS" moodCode="EVN">
                                    <templateId root="2.16.840.1.113883.10.20.22.4.19" extension="2014-06-09" />
                  <id root="e63166c7-6482-4a44-83a1-37ccdbde725b" />
                  <code code="404684003" displayName="Clinical finding" codeSystem="2.16.840.1.113883.6.96"
                    codeSystemName="SNOMED CT" />
                  <statusCode code="completed" />
                  <value xsi:type="CD" code="38341003" displayName="Hypertensive disorder, systemic arterial" codeSystem="2.16.840.1.113883.6.96" />
                </observation>
              </entryRelationship>
            </substanceAdministration>
          </entry>
        </section>
      </component>
            <component>
        <section>
          <templateId root="2.16.840.1.113883.10.20.22.2.18" extension="2014-06-09" />
                    <code code="48768-6" codeSystem="2.16.840.1.113883.6.1" codeSystemName="LOINC" displayName="Payments" />
          <title>Insurance Providers</title>
          <text>
            <table border="1" width="100%">
              <thead>
                <tr>
                  <th>Payer name</th>
                  <th>Policy type / Coverage type</th>
                  <th>Policy ID</th>
                  <th>Covered party ID</th>
                  <th>Policy Holder</th>
                </tr>
              </thead>
              <tbody>
                <tr>
                  <td>Good Health Insurance</td>
                  <td>Extended healthcare / Family</td>
                  <td>Contract Number</td>
                  <td>1138345</td>
                  <td>Patient's Mother</td>
                </tr>
              </tbody>
            </table>
          </text>
          <entry typeCode="DRIV">
            <act classCode="ACT" moodCode="EVN">
              <templateId root="2.16.840.1.113883.10.20.22.4.60" extension="2014-06-09" />
                            <id root="1fe2cdd0-7aad-11db-9fe1-0800200c9a66" />
              <code code="48768-6" codeSystem="2.16.840.1.113883.6.1" codeSystemName="LOINC"
                displayName="Payment sources" />
              <statusCode code="completed" />
              <entryRelationship typeCode="COMP">
                <act classCode="ACT" moodCode="EVN">
                  <templateId root="2.16.840.1.113883.10.20.22.4.61" extension="2014-06-09" />
                                    <id root="3e676a50-7aac-11db-9fe1-0800200c9a66" />
                  <code code="SELF" codeSystemName="HL7 RoleClassRelationship" codeSystem="2.16.840.1.113883.5.110" />
                  <statusCode code="completed" />
                                    <performer typeCode="PRF">
                    <templateId root="2.16.840.1.113883.10.20.22.4.87" />
                    <assignedEntity>
                      <id root="2.16.840.1.113883.19" />
                      <code code="PAYOR" codeSystem="2.16.840.1.113883.5.110" codeSystemName="HL7 RoleClassRelationship" displayName="invoice payor" />
                      <addr use="WP">
                                                <streetAddressLine>123 Insurance Road</streetAddressLine>
                        <city>Blue Bell</city>
                        <state>MA</state>
                        <postalCode>02368</postalCode>
                        <country>US</country>
                                              </addr>
                      <telecom value="tel:+1(781)555-1515" use="WP" />
                      <representedOrganization>
                        <name>Good Health Insurance</name>
                        <telecom />
                        <addr />
                      </representedOrganization>
                    </assignedEntity>
                  </performer>
                                    <performer typeCode="PRF">
                    <time />
                    <assignedEntity>
                      <id root="329fcdf0-7ab3-11db-9fe1-0800200c9a66" />
                      <code code="GUAR" codeSystem="2.16.840.1.113883.5.110" codeSystemName="HL7 RoleClassRelationship" displayName="guarantor" />
                      <addr use="HP">
                                                <streetAddressLine>17 Daws Rd.</streetAddressLine>
                        <city>Blue Bell</city>
                        <state>MA</state>
                        <postalCode>02368</postalCode>
                        <country>US</country>
                                              </addr>
                      <telecom value="tel:+1(781)555-1212" use="HP" />
                      <assignedPerson>
                        <name>
                          <prefix>Mr.</prefix>
                          <given>Adam</given>
                          <given>Frankie</given>
                          <family>Everyman</family>
                        </name>
                      </assignedPerson>
                    </assignedEntity>
                  </performer>
                  <participant typeCode="COV">
                                        <templateId root="2.16.840.1.113883.10.20.22.4.89" />
                    <time>
                      <low nullFlavor="UNK" />
                      <high nullFlavor="UNK" />
                    </time>
                    <participantRole classCode="PAT">
                      <id root="14d4a520-7aae-11db-9fe1-0800200c9a66" extension="1138345" />
                                            <code code="SELF" codeSystem="2.16.840.1.113883.5.111" displayName="self" />
                      <addr use="HP">
                                                <streetAddressLine>17 Daws Rd.</streetAddressLine>
                        <city>Blue Bell</city>
                        <state>MA</state>
                        <postalCode>02368</postalCode>
                      </addr>
                      <playingEntity>
                        <name>
                                                    <prefix>Mr.</prefix>
                          <given>Frank</given>
                          <given>A.</given>
                          <family>Everyman</family>
                        </name>
                        <sdtc:birthTime value="19750501" />
                      </playingEntity>
                    </participantRole>
                  </participant>
                  <participant typeCode="HLD">
                    <templateId root="2.16.840.1.113883.10.20.22.4.90" />
                    <participantRole>
                      <id extension="1138345" root="2.16.840.1.113883.19" />
                      <addr use="HP">
                        <streetAddressLine>17 Daws Rd.</streetAddressLine>
                        <city>Blue Bell</city>
                        <state>MA</state>
                        <postalCode>02368</postalCode>
                        <country>US</country>
                      </addr>
                    </participantRole>
                  </participant>
                  <entryRelationship typeCode="REFR">
                    <act classCode="ACT" moodCode="EVN">
                      <templateId root="2.16.840.1.113883.10.20.1.19" />
                                            <id root="f4dce790-8328-11db-9fe1-0800200c9a66" />
                      <code nullFlavor="NA" />
                      <entryRelationship typeCode="SUBJ">
                        <procedure classCode="PROC" moodCode="PRMS">
                          <code code="73761001" codeSystem="2.16.840.1.113883.6.96" displayName="Colonoscopy" />
                        </procedure>
                      </entryRelationship>
                    </act>
                  </entryRelationship>
                </act>
              </entryRelationship>
            </act>
          </entry>
        </section>
      </component>
            <component>
        <section>
          <templateId root="2.16.840.1.113883.10.20.2.10" extension="2014-06-09" />
          <code codeSystem="2.16.840.1.113883.6.1" codeSystemName="LOINC" code="29545-1" displayName="Physical Findings" />
          <title>Physical Examination</title>
          <text> Key Findings: In addition to Assessment finding above. 1. Type II diabetes, uncontrolled. 2. Slow
            healing Open wound on left knee. 3. Early signs of peripheral neuropathy. 4. Mild dysphagia. 5. Mild
            footdrop. </text>
                              <component>
            <section>
                            <templateId root="2.16.840.1.113883.10.20.22.2.63" />
              <code codeSystem="2.16.840.1.113883.6.1" codeSystemName="LOINC" code="8709-8"
                displayName="Physical findings of Skin" />
              <title>SKIN, PHYSICAL FINDING</title>
              <text>
                <list listType="ordered">
                  <item>Stage 3 Pressure Ulcer anterior aspect of knee<br />
                    <content>Measuring 1"W X 2"L</content>
                    <content>Wound Characteristic: Offensive wound odor</content>
                    <content>Three Stage 3 pressure ulcers.</content>
                    <content>Worst pressure ulcer with necrotic eschar.</content>
                  </item>
                </list>
              </text>
              <entry>
                <observation classCode="OBS" moodCode="EVN">
                                    <templateId root="2.16.840.1.113883.10.20.22.4.114" />
                  <id root="ab1791b0-5c71-11db-b0de-0800200c9a66" />
                  <code code="ASSERTION" codeSystem="2.16.840.1.113883.5.4" />
                  <statusCode code="completed" />
                  <effectiveTime>
                    <low value="421927004" />
                  </effectiveTime>
                  <value xsi:type="CD" code="425144005" codeSystem="2.16.840.1.113883.6.6"
                    displayName="pressure ulcer stage 3" />
                  <targetSiteCode code="182295001" codeSystem="2.16.840.1.113883.6.96"
                    displayName="Structure of anterior aspect of knee"> </targetSiteCode>
                  <author typeCode="AUT">
                    <templateId root="2.16.840.1.113883.10.20.22.4.119" />
                    <time value="200130311" />
                    <assignedAuthor>
                      <id extension="KP00017" root="2.16.840.1.113883.19.5" />
                      <addr>
                        <streetAddressLine>1003 Health Care Drive</streetAddressLine>
                        <city>Ann Arbor</city>
                        <state>MI</state>
                        <postalCode>02368</postalCode>
                        <country>US</country>
                      </addr>
                      <telecom use="WP" value="tel:+1(555)555-1003" />
                      <assignedPerson>
                        <name>
                          <given>Assigned</given>
                          <family>Amanda</family>
                        </name>
                      </assignedPerson>
                    </assignedAuthor>
                  </author>
                  <entryRelationship typeCode="COMP">
                                        <observation classCode="OBS" moodCode="EVN">
                      <templateId root="2.16.840.1.113883.10.20.22.4.133" />
                      <id root="a4d03aa6-43b7-44a8-bf62-ccc68877bd82" />
                      <code code="401239006" codeSystem="2.16.840.1.113883.6.96" displayName="Width of Wound" />
                      <statusCode code="completed" />
                      <effectiveTime value="20013103" />
                      <value xsi:type="PQ" value="1" unit="[in_i]" />
                    </observation>
                  </entryRelationship>
                  <entryRelationship typeCode="COMP">
                                        <observation classCode="OBS" moodCode="EVN">
                      <templateId root="2.16.840.1.113883.10.20.22.4.133" />
                      <id root="d2b46280-eb34-11e2-91e2-0800200c9a66" />
                      <code code=" 401238003" codeSystem="2.16.840.1.113883.6.96" displayName="Length of Wound" />
                      <statusCode code="completed" />
                      <effectiveTime value="20013103" />
                      <value xsi:type="PQ" value="2" unit="[in_i]" />
                    </observation>
                  </entryRelationship>
                  <entryRelationship typeCode="COMP">
                                        <observation classCode="OBS" moodCode="EVN">
                      <templateId root="2.16.840.1.113883.10.20.22.4.134" />
                      <id root="763428a0-eb35-11e2-91e2-0700200c9a66" />
                      <code code="ASSERTION" codeSystem="2.16.840.1.113883.5.4" />
                      <statusCode code="completed" />
                      <effectiveTime value="20013103" />
                      <value xsi:type="CD" code="447547000" displayName="Offensive wound odor"
                        codeSystem="2.16.840.1.113883.6.96" codeSystemName="SNOMED-CT" />
                    </observation>
                  </entryRelationship>
                  <entryRelationship typeCode="COMP">
                                        <observation classCode="OBS" moodCode="EVN">
                      <templateId root="2.16.840.1.113883.10.20.22.4.76" extension="2014-06-09" />
                      <id root="08edb7c0-2111-43f2-a784-9a5fdfaa67f0" />
                      <code code="75277-4" codeSystem="2.16.840.1.113883.6.1" displayName="Number of pressure ulcers" />
                      <statusCode code="completed" />
                      <effectiveTime value="20013103" />
                      <value xsi:type="INT" value="3" />
                      <entryRelationship typeCode="SUBJ">
                        <observation classCode="OBS" moodCode="EVN">
                          <code code="ASSERTION" codeSystem="2.16.840.1.113883.5.4" />
                          <value xsi:type="CD" code="421927004" codeSystem="2.16.840.1.113883.6.96"
                            displayName="Pressure ulcer stage 3" />
                        </observation>
                      </entryRelationship>
                    </observation>
                  </entryRelationship>
                  <entryRelationship typeCode="COMP">
                                        <observation classCode="OBS" moodCode="EVN">
                      <templateId root="2.16.840.1.113883.10.20.22.4.77" />
                      <id root="08edb7c0-2111-43f2-a784-9a5fdfaa67f0" />
                      <code code="420905001" codeSystem="2.16.840.1.113883.6.96"
                        displayName=" Highest Pressure Ulcer Stage" />
                      <statusCode code="completed" />
                      <value xsi:type="CD" code="421306004" codeSystem="2.16.840.1.113883.6.96"
                        displayName="Necrotic eschar" />
                    </observation>
                  </entryRelationship>
                </observation>
              </entry>
            </section>
          </component>
        </section>
      </component>
            <component>
        <section>
          <templateId root="2.16.840.1.113883.10.20.22.2.10" extension="2014-06-09" />
                    <code code="18776-5" codeSystem="2.16.840.1.113883.6.1" codeSystemName="LOINC" displayName="Plan of care note" />
          <title>TREATMENT PLAN</title>
          <text>
            <content styleCode="Bold"> Hand-off Communication:</content>
            <content>Nurse Florence, RN to Nancy Nightingale, RN</content>
            <br />
            <br />
            <table border="1" width="100%">
              <thead>
                <tr>
                  <th>Planned Care</th>
                  <th>Start Date</th>
                  <th>Patient Provider Rating</th>
                  <th>Provider Provider Rating</th>
                  <th>Provider</th>
                  <th>Patient Support/Caregiver</th>
                </tr>
              </thead>
              <tbody>
                <tr>
                  <td>Encounter for Check-up</td>
                  <td>June 15, 2013</td>
                  <td>1st, Normal Priority</td>
                  <td>3rd, Normal Priority</td>
                  <td>Dr. James Case</td>
                  <td />
                </tr>
                <tr>
                  <td>Care Goal: Pulse Oximetry 95%</td>
                  <td>June 15, 2013</td>
                  <td>1st, Normal Priority</td>
                  <td>3rd, Normal Priority</td>
                  <td />
                  <td>Caregiver: Mother</td>
                </tr>
                <tr>
                  <td>Treatment: Wound Care</td>
                  <td>June 15, 2013</td>
                  <td>1st, Normal Priority</td>
                  <td>3rd, Normal Priority</td>
                  <td />
                  <td>Caregiver: Mother</td>
                </tr>
                <tr>
                  <td>Nutrition Recommendation: Education</td>
                  <td>June 13, 2013</td>
                  <td />
                  <td />
                  <td />
                  <td />
                </tr>
                <tr>
                  <td>Procedure: Colonoscopy</td>
                  <td>June 15, 2013</td>
                  <td>1st, Normal Priority</td>
                  <td>3rd, Normal Priority</td>
                  <td />
                  <td>Caregiver: Mother</td>
                </tr>
                <tr>
                  <td>Medication: Heparin 0.25 ml pre-filled syringe</td>
                  <td>July 12, 2013</td>
                  <td>1st, Normal Priority</td>
                  <td>3rd, Normal Priority</td>
                  <td />
                  <td>Caregiver: Mother</td>
                </tr>
                <tr>
                  <td>Supply: 0.25 ML Heparin sodium 10000 UNT/ML Prefilled Syringe</td>
                  <td>June 15, 2013</td>
                  <td />
                  <td />
                  <td>Dr. Henry Seven</td>
                  <td />
                </tr>
                <tr>
                  <td>Immunization: Influenza virus vaccine, IM</td>
                  <td>November 15, 2013</td>
                  <td>1st, Normal Priority</td>
                  <td>3rd, Normal Priority</td>
                  <td>Dr. Henry Seven</td>
                  <td />
                </tr>
              </tbody>
            </table>
          </text>
          <entry>
            <act classCode="ACT" moodCode="EVN">
              <templateId root="2.16.840.1.113883.10.20.22.4.141" />
              <code code="432138007" codeSystem="2.16.840.1.113883.6.96" codeSystemName="SNOMED CT"
                displayName="Handoff communication (procedure)" />
              <statusCode code="completed" />
              <effectiveTime value="20130712" />
              <author typeCode="AUT">
                <templateId root="2.16.840.1.113883.10.20.22.4.119" />
                <time value="20130730" />
                <assignedAuthor>
                  <id root="d839038b-7171-4165-a760-467925b43857" />
                  <code code="163W00000X" codeSystem="2.16.840.1.113883.6.101"
                    codeSystemName="Healthcare Provider Taxonomy (HIPAA)" displayName="Nursing Service Providers; Registered Nurse" />
                  <assignedPerson>
                    <name>
                      <given>Nurse</given>
                      <family>Florence</family>
                      <suffix>RN</suffix>
                    </name>
                  </assignedPerson>
                </assignedAuthor>
              </author>
              <participant typeCode="IRCP">
                <participantRole>
                  <id extension="1138345" root="2.16.840.1.113883.19" />
                  <code code="163W00000X" codeSystem="2.16.840.1.113883.6.101"
                    codeSystemName="NUCC Health Care Provider Taxonomy" displayName="Nursing Service Providers; Registered Nurse" />
                  <addr>
                    <streetAddressLine>1006 Health Drive</streetAddressLine>
                    <city>Ann Arbor</city>
                    <state>MI</state>
                    <postalCode>97867</postalCode>
                    <country>US</country>
                  </addr>
                  <telecom value="tel:+1(555)555-1014" use="WP" />
                  <playingEntity>
                    <name>
                      <family>Nancy</family>
                      <given>Nightingale</given>
                      <suffix>RN</suffix>
                    </name>
                  </playingEntity>
                </participantRole>
              </participant>
            </act>
          </entry>
          <entry>
            <encounter moodCode="INT" classCode="ENC">
              <templateId root="2.16.840.1.113883.10.20.22.4.40" extension="2014-06-09" />
                            <id root="9a6d1bac-17d3-4195-89a4-1121bc809b4d" />
              <code code="185349003" displayName="Encounter for check up (procedure)" codeSystemName="SNOMED CT"
                codeSystem="2.16.840.1.113883.6.96"> </code>
              <statusCode code="active" />
              <effectiveTime value="20130615" />
              <performer>
                <assignedEntity>
                  <id root="2a620155-9d11-439e-92a3-5d9815ff4de8" />
                  <code code="158965000" codeSystem="2.16.840.1.113883.6.96" codeSystemName="SNOMED-CT"
                    displayName="Medical practitioner" />
                  <addr>
                    <streetAddressLine>1006 Health Drive</streetAddressLine>
                    <city>Ann Arbor</city>
                    <state>MI</state>
                    <postalCode>97867</postalCode>
                    <country>US</country>
                  </addr>
                  <telecom value="tel:+1(995)555-1006" use="WP" />
                  <assignedPerson>
                    <name>
                      <prefix>Dr.</prefix>
                      <family>James</family>
                      <given>Case</given>
                    </name>
                  </assignedPerson>
                </assignedEntity>
              </performer>
              <author typeCode="AUT">
                <templateId root="2.16.840.1.113883.10.20.22.4.119" />
                <time value="20130730" />
                <assignedAuthor>
                  <id extension="5555555555" root="2.16.840.1.113883.4.6" />
                  <code code="207QA0505X" displayName="Allopathic &amp; Osteopathic Physicians; Family Medicine, Adult Medicine" codeSystem="2.16.840.1.113883.6.101"
                    codeSystemName="Healthcare Provider Taxonomy (HIPAA)" />
                  <addr>
                    <streetAddressLine>1004 Healthcare Drive </streetAddressLine>
                    <city>Portland</city>
                    <state>OR</state>
                    <postalCode>99123</postalCode>
                    <country>US</country>
                  </addr>
                  <telecom use="WP" value="tel:+1(555)555-1004" />
                  <assignedPerson>
                    <name>
                      <given>Patricia</given>
                      <given qualifier="CL">Patty</given>
                      <family>Primary</family>
                      <suffix qualifier="AC">M.D.</suffix>
                    </name>
                  </assignedPerson>
                </assignedAuthor>
              </author>
              <entryRelationship typeCode="REFR">
                <observation classCode="OBS" moodCode="EVN">
                                    <templateId root="2.16.840.1.113883.10.20.22.4.143" />
                  <id root="9a6d1bac-17d3-4195-89a4-1121bc809b4d" />
                  <code code="225773000" codeSystem="2.16.840.1.113883.6.96" codeSystemName="SNOMED CT"
                    displayName="preference" />
                  <effectiveTime value="20130615" />
                  <priorityCode code="255216001" codeSystem="2.16.840.1.113883.6.96" codeSystemName="SNOMED CT"
                    displayName="First" />
                  <value xsi:type="CD" code="394848005" codeSystem="2.16.840.1.113883.6.96"
                    displayName="Normal priority" />
                </observation>
              </entryRelationship>
              <entryRelationship typeCode="REFR">
                <observation classCode="OBS" moodCode="EVN">
                                    <templateId root="2.16.840.1.113883.10.20.22.4.143" />
                  <id root="9a6d1bac-17d3-4195-89a4-1121bc809b4d" />
                  <code code="225773000" codeSystem="2.16.840.1.113883.6.96" codeSystemName="SNOMED CT"
                    displayName="preference" />
                  <effectiveTime value="20130615" />
                  <value xsi:type="CD" code="394848005" codeSystem="2.16.840.1.113883.6.96"
                    displayName="Normal priority" />
                  <author typeCode="AUT">
                    <templateId root="2.16.840.1.113883.10.20.22.4.119" />
                    <time value="20130730" />
                    <assignedAuthor>
                      <id extension="5555555555" root="2.16.840.1.113883.4.6" />
                      <code code="207QA0505X" displayName="Allopathic &amp; Osteopathic Physicians; Family Medicine, Adult Medicine" codeSystem="2.16.840.1.113883.6.101"
                        codeSystemName="Healthcare Provider Taxonomy (HIPAA)" />
                      <addr>
                        <streetAddressLine>1004 Healthcare Drive </streetAddressLine>
                        <city>Portland</city>
                        <state>OR</state>
                        <postalCode>99123</postalCode>
                        <country>US</country>
                      </addr>
                      <telecom use="WP" value="tel:+1(555)555-1004" />
                      <assignedPerson>
                        <name>
                          <given>Patricia</given>
                          <given qualifier="CL">Patty</given>
                          <family>Primary</family>
                          <suffix qualifier="AC">M.D.</suffix>
                        </name>
                      </assignedPerson>
                    </assignedAuthor>
                  </author>
                </observation>
              </entryRelationship>
            </encounter>
          </entry>
          <entry>
                                    <observation classCode="OBS" moodCode="GOL">
                            <templateId root="2.16.840.1.113883.10.20.22.4.121" />
              <id root="3700b3b0-fbed-11e2-b778-0800200c9a66" />
              <code code="44616-1" codeSystem="2.16.840.1.113883.6.1" codeSystemName="LOINC"
                displayName="Pulse oximetry panel" />
              <statusCode code="active" />
              <effectiveTime value="20130902" />
              <value xsi:type="IVL_PQ">
                <low value="92" unit="%" />
              </value>
                                          <author>
                <templateId root="2.16.840.1.113883.10.20.22.4.119" />
                <time value="20130730" />
                <assignedAuthor>
                  <id root="d839038b-7171-4165-a760-467925b43857" />
                  <code code="163W00000X" displayName="Nursing Service Providers; Registered Nurse" codeSystem="2.16.840.1.113883.6.101"
                    codeSystemName="Healthcare Provider Taxonomy (HIPAA)" />
                  <assignedPerson>
                    <name>
                      <given>Nurse</given>
                      <family>Florence</family>
                      <suffix>RN</suffix>
                    </name>
                  </assignedPerson>
                </assignedAuthor>
              </author>
                            <author typeCode="AUT">
                <templateId root="2.16.840.1.113883.10.20.22.4.119" />
                <time />
                <assignedAuthor>
                                                      <id extension="996-756-495" root="2.16.840.1.113883.19.5" />
                </assignedAuthor>
              </author>
                            <entryRelationship typeCode="REFR">
                                <act classCode="ACT" moodCode="EVN">
                  <templateId root="2.16.840.1.113883.10.20.22.4.122" />
                                    <id root="4eab0e52-dd7d-4285-99eb-72d32ddb195c" />
                  <code nullFlavor="NP" />
                  <statusCode code="completed" />
                </act>
              </entryRelationship>
                            <entryRelationship typeCode="RSON">
                                <observation classCode="OBS" moodCode="EVN">
                  <templateId root="2.16.840.1.113883.10.20.22.4.143" />
                  <id root="a5b64706-9438-4d13-8dcf-651da3ef83bf" />
                  <code code="225773000" codeSystem="2.16.840.1.113883.6.96" codeSystemName="SNOMED CT"
                    displayName="Preference" />
                  <value xsi:type="CD" code="394849002" codeSystem="2.16.840.1.113883.6.96" codeSystemName="SNOMED CT"
                    displayName="High priority" />
                                    <author>
                    <templateId root="2.16.840.1.113883.10.20.22.4.119" />
                    <time value="20130801" />
                    <assignedAuthor>
                                            <id extension="444222222" root="2.16.840.1.113883.4.1" />
                    </assignedAuthor>
                  </author>
                </observation>
              </entryRelationship>
                            <entryRelationship typeCode="RSON">
                                <observation classCode="OBS" moodCode="EVN">
                  <templateId root="2.16.840.1.113883.10.20.22.4.143" />
                  <id root="7d66f448-ba82-4291-a9da-9e5db5e58803" />
                  <code code="225773000" codeSystem="2.16.840.1.113883.6.96" codeSystemName="SNOMED CT"
                    displayName="preference" />
                  <value xsi:type="CD" code="394849002" codeSystem="2.16.840.1.113883.6.96" codeSystemName="SNOMED"
                    displayName="High priority" />
                                    <author>
                    <templateId root="2.16.840.1.113883.10.20.22.4.119" />
                    <time value="20130801" />
                    <assignedAuthor>
                                            <id root="20cf14fb-b65c-4c8c-a54d-b0cca834c18c" />
                    </assignedAuthor>
                  </author>
                </observation>
              </entryRelationship>
            </observation>
          </entry>
          <entry>
            <act moodCode="INT" classCode="ACT">
              <templateId root="2.16.840.1.113883.10.20.22.4.39" extension="2014-06-09" />
                            <id root="9a6d1bac-17d3-4195-89a4-1121bc809a5c" />
              <code code="225358003" displayName="Wound care (regime/therapy)" codeSystem="2.16.840.1.113883.6.96"
                codeSystemName="SNOMED CT" />
              <statusCode code="active" />
              <effectiveTime value="20130615" />
              <participant typeCode="IND">
                <participantRole classCode="IND">
                  <code code="MTH" codeSystem="2.16.840.1.113883.5.111" displayName="mother" />
                </participantRole>
              </participant>
                            <entryRelationship typeCode="RSON">
                                <observation classCode="OBS" moodCode="EVN">
                  <templateId root="2.16.840.1.113883.10.20.22.4.143" />
                  <id root="a5b64706-9438-4d13-8dcf-651da3ef83bf" />
                  <code code="225773000" codeSystem="2.16.840.1.113883.6.96" codeSystemName="SNOMED CT"
                    displayName="Preference" />
                  <value xsi:type="CD" code="394849002" codeSystem="2.16.840.1.113883.6.96" codeSystemName="SNOMED CT"
                    displayName="High priority" />
                                    <author>
                    <templateId root="2.16.840.1.113883.10.20.22.4.119" />
                    <time value="20130801" />
                    <assignedAuthor>
                                            <id extension="444222222" root="2.16.840.1.113883.4.1" />
                    </assignedAuthor>
                  </author>
                </observation>
              </entryRelationship>
              <entryRelationship typeCode="REFR">
                <observation classCode="OBS" moodCode="EVN">
                                    <templateId root="2.16.840.1.113883.10.20.22.4.143" />
                  <id root="9a6d1bac-17d3-4195-89a4-1121bc809b4d" />
                  <code code="225773000" codeSystem="2.16.840.1.113883.6.96" codeSystemName="SNOMED CT"
                    displayName="preference" />
                  <effectiveTime value="20130615" />
                  <value xsi:type="CD" code="394848005" codeSystem="2.16.840.1.113883.6.96"
                    displayName="Normal priority" />
                  <author typeCode="AUT">
                    <templateId root="2.16.840.1.113883.10.20.22.4.119" />
                    <time value="20130730" />
                    <assignedAuthor>
                      <id extension="5555555555" root="2.16.840.1.113883.4.6" />
                      <code code="207QA0505X" displayName="Allopathic &amp; Osteopathic Physicians; Family Medicine, Adult Medicine" codeSystem="2.16.840.1.113883.6.101"
                        codeSystemName="Healthcare Provider Taxonomy (HIPAA)" />
                      <addr>
                        <streetAddressLine>1004 Healthcare Drive </streetAddressLine>
                        <city>Portland</city>
                        <state>OR</state>
                        <postalCode>99123</postalCode>
                        <country>US</country>
                      </addr>
                      <telecom use="WP" value="tel:+1(555)555-1004" />
                      <assignedPerson>
                        <name>
                          <given>Patricia</given>
                          <given qualifier="CL">Patty</given>
                          <family>Primary</family>
                          <suffix qualifier="AC">M.D.</suffix>
                        </name>
                      </assignedPerson>
                    </assignedAuthor>
                  </author>
                </observation>
              </entryRelationship>
            </act>
          </entry>
          <entry>
            <act moodCode="INT" classCode="ACT">
                            <templateId root="2.16.840.1.113883.10.20.22.4.130" />
              <id root="9a6d1bac-17d3-4195-89a4-1121bc809a5c" />
              <code code="61310001" displayName="Nutrition education" codeSystem="2.16.840.1.113883.6.96"
                codeSystemName="SNOMED CT" />
              <statusCode code="active" />
              <effectiveTime value="20130512" />
            </act>
          </entry>
          <entry>
            <procedure moodCode="RQO" classCode="PROC">
              <templateId root="2.16.840.1.113883.10.20.22.4.41" extension="2014-06-09" />
                            <id root="9a6d1bac-17d3-4195-89c4-1121bc809b5a" />
              <code code="73761001" codeSystem="2.16.840.1.113883.6.96" codeSystemName="SNOMED CT"
                displayName="Colonoscopy" />
              <statusCode code="active" />
              <effectiveTime value="20130613" />
                            <author typeCode="AUT">
                <templateId root="2.16.840.1.113883.10.20.22.4.119" />
                <time value="20130801" />
                <assignedAuthor>
                                    <id extension="5555555555" root="2.16.840.1.113883.4.6" />
                </assignedAuthor>
              </author>
              <entryRelationship typeCode="COMP">
                                <act classCode="ACT" moodCode="INT">
                  <templateId root="2.16.840.1.113883.10.20.22.4.129" />
                  <id root="03f5e10b-7e79-4610-9626-d2984ff10cc1" />
                  <code code="48768-6" codeSystem="2.16.840.1.113883.6.1" codeSystemName="LOINC"
                    displayName="Payment Sources" />
                  <statusCode code="active" />
                  <entryRelationship typeCode="COMP">
                    <act classCode="ACT" moodCode="INT">
                                            <id root="4c9a3be1-5f09-46dd-88e7-14c8ec612e4c" />
                      <code code="111" codeSystem="2.16.840.1.113883.3.221.5"
                        codeSystemName="Source of Payment Typology Health Insurance Type Code List"
                        displayName="Medicare HMO" />
                      <statusCode code="active" />
                    </act>
                  </entryRelationship>
                </act>
              </entryRelationship>
            </procedure>
          </entry>
          <entry>
            <substanceAdministration moodCode="INT" classCode="SBADM">
              <templateId root="2.16.840.1.113883.10.20.22.4.42" extension="2014-06-09" />
                            <id root="cdbd33f0-6cde-11db-9fe1-0800200c9a66" />
              <text>Heparin 0.25 ml Pre-filled Syringe</text>
              <statusCode code="active" />
                            <effectiveTime value="20130905" />
              <consumable>
                <manufacturedProduct classCode="MANU">
                                    <templateId root="2.16.840.1.113883.10.20.22.4.23" extension="2014-06-09" />
                  <id root="2a620155-9d11-439e-92b3-5d9815ff4ee8" />
                  <manufacturedMaterial>
                    <code code="829888" codeSystem="2.16.840.1.113883.3.88.12.80.17"
                      displayName="0.25 ML Heparin sodium 10000 UNT/ML Prefilled Syringe" />
                  </manufacturedMaterial>
                  <manufacturerOrganization>
                    <name>Medication Factory Inc.</name>
                  </manufacturerOrganization>
                </manufacturedProduct>
              </consumable>
                            <entryRelationship typeCode="RSON">
                                <observation classCode="OBS" moodCode="EVN">
                  <templateId root="2.16.840.1.113883.10.20.22.4.143" />
                  <id root="a5b64706-9438-4d13-8dcf-651da3ef83bf" />
                  <code code="225773000" codeSystem="2.16.840.1.113883.6.96" codeSystemName="SNOMED CT"
                    displayName="Preference" />
                  <value xsi:type="CD" code="394849002" codeSystem="2.16.840.1.113883.6.96" codeSystemName="SNOMED CT"
                    displayName="High priority" />
                                    <author>
                    <templateId root="2.16.840.1.113883.10.20.22.4.119" />
                    <time value="20130801" />
                    <assignedAuthor>
                                            <id extension="444222222" root="2.16.840.1.113883.4.1" />
                    </assignedAuthor>
                  </author>
                </observation>
              </entryRelationship>
              <entryRelationship typeCode="REFR">
                <observation classCode="OBS" moodCode="EVN">
                                    <templateId root="2.16.840.1.113883.10.20.22.4.143" />
                  <id root="9a6d1bac-17d3-4195-89a4-1121bc809b4d" />
                  <code code="225773000" codeSystem="2.16.840.1.113883.6.96" codeSystemName="SNOMED CT"
                    displayName="preference" />
                  <effectiveTime value="20130615" />
                  <value xsi:type="CD" code="394848005" codeSystem="2.16.840.1.113883.6.96"
                    displayName="Normal priority" />
                  <author typeCode="AUT">
                    <templateId root="2.16.840.1.113883.10.20.22.4.119" />
                    <time value="20130730" />
                    <assignedAuthor>
                      <id extension="5555555555" root="2.16.840.1.113883.4.6" />
                      <code code="207QA0505X" displayName="Allopathic &amp; Osteopathic Physicians; Family Medicine, Adult Medicine" codeSystem="2.16.840.1.113883.6.101"
                        codeSystemName="Healthcare Provider Taxonomy (HIPAA)" />
                      <addr>
                        <streetAddressLine>1004 Healthcare Drive </streetAddressLine>
                        <city>Portland</city>
                        <state>OR</state>
                        <postalCode>99123</postalCode>
                        <country>US</country>
                      </addr>
                      <telecom use="WP" value="tel:+1(555)555-1004" />
                      <assignedPerson>
                        <name>
                          <given>Patricia</given>
                          <given qualifier="CL">Patty</given>
                          <family>Primary</family>
                          <suffix qualifier="AC">M.D.</suffix>
                        </name>
                      </assignedPerson>
                    </assignedAuthor>
                  </author>
                </observation>
              </entryRelationship>
            </substanceAdministration>
          </entry>
          <entry>
            <substanceAdministration classCode="SBADM" moodCode="INT">
                            <templateId root="2.16.840.1.113883.10.20.22.4.120" />
              <id root="81505d5e-2305-42b3-9273-f579d622000d" />
              <statusCode code="active" />
              <effectiveTime xsi:type="IVL_TS" value="20131115" />
              <repeatNumber value="1" />
              <routeCode code="IM" codeSystem="2.16.840.1.113883.5.112" codeSystemName="RouteOfAdministration"
                displayName="Intramuscular injection" />
              <consumable>
                <manufacturedProduct classCode="MANU">
                  <templateId root="2.16.840.1.113883.10.20.22.4.54" extension="2014-06-09" />
                                    <manufacturedMaterial>
                    <code code="88" codeSystem="2.16.840.1.113883.12.292" displayName="influenza virus vaccine, unspecified formulation"
                      codeSystemName="CVX">
                      <originalText>Influenza virus vaccine</originalText>
                      <translation code="111" displayName="influenza virus vaccine, live, attenuated, for intranasal use" codeSystemName="CVX"
                        codeSystem="2.16.840.1.113883.12.292" />
                    </code>
                    <lotNumberText>1</lotNumberText>
                  </manufacturedMaterial>
                </manufacturedProduct>
              </consumable>
                            <entryRelationship typeCode="RSON">
                                <observation classCode="OBS" moodCode="EVN">
                  <templateId root="2.16.840.1.113883.10.20.22.4.143" />
                  <id root="a5b64706-9438-4d13-8dcf-651da3ef83bf" />
                  <code code="225773000" codeSystem="2.16.840.1.113883.6.96" codeSystemName="SNOMED CT"
                    displayName="Preference" />
                  <value xsi:type="CD" code="394849002" codeSystem="2.16.840.1.113883.6.96" codeSystemName="SNOMED CT"
                    displayName="High priority" />
                                    <author>
                    <templateId root="2.16.840.1.113883.10.20.22.4.119" />
                    <time value="20130801" />
                    <assignedAuthor>
                                            <id extension="444222222" root="2.16.840.1.113883.4.1" />
                    </assignedAuthor>
                  </author>
                </observation>
              </entryRelationship>
              <entryRelationship typeCode="REFR">
                <observation classCode="OBS" moodCode="EVN">
                                    <templateId root="2.16.840.1.113883.10.20.22.4.143" />
                  <id root="9a6d1bac-17d3-4195-89a4-1121bc809b4d" />
                  <code code="225773000" codeSystem="2.16.840.1.113883.6.96" codeSystemName="SNOMED CT"
                    displayName="preference" />
                  <effectiveTime value="20130615" />
                  <value xsi:type="CD" code="394848005" codeSystem="2.16.840.1.113883.6.96"
                    displayName="Normal priority" />
                  <author typeCode="AUT">
                    <templateId root="2.16.840.1.113883.10.20.22.4.119" />
                    <time value="20130730" />
                    <assignedAuthor>
                      <id extension="5555555555" root="2.16.840.1.113883.4.6" />
                      <code code="207QA0505X" displayName="Allopathic &amp; Osteopathic Physicians; Family Medicine, Adult Medicine" codeSystem="2.16.840.1.113883.6.101"
                        codeSystemName="Healthcare Provider Taxonomy (HIPAA)" />
                      <addr>
                        <streetAddressLine>1004 Healthcare Drive </streetAddressLine>
                        <city>Portland</city>
                        <state>OR</state>
                        <postalCode>99123</postalCode>
                        <country>US</country>
                      </addr>
                      <telecom use="WP" value="tel:+1(555)555-1004" />
                      <assignedPerson>
                        <name>
                          <given>Patricia</given>
                          <given qualifier="CL">Patty</given>
                          <family>Primary</family>
                          <suffix qualifier="AC">M.D.</suffix>
                        </name>
                      </assignedPerson>
                    </assignedAuthor>
                  </author>
                </observation>
              </entryRelationship>
              <entryRelationship typeCode="SUBJ">
                <act classCode="ACT" moodCode="INT">
                  <templateId root="2.16.840.1.113883.10.20.22.4.20" extension="2014-06-09" />
                                    <code code="171044003" codeSystem="2.16.840.1.113883.6.96" displayName="Immunization education" />
                  <text>Possible flu-like symptoms for three days.</text>
                  <statusCode code="completed" />
                </act>
              </entryRelationship>
            </substanceAdministration>
          </entry>
          <entry>
            <supply moodCode="INT" classCode="SPLY">
              <templateId root="2.16.840.1.113883.10.20.22.4.43" extension="2014-06-09" />
                            <id root="9a6d1bac-17d3-4195-89c4-1121bc809b5d" />
              <statusCode code="active" />
                            <effectiveTime value="20130615" />
              <repeatNumber value="1" />
              <quantity value="3" />
                            <product>
                <manufacturedProduct classCode="MANU">
                                    <templateId root="2.16.840.1.113883.10.20.22.4.23" extension="2014-06-09" />
                  <id root="2a620155-9d11-439e-92b3-5d9815ff4ee8" />
                  <manufacturedMaterial>
                    <code code="573621" codeSystem="2.16.840.1.113883.6.88"
                      displayName="albuterol 0.09 MG/ACTUAT [Proventil]">
                      <translation code="573621" displayName="albuterol 0.09 MG/ACTUAT [Proventil]"
                        codeSystem="2.16.840.1.113883.6.88" codeSystemName="RxNorm" />
                    </code>
                  </manufacturedMaterial>
                  <manufacturerOrganization>
                    <name>Medication Factory Inc.</name>
                  </manufacturerOrganization>
                </manufacturedProduct>
              </product>
                            <performer>
                <time nullFlavor="UNK" />
                <assignedEntity>
                  <id root="2.16.840.1.113883.19.5.9999.456" extension="2981823" />
                  <addr>
                    <streetAddressLine>1001 Village Avenue</streetAddressLine>
                    <city>Portland</city>
                    <state>OR</state>
                    <postalCode>99123</postalCode>
                    <country>US</country>
                  </addr>
                  <telecom nullFlavor="UNK" />
                  <assignedPerson>
                    <name>
                      <prefix>Dr.</prefix>
                      <given>Henry</given>
                      <family>Seven</family>
                    </name>
                  </assignedPerson>
                  <representedOrganization>
                    <id root="2.16.840.1.113883.19.5.9999.1393" />
                    <name>Community Health and Hospitals</name>
                    <telecom nullFlavor="UNK" />
                    <addr nullFlavor="UNK" />
                  </representedOrganization>
                </assignedEntity>
              </performer>
                            <author typeCode="AUT">
                <templateId root="2.16.840.1.113883.10.20.22.4.119" />
                <time value="20130730" />
                <assignedAuthor>
                  <id extension="5555555555" root="2.16.840.1.113883.4.6" />
                  <code code="207QA0505X" displayName="Allopathic &amp; Osteopathic Physicians; Family Medicine, Adult Medicine" codeSystem="2.16.840.1.113883.6.101"
                    codeSystemName="Healthcare Provider Taxonomy (HIPAA)" />
                  <addr>
                    <streetAddressLine>1004 Healthcare Drive </streetAddressLine>
                    <city>Portland</city>
                    <state>OR</state>
                    <postalCode>99123</postalCode>
                    <country>US</country>
                  </addr>
                  <telecom use="WP" value="tel:+1(555)555-1004" />
                  <assignedPerson>
                    <name>
                      <given>Patricia</given>
                      <given qualifier="CL">Patty</given>
                      <family>Primary</family>
                      <suffix qualifier="AC">M.D.</suffix>
                    </name>
                  </assignedPerson>
                </assignedAuthor>
              </author>
            </supply>
          </entry>
        </section>
      </component>
            <component>
        <section>
                    <templateId root="2.16.840.1.113883.10.20.22.2.5.1" extension="2014-06-09" />
          <code code="11450-4" codeSystem="2.16.840.1.113883.6.1" codeSystemName="LOINC" displayName="PROBLEM LIST" />
          <title>PROBLEMS</title>
          <text>
            <paragraph>Active Concerns</paragraph>
            <list>
              <item>Problem #1<list>
                  <item>malignant neoplasm of liver (onset July 3, 2013)[authored July 3, 2013]</item>
                  <item>Prognosis: Presence of a life limiting condition(>50% possibility of death within 2 year </item>
                </list>
              </item>
              <item>Problem #2<list>
                  <item>Chest pain (onset Apr 14, 2007) [authored Apr 14, 2007]</item>
                  <item>Angina (onset Apr 17, 2007) [authored Apr 17, 2007]</item>
                </list>
              </item>
            </list>
            <paragraph>Resolved Concerns</paragraph>
            <list>
              <item>Problem #3<list>
                  <item>Pneumonia - Left lower lobe (onset Mar 10, 1998; resolution Mar 16, 1998) [authored Mar 16,
                    1998]</item>
                </list>
              </item>
            </list>
          </text>
          <entry typeCode="DRIV">
            <act classCode="ACT" moodCode="EVN">
                            <templateId root="2.16.840.1.113883.10.20.22.4.3" extension="2014-06-09" />
              <id root="ec8a6ff8-ed4b-4f7e-82c3-e98e58b45de7" />
              <code code="CONC" codeSystem="2.16.840.1.113883.5.6" displayName="Concern" />
                                          <statusCode code="active" />
              <effectiveTime>
                                                <low value="201307061145-0800" />
              </effectiveTime>
              <author typeCode="AUT">
                <templateId root="2.16.840.1.113883.10.20.22.4.119" />
                                <time value="201307061145-0800" />
                <assignedAuthor>
                  <id extension="555555555" root="2.16.840.1.113883.4.6" />
                  <code code="207QA0505X" displayName="Allopathic &amp; Osteopathic Physicians; Family Medicine, Adult Medicine" codeSystem="2.16.840.1.113883.6.101"
                    codeSystemName="Healthcare Provider Taxonomy (HIPAA)" />
                </assignedAuthor>
              </author>
              <entryRelationship typeCode="SUBJ">
                <observation classCode="OBS" moodCode="EVN">
                                    <templateId root="2.16.840.1.113883.10.20.22.4.4" extension="2014-06-09" />
                  <id root="ab1791b0-5c71-11db-b0de-0800200c9a66" />
                  <code code="64572001" codeSystem="2.16.840.1.113883.6.96" displayName="Disease" />
                                    <statusCode code="completed" />
                  <effectiveTime>
                                                            <low value="20130703" />
                                        <high nullFlavor="UNK"/>
                  </effectiveTime>
                  <value xsi:type="CD" code="93870000" codeSystem="2.16.840.1.113883.6.96"
                    displayName="Malignant neoplasm of liver (disorder)" />
                  <author typeCode="AUT">
                    <templateId root="2.16.840.1.113883.10.20.22.4.119" />
                    <time value="20130703" />
                    <assignedAuthor>
                      <id extension="5555555555" root="2.16.840.1.113883.4.6" />
                      <code code="207QA0505X" displayName="Allopathic &amp; Osteopathic Physicians; Family Medicine, Adult Medicine" codeSystem="2.16.840.1.113883.6.101"
                        codeSystemName="Healthcare Provider Taxonomy (HIPAA)" />
                      <addr>
                        <streetAddressLine>1004 Healthcare Drive </streetAddressLine>
                        <city>Portland</city>
                        <state>OR</state>
                        <postalCode>99123</postalCode>
                        <country>US</country>
                      </addr>
                      <telecom use="WP" value="tel:+1(555)555-1004" />
                      <assignedPerson>
                        <name>
                          <given>Patricia</given>
                          <given qualifier="CL">Patty</given>
                          <family>Primary</family>
                          <suffix qualifier="AC">M.D.</suffix>
                        </name>
                      </assignedPerson>
                    </assignedAuthor>
                  </author>
                  <entryRelationship typeCode="REFR">
                    <observation classCode="OBS" moodCode="EVN">
                                            <templateId root="2.16.840.1.113883.10.20.22.4.113" />
                      <id root="2097c709-291b-4a0f-bef9-ad9b23b3bb43" />
                      <code code="75328-5" codeSystem="2.16.840.1.113883.6.1" codeSystemName="LOINC"
                        displayName="Prognosis" />
                      <text> Pognosis</text>
                      <statusCode code="completed" />
                      <effectiveTime value="20130606" />
                      <value xsi:type="ST">Presence of a life limiting condition(>50% possibility of death within 2
                        year</value>
                    </observation>
                  </entryRelationship>
                  <entryRelationship typeCode="REFR">
                    <observation classCode="OBS" moodCode="EVN">
                                            <templateId root="2.16.840.1.113883.10.20.22.4.6" extension="2014-06-09" />
                      <code code="33999-4" codeSystem="2.16.840.1.113883.6.1" codeSystemName="LOINC"
                        displayName="Status" />
                      <statusCode code="completed" />
                      <value xsi:type="CD" code="55561003" codeSystem="2.16.840.1.113883.6.96" displayName="Active"
                        codeSystemName="SNOMED CT" />
                    </observation>
                  </entryRelationship>
                </observation>
              </entryRelationship>
            </act>
          </entry>
          <entry typeCode="DRIV">
            <act classCode="ACT" moodCode="EVN">
                            <templateId root="2.16.840.1.113883.10.20.22.4.3" extension="2014-06-09" />
              <id root="682f6be1-0793-42f4-904b-e199e6e8e457" />
              <code code="CONC" codeSystem="2.16.840.1.113883.5.6" displayName="Concern" />
                            <statusCode code="active" />
                            <effectiveTime>
                                <low value="200704141515-0800" />
                              </effectiveTime>
              <author typeCode="AUT">
                <templateId root="2.16.840.1.113883.10.20.22.4.119" />
                <time value="200704141515-0800" />
                                <assignedAuthor>
                  <id extension="555555555" root="2.16.840.1.113883.4.6" />
                  <code code="207QA0505X" displayName="Allopathic &amp; Osteopathic Physicians; Family Medicine, Adult Medicine" codeSystem="2.16.840.1.113883.6.101"
                    codeSystemName="Healthcare Provider Taxonomy (HIPAA)" />
                </assignedAuthor>
              </author>
              <entryRelationship typeCode="SUBJ">
                <observation classCode="OBS" moodCode="EVN">
                                    <templateId root="2.16.840.1.113883.10.20.22.4.4" extension="2014-06-09" />
                  <id root="11d088a8-b957-401c-8ee0-3bd20a772fc0" />
                  <code code="64572001" codeSystem="2.16.840.1.113883.6.96" displayName="Disease" />
                                    <statusCode code="completed" />
                  <effectiveTime>
                                        <low value="20070414" />
                                                          </effectiveTime>
                  <value xsi:type="CD" code="29857009" codeSystem="2.16.840.1.113883.6.96" displayName="Chest pain" />
                  <author typeCode="AUT">
                    <templateId root="2.16.840.1.113883.10.20.22.4.119" />
                    <time value="200704141515-0800" />
                    <assignedAuthor>
                      <id extension="555555555" root="2.16.840.1.113883.4.6" />
                      <code code="207QA0505X" displayName="Allopathic &amp; Osteopathic Physicians; Family Medicine, Adult Medicine" codeSystem="2.16.840.1.113883.6.101"
                        codeSystemName="Healthcare Provider Taxonomy (HIPAA)" />
                    </assignedAuthor>
                  </author>
                </observation>
              </entryRelationship>
              <entryRelationship typeCode="SUBJ">
                <observation classCode="OBS" moodCode="EVN">
                                    <templateId root="2.16.840.1.113883.10.20.22.4.4" extension="2014-06-09" />
                  <id root="4991db40-4c4f-41e8-9146-50c12d716424" />
                  <code code="64572001" codeSystem="2.16.840.1.113883.6.96" displayName="Disease" />
                                    <statusCode code="completed" />
                  <effectiveTime>
                                        <low value="20070417" />
                                      </effectiveTime>
                  <value xsi:type="CD" code="194828000" codeSystem="2.16.840.1.113883.6.96" displayName="Angina" />
                  <author typeCode="AUT">
                    <templateId root="2.16.840.1.113883.10.20.22.4.119" />
                    <time value="200704171515-0800" />
                    <assignedAuthor>
                      <id extension="222334444" root="2.16.840.1.113883.4.6" />
                      <code code="207QA0505X" displayName="Allopathic &amp; Osteopathic Physicians; Family Medicine, Adult Medicine" codeSystem="2.16.840.1.113883.6.101"
                        codeSystemName="Healthcare Provider Taxonomy (HIPAA)" />
                    </assignedAuthor>
                  </author>
                </observation>
              </entryRelationship>
            </act>
          </entry>
          <entry typeCode="DRIV">
            <act classCode="ACT" moodCode="EVN">
                            <templateId root="2.16.840.1.113883.10.20.22.4.3" extension="2014-06-09" />
              <id root="b5159d48-04aa-4927-b355-00d1dcb7158c" />
              <code code="CONC" codeSystem="2.16.840.1.113883.5.6" displayName="Concern" />
                                          <statusCode code="completed" />
              <effectiveTime>
                                                <low value="199803101030-0800" />
                                                <high value="199805041145-0800" />
              </effectiveTime>
              <author typeCode="AUT">
                <templateId root="2.16.840.1.113883.10.20.22.4.119" />
                                <time value="199803161030-0800" />
                <assignedAuthor>
                  <id extension="555555555" root="2.16.840.1.113883.4.6" />
                  <code code="207QA0505X" displayName="Allopathic &amp; Osteopathic Physicians; Family Medicine, Adult Medicine" codeSystem="2.16.840.1.113883.6.101"
                    codeSystemName="Healthcare Provider Taxonomy (HIPAA)" />
                </assignedAuthor>
              </author>
              <entryRelationship typeCode="SUBJ">
                <observation classCode="OBS" moodCode="EVN">
                                    <templateId root="2.16.840.1.113883.10.20.22.4.4" extension="2014-06-09" />
                  <id root="10506b4d-c30a-4220-8bec-97bff9568fd1" />
                  <code code="64572001" codeSystem="2.16.840.1.113883.6.96" displayName="Disease" />
                                    <statusCode code="completed" />
                  <effectiveTime>
                                                            <low value="19980310" />
                                        <high value="19980316" />
                  </effectiveTime>
                  <value xsi:type="CD" code="233604007" codeSystem="2.16.840.1.113883.6.96" displayName="Pneumonia">
                    <qualifier>
                      <name code="363698007" displayName="Finding site" />
                      <value code="41224006" displayName="Left lower lobe of lung" />
                    </qualifier>
                  </value>
                  <author typeCode="AUT">
                    <templateId root="2.16.840.1.113883.10.20.22.4.119" />
                    <time value="199803161030-0800" />
                    <assignedAuthor>
                      <id extension="555555555" root="2.16.840.1.113883.4.6" />
                      <code code="207QA0505X" displayName="Allopathic &amp; Osteopathic Physicians; Family Medicine, Adult Medicine" codeSystem="2.16.840.1.113883.6.101"
                        codeSystemName="Healthcare Provider Taxonomy (HIPAA)" />
                    </assignedAuthor>
                  </author>
                </observation>
              </entryRelationship>
            </act>
          </entry>
        </section>
      </component>
            <component>
        <section>
          <templateId root="2.16.840.1.113883.10.20.22.2.7.1" extension="2014-06-09" />
                    <code code="47519-4" codeSystem="2.16.840.1.113883.6.1" codeSystemName="LOINC"
            displayName="History of Procedures" />
          <title>Procedures</title>
          <text>
            <table border="1" width="100%">
              <thead>
                <tr>
                  <th>Procedure</th>
                  <th>Date</th>
                </tr>
              </thead>
              <tbody>
                <tr>
                  <td>
                    <content ID="Proc2">Colonic polypectomy</content>
                  </td>
                  <td>1998</td>
                </tr>
              </tbody>
            </table>
          </text>
          <entry typeCode="DRIV">
            <procedure classCode="PROC" moodCode="EVN">
              <templateId root="2.16.840.1.113883.10.20.22.4.14" extension="2014-06-09" />
                            <id root="d68b7e32-7810-4f5b-9cc2-acd54b0fd85d" />
              <code code="274025005" codeSystem="2.16.840.1.113883.6.96" displayName="Colonic polypectomy">
                <originalText>
                  <reference value="#Proc2" />
                </originalText>
              </code>
              <statusCode code="completed" />
              <effectiveTime value="20110215" />
              <methodCode nullFlavor="UNK" />
              <specimen typeCode="SPC">
                <specimenRole classCode="SPEC">
                  <id root="c2ee9ee9-ae31-4628-a919-fec1cbb58683" />
                  <specimenPlayingEntity>
                    <code code="309226005" codeSystem="2.16.840.1.113883.6.96" displayName="colonic polyp sample" />
                  </specimenPlayingEntity>
                </specimenRole>
              </specimen>
              <performer>
                <assignedEntity>
                  <id root="c2ee9ee9-ae31-4628-a919-fec1cbb58687" />
                  <addr>
                    <streetAddressLine>17 Daws Rd.</streetAddressLine>
                    <city>Blue Bell</city>
                    <state>MA</state>
                    <postalCode>02368</postalCode>
                    <country>US</country>
                  </addr>
                  <telecom use="WP" value="+1(555)555-1234" />
                  <representedOrganization>
                    <id root="c2ee9ee9-ae31-4628-a919-fec1cbb58686" />
                    <name>Good Health Clinic</name>
                    <telecom use="WP" value="+1(555)555-1234" />
                    <addr>
                      <streetAddressLine>17 Daws Rd.</streetAddressLine>
                      <city>Blue Bell</city>
                      <state>MA</state>
                      <postalCode>02368</postalCode>
                      <country>US</country>
                    </addr>
                  </representedOrganization>
                </assignedEntity>
              </performer>
              <participant typeCode="LOC">
                <participantRole classCode="MANU">
                  <templateId root="2.16.840.1.113883.10.20.22.4.37" />
                                    <id root="eb936010-7b17-11db-9fe1-0800200c9a68" />
                  <playingDevice>
                    <code code="90412006" codeSystem="2.16.840.1.113883.6.96" displayName="Colonoscope" />
                  </playingDevice>
                  <scopingEntity>
                    <id root="eb936010-7b17-11db-9fe1-0800200c9b65" />
                  </scopingEntity>
                </participantRole>
              </participant>
            </procedure>
          </entry>
          <entry>
            <observation classCode="OBS" moodCode="EVN">
              <templateId root="2.16.840.1.113883.10.20.22.4.13" extension="2014-06-09" />
                            <id extension="123456789" root="2.16.840.1.113883.19" />
              <code code="274025005" codeSystem="2.16.840.1.113883.6.96" displayName="Colonic polypectomy"
                codeSystemName="SNOMED-CT">
                <originalText>
                  <reference value="#Proc2" />
                </originalText>
              </code>
              <statusCode code="aborted" />
              <effectiveTime value="20110203" />
              <priorityCode code="CR" codeSystem="2.16.840.1.113883.5.7" codeSystemName="ActPriority"
                displayName="callback results" />
              <value xsi:type="CD" />
              <methodCode nullFlavor="UNK" />
              <targetSiteCode code="416949008" codeSystem="2.16.840.1.113883.6.96" codeSystemName="SNOMED CT"
                displayName="Abdomen and pelvis" />
              <performer>
                <assignedEntity>
                  <id root="2.16.840.1.113883.19.5" extension="1234" />
                  <addr>
                    <streetAddressLine>17 Daws Rd.</streetAddressLine>
                    <city>Blue Bell</city>
                    <state>MA</state>
                    <postalCode>02368</postalCode>
                    <country>US</country>
                  </addr>
                  <telecom use="WP" value="+1(555)555-1234" />
                  <representedOrganization>
                    <id root="2.16.840.1.113883.19.5" />
                    <name>Good Health Clinic</name>
                    <telecom nullFlavor="UNK" />
                    <addr nullFlavor="UNK" />
                  </representedOrganization>
                </assignedEntity>
              </performer>
              <participant typeCode="LOC">
                <participantRole classCode="SDLOC">
                  <templateId root="2.16.840.1.113883.10.20.22.4.32" />
                                    <code code="1060-3" codeSystem="2.16.840.1.113883.6.259"
                    codeSystemName="HL7 HealthcareServiceLocation" displayName="Medical Ward" />
                  <addr>
                    <streetAddressLine>17 Daws Rd.</streetAddressLine>
                    <city>Blue Bell</city>
                    <state>MA</state>
                    <postalCode>02368</postalCode>
                    <country>US</country>
                  </addr>
                  <telecom nullFlavor="UNK" />
                  <playingEntity classCode="PLC">
                    <name>Good Health Clinic</name>
                  </playingEntity>
                </participantRole>
              </participant>
            </observation>
          </entry>
          <entry>
            <act classCode="ACT" moodCode="EVN">
              <templateId root="2.16.840.1.113883.10.20.22.4.12" extension="2014-06-09" />
              <id root="1.2.3.4.5.6.7.8" extension="1234567" />
              <code code="274025005" codeSystem="2.16.840.1.113883.6.96" codeSystemName="SNOMED CT"
                displayName="Colonic polypectomy">
                <originalText>
                  <reference value="#Proc1" />
                </originalText>
              </code>
              <statusCode code="completed" />
              <effectiveTime value="20110203" />
              <priorityCode code="CR" codeSystem="2.16.840.1.113883.5.7" codeSystemName="ActPriority"
                displayName="callback results" />
              <performer>
                <assignedEntity>
                  <id root="2.16.840.1.113883.19" extension="1234" />
                  <addr>
                    <streetAddressLine>17 Daws Rd.</streetAddressLine>
                    <city>Blue Bell</city>
                    <state>MA</state>
                    <postalCode>02368</postalCode>
                    <country>US</country>
                  </addr>
                  <telecom use="WP" value="+1(555)555-1234" />
                  <representedOrganization>
                    <id root="2.16.840.1.113883.19.5" />
                    <name>Good Health Clinic</name>
                    <telecom nullFlavor="UNK" />
                    <addr nullFlavor="UNK" />
                  </representedOrganization>
                </assignedEntity>
              </performer>
              <participant typeCode="LOC">
                <participantRole classCode="SDLOC">
                  <templateId root="2.16.840.1.113883.10.20.22.4.32" />
                                    <code code="1060-3" codeSystem="2.16.840.1.113883.6.259"
                    codeSystemName="HL7 HealthcareServiceLocation" displayName="Medical Ward" />
                  <addr>
                    <streetAddressLine>17 Daws Rd.</streetAddressLine>
                    <city>Blue Bell</city>
                    <state>MA</state>
                    <postalCode>02368</postalCode>
                    <country>US</country>
                  </addr>
                  <telecom nullFlavor="UNK" />
                  <playingEntity classCode="PLC">
                    <name>Good Health Clinic</name>
                  </playingEntity>
                </participantRole>
              </participant>
            </act>
          </entry>
        </section>
      </component>
            <component>
        <section>
          <templateId root="1.3.6.1.4.1.19376.1.5.3.1.3.1" extension="2014-06-09" />
          <code code="42349-1" codeSystem="2.16.840.1.113883.6.1" codeSystemName="LOINC"
            displayName="Reason for Referral " />
          <title>REASON FOR REFERRAL</title>
          <text>
            <content>Patient referral for consultation for full care. Referral Nurse: Nurse Florence,RN</content>
          </text>
          <entry>
            <act classCode="PCPR" moodCode="INT">
                            <templateId root="2.16.840.1.113883.10.20.22.4.140" />
              <id root="70bdd7db-e02d-4eff-9829-35e3b7d9e154" />
              <code code="44383000" displayName="Patient referral for consultation" codeSystemName="SNOMED"
                codeSystem="2.16.840.1.113883.6.96"> </code>
              <statusCode code="active" />
              <effectiveTime value="20130311" />
              <priorityCode code="A" codeSystem="2.16.840.1.113883.5.7" codeSystemName="ActPriority" displayName="ASAP" />
              <author typeCode="AUT">
                <templateId root="2.16.840.1.113883.10.20.22.4.119" />
                <time value="20130730" />
                <assignedAuthor>
                  <id root="d839038b-7171-4165-a760-467925b43857" />
                  <code code="163W00000X" codeSystem="2.16.840.1.113883.6.101"
                    codeSystemName="Healthcare Provider Taxonomy (HIPAA)" displayName="Nursing Service Providers; Registered Nurse" />
                  <assignedPerson>
                    <name>
                      <given>Nurse</given>
                      <family>Florence</family>
                      <suffix>RN</suffix>
                    </name>
                  </assignedPerson>
                </assignedAuthor>
              </author>
              <entryRelationship typeCode="SUBJ">
                <observation classCode="OBS" moodCode="EVN">
                  <code code="ASSERTION" codeSystem="2.16.840.1.113883.5.4" />
                  <statusCode code="completed" />
                  <value xsi:type="CD" code="268528005" displayName="Full care by specialist"
                    codeSystem="2.16.840.1.113883.6.96" />
                </observation>
              </entryRelationship>
            </act>
          </entry>
        </section>
      </component>
            <component>
        <section>
                    <templateId root="2.16.840.1.113883.10.20.22.2.3.1" extension="2014-06-09" />
          <code code="30954-2" codeSystem="2.16.840.1.113883.6.1" codeSystemName="LOINC" displayName="RESULTS" />
          <title>RESULTS</title>
          <text>
            <table>
              <thead>
                <tr>
                  <th>Result Type</th>
                  <th>Result Value</th>
                  <th>Relevant Reference Range</th>
                  <th>Interpretation</th>
                  <th>Date</th>
                </tr>
              </thead>
              <tbody>
                <tr>
                  <td>
                    <content ID="result1">Hemoglobin</content>
                  </td>
                  <td>
                    <content ID="resultvalue1">13.2 g/dL</content>
                  </td>
                  <td>
                    <content ID="referencerange1">Normal range for women is 12.0 to 15.5 grams per deciliter</content>
                  </td>
                  <td>Normal</td>
                  <td>March 11, 2013</td>
                </tr>
                <tr>
                  <td>
                    <content ID="result2">Leukocytes</content>
                  </td>
                  <td>
                    <content ID="resultvalue2">6.7 10*9/L</content>
                  </td>
                  <td>
                    <content ID="referencerange2">Normal white blood cell count range 3.5-10.5 billion cells/L</content>
                  </td>
                  <td>Normal</td>
                  <td>March 11, 2013</td>
                </tr>
                <tr>
                  <td>
                    <content ID="result3">Platelets</content>
                  </td>
                  <td>
                    <content ID="resultvalue3">123 10*9/L</content>
                  </td>
                  <td>
                    <content ID="referencerange3">Normal white blood cell count range 3.5-10.5 billion cells/L</content>
                  </td>
                  <td>Low</td>
                  <td>March 11, 2013</td>
                </tr>
                <tr>
                  <td>
                    <content ID="result4">Hematocrit</content>
                  </td>
                  <td>
                    <content ID="resultvalue4">35.3 %</content>
                  </td>
                  <td>
                    <content ID="referencerange4">Normal hematocrit range for female: 34.9-44.5 percent</content>
                  </td>
                  <td>Normal</td>
                  <td>March 11, 2013</td>
                </tr>
                <tr>
                  <td>
                    <content ID="result5">Erythrocytes</content>
                  </td>
                  <td>
                    <content ID="resultvalue5">4.21 10*12/L</content>
                  </td>
                  <td>
                    <content ID="referencerange5">Normal red blood cell count range 3.90-5.03 trillion cells/L</content>
                  </td>
                  <td>Normal</td>
                  <td>March 11, 2013</td>
                </tr>
                <tr>
                  <td>
                    <content ID="result6">Urea nitrogen, Serum</content>
                  </td>
                  <td>Pending</td>
                  <td>Pending</td>
                  <td>Pending</td>
                  <td>March 11, 2013</td>
                </tr>
              </tbody>
            </table>
          </text>
          <entry typeCode="DRIV">
            <organizer classCode="BATTERY" moodCode="EVN">
                            <templateId root="2.16.840.1.113883.10.20.22.4.1" extension="2014-06-09" />
              <id root="7d5a02b0-67a4-11db-bd13-0800200c9a66" />
              <code code="57021-8" displayName="CBC W Auto Differential panel in Blood"
                codeSystem="2.16.840.1.113883.6.1" codeSystemName="LOINC" />
              <statusCode code="completed" />
              <effectiveTime>
                <low value="201303110830-0800" />
                <high value="201303110830-0800" />
              </effectiveTime>
              <component>
                <observation classCode="OBS" moodCode="EVN">
                                    <templateId root="2.16.840.1.113883.10.20.22.4.2" extension="2014-06-09" />
                  <id root="107c2dc0-67a5-11db-bd13-0800200c9a66" />
                  <code code="718-7" displayName="Hemoglobin" codeSystem="2.16.840.1.113883.6.1" codeSystemName="LOINC" />
                  <statusCode code="completed" />
                  <effectiveTime value="201303110830-0800" />
                  <value xsi:type="PQ" value="13.2" unit="g/dL" />
                  <interpretationCode code="N" codeSystem="2.16.840.1.113883.5.83" displayName="Normal" />
                  <author typeCode="AUT">
                    <templateId root="2.16.840.1.113883.10.20.22.4.119" />
                    <time value="201303110830-0800" />
                    <assignedAuthor>
                      <id extension="333444444" root="2.16.840.1.113883.4.6" />
                      <code code="246Q00000X" displayName="&quot;Technologists, Technicians &amp; Other Technical Service Providers&quot;; &quot;Specialist/Technologist, Pathology&quot;" codeSystem="2.16.840.1.113883.6.101"
                        codeSystemName="Healthcare Provider Taxonomy (HIPAA)" />
                    </assignedAuthor>
                  </author>
                  <referenceRange>
                    <observationRange>
                      <value xsi:type="IVL_PQ">
                        <low value="12.0" unit="g/dL" />
                        <high value="15.5" unit="g/dL" />
                      </value>
                    </observationRange>
                  </referenceRange>
                </observation>
              </component>
              <component>
                <observation classCode="OBS" moodCode="EVN">
                                    <templateId root="2.16.840.1.113883.10.20.22.4.2" extension="2014-06-09" />
                  <id root="a69b3d60-2ffd-4440-958b-72b3335ff35f" />
                  <code code="6690-2" displayName="Leukocytes" codeSystem="2.16.840.1.113883.6.1" codeSystemName="LOINC" />
                  <statusCode code="completed" />
                  <effectiveTime value="201303110830-0800" />
                  <value xsi:type="PQ" value="6.7" unit="10*9/L">
                    <translation>
                      <originalText>6.7 billion per liter</originalText>
                    </translation>
                  </value>
                  <interpretationCode code="N" codeSystem="2.16.840.1.113883.5.83" displayName="Normal" />
                  <author typeCode="AUT">
                    <templateId root="2.16.840.1.113883.10.20.22.4.119" />
                    <time value="201303110830-0800" />
                    <assignedAuthor>
                      <id extension="333444444" root="2.16.840.1.113883.4.6" />
                      <code code="246Q00000X" displayName="&quot;Technologists, Technicians &amp; Other Technical Service Providers&quot;; &quot;Specialist/Technologist, Pathology&quot;" codeSystem="2.16.840.1.113883.6.101"
                        codeSystemName="Healthcare Provider Taxonomy (HIPAA)" />
                    </assignedAuthor>
                  </author>
                  <referenceRange>
                    <observationRange>
                      <value xsi:type="IVL_PQ">
                        <low value="4.3" unit="10*9/L" />
                        <high value="10.8" unit="10*9/L" />
                      </value>
                    </observationRange>
                  </referenceRange>
                </observation>
              </component>
              <component>
                <observation classCode="OBS" moodCode="EVN">
                                    <templateId root="2.16.840.1.113883.10.20.22.4.2" extension="2014-06-09" />
                  <id root="ef5c1c58-4665-4556-a8e8-6e720d82f572" />
                  <code code="777-3" displayName="Platelets" codeSystem="2.16.840.1.113883.6.1" codeSystemName="LOINC" />
                  <text>
                    <reference value="#result3" />
                  </text>
                  <statusCode code="completed" />
                  <effectiveTime value="201303110830-0800" />
                  <value xsi:type="PQ" value="123" unit="10*9/L" />
                  <interpretationCode code="LX" codeSystem="2.16.840.1.113883.5.83" displayName="below low threshold" />
                  <author typeCode="AUT">
                    <templateId root="2.16.840.1.113883.10.20.22.4.119" />
                    <time value="201303110830-0800" />
                    <assignedAuthor>
                      <id extension="333444444" root="2.16.840.1.113883.4.6" />
                      <code code="246Q00000X" displayName="&quot;Technologists, Technicians &amp; Other Technical Service Providers&quot;; &quot;Specialist/Technologist, Pathology&quot;" codeSystem="2.16.840.1.113883.6.101"
                        codeSystemName="Healthcare Provider Taxonomy (HIPAA)" />
                    </assignedAuthor>
                  </author>
                  <referenceRange>
                    <observationRange>
                      <value xsi:type="IVL_PQ">
                        <low value="150" unit="10*9/L" />
                        <high value="350" unit="10*9/L" />
                      </value>
                    </observationRange>
                  </referenceRange>
                </observation>
              </component>
              <component>
                <observation classCode="OBS" moodCode="EVN">
                                    <templateId root="2.16.840.1.113883.10.20.22.4.2" extension="2014-06-09" />
                  <id root="7c0704bb-9c40-41b5-9c7d-26b2d59e234f" />
                  <code code="4544-3" displayName="Hematocrit" codeSystem="2.16.840.1.113883.6.1" codeSystemName="LOINC" />
                  <text>
                    <reference value="#result4" />
                  </text>
                  <statusCode code="completed" />
                  <effectiveTime value="201303110830-0800" />
                  <value xsi:type="PQ" value="35.3" unit="%" />
                  <interpretationCode code="LX" codeSystem="2.16.840.1.113883.5.83" displayName="below low threshold" />
                  <author typeCode="AUT">
                    <templateId root="2.16.840.1.113883.10.20.22.4.119" />
                    <time value="201303110830-0800" />
                    <assignedAuthor>
                      <id extension="333444444" root="2.16.840.1.113883.4.6" />
                      <code code="246Q00000X" displayName="&quot;Technologists, Technicians &amp; Other Technical Service Providers&quot;; &quot;Specialist/Technologist, Pathology&quot;" codeSystem="2.16.840.1.113883.6.101"
                        codeSystemName="Healthcare Provider Taxonomy (HIPAA)" />
                    </assignedAuthor>
                  </author>
                  <referenceRange>
                    <observationRange>
                      <value xsi:type="IVL_PQ">
                        <low value="34.9" unit="%" />
                        <high value="44.5" unit="%" />
                      </value>
                    </observationRange>
                  </referenceRange>
                </observation>
              </component>
              <component>
                <observation classCode="OBS" moodCode="EVN">
                                    <templateId root="2.16.840.1.113883.10.20.22.4.2" extension="2014-06-09" />
                  <id root="bccd6fc9-0c7f-455e-8616-923ed0d04d09" />
                  <code code="789-8" displayName="Erythrocytes" codeSystem="2.16.840.1.113883.6.1"
                    codeSystemName="LOINC" />
                  <statusCode code="completed" />
                  <effectiveTime value="201303110830-0800" />
                  <value xsi:type="PQ" value="4.21" unit="10*12/L" />
                  <interpretationCode code="N" codeSystem="2.16.840.1.113883.5.83" displayName="Normal" />
                  <author typeCode="AUT">
                    <templateId root="2.16.840.1.113883.10.20.22.4.119" />
                    <time value="201303110830-0800" />
                    <assignedAuthor>
                      <id extension="333444444" root="2.16.840.1.113883.4.6" />
                      <code code="246Q00000X" displayName="&quot;Technologists, Technicians &amp; Other Technical Service Providers&quot;; &quot;Specialist/Technologist, Pathology&quot;" codeSystem="2.16.840.1.113883.6.101"
                        codeSystemName="Healthcare Provider Taxonomy (HIPAA)" />
                    </assignedAuthor>
                  </author>
                  <referenceRange>
                    <observationRange>
                      <value xsi:type="IVL_PQ">
                        <low value="3.90" unit="10*12/L" />
                        <high value="5.03" unit="10*12/L" />
                      </value>
                    </observationRange>
                  </referenceRange>
                </observation>
              </component>
            </organizer>
          </entry>
          <entry typeCode="DRIV">
            <organizer classCode="BATTERY" moodCode="EVN">
                            <templateId root="2.16.840.1.113883.10.20.22.4.1" extension="2014-06-09" />
              <id root="122ed3ae-6d9e-43d0-bfa2-434ea34b1426" />
              <code code="166312007" displayName="Blood chemistry" codeSystem="2.16.840.1.113883.6.96"
                codeSystemName="SNOMED CT" />
              <statusCode code="active" />
              <effectiveTime>
                <low value="200803200930-0800" />
                <high value="200803200930-0800" />
              </effectiveTime>
              <component>
                <observation classCode="OBS" moodCode="EVN">
                                    <templateId root="2.16.840.1.113883.10.20.22.4.2" extension="2014-06-09" />
                  <id root="aed821af-3330-4138-97f0-e84dfe5f3c35" />
                  <code code="3094-0" displayName="Urea nitrogen, Serum" codeSystem="2.16.840.1.113883.6.1"
                    codeSystemName="LOINC" />
                  <statusCode code="active" />
                  <effectiveTime value="200803200930-0800" />
                  <value xsi:type="PQ" nullFlavor="NI" />
                  <interpretationCode code="N" displayName="Normal" codeSystem="2.16.840.1.113883.5.83" />
                </observation>
              </component>
            </organizer>
          </entry>
        </section>
      </component>
            <component>
        <section>
          <templateId root="1.3.6.1.4.1.19376.1.5.3.1.3.18" />
          <code code="10187-3" codeSystem="2.16.840.1.113883.6.1" codeSystemName="LOINC" displayName="REVIEW OF SYSTEMS" />
          <title>REVIEW OF SYSTEMS</title>
          <text>
            <paragraph> Patient denies recent history of fever or malaise. Positive For weakness and shortness of
              breath. One episode of melena. No recent headaches. Positive for osteoarthritis in hips, knees and hands.
            </paragraph>
          </text>
        </section>
      </component>
            <component>
        <section>
          <templateId root="2.16.840.1.113883.10.20.22.2.17" extension="2014-06-09" />
                    <code code="29762-2" codeSystem="2.16.840.1.113883.6.1" displayName="Social History" />
          <title>SOCIAL HISTORY</title>
          <text>
            <table border="1" width="100%">
              <thead>
                <tr>
                  <th>Social History Element</th>
                  <th>Description</th>
                  <th>Effective Dates</th>
                </tr>
              </thead>
              <tbody>
                <tr>
                  <td>Alcohol Use</td>
                  <td>2 Drinks per Week</td>
                  <td>March 12, 2013</td>
                </tr>
                <tr>
                  <td>Smoking Status</td>
                  <td>Former smoker</td>
                  <td>May 1, 2005 - Feb 27, 2009</td>
                </tr>
                <tr>
                  <td>Characteristics of Home Environment</td>
                  <td>Unsatisfactory Living Conditions</td>
                  <td>March 12, 2013</td>
                </tr>
                <tr>
                  <td>Cultural and Religious Observations</td>
                  <td>Does not accept blood transfusions, or donates, or stores blood for transfusion.</td>
                  <td>March 12, 2013</td>
                </tr>
              </tbody>
            </table>
          </text>
          <entry>
            <observation classCode="OBS" moodCode="EVN">
                            <templateId root="2.16.840.1.113883.10.20.22.4.38" extension="2014-06-09" />
              <id root="9b56c25d-9104-45ee-9fa4-e0f3afaa01c1" />
              <code code="74013-4" codeSystem="2.16.840.1.113883.6.1" codeSystemName="LOINC"
                displayName="Alcoholic drinks per day" />
              <statusCode code="completed" />
              <effectiveTime value="20130312" />
              <value xsi:type="PQ" value="1" />
              <author typeCode="AUT">
                <templateId root="2.16.840.1.113883.10.20.22.4.119" />
                <time value="20130730" />
                <assignedAuthor>
                  <id extension="5555555555" root="2.16.840.1.113883.4.6" />
                  <code code="207QA0505X" displayName="Allopathic &amp; Osteopathic Physicians; Family Medicine, Adult Medicine" codeSystem="2.16.840.1.113883.6.101"
                    codeSystemName="Healthcare Provider Taxonomy (HIPAA)" />
                  <addr>
                    <streetAddressLine>1004 Healthcare Drive </streetAddressLine>
                    <city>Portland</city>
                    <state>OR</state>
                    <postalCode>99123</postalCode>
                    <country>US</country>
                  </addr>
                  <telecom use="WP" value="tel:+1(555)555-1004" />
                  <assignedPerson>
                    <name>
                      <given>Patricia</given>
                      <given qualifier="CL">Patty</given>
                      <family>Primary</family>
                      <suffix qualifier="AC">M.D.</suffix>
                    </name>
                  </assignedPerson>
                </assignedAuthor>
              </author>
            </observation>
          </entry>
          <entry>
            <observation classCode="OBS" moodCode="EVN">
                            <templateId root="2.16.840.1.113883.10.20.22.4.78" extension="2014-06-09" />
              <id extension="123456789" root="2.16.840.1.113883.19" />
              <code code="72166-2" codeSystem="2.16.840.1.113883.6.1" displayName="Tobacco smoking status NHIS" />
              <statusCode code="completed" />
              <effectiveTime value="20130621"> </effectiveTime>
              <value xsi:type="CD" code="8517006" displayName="Ex-smoker" codeSystem="2.16.840.1.113883.6.96" />
              <author typeCode="AUT">
                <templateId root="2.16.840.1.113883.10.20.22.4.119" />
                <time value="20130730" />
                <assignedAuthor>
                  <id extension="5555555555" root="2.16.840.1.113883.4.6" />
                  <code code="207QA0505X" displayName="Allopathic &amp; Osteopathic Physicians; Family Medicine, Adult Medicine" codeSystem="2.16.840.1.113883.6.101"
                    codeSystemName="Healthcare Provider Taxonomy (HIPAA)" />
                  <addr>
                    <streetAddressLine>1004 Healthcare Drive </streetAddressLine>
                    <city>Portland</city>
                    <state>OR</state>
                    <postalCode>99123</postalCode>
                    <country>US</country>
                  </addr>
                  <telecom use="WP" value="tel:+1(555)555-1004" />
                  <assignedPerson>
                    <name>
                      <given>Patricia</given>
                      <given qualifier="CL">Patty</given>
                      <family>Primary</family>
                      <suffix qualifier="AC">M.D.</suffix>
                    </name>
                  </assignedPerson>
                </assignedAuthor>
              </author>
            </observation>
          </entry>
          <entry>
            <observation classCode="OBS" moodCode="EVN">
                            <templateId root="2.16.840.1.113883.10.20.22.4.72" />
              <id nullFlavor="NI" />
              <code code="ASSERTION" codeSystem="2.16.840.1.113883.5.4" />
              <statusCode code="completed" />
              <effectiveTime value="20130312" />
              <value xsi:type="CD" code="422615001" codeSystem="2.16.840.1.113883.6.96"
                displayName="Caregiver difficulty providing physical care" />
              <participant typeCode="IND">
                <participantRole classCode="CAREGIVER">
                  <code code="MTH" codeSystem="2.16.840.1.113883.5.111" displayName="mother" />
                </participantRole>
              </participant>
            </observation>
          </entry>
          <entry>
            <observation classCode="OBS" moodCode="EVN">
                            <templateId root="2.16.840.1.113883.10.20.22.4.111" />
              <id root="37f76c51-6411-4e1d-8a37-957fd49d2cef" />
              <code code="75281-6" codeSystem="2.16.840.1.113883.6.1" displayName="Personal belief" />
              <statusCode code="completed" />
              <effectiveTime value="20130312" />
              <value xsi:type="ST">Does not accept blood transfusions, or donates, or stores blood for
                transfusion.</value>
            </observation>
          </entry>
          <entry>
            <observation classCode="OBS" moodCode="EVN">
                            <templateId root="2.16.840.1.113883.10.20.22.4.109" />
              <id root="37f76c51-6411-4e1d-8a37-957fd49d2ceg" />
              <code code="75274-1" codeSystem="2.16.840.1.113883.6.1" codeSystemName="LOINC"
                displayName="Characteristics of residence" />
              <statusCode code="completed" />
              <effectiveTime value="20130312" />
                            <value xsi:type="CD" code="308899009" displayName="Unsatisfactory living conditions (finding)"
                codeSystem="2.16.840.1.113883.6.96" codeSystemName="SNOMED CT" />
            </observation>
          </entry>
        </section>
      </component>
            <component>
        <section>
                    <templateId root="2.16.840.1.113883.10.20.22.2.4.1" extension="2014-06-09" />
          <code code="8716-3" codeSystem="2.16.840.1.113883.6.1" codeSystemName="LOINC" displayName="Vital signs" />
          <title>VITAL SIGNS</title>
          <text>
            <table border="1" width="100%">
              <thead>
                <tr>
                  <th align="right">Date / Time: </th>
                  <th>February 12, 2013</th>
                  <th>August 1, 2013</th>
                </tr>
              </thead>
              <tbody>
                <tr>
                  <th align="left">Height</th>
                  <td>
                    <content ID="vit1">177 cm</content>
                  </td>
                  <td>
                    <content ID="vit2">177 cm</content>
                  </td>
                </tr>
                <tr>
                  <th align="left">Weight</th>
                  <td>
                    <content ID="vit3">86 kg</content>
                  </td>
                  <td>
                    <content ID="vit4">88 kg</content>
                  </td>
                </tr>
                <tr>
                  <th align="left">Blood Pressure</th>
                  <td>
                    <content ID="vit5">132/88</content>
                  </td>
                  <td>
                    <content ID="vit6">128/80</content>
                  </td>
                </tr>
              </tbody>
            </table>
          </text>
          <entry typeCode="DRIV">
            <organizer classCode="CLUSTER" moodCode="EVN">
                            <templateId root="2.16.840.1.113883.10.20.22.4.26" extension="2014-06-09" />
              <id root="31b73bd0-cffc-4599-902e-dbe54bc56cb4" />
              <code code="74728-7" codeSystem="2.16.840.1.113883.6.1" codeSystemName="LOINC" displayName="Vital signs" />
              <statusCode code="completed" />
              <effectiveTime>
                <low value="20130212" />
                <high value="20130801" />
              </effectiveTime>
              <component>
                <observation classCode="OBS" moodCode="EVN">
                                    <templateId root="2.16.840.1.113883.10.20.22.4.27" extension="2014-06-09" />
                  <id root="ed9589fd-fda0-41f7-a3d0-dc537554f5c2" />
                  <code code="8302-2" codeSystem="2.16.840.1.113883.6.1" codeSystemName="LOINC" displayName="Body height" />
                  <statusCode code="completed" />
                  <effectiveTime value="20120910" />
                  <value xsi:type="PQ" value="177" unit="cm" />
                  <interpretationCode code="N" codeSystem="2.16.840.1.113883.5.83" displayName="Normal" />
                  <author typeCode="AUT">
                    <templateId root="2.16.840.1.113883.10.20.22.4.119" />
                    <time value="201209101145-0800" />
                    <assignedAuthor>
                      <id extension="555555555" root="2.16.840.1.113883.4.6" />
                      <code code="207QA0505X" displayName="Allopathic &amp; Osteopathic Physicians; Family Medicine, Adult Medicine" codeSystem="2.16.840.1.113883.6.101"
                        codeSystemName="Healthcare Provider Taxonomy (HIPAA)" />
                    </assignedAuthor>
                  </author>
                </observation>
              </component>
              <component>
                <observation classCode="OBS" moodCode="EVN">
                                    <templateId root="2.16.840.1.113883.10.20.22.4.27" extension="2014-06-09" />
                  <id root="f4e729e2-a97f-4a7e-8e23-c92f9b6b55cf" />
                  <code code="3141-9" codeSystem="2.16.840.1.113883.6.1" codeSystemName="LOINC"
                    displayName="Patient Body Weight - Measured" />
                  <statusCode code="completed" />
                  <effectiveTime value="20130212" />
                  <value xsi:type="PQ" value="86" unit="kg" />
                  <interpretationCode code="N" codeSystem="2.16.840.1.113883.5.83" displayName="Normal" />
                  <author typeCode="AUT">
                    <templateId root="2.16.840.1.113883.10.20.22.4.119" />
                    <time value="201209101145-0800" />
                    <assignedAuthor>
                      <id extension="555555555" root="2.16.840.1.113883.4.6" />
                      <code code="207QA0505X" displayName="Allopathic &amp; Osteopathic Physicians; Family Medicine, Adult Medicine" codeSystem="2.16.840.1.113883.6.101"
                        codeSystemName="Healthcare Provider Taxonomy (HIPAA)" />
                    </assignedAuthor>
                  </author>
                </observation>
              </component>
              <component>
                <observation classCode="OBS" moodCode="EVN">
                                    <templateId root="2.16.840.1.113883.10.20.22.4.27" extension="2014-06-09" />
                  <id root="a0e39c70-9674-4b2a-9837-cdf74200d8d5" />
                  <code code="8480-6" codeSystem="2.16.840.1.113883.6.1" codeSystemName="LOINC"
                    displayName="Systolic blood pressure" />
                  <statusCode code="completed" />
                  <effectiveTime value="20130801" />
                  <value xsi:type="PQ" value="132" unit="mm[Hg]" />
                  <interpretationCode code="N" codeSystem="2.16.840.1.113883.5.83" displayName="Normal" />
                  <author typeCode="AUT">
                    <templateId root="2.16.840.1.113883.10.20.22.4.119" />
                    <time value="201209101145-0800" />
                    <assignedAuthor>
                      <id extension="555555555" root="2.16.840.1.113883.4.6" />
                      <code code="207QA0505X" displayName="Allopathic &amp; Osteopathic Physicians; Family Medicine, Adult Medicine" codeSystem="2.16.840.1.113883.6.101"
                        codeSystemName="Healthcare Provider Taxonomy (HIPAA)" />
                    </assignedAuthor>
                  </author>
                </observation>
              </component>
              <component>
                <observation classCode="OBS" moodCode="EVN">
                                    <templateId root="2.16.840.1.113883.10.20.22.4.27" extension="2014-06-09" />
                  <id root="1c2748b7-e440-41ba-bc01-dde97d84a036" />
                  <code code="8462-4" codeSystem="2.16.840.1.113883.6.1" codeSystemName="LOINC"
                    displayName="Diastolic blood pressure" />
                  <statusCode code="completed" />
                  <effectiveTime value="20120910" />
                  <value xsi:type="PQ" value="88" unit="mm[Hg]" />
                  <interpretationCode code="N" codeSystem="2.16.840.1.113883.5.83" displayName="Normal" />
                  <author typeCode="AUT">
                    <templateId root="2.16.840.1.113883.10.20.22.4.119" />
                    <time value="201109010915-0800" />
                    <assignedAuthor>
                      <id extension="555555555" root="2.16.840.1.113883.4.6" />
                      <code code="207QA0505X" displayName="Allopathic &amp; Osteopathic Physicians; Family Medicine, Adult Medicine" codeSystem="2.16.840.1.113883.6.101"
                        codeSystemName="Healthcare Provider Taxonomy (HIPAA)" />
                    </assignedAuthor>
                  </author>
                </observation>
              </component>
            </organizer>
          </entry>
          <entry typeCode="DRIV">
            <organizer classCode="CLUSTER" moodCode="EVN">
                            <templateId root="2.16.840.1.113883.10.20.22.4.26" extension="2014-06-09" />
              <id root="24f6ad18-c512-40fc-82bd-1e131aa9e52b" />
              <code code="74728-7" codeSystem="2.16.840.1.113883.6.1" codeSystemName="LOINC" displayName="Vital signs" />
              <statusCode code="completed" />
              <effectiveTime>
                <low value="20110901" />
                <high value="20110901" />
              </effectiveTime>
              <component>
                <observation classCode="OBS" moodCode="EVN">
                                    <templateId root="2.16.840.1.113883.10.20.22.4.27" extension="2014-06-09" />
                  <id root="05c047cd-28c3-41cd-be6c-56f8cc0c3f2f" />
                  <code code="8302-2" codeSystem="2.16.840.1.113883.6.1" codeSystemName="LOINC" displayName="Body height" />
                  <statusCode code="completed" />
                  <effectiveTime value="20110901" />
                  <value xsi:type="PQ" value="177" unit="cm" />
                  <interpretationCode code="N" codeSystem="2.16.840.1.113883.5.83" displayName="Normal" />
                  <author typeCode="AUT">
                    <templateId root="2.16.840.1.113883.10.20.22.4.119" />
                    <time value="201109010915-0800" />
                    <assignedAuthor>
                      <id extension="555555555" root="2.16.840.1.113883.4.6" />
                      <code code="207QA0505X" displayName="Allopathic &amp; Osteopathic Physicians; Family Medicine, Adult Medicine" codeSystem="2.16.840.1.113883.6.101"
                        codeSystemName="Healthcare Provider Taxonomy (HIPAA)" />
                    </assignedAuthor>
                  </author>
                </observation>
              </component>
              <component>
                <observation classCode="OBS" moodCode="EVN">
                                    <templateId root="2.16.840.1.113883.10.20.22.4.27" extension="2014-06-09" />
                  <id root="21b0f3d5-7d07-4f4f-ad7e-c33dc2ca3835" />
                  <code code="3141-9" codeSystem="2.16.840.1.113883.6.1" codeSystemName="LOINC"
                    displayName="Patient Body Weight - Measured" />
                  <statusCode code="completed" />
                  <effectiveTime value="20110901" />
                  <value xsi:type="PQ" value="88" unit="kg" />
                  <interpretationCode code="N" codeSystem="2.16.840.1.113883.5.83" displayName="Normal" />
                  <author typeCode="AUT">
                    <templateId root="2.16.840.1.113883.10.20.22.4.119" />
                    <time value="201109010915-0800" />
                    <assignedAuthor>
                      <id extension="555555555" root="2.16.840.1.113883.4.6" />
                      <code code="207QA0505X" displayName="Allopathic &amp; Osteopathic Physicians; Family Medicine, Adult Medicine" codeSystem="2.16.840.1.113883.6.101"
                        codeSystemName="Healthcare Provider Taxonomy (HIPAA)" />
                    </assignedAuthor>
                  </author>
                </observation>
              </component>
              <component>
                <observation classCode="OBS" moodCode="EVN">
                                    <templateId root="2.16.840.1.113883.10.20.22.4.27" extension="2014-06-09" />
                  <id root="b046c35a-59c7-4215-ae09-9a8409a30b21" />
                  <code code="8480-6" codeSystem="2.16.840.1.113883.6.1" codeSystemName="LOINC"
                    displayName="Systolic blood pressure" />
                  <statusCode code="completed" />
                  <effectiveTime value="20110901" />
                  <value xsi:type="PQ" value="128" unit="mm[Hg]" />
                  <interpretationCode code="N" codeSystem="2.16.840.1.113883.5.83" displayName="Normal" />
                  <author typeCode="AUT">
                    <templateId root="2.16.840.1.113883.10.20.22.4.119" />
                    <time value="201109010915-0800" />
                    <assignedAuthor>
                      <id extension="555555555" root="2.16.840.1.113883.4.6" />
                      <code code="207QA0505X" displayName="Allopathic &amp; Osteopathic Physicians; Family Medicine, Adult Medicine" codeSystem="2.16.840.1.113883.6.101"
                        codeSystemName="Healthcare Provider Taxonomy (HIPAA)" />
                    </assignedAuthor>
                  </author>
                </observation>
              </component>
              <component>
                <observation classCode="OBS" moodCode="EVN">
                                    <templateId root="2.16.840.1.113883.10.20.22.4.27" extension="2014-06-09" />
                  <id root="44f54e66-fb4b-4ee5-9ced-9574ef307a23" />
                  <code code="8462-4" codeSystem="2.16.840.1.113883.6.1" codeSystemName="LOINC"
                    displayName="Diastolic blood pressure" />
                  <statusCode code="completed" />
                  <effectiveTime value="20110901" />
                  <value xsi:type="PQ" value="80" unit="mm[Hg]" />
                  <interpretationCode code="N" codeSystem="2.16.840.1.113883.5.83" displayName="Normal" />
                  <author typeCode="AUT">
                    <templateId root="2.16.840.1.113883.10.20.22.4.119" />
                    <time value="201109010915-0800" />
                    <assignedAuthor>
                      <id extension="555555555" root="2.16.840.1.113883.4.6" />
                      <code code="207QA0505X" displayName="Allopathic &amp; Osteopathic Physicians; Family Medicine, Adult Medicine" codeSystem="2.16.840.1.113883.6.101"
                        codeSystemName="Healthcare Provider Taxonomy (HIPAA)" />
                    </assignedAuthor>
                  </author>
                </observation>
              </component>
            </organizer>
          </entry>
        </section>
      </component>
    </structuredBody>
  </component>
</ClinicalDocument>
""";

        var sxmls = new SoapXmlSerializer(XmlSettings.Soap);

        var docc = await sxmls.DeserializeSoapMessageAsync<ClinicalDocument>(cdaTestString);

        Console.WriteLine(docc.Code);

        var doccCDA = sxmls.SerializeSoapMessageToXmlString(docc);

    }
}