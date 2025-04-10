﻿using XcaXds.Commons.Models.Soap.XdsTypes;

namespace XcaXds.Commons.Models.Custom;

public class DocumentRequestValuesInput
{
    public DocumentRequestValuesInput()
    {
        DocumentRequest = new();
        DocumentRequest.DocumentUniqueId ??= string.Empty;
        DocumentRequest.RepositoryUniqueId ??= string.Empty;
        DocumentRequest.HomeCommunityId ??= string.Empty;
    }
    public Guid Id { get; set; } = Guid.NewGuid();
    public DocumentRequestType DocumentRequest { get; set; } = new() { };

}

