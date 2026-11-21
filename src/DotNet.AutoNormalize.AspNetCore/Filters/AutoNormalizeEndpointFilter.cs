using System;
using System.Threading.Tasks;
using DotNet.AutoNormalize.Abstractions;
using DotNet.AutoNormalize.Core.Engine;
using Microsoft.AspNetCore.Http;

namespace DotNet.AutoNormalize.AspNetCore.Filters;

/// <summary>
/// Minimal API endpoint filter that normalizes scalar string arguments and non-JSON objects before endpoint execution.
/// </summary>
public sealed class AutoNormalizeEndpointFilter : IEndpointFilter
{
    private readonly IAutoNormalizer _normalizer;
    private readonly StringNormalizerPipeline _pipeline;

    /// <summary>
    /// Initializes a new instance of the <see cref="AutoNormalizeEndpointFilter"/> class.
    /// </summary>
    /// <param name="normalizer">The auto normalizer service.</param>
    public AutoNormalizeEndpointFilter(IAutoNormalizer normalizer)
    {
        _normalizer = normalizer ?? throw new ArgumentNullException(nameof(normalizer));
        if (normalizer is AutoNormalizer concrete)
        {
            _pipeline = concrete.Pipeline;
        }
        else
        {
            _pipeline = new StringNormalizerPipeline(new Core.AutoNormalizeOptions());
        }
    }

    /// <inheritdoc />
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        if (context is null) throw new ArgumentNullException(nameof(context));

        var arguments = context.Arguments;
        for (int i = 0; i < arguments.Count; i++)
        {
            object? arg = arguments[i];
            if (arg is null)
            {
                continue;
            }

            if (arg is string str)
            {
                var normContext = new NormalizationContext
                {
                    Services = context.HttpContext.RequestServices
                };
                arguments[i] = _pipeline.Normalize(str, normContext);
            }
        }

        return await next(context).ConfigureAwait(false);
    }
}
