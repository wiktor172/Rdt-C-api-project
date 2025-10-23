using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;

namespace WorkExperienceOct2024.Client.Core.Services;

internal static class ApiClientHelpers
{
    private sealed record ProblemPayload(string? Title, string? Detail, string? Message);

    public static async Task<string?> ExtractErrorMessageAsync(HttpResponseMessage response, CancellationToken ct)
    {
        if (response.Content is null)
        {
            return null;
        }

        try
        {
            var payload = await response.Content.ReadFromJsonAsync<ProblemPayload>(cancellationToken: ct);
            if (payload is not null)
            {
                if (!string.IsNullOrWhiteSpace(payload.Detail))
                {
                    return payload.Detail.Trim();
                }

                if (!string.IsNullOrWhiteSpace(payload.Message))
                {
                    return payload.Message.Trim();
                }

                if (!string.IsNullOrWhiteSpace(payload.Title))
                {
                    return payload.Title.Trim();
                }
            }
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            throw;
        }
        catch (NotSupportedException)
        {
            // The response did not contain JSON. Fall back to reading the raw body.
        }
        catch (JsonException)
        {
            // The response was not valid JSON.
        }

        var fallback = await response.Content.ReadAsStringAsync(ct);
        return string.IsNullOrWhiteSpace(fallback) ? null : fallback.Trim();
    }
}
