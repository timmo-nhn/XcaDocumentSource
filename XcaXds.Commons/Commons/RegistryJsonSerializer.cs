﻿using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using XcaXds.Commons.Models.Custom.DocumentEntry;

namespace XcaXds.Commons.Commons;

public static class RegistryJsonSerializer
{
    private static JsonSerializerOptions _jsonOptions = new()
    {
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
        WriteIndented = true,
        TypeInfoResolver = new DefaultJsonTypeInfoResolver
        {
            Modifiers =
            {
                type =>
                {
                    if (type.Type == typeof(RegistryObjectDto))
                    {
                        type.PolymorphismOptions = new JsonPolymorphismOptions
                        {
                            TypeDiscriminatorPropertyName = "$type",
                            IgnoreUnrecognizedTypeDiscriminators = true,
                            DerivedTypes =
                            {
                                new JsonDerivedType(typeof(DocumentEntryDto), "DocumentEntryDto"),
                                new JsonDerivedType(typeof(SubmissionSetDto), "SubmissionSetDto"),
                                new JsonDerivedType(typeof(AssociationDto), "AssociationDto")
                            }
                        };
                    }
                }
            }
        }
    };

    public static T? Deserialize<T>(string input)
    {
        return JsonSerializer.Deserialize<T>(input, _jsonOptions);
    }

    public static string Serialize(object input)
    {
        return JsonSerializer.Serialize(input, _jsonOptions);
    }
}
