// See https://aka.ms/new-console-template for more information

using System.Diagnostics;
using NasdaqTrader.Bot.Core;
using NasdaqTraderSystem.Core;
using NasdaqTraderSystem.Html;

var html = new HtmlGenerator();

var parameters = Environment.GetCommandLineArgs();

string dataFolder = "";
dataFolder = GetParameter("-d", $"Map met data voor koersen (default ../Data))",
    parameters);
if (string.IsNullOrWhiteSpace(dataFolder))
{
    dataFolder = "../Data";
}

int amountOfStock = 100;
string amountOfStocksAsText = "";
amountOfStocksAsText = GetParameter("-n", $"Aantal aandelen op beurs (default 100)",
    parameters);
if (!int.TryParse(amountOfStocksAsText, out amountOfStock))
{
    amountOfStock = 100;
}

bool runSilent = args.Any(b => b.Equals("-silent"));

int timeLimit = 10000;
string timeLimitAsText = "";
timeLimitAsText = GetParameter("-t", $"Tijdlimiet voor totale run (default 10000)",
    parameters);
if (!int.TryParse(timeLimitAsText, out timeLimit))
{
    timeLimit = 10000;
}

int startingCash = 100000;
string startingCashAsText = "";
startingCashAsText = GetParameter("-s", $"Start bedrag(default 100.000)",
    parameters);
if (!int.TryParse(startingCashAsText, out startingCash))
{
    startingCash = 100000;
}

BotLoader botLoader = new BotLoader();

var botTypes = new Dictionary<string, Type>();
botLoader.DetermineBots(AppContext.BaseDirectory + "Bots", botTypes);

var stocksLoader = new StockLoader(dataFolder, amountOfStock);

TraderSystemSimulation traderSystemSimulation = new TraderSystemSimulation(
    botTypes.Values.Select(b => (ITraderBot)Activator.CreateInstance(b)).ToList(),
    startingCash, new DateOnly(2021, 01, 01),
    new DateOnly(2024, 12, 31), 5,
    stocksLoader);

Dictionary<ITraderBot, Task> playerTasks = new();
foreach (var player in traderSystemSimulation.Players)
{
    playerTasks.Add(player,
        Task.Run(async () =>
        {
            while (await traderSystemSimulation.DoSimulationStep(player))
            {
            }
        }));
}

await Task.Delay(timeLimit);
foreach (var player in traderSystemSimulation.Players)
{
    if (playerTasks[player].IsCompleted == false)
    {
        traderSystemSimulation.DidNotFinished.Add(player);
    }
}

foreach (var player in traderSystemSimulation.Players)
{
    playerTasks[player].Wait();
}

Console.WriteLine("Results:");
foreach (var player in traderSystemSimulation.Players)
{
    Console.WriteLine(
        $"{player.CompanyName}    -   ${traderSystemSimulation.BankAccounts[player] + traderSystemSimulation.Holdings[player].GetCurrentValue(traderSystemSimulation.EndDate):0.00}");
}

Console.WriteLine("Generating html results");
html.GenerateFiles(AppContext.BaseDirectory + "Results\\", traderSystemSimulation);


if (!runSilent)
{
    var p = new Process();
    p.StartInfo = new ProcessStartInfo(AppContext.BaseDirectory + "Results\\index.html")
    {
        UseShellExecute = true
    };
    p.Start();
}

string GetParameter(string parameter, string question, string[] arguments)
{
    int indexOf = Array.IndexOf(arguments, parameter);
    if (indexOf == -1)
    {
        Console.WriteLine(question);
        return Console.ReadLine();
    }

    return arguments[indexOf + 1];

    return "";
}