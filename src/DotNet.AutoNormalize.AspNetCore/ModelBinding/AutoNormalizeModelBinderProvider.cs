using System;
using DotNet.AutoNormalize.Abstractions;
using DotNet.AutoNormalize.Core;
using DotNet.AutoNormalize.Core.Engine;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace DotNet.AutoNormalize.AspNetCore.ModelBinding;

/// <summary>
/// Model binder provider that supplies <see cref="AutoNormalizeModelBinder"/> for string model types.
/// </summary>
public sealed class AutoNormalizeModelBinderProvider : IModelBinderProvider
{
    private readonly AutoNormalizeOptions _options;
    private readonly StringNormalizerPipeline _pipeline;

    /// <summary>
    /// Initializes a new instance of the <see cref="AutoNormalizeModelBinderProvider"/> class.
    /// </summary>
    /// <param name="options">The auto-normalize options.</param>
    public AutoNormalizeModelBinderProvider(AutoNormalizeOptions options)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _pipeline = new StringNormalizerPipeline(_options);
    }

    /// <inheritdoc />
    public IModelBinder? GetBinder(ModelBinderProviderContext context)
    {
        if (context is null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        // Only handle string models
        if (context.Metadata.ModelType != typeof(string))
        {
            return null;
        }

        // Check if declaring container type is decorated with [DoNotNormalize]
        if (context.Metadata.ContainerType?.IsDefined(typeof(DoNotNormalizeAttribute), true) == true)
        {
            return null;
        }

        return new AutoNormalizeModelBinder(_pipeline, _options);
    }
}
