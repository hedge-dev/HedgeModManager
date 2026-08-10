namespace HedgeModManager.CodeCompiler;
using System.Text.Json.Serialization.Metadata;

[AttributeUsage(AttributeTargets.Property)]
public class CodeMetadataAttribute : Attribute
{
    public static CodeMetadataJsonTypeInfoResolver JsonTypeInfoResolver = new();

    public class CodeMetadataJsonTypeInfoResolver : DefaultJsonTypeInfoResolver
    {
        public CodeMetadataJsonTypeInfoResolver()
        {
            Modifiers.Add
            (
                typeInfo =>
                {
                    if (typeInfo.Kind != JsonTypeInfoKind.Object)
                        return;

                    foreach (var property in typeInfo.Properties)
                    {
                        var hasAttribute = property.AttributeProvider?
                            .GetCustomAttributes(typeof(CodeMetadataAttribute), true)
                            .Any() ?? false;

                        if (hasAttribute)
                            continue;

                        // Skip all properties without the CodeMetadata attribute.
                        property.ShouldSerialize = (obj, value) => false;
                    }
                }
            );
        }
    }
}
