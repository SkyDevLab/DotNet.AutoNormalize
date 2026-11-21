namespace DotNet.AutoNormalize.Core;

/// <summary>
/// Specifies how whitespace is trimmed from string values.
/// </summary>
public enum WhitespaceTrimMode
{
    /// <summary>
    /// Trims both leading and trailing whitespace.
    /// </summary>
    Both = 0,

    /// <summary>
    /// Trims only leading whitespace.
    /// </summary>
    Start = 1,

    /// <summary>
    /// Trims only trailing whitespace.
    /// </summary>
    End = 2
}
