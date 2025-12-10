using System.Net.Http.Headers;
using System.Text.Json;
using VitalHelse.Models;

namespace VitalHelse.Services;

/// <summary>
/// Service used to communicate with the Tripletex API to retrieve products and stock information.
/// </summary>
public class TripletexService
{
    private readonly HttpClient _client;
    private readonly IConfiguration _config;

    /// <summary>
    /// Initializes a new instance of the <see cref="TripletexService"/> class with configuration and HttpClient.
    /// </summary>
    public TripletexService(IConfiguration config)
    {
        _config = config;
        _client = new HttpClient();
        _client.BaseAddress = new Uri("https://api-test.tripletex.tech/v2/"); // Test environment
    }

    /// <summary>
    /// Retrieves a Tripletex session token using the Consumer and Employee tokens stored in configuration.
    /// </summary>
    /// <returns>A valid Tripletex session token as a string.</returns>
    /// <exception cref="Exception">Thrown if the API request fails.</exception>
    public async Task<string> GetSessionTokenAsync()
    {
        var consumerToken = Uri.EscapeDataString(_config["Tripletex:ConsumerToken"] ?? "");
        var employeeToken = Uri.EscapeDataString(_config["Tripletex:EmployeeToken"] ?? "");
        var expirationDate = DateTime.UtcNow.AddDays(1).ToString("yyyy-MM-dd");

        var url = $"token/session/:create?consumerToken={consumerToken}&employeeToken={employeeToken}&expirationDate={expirationDate}";
        var response = await _client.PutAsync(url, null);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new Exception($"Failed to retrieve session token ({response.StatusCode}):\n{error}");
        }

        var json = await response.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(json);
        return doc.RootElement.GetProperty("value").GetProperty("token").GetString()!;
    }

    /// <summary>
    /// Retrieves a list of products from Tripletex, including name, prices, and stock count.
    /// </summary>
    /// <param name="sessionToken">A valid Tripletex session token.</param>
    /// <returns>A list of <see cref="TripletexProduct"/> objects.</returns>
    /// <exception cref="Exception">Thrown if the API request fails.</exception>
    public async Task<List<TripletexProduct>> GetProductsAsync(string sessionToken)
    {
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Basic",
                Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes($"0:{sessionToken}")));

        var response = await _client.GetAsync("product");

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync();
            throw new Exception($"Tripletex GET /product failed: {response.StatusCode}\n{body}");
        }

        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(json);

        var values = doc.RootElement.GetProperty("values");
        var tasks = values.EnumerateArray().Select(async item =>
        {
            int id = item.GetProperty("id").GetInt32();
            double stock = await GetProductDetailsAsync(sessionToken, id);
            return new TripletexProduct
            {
                Id = id,
                Name = item.GetProperty("name").GetString() ?? "",
                PriceExVat = item.TryGetProperty("priceExcludingVatCurrency", out var p1) ? p1.GetDecimal() : 0,
                PriceInVat = item.TryGetProperty("priceIncludingVatCurrency", out var p2) ? p2.GetDecimal() : 0,
                StockCount = stock
            };
        });
        return (await Task.WhenAll(tasks)).ToList();
    }

    /// <summary>
    /// Retrieves stock quantity for a single product.
    /// </summary>
    /// <param name="sessionToken">A valid Tripletex session token.</param>
    /// <param name="productId">The Tripletex product ID.</param>
    /// <returns>The stock count as a double.</returns>
    /// <exception cref="Exception">Thrown if the API request fails.</exception>
    public async Task<double> GetProductDetailsAsync(string sessionToken, int productId)
    {
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Basic",
                Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes($"0:{sessionToken}")));

        var response = await _client.GetAsync($"product/{productId}?fields=stockOfGoods");

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new Exception($"Tripletex GET /product/{productId} feilet: {response.StatusCode}\n{error}");
        }

        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);

        var stockElement = doc.RootElement.GetProperty("value").GetProperty("stockOfGoods");
        return stockElement.GetDouble();
    }
}