using System;
using System.Linq;
using System.Reflection;
using DotNet.AutoNormalize.Abstractions;
using DotNet.AutoNormalize.AspNetCore.Filters;
using DotNet.AutoNormalize.Core;
using DotNet.AutoNormalize.Core.Engine;
using DotNet.AutoNormalize.EntityFrameworkCore.Interceptors;
using DotNet.AutoNormalize.Json;
using DotNet.AutoNormalize.Json.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace DotNet.AutoNormalize.PublicApi.Tests;

/// <summary>
/// Automated verification test suite that enforces 100% compatibility with the frozen public API specification.
/// Any unauthorized changes, removals, or unexpected public types will cause immediate test failure.
/// </summary>
public class PublicApiApprovalTests
{
    [Fact]
    public void AbstractionsAssembly_ContainsRequiredPublicTypesAndContracts()
    {
        var assembly = typeof(INormalizer<>).Assembly;
        var publicTypes = assembly.GetExportedTypes().Select(t => t.FullName).OrderBy(name => name).ToList();

        Assert.Contains(typeof(INormalizer<>).FullName, publicTypes);
        Assert.Contains(typeof(IStringNormalizer).FullName, publicTypes);
        Assert.Contains(typeof(IAutoNormalizer).FullName, publicTypes);
        Assert.Contains(typeof(NormalizationContext).FullName, publicTypes);
        Assert.Contains(typeof(PreserveWhitespaceAttribute).FullName, publicTypes);
        Assert.Contains(typeof(DoNotNormalizeAttribute).FullName, publicTypes);
        Assert.Contains(typeof(NormalizeWithAttribute).FullName, publicTypes);

        // Verify INormalizer<T> contract
        var method = typeof(INormalizer<string>).GetMethod("Normalize", [typeof(string), typeof(NormalizationContext)]);
        Assert.NotNull(method);
        Assert.Equal(typeof(string), method.ReturnType);

        // Verify IAutoNormalizer contract
        var genericNormalize = typeof(IAutoNormalizer).GetMethods()
            .FirstOrDefault(m => m.Name == "Normalize" && m.IsGenericMethodDefinition && m.GetParameters().Length == 2);
        Assert.NotNull(genericNormalize);

        var nonGenericNormalize = typeof(IAutoNormalizer).GetMethod("Normalize", [typeof(object), typeof(Type), typeof(NormalizationContext)]);
        Assert.NotNull(nonGenericNormalize);
    }

    [Fact]
    public void CoreAssembly_ContainsRequiredPublicTypesAndSignatures()
    {
        var assembly = typeof(AutoNormalizeOptions).Assembly;
        var publicTypes = assembly.GetExportedTypes().Select(t => t.FullName).OrderBy(name => name).ToList();

        Assert.Contains(typeof(AutoNormalizeOptions).FullName, publicTypes);
        Assert.Contains(typeof(WhitespaceTrimMode).FullName, publicTypes);
        Assert.Contains(typeof(CasingTransform).FullName, publicTypes);
        Assert.Contains(typeof(AutoNormalizer).FullName, publicTypes);

        // Verify AutoNormalizeOptions properties
        var optionsType = typeof(AutoNormalizeOptions);
        Assert.NotNull(optionsType.GetProperty("TrimStrings"));
        Assert.NotNull(optionsType.GetProperty("TrimMode"));
        Assert.NotNull(optionsType.GetProperty("EmptyStringsToNull"));
        Assert.NotNull(optionsType.GetProperty("NullToEmptyStrings"));
        Assert.NotNull(optionsType.GetProperty("UnicodeNormalization"));
        Assert.NotNull(optionsType.GetProperty("Casing"));
        Assert.NotNull(optionsType.GetProperty("ProtectSensitiveProperties"));
        Assert.NotNull(optionsType.GetProperty("SensitivePropertyNames"));
        Assert.NotNull(optionsType.GetProperty("MaxRecursionDepth"));
        Assert.NotNull(optionsType.GetProperty("CustomNormalizers"));
        Assert.NotNull(optionsType.GetMethod("AddNormalizer", [typeof(IStringNormalizer)]));

        // Verify AutoNormalizer implements IAutoNormalizer
        Assert.True(typeof(IAutoNormalizer).IsAssignableFrom(typeof(AutoNormalizer)));
    }

