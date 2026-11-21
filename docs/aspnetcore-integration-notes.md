# ASP.NET Core Integration Implementation Notes: Phase 4

## 1. Overview & Components Implemented

Phase 4 delivers the web integration layer connecting the core engine to modern ASP.NET Core (.NET 8 & .NET 9) applications:

```text
DotNet.AutoNormalize/
├── src/
│   ├── DotNet.AutoNormalize.Json/
│   │   ├── AutoNormalizeJsonTypeInfoModifier.cs             # Property contract inspection & converter attachment
│   │   ├── Converters/AutoNormalizeStringJsonConverter.cs   # Property-scoped System.Text.Json string converter
│   │   └── Extensions/AutoNormalizeJsonSerializerOptionsExtensions.cs # AddAutoNormalize for JsonSerializerOptions
│   │
│   └── DotNet.AutoNormalize.AspNetCore/
│       ├── ModelBinding/AutoNormalizeModelBinder.cs         # MVC ModelBinder for Query, Route, Form, Headers
│       ├── ModelBinding/AutoNormalizeModelBinderProvider.cs # MVC ModelBinderProvider for string types
│       ├── Filters/AutoNormalizeEndpointFilter.cs           # Minimal API endpoint filter for parameter normalization
│       └── Extensions/AutoNormalizeServiceCollectionExtensions.cs # builder.Services.AddAutoNormalize()
│
└── tests/
    └── DotNet.AutoNormalize.AspNetCore.Tests/               # Live integration test suite via WebApplicationFactory
        ├── SystemTextJsonDeserializationTests.cs            # Direct JSON deserialization verification
        ├── MinimalApiIntegrationTests.cs                    # Minimal APIs live HTTP integration tests
        └── MvcIntegrationTests.cs                           # MVC Controller live HTTP integration tests
```

---

## 2. Ingress Boundary Mechanics

### 2.1 System.Text.Json Deserialization (Minimal APIs & MVC `[FromBody]`)
Rather than running an expensive post-deserialization reflection pass over deserialized objects, `DotNet.AutoNormalize.Json` configures an `IJsonTypeInfoResolver` modifier:
* Intercepts `JsonPropertyInfo` creation once during contract resolution (cold path).
* Detects sensitive property names, `[PreserveWhitespace]`, and `[DoNotNormalize]` on the target property.
* Attaches `AutoNormalizeStringJsonConverter` as `jsonProp.CustomConverter`.
* **C# Records & Init-Only Properties**: Because `CustomConverter.Read()` executes *while* JSON tokens are being parsed, values passed to primary constructor parameters and `init;` properties are normalized **at birth**.

### 2.2 MVC Model Binding (Query, Route, Form, Headers)
* `AutoNormalizeModelBinderProvider` registers at index `0` of `MvcOptions.ModelBinderProviders`.
* Binds incoming `string` values from query strings (`[FromQuery]`), route values (`[FromRoute]`), form fields (`[FromForm]`), and headers (`[FromHeader]`).
* Resolves parameter and container metadata, applying whitespace trimming, empty-to-null conversion, and sensitive field protection before action execution.

### 2.3 Minimal APIs Parameter Normalization
* Request bodies are automatically normalized via `Microsoft.AspNetCore.Http.Json.JsonOptions`.
* For query parameters and route parameters in Minimal APIs, `AutoNormalizeEndpointFilter` can be applied to endpoints or route groups (`app.MapGroup("/api").AddEndpointFilter<AutoNormalizeEndpointFilter>()`).

---

## 3. Live Test Verification Summary

All 11 integration tests executed against real HTTP test servers (`TestServer`) passed with 100% success:

1. **System.Text.Json Deserialization**:
   - `Deserialize_ClassDto_NormalizesStringsAndPreservesSensitiveFields`: PASS
   - `Deserialize_PositionalRecord_NormalizesPropertiesAtBirth`: PASS
   - `Deserialize_EmptyString_ConvertsToNullWhenConfigured`: PASS
2. **Minimal APIs**:
   - `Post_ClassDto_NormalizesBodyAndPreservesSensitivePassword`: PASS
   - `Post_PositionalRecord_NormalizesPropertiesAtBirth`: PASS
   - `Get_ScalarParameterWithEndpointFilter_NormalizesQueryParameter`: PASS
3. **MVC Controllers**:
   - `Mvc_FromBody_NormalizesDtoAndPreservesPassword`: PASS
   - `Mvc_FromQuery_NormalizesQueryStringParameters`: PASS
   - `Mvc_FromRoute_NormalizesRouteParameter`: PASS
   - `Mvc_FromForm_NormalizesFormDataInputs`: PASS
   - `DependencyInjection_ResolvesIAutoNormalizerInstance`: PASS
