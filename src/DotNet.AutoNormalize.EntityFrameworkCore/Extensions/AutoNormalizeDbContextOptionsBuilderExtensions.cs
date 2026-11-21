using System;
using DotNet.AutoNormalize.Core;
using DotNet.AutoNormalize.EntityFrameworkCore.Interceptors;
using Microsoft.EntityFrameworkCore;

namespace Microsoft.EntityFrameworkCore;

/// <summary>
/// Extension methods for configuring AutoNormalize on <see cref="DbContextOptionsBuilder"/>.
/// </summary>
public static class AutoNormalizeDbContextOptionsBuilderExtensions
{
    /// <summary>
    /// Adds the <see cref="AutoNormalizeSaveChangesInterceptor"/> to normalize entity strings prior to database persistence with custom options.
    /// </summary>
    /// <param name="optionsBuilder">The DbContext options builder.</param>
    /// <param name="configure">Delegate to configure options.</param>
    /// <param name="serviceProvider">Optional service provider for resolving custom normalizers.</param>
    /// <returns>The options builder instance for chaining.</returns>
    public static DbContextOptionsBuilder AddAutoNormalize(
        this DbContextOptionsBuilder optionsBuilder,
        Action<AutoNormalizeOptions> configure,
        IServiceProvider? serviceProvider = null)
    {
        if (optionsBuilder is null) throw new ArgumentNullException(nameof(optionsBuilder));
        if (configure is null) throw new ArgumentNullException(nameof(configure));

        var options = new AutoNormalizeOptions();
        configure(options);
        return optionsBuilder.AddAutoNormalize(options, serviceProvider);
    }

    /// <summary>
    /// Adds the <see cref="AutoNormalizeSaveChangesInterceptor"/> to normalize entity strings prior to database persistence.
    /// </summary>
    /// <param name="optionsBuilder">The DbContext options builder.</param>
    /// <param name="options">Optional auto-normalize options.</param>
    /// <param name="serviceProvider">Optional service provider for resolving custom normalizers.</param>
    /// <returns>The options builder instance for chaining.</returns>
    public static DbContextOptionsBuilder AddAutoNormalize(
        this DbContextOptionsBuilder optionsBuilder,
        AutoNormalizeOptions? options = null,
        IServiceProvider? serviceProvider = null)
    {
        if (optionsBuilder is null) throw new ArgumentNullException(nameof(optionsBuilder));

        var interceptor = new AutoNormalizeSaveChangesInterceptor(options, serviceProvider);
        return optionsBuilder.AddInterceptors(interceptor);
    }

    /// <summary>
    /// Adds the <see cref="AutoNormalizeSaveChangesInterceptor"/> to normalize entity strings prior to database persistence with custom options.
    /// </summary>
    /// <typeparam name="TContext">The DbContext type.</typeparam>
    /// <param name="optionsBuilder">The generic DbContext options builder.</param>
    /// <param name="configure">Delegate to configure options.</param>
    /// <param name="serviceProvider">Optional service provider for resolving custom normalizers.</param>
    /// <returns>The options builder instance for chaining.</returns>
    public static DbContextOptionsBuilder<TContext> AddAutoNormalize<TContext>(
        this DbContextOptionsBuilder<TContext> optionsBuilder,
        Action<AutoNormalizeOptions> configure,
        IServiceProvider? serviceProvider = null)
        where TContext : DbContext
    {
        if (optionsBuilder is null) throw new ArgumentNullException(nameof(optionsBuilder));
        if (configure is null) throw new ArgumentNullException(nameof(configure));

        var options = new AutoNormalizeOptions();
        configure(options);
        return optionsBuilder.AddAutoNormalize(options, serviceProvider);
    }

    /// <summary>
    /// Adds the <see cref="AutoNormalizeSaveChangesInterceptor"/> to normalize entity strings prior to database persistence.
    /// </summary>
    /// <typeparam name="TContext">The DbContext type.</typeparam>
    /// <param name="optionsBuilder">The generic DbContext options builder.</param>
    /// <param name="options">Optional auto-normalize options.</param>
    /// <param name="serviceProvider">Optional service provider for resolving custom normalizers.</param>
    /// <returns>The options builder instance for chaining.</returns>
    public static DbContextOptionsBuilder<TContext> AddAutoNormalize<TContext>(
        this DbContextOptionsBuilder<TContext> optionsBuilder,
        AutoNormalizeOptions? options = null,
        IServiceProvider? serviceProvider = null)
        where TContext : DbContext
    {
        if (optionsBuilder is null) throw new ArgumentNullException(nameof(optionsBuilder));

        var interceptor = new AutoNormalizeSaveChangesInterceptor(options, serviceProvider);
        return (DbContextOptionsBuilder<TContext>)optionsBuilder.AddInterceptors(interceptor);
    }
}
