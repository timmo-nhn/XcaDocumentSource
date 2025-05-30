﻿namespace XcaXds.Commons.Models.Custom.DocumentEntryDto;

public class SubmissionSetDto : RegistryObjectDto
{
    public Author? Author { get; set; }
    public string? AvailabilityStatus { get; internal set; }
    public CodedValue? ContentTypeCode { get; internal set; }
    public string? HomeCommunityId { get; set; }
    public string? Id { get; set; }
    public CodedValue? PatientId { get; set; }
    public DateTime? SubmissionTime { get; set; }
    public string? Title { get; set; }   
    public string? UniqueId { get; set; }
    public string? SourceId { get; set; }
}