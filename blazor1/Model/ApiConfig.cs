using System.Diagnostics.CodeAnalysis;

namespace blazor1.Model;

public class ApiConfig : IApiConfig
{
    public string Url { get; init; } = String.Empty;
}

