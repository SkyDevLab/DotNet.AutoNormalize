namespace DotNet.AutoNormalize.Abstractions;

/// <summary>
/// Specialized normalization contract for nullable string values.
/// </summary>
public interface IStringNormalizer : INormalizer<string?>
{
}
