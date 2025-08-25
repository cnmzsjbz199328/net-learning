using System.Net.Http;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Frontend.Web.Pages;

public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;

    public IndexModel(ILogger<IndexModel> logger)
    {
        _logger = logger;
    }

    public List<Pizza>? Pizzas { get; set; }

    public async Task OnGetAsync()
    {
        using var http = new HttpClient();
        var backendUrl = Environment.GetEnvironmentVariable("BackendUrl") ?? "http://localhost:5200";
        var json = await http.GetStringAsync($"{backendUrl}/pizzas");
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        Pizzas = JsonSerializer.Deserialize<List<Pizza>>(json, options);
    }

    public record Pizza(string Name, string[] Toppings, int Id);
}
