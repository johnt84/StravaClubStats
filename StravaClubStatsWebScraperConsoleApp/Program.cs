using Microsoft.Playwright;

using var playwright = await Playwright.CreateAsync();

await using var browser = await playwright.Chromium.LaunchAsync(new()
{
    Headless = true,

});

var page = await browser.NewPageAsync();

await page.GotoAsync("https://www.strava.com/athletes/32760466");

var html = await page.ContentAsync();

Console.WriteLine("Press any key to continue...");
Console.ReadKey();