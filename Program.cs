using Microsoft.Playwright;
#pragma warning disable S1075
Dictionary<string, string> usernameXpaths = new()
{
    {"@nearotlivuyescort", "/html/body/div[2]/main/section/div[3]/table/tbody/tr[115]/td[2]/a/div/button"},
    {"@thischannelprotected", "/html/body/div[2]/main/section/div[3]/table/tbody/tr[163]/td[2]/a/div/button"},
    {"@danielamramnewchannel", "/html/body/div[2]/main/section/div[3]/table/tbody/tr[38]/td[2]/a/div/button"}
};

string[] usernames =
{
    "@nearotlivuyescort", "@thischannelprotected", "@danielamramnewchannel"
};

const string channel = "מוחמד";
const string owner = "עע";

var exitCode = Microsoft.Playwright.Program.Main(new[] { "install" });
if (exitCode != 0)
{
    throw new InvalidOperationException($"Playwright exited with code {exitCode}");
}

using var playwright = await Playwright.CreateAsync();
var dataDir = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments));
BrowserTypeLaunchPersistentContextOptions options = new BrowserTypeLaunchPersistentContextOptions()
{
    Headless = false,
    UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/114.0.0.0 Safari/537.36",
    StrictSelectors = false,
    
};

var browser = await playwright.Chromium.LaunchPersistentContextAsync(dataDir, options: options);
var page = await browser.NewPageAsync();
await page.GotoAsync("https://webogram.org/a");
var fragmentPage = await browser.NewPageAsync();
await fragmentPage.GotoAsync("https://fragment.com/");
var originalColor = Console.ForegroundColor;
Console.ForegroundColor = ConsoleColor.Yellow;
Console.WriteLine("Login your Telegram in one page and also the fragment website in the other page");
Console.WriteLine("Press Enter when the login is complete");
ConsoleKey? key = null;
do
{
    key = Console.ReadKey().Key;
    
}
while(key != ConsoleKey.Enter);
Console.ForegroundColor = originalColor;

await StartAutomation();
Console.ReadLine();

async Task<bool> ActivateUsername(string name)
{
    try
    {
        await page.GotoAsync("https://webogram.org/a/#-1631284958");
        var prof = page.Locator("xpath=/html/body/div[2]/div/div[2]/div[4]/div[1]/div[1]/div/div/div/div[2]/div");
        await prof.ClickAsync();
        await Task.Delay(1000);
        var editButton = page.GetByTitle("Edit");
        await editButton.ClickAsync();
        await Task.Delay(1000);
        var publicButton = page.Locator("xpath=/html/body/div[2]/div/div[3]/div/div[2]/div/div/div[1]/div[4]/div/div");
        await publicButton.ClickAsync();
        await Task.Delay(1000);
        var username = page.GetByText(name);
        await username.ClickAsync();
        await Task.Delay(1000);
        try
        {
            var show = page.Locator("xpath=/html/body/div[1]/div[1]/div/div/div[2]/div[2]/div/button[1]");
            await show.ClickAsync();
        }
        catch
        {
            // probably the username is already activated
            return true; 
        }
    }
    catch
    {
        return false;
    }
    return true;
}


async Task AssignAllUsernames(string channel)
{
    await fragmentPage!.GotoAsync("https://fragment.com/my/assets");
    Console.WriteLine("Assigning usernames");
    foreach (var xpath in usernameXpaths)
    {
        try
        {
            var assign = "/html/body/div[2]/div[5]/div/div/section/form/div[2]/button";
            await fragmentPage!.Locator($"xpath={xpath.Value}").ClickAsync();
            await fragmentPage.GetByText(channel).ClickAsync();
            await fragmentPage.Locator($"xpath={assign}").ClickAsync();
            Console.WriteLine($"[{xpath.Key}] assigned");
            await fragmentPage.ReloadAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
        finally
        {
            Console.WriteLine();
        }
    }
}

async Task<bool> DeactivateUsername(string username, string owner)
{
    try
    {
        await fragmentPage!.GotoAsync("https://fragment.com/my/assets");
        var assign = "/html/body/div[2]/div[5]/div/div/section/form/div[2]/button";
        var xpath = usernameXpaths[username];
        if (xpath == null)
            return false;
        var locator = fragmentPage.Locator($"xpath={xpath}");
        await locator.ClickAsync();
        var newOwner = fragmentPage.GetByText(owner);
        var list = await newOwner.AllAsync();
        bool newOwnerClicked = false;
        foreach (var item in list)
        {
            var itemClass = await item.GetAttributeAsync("class");
            if (itemClass!.Contains("tm-assign-account-name"))
            {
                await item.ClickAsync();
                newOwnerClicked = true;
                break;
            }
        }
        if (!newOwnerClicked)
        {
            return false;
        }
        await fragmentPage.Locator($"xpath={assign}").ClickAsync();
        await fragmentPage.ReloadAsync();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Errror : {ex.Message}");
        return false;
    }
    return true;
}

async Task StartAutomation()
{
    Random.Shared.Shuffle(usernames);
    await AssignAllUsernames(channel);
    string currentUsername = "";
    List<string> alreadyActivated = new();

    string GetNewName()
    {
        while (true)
        {

            var newName = Random.Shared.GetItems(usernames!, 1)[0];
            if (newName != currentUsername && !alreadyActivated.Contains(newName))
                return newName;
        }
    }
    bool terminate = false;
    while (!terminate)
    {
        var limit = usernames.Length - alreadyActivated.Count;
        for(var i = 0; i < limit; ++i)
        {
            var name = GetNewName();
            var activated = await ActivateUsername(name);
            if (!activated)
            {
                terminate = true;
                break;
            }
            Console.WriteLine($"[activated] {name}");
            alreadyActivated.Add(name);
            if (!string.IsNullOrEmpty(currentUsername))
            {
                var deactivated = await DeactivateUsername(currentUsername, owner);
                if (deactivated)
                    Console.WriteLine($"[deactivated] {currentUsername}");
                else Console.WriteLine($"[error] unable to deactivate {currentUsername}");
            }
            currentUsername = name;
            Console.WriteLine("\t===== Waiting 2 minutes ======");
            await Task.Delay(120000);
        }
        alreadyActivated.Clear();
        alreadyActivated.Add(currentUsername);
        Random.Shared.Shuffle(usernames);
        await AssignAllUsernames(channel);
    }

}
#pragma warning restore S1075