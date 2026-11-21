using System;
using System.Reflection;
using System.Threading.Tasks;
using DotNet.AutoNormalize.Abstractions;
using DotNet.AutoNormalize.Core;
using DotNet.AutoNormalize.Core.Engine;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace DotNet.AutoNormalize.AspNetCore.ModelBinding;

/// <summary>
/// ASP.NET Core MVC model binder that applies string normalization rules to query, route, header, and form inputs.
/// </summary>
public sealed class AutoNormalizeModelBinder : IModelBinder
{
    private readonly StringNormalizerPipeline _pipeline;
    private readonly AutoNormalizeOptions _options;

    /// <summary>
    /// Initializes a new instance of the <see cref="AutoNormalizeModelBinder"/> class.
    /// </summary>
    public AutoNormalizeModelBinder(StringNormalizerPipeline pipeline, AutoNormalizeOptions options)
    {
        _pipeline = pipeline ?? throw new ArgumentNullException(nameof(pipeline));
        _options = options ?? throw new ArgumentNullException(nameof(options));
    }

    /// <inheritdoc />
    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
        if (bindingContext is null)
        {
            throw new ArgumentNullException(nameof(bindingContext));
        }

        string modelName = bindingContext.ModelName;
        ValueProviderResult valueProviderResult = bindingContext.ValueProvider.GetValue(modelName);

        if (valueProviderResult == ValueProviderResult.None)
        {
            return Task.CompletedTask;
        }

        bindingContext.ModelState.SetModelValue(modelName, valueProviderResult);

        string? rawValue = valueProviderResult.FirstValue;

        // Extract property/parameter attributes from ModelMetadata
        PropertyNormalizationMetadata? metadata = ExtractMetadata(bindingContext);

        if (metadata?.DoNotNormalize == true)
        {
            bindingContext.Result = ModelBindingResult.Success(rawValue);
            return Task.CompletedTask;
        }

        var context = new NormalizationContext
        {
            PropertyInfo = metadata?.PropertyInfo,
            TargetType = bindingContext.ModelMetadata.ContainerType ?? typeof(object),
            Services = bindingContext.HttpContext.RequestServices
        };

        string? normalized = _pipeline.Normalize(rawValue, context, metadata);
        bindingContext.Result = ModelBindingResult.Success(normalized);
        return Task.CompletedTask;
    }

    private PropertyNormalizationMetadata? ExtractMetadata(ModelBindingContext bindingContext)
    {
        var modelMetadata = bindingContext.ModelMetadata;
        if (modelMetadata.ContainerType is null || modelMetadata.PropertyName is null)
        {
            return null;
        }

        PropertyInfo? prop = modelMetadata.ContainerType.GetProperty(
            modelMetadata.PropertyName,
            BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);

        if (prop is null)
        {
            return null;
        }

        return new PropertyNormalizationMetadata(prop, _options, bindingContext.HttpContext.RequestServices);
    }
}
