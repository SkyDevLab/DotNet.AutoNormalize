using System;
using DotNet.AutoNormalize.Abstractions;
using DotNet.AutoNormalize.AspNetCore.Filters;
using DotNet.AutoNormalize.AspNetCore.ModelBinding;
using DotNet.AutoNormalize.Core;
using DotNet.AutoNormalize.Core.Engine;
using DotNet.AutoNormalize.Json.Extensions;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Extension methods for configuring AutoNormalize services in ASP.NET Core applications.
/// </summary>
public static class AutoNormalizeServiceCollectionExtensions
{
    /// <summary>
    /// Adds AutoNormalize services with default options to the specified <see cref="IServiceCollection"/>,
    /// enabling automatic input normalization across System.Text.Json request bodies and MVC model binding.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddAutoNormalize(this IServiceCollection services)
    {
        return services.AddAutoNormalize(_ => { });
    }

    /// <summary>
    /// Adds AutoNormalize services with custom options to the specified <see cref="IServiceCollection"/>,
    /// enabling automatic input normalization across System.Text.Json request bodies and MVC model binding.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configure">Delegate to configure <see cref="AutoNormalizeOptions"/>.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddAutoNormalize(
        this IServiceCollection services,
        Action<AutoNormalizeOptions> configure)
    {
        if (services is null) throw new ArgumentNullException(nameof(services));

        var options = new AutoNormalizeOptions();
        configure?.Invoke(options);
        options.Validate();

        services.Configure<AutoNormalizeOptions>(opt =>
        {
            configure?.Invoke(opt);
            opt.Validate();
        });

        // Register standalone engine
        services.TryAddSingleton<IAutoNormalizer>(sp => new AutoNormalizer(options, sp));

        // Register Minimal API endpoint filter
        services.TryAddTransient<AutoNormalizeEndpointFilter>();

        // Configure System.Text.Json for Minimal APIs
        services.Configure<Microsoft.AspNetCore.Http.Json.JsonOptions>(httpJsonOptions =>
        {
            httpJsonOptions.SerializerOptions.AddAutoNormalize(options);
        });

        // Configure System.Text.Json for MVC Controllers
        services.Configure<Microsoft.AspNetCore.Mvc.JsonOptions>(mvcJsonOptions =>
        {
            mvcJsonOptions.JsonSerializerOptions.AddAutoNormalize(options);
        });

        // Configure MVC Model Binding for Query, Route, Form, and Header inputs
        services.Configure<Microsoft.AspNetCore.Mvc.MvcOptions>(mvcOptions =>
        {
            mvcOptions.ModelBinderProviders.Insert(0, new AutoNormalizeModelBinderProvider(options));
        });

        return services;
    }
}
