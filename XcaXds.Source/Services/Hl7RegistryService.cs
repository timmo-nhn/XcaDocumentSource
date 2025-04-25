﻿using System.Globalization;
using System.Reflection.Metadata;
using System.Text;
using Efferent.HL7.V2;
using XcaXds.Commons;
using XcaXds.Commons.Models.Custom;
using XcaXds.Commons.Models.Hl7.DataType;
using XcaXds.Commons.Models.Soap.XdsTypes;

namespace XcaXds.Source.Services;

public partial class RegistryService
{
    public Message PatientDemographicsQueryGetPatientIdentifiersInRegistry(Message findCandidatesQuery)
    {
        var qpdSegment = findCandidatesQuery.Segments("QPD").FirstOrDefault();


        var patientIds = qpdSegment.GetAllFields().Where(f => f.Value.Contains("PID"));

        // QPD|IHE PDQ Query|Q1234|@PID.3.1^131169~@PID.5^Danser^Line


        var patientFields = patientIds.FirstOrDefault().Value.Split("~");

        var patient = new PID();
        patient.PatientIdentifier ??= new();


        for (int i = 0; i < patientFields.Length; i++)
        {
            var field = patientFields[i];
            if (field.StartsWith("@PID.3"))
            {
                var value = field.Substring(field.IndexOf('^') + 1);
                patient.PatientIdentifier = Hl7Object.Parse<CX>(value);
            }
            if (field.StartsWith("@PID.5"))
            {
                var value = field.Substring(field.IndexOf('^') + 1);
                patient.PatientName = Hl7Object.Parse<XPN>(value);
            }
            if (field.StartsWith("@PID.7"))
            {
                var value = field.Substring(field.IndexOf('^') + 1);
                patient.BirthDate = DateTime.ParseExact(value, Constants.Hl7.Dtm.CdaFormats, CultureInfo.InvariantCulture);
            }
            if (field.StartsWith("@PID.8"))
            {
                var value = field.Substring(field.IndexOf('^') + 1);
                patient.Gender = value;
            }
        }


        var extrinsicObjectPatientIds = _documentRegistry.RegistryObjectList
            .OfType<ExtrinsicObjectType>()
            .Select(eo => eo.GetPatientIdentifiersFromExtrinsicObject())
            .ToList();


        var matchingPatientIds = extrinsicObjectPatientIds
            .Where(eop =>
            {
                bool nameMatch = 
                    eop.PatientName != null && patient.PatientName != null &&
                    !string.IsNullOrEmpty(patient.PatientName?.GivenName) &&
                    eop.PatientName.GivenName?.Contains(patient.PatientName?.GivenName) == true ||
                    !string.IsNullOrEmpty(patient.PatientName?.FamilyName) &&
                    eop.PatientName?.FamilyName?.Contains(patient.PatientName?.FamilyName) == true;

                bool birthDateMatch = 
                    eop.BirthDate != DateTime.MinValue && patient.BirthDate != DateTime.MinValue &&
                    eop.BirthDate == patient.BirthDate;

                bool genderMatch = 
                    !string.IsNullOrEmpty(eop.Gender) &&
                    eop.Gender == patient.Gender;

                bool identifierMatch = eop.PatientIdentifier != null &&
                    patient.PatientIdentifier != null &&
                    !string.IsNullOrEmpty(patient.PatientIdentifier.IdNumber) &&
                    eop.PatientIdentifier.IdNumber?.Contains(patient.PatientIdentifier.IdNumber) == true;

                return nameMatch || birthDateMatch || genderMatch || identifierMatch;
            })
            .ToList();

        var enc = new HL7Encoding();

        var responseMessage = new Message();
        responseMessage.Encoding = enc;

        var reqMsh = findCandidatesQuery.Segments("MSH")[0].GetAllFields();

        responseMessage.AddSegmentMSH
        (
            sendingApplication: reqMsh[4].Value, // Sending app becomes receiving app
            sendingFacility: reqMsh[5].Value, // Sending facility becomes receiving facility
            receivingApplication: reqMsh[2].Value, // Receiving app becomes sending app
            receivingFacility: reqMsh[3].Value, // Receiving facility becomes sending facility
            security: DateTime.Now.ToString(Constants.Hl7.Dtm.DtmYmdFormat),
            messageType: "RSP^K22^RSP_K21",
            messageControlID: reqMsh[9].Value, // Message control ÌD
            processingID: "T", // Processing ID,
            version: reqMsh[11].Value // HL7 version
        );

        foreach (var pid in matchingPatientIds)
        {
            responseMessage.AddNewSegment(new Segment(enc)
            {
                Name = "PID",
                Value = "PID" + pid.Serialize('|')
            });
        }
        return responseMessage;
    }
}