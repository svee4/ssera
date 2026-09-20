namespace Ssera.Client.Pages;

public partial class Home
{
    private string _meow = "";

    private async Task Test()
    {
        Console.WriteLine("TESTING");
        var client = new HttpClient();
        _meow = await client.GetStringAsync(
            "https://localhost:7224/api/events?page=1&pageSize=50");
    }
}