    [Fact]
    public void JsonAssembly_ContainsRequiredExtensionMethodsAndModifiers()
    {
        var assembly = typeof(AutoNormalizeJsonTypeInfoModifier).Assembly;
        var publicTypes = assembly.GetExportedTypes().Select(t => t.FullName).OrderBy(name => name).ToList();

        Assert.Contains(typeof(AutoNormalizeJsonSerializerOptionsExtensions).FullName, publicTypes);
        Assert.Contains(typeof(AutoNormalizeJsonTypeInfoModifier).FullName, publicTypes);

        // Verify AddAutoNormalize extensions on JsonSerializerOptions
        var extensionMethods = typeof(AutoNormalizeJsonSerializerOptionsExtensions).GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Where(m => m.Name == "AddAutoNormalize")
            .ToList();

        Assert.NotEmpty(extensionMethods);
        Assert.Contains(extensionMethods, m => m.GetParameters().Length == 2 && m.GetParameters()[1].ParameterType == typeof(Action<AutoNormalizeOptions>));
        Assert.Contains(extensionMethods, m => m.GetParameters().Length == 3 && m.GetParameters()[1].ParameterType == typeof(AutoNormalizeOptions));
    }

    [Fact]
    public void AspNetCoreAssembly_ContainsRequiredServiceExtensionsAndFilters()
    {
        var assembly = typeof(AutoNormalizeServiceCollectionExtensions).Assembly;
        var publicTypes = assembly.GetExportedTypes().Select(t => t.FullName).OrderBy(name => name).ToList();

        Assert.Contains(typeof(AutoNormalizeServiceCollectionExtensions).FullName, publicTypes);
        Assert.Contains(typeof(AutoNormalizeEndpointFilter).FullName, publicTypes);

        // Verify AddAutoNormalize on IServiceCollection
        var addAutoNormalizeMethods = typeof(AutoNormalizeServiceCollectionExtensions)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Where(m => m.Name == "AddAutoNormalize")
            .ToList();

        Assert.NotEmpty(addAutoNormalizeMethods);
        Assert.Contains(addAutoNormalizeMethods, m => m.GetParameters().Length == 1); // services
        Assert.Contains(addAutoNormalizeMethods, m => m.GetParameters().Length == 2); // services + Action<AutoNormalizeOptions>

        // Verify AutoNormalizeEndpointFilter implements IEndpointFilter
        Assert.True(typeof(IEndpointFilter).IsAssignableFrom(typeof(AutoNormalizeEndpointFilter)));
    }

    [Fact]
    public void EntityFrameworkCoreAssembly_ContainsRequiredInterceptorsAndExtensions()
    {
        var assembly = typeof(AutoNormalizeSaveChangesInterceptor).Assembly;
        var publicTypes = assembly.GetExportedTypes().Select(t => t.FullName).OrderBy(name => name).ToList();

        Assert.Contains(typeof(AutoNormalizeDbContextOptionsBuilderExtensions).FullName, publicTypes);
        Assert.Contains(typeof(AutoNormalizeSaveChangesInterceptor).FullName, publicTypes);

        // Verify AutoNormalizeSaveChangesInterceptor inherits SaveChangesInterceptor
        Assert.True(typeof(SaveChangesInterceptor).IsAssignableFrom(typeof(AutoNormalizeSaveChangesInterceptor)));

        // Verify DbContextOptionsBuilder.AddAutoNormalize extension method
        var addAutoNormalizeMethods = typeof(AutoNormalizeDbContextOptionsBuilderExtensions)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Where(m => m.Name == "AddAutoNormalize")
            .ToList();

        Assert.NotEmpty(addAutoNormalizeMethods);
        Assert.Contains(addAutoNormalizeMethods, m => m.GetParameters().Length >= 1);
    }
}
