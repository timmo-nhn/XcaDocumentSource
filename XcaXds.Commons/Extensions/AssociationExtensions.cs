using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XcaXds.Commons.Models.Soap.XdsTypes;

namespace XcaXds.Commons.Extensions;

public static class AssociationExtensions
{

    /// A SubmissionSet and a DocumentEntry – SS-DE HasMember
    public static void DeprecateDocumentEntry
        (this ExtrinsicObjectType extrinsicObjectDocumentEntry)
    {
        extrinsicObjectDocumentEntry.Status = Constants.Xds.StatusValues.Deprecated;
    }

    // Implemeneted according to relationships defined here
    // https://profiles.ihe.net/ITI/TF/Volume3/ch-4.2.html#4.2.2

    /// A SubmissionSet and a DocumentEntry – SS-DE HasMember
    public static AssociationType CreateAssociationBetweenSubmissionsetAndDocumentEntry
        (RegistryPackageType registryPackageSubmissionset, ExtrinsicObjectType extrinsicObjectDocumentEntry)
    {
        return CreateAssociation(registryPackageSubmissionset, extrinsicObjectDocumentEntry);
        
    }

    /// A SubmissionSet and a Folder – SS-FD HasMember
    public static AssociationType CreateAssociationBetweenSubmissionsetAndFolder
        (RegistryPackageType registryPackageSubmissionset, RegistryPackageType registryPackageFolder)
    {
        return CreateAssociation(registryPackageSubmissionset, registryPackageFolder);
    }

    /// A Folder and a DocumentEntry – FD-DE HasMember
    public static AssociationType CreateAssociationBetweenFolderAndDocumentEntry
        (RegistryPackageType registryPackageFolder, ExtrinsicObjectType extrinsicObjectDocumentEntry)
    {
        return CreateAssociation(registryPackageFolder, extrinsicObjectDocumentEntry);
    }

    /// A SubmissionSet and an Association – SS-HM HasMember
    public static AssociationType CreateAssociationBetweenSubmissionsetAndAssociation
        (RegistryPackageType registryPackageSubmissionset, AssociationType association)
    {
        return CreateAssociation(registryPackageSubmissionset, association);
    }

    /// A DocumentEntry and another DocumentEntry – Relationship
    public static AssociationType CreateAssociationBetweenDocumentEntryAndDocumentEntry
        (RegistryPackageType registryPackageSubmissionset, ExtrinsicObjectType extrinsicObjectDocumentEntry)
    {
        return CreateAssociation(registryPackageSubmissionset, extrinsicObjectDocumentEntry);
    }

    private static AssociationType CreateAssociation(IdentifiableType it1, IdentifiableType it2)
    {
        var assoc = new AssociationType()
        {
            
        };
        throw new NotImplementedException();
    }
}
