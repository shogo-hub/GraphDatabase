using System.Text.Json;

namespace Backend.Common.Serialization.Json;

public static class JsonSerializerOptionsExtensions
{
    public static void CopyTo(this JsonSerializerOptions self, JsonSerializerOptions other)
    {
        other.AllowOutOfOrderMetadataProperties = self.AllowOutOfOrderMetadataProperties;
        other.AllowTrailingCommas = self.AllowTrailingCommas;
        other.DictionaryKeyPolicy = self.DictionaryKeyPolicy;
        other.DefaultIgnoreCondition = self.DefaultIgnoreCondition;
        other.DefaultBufferSize = self.DefaultBufferSize;
        other.Encoder = self.Encoder;
        other.IgnoreReadOnlyFields = self.IgnoreReadOnlyFields;
        other.IgnoreReadOnlyProperties = self.IgnoreReadOnlyProperties;
        other.IncludeFields = self.IncludeFields;
        other.IndentCharacter = self.IndentCharacter;
        other.IndentSize = self.IndentSize;
        other.MaxDepth = self.MaxDepth;
        other.NewLine = self.NewLine;
        other.NumberHandling = self.NumberHandling;
        other.PreferredObjectCreationHandling = self.PreferredObjectCreationHandling;
        other.PropertyNameCaseInsensitive = self.PropertyNameCaseInsensitive;
        other.PropertyNamingPolicy = self.PropertyNamingPolicy;
        other.ReadCommentHandling = self.ReadCommentHandling;
        other.ReferenceHandler = self.ReferenceHandler;
        other.RespectNullableAnnotations = self.RespectNullableAnnotations;
        other.RespectRequiredConstructorParameters = self.RespectRequiredConstructorParameters;
        other.TypeInfoResolver = self.TypeInfoResolver;
        other.UnknownTypeHandling = self.UnknownTypeHandling;
        other.UnmappedMemberHandling = self.UnmappedMemberHandling;
        other.WriteIndented = self.WriteIndented;

        other.Converters.Clear();

        foreach (var converter in self.Converters)
        {
            other.Converters.Add(converter);
        }

        other.TypeInfoResolverChain.Clear();

        foreach (var typeInfoResolver in self.TypeInfoResolverChain)
        {
            other.TypeInfoResolverChain.Add(typeInfoResolver);
        }
    }

    public static JsonSerializerOptions Copy(this JsonSerializerOptions self)
    {
        var other = new JsonSerializerOptions
        {
            AllowOutOfOrderMetadataProperties = self.AllowOutOfOrderMetadataProperties,
            AllowTrailingCommas = self.AllowTrailingCommas,
            DictionaryKeyPolicy = self.DictionaryKeyPolicy,
            DefaultIgnoreCondition = self.DefaultIgnoreCondition,
            DefaultBufferSize = self.DefaultBufferSize,
            Encoder = self.Encoder,
            IgnoreReadOnlyFields = self.IgnoreReadOnlyFields,
            IgnoreReadOnlyProperties = self.IgnoreReadOnlyProperties,
            IncludeFields = self.IncludeFields,
            IndentCharacter = self.IndentCharacter,
            IndentSize = self.IndentSize,
            MaxDepth = self.MaxDepth,
            NewLine = self.NewLine,
            NumberHandling = self.NumberHandling,
            PreferredObjectCreationHandling = self.PreferredObjectCreationHandling,
            PropertyNameCaseInsensitive = self.PropertyNameCaseInsensitive,
            PropertyNamingPolicy = self.PropertyNamingPolicy,
            ReadCommentHandling = self.ReadCommentHandling,
            ReferenceHandler = self.ReferenceHandler,
            RespectNullableAnnotations = self.RespectNullableAnnotations,
            RespectRequiredConstructorParameters = self.RespectRequiredConstructorParameters,
            TypeInfoResolver = self.TypeInfoResolver,
            UnknownTypeHandling = self.UnknownTypeHandling,
            UnmappedMemberHandling = self.UnmappedMemberHandling,
            WriteIndented = self.WriteIndented
        };

        other.Converters.Clear();

        foreach (var converter in self.Converters)
        {
            other.Converters.Add(converter);
        }

        other.TypeInfoResolverChain.Clear();

        foreach (var typeInfoResolver in self.TypeInfoResolverChain)
        {
            other.TypeInfoResolverChain.Add(typeInfoResolver);
        }

        return other;
    }

    public static JsonSerializerOptions With(this JsonSerializerOptions self, Action<JsonSerializerOptions> mutate)
    {
        var result = self.Copy();
        mutate(result);
        return result;
    }
}