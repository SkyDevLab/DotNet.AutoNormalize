using System;
using System.Threading;
using System.Threading.Tasks;
using DotNet.AutoNormalize.Abstractions;
using DotNet.AutoNormalize.Core;
using DotNet.AutoNormalize.Core.Engine;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace DotNet.AutoNormalize.EntityFrameworkCore.Interceptors;

/// <summary>
/// Entity Framework Core <see cref="ISaveChangesInterceptor"/> that normalizes entity string properties
/// before persistence during <c>SaveChanges</c> and <c>SaveChangesAsync</c>.
/// </summary>
public sealed class AutoNormalizeSaveChangesInterceptor : SaveChangesInterceptor
{
    private readonly AutoNormalizeOptions _options;
    private readonly StringNormalizerPipeline _pipeline;
    private readonly NormalizationMetadataCache _metadataCache;
    private readonly IServiceProvider? _serviceProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="AutoNormalizeSaveChangesInterceptor"/> class.
    /// </summary>
    /// <param name="options">Optional auto-normalize options; defaults to standard options if null.</param>
    /// <param name="serviceProvider">Optional service provider for resolving custom normalizers.</param>
    public AutoNormalizeSaveChangesInterceptor(
        AutoNormalizeOptions? options = null,
        IServiceProvider? serviceProvider = null)
    {
        _options = options ?? new AutoNormalizeOptions();
        _options.Validate();

        _pipeline = new StringNormalizerPipeline(_options);
        _metadataCache = new NormalizationMetadataCache(_options, serviceProvider);
        _serviceProvider = serviceProvider;
    }

    /// <inheritdoc />
    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        if (eventData.Context != null)
        {
            NormalizeEntities(eventData.Context);
        }

        return base.SavingChanges(eventData, result);
    }

    /// <inheritdoc />
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (eventData.Context != null)
        {
            NormalizeEntities(eventData.Context);
        }

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void NormalizeEntities(DbContext context)
    {
        var entries = context.ChangeTracker.Entries();

        foreach (EntityEntry entry in entries)
        {
            if (entry.State != EntityState.Added && entry.State != EntityState.Modified)
            {
                continue;
            }

            Type entityType = entry.Entity.GetType();

            // Check type-level exclusion
            if (entityType.IsDefined(typeof(DoNotNormalizeAttribute), true))
            {
                continue;
            }

            TypeNormalizationMetadata typeMeta = _metadataCache.GetMetadata(entityType);
            if (typeMeta.IsDoNotNormalize || typeMeta.IsIgnoredType)
            {
                continue;
            }

            for (int i = 0; i < typeMeta.StringProperties.Count; i++)
            {
                PropertyNormalizationMetadata propMeta = typeMeta.StringProperties[i];
                if (propMeta.DoNotNormalize)
                {
                    continue;
                }

                PropertyEntry? efProp = entry.Property(propMeta.PropertyName);
                if (efProp is null)
                {
                    continue;
                }

                // Only normalize if modified or added
                if (!efProp.IsModified && entry.State != EntityState.Added)
                {
                    continue;
                }

                string? current = efProp.CurrentValue as string;

                var normContext = new NormalizationContext
                {
                    PropertyInfo = propMeta.PropertyInfo,
                    TargetType = entityType,
                    Services = _serviceProvider
                };

                string? normalized = _pipeline.Normalize(current, normContext, propMeta);

                if (!ReferenceEquals(current, normalized) && current != normalized)
                {
                    efProp.CurrentValue = normalized;
                }
            }
        }
    }
}
