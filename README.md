# Bij de zomercompetitie 2025 - NasdaqTrader
Het doel van het spel is om zo rijk mogelijk te worden. Je gaat dit doen door automatisch te handelen op de beurs. 

## Hoe werkt het spel
Je maakt een Trading bot, deze krijgt toegang tot de beurs. Maar er is iets bijzonders met deze beurs, alle koersen  ook die in de toekomst zijn al bekend. Er is dus een optimale koop/verkoopstrategie er komt geen geluk bij kijken. 
Je trader bot heeft een enkele methode **DoTurn** binnen deze functie kan je de koersen opvragen en aandelen kopen of verkopen. Deze functie heeft een parameter **ITraderSystemContext**:
```
public interface ITraderSystemContext  
{  
	DateOnly StartDate { get; set; }  
	DateOnly EndDate { get; set; }  
	DateOnly CurrentDate { get; set; }  
	int AmountOfTradesPerDay { get; }  
	
	decimal GetCurrentCash(ITraderBot traderBot);  
	decimal GetPriceOnDay(IStockListing listing);  
	ReadOnlyCollection<IStockListing> GetListings();  
	int GetTradesLeftForToday(ITraderBot traderBot);  
	bool BuyStock(ITraderBot traderBot,IStockListing listing, int amount);  
	bool SellStock(ITraderBot traderBot, IStockListing listing, int amount);  
	IHolding GetHolding(ITraderBot traderBot, IStockListing listing);  
	IHolding[] GetHoldings(ITraderBot traderBot);  
}
```
Hiermee heb je alle informatie en mogelijke acties om te kunnen winnen.

### Beurs regels
Per dag kan je maar 5 transacties doen(verkopen of kopen). 

Je mag per aandeel op de markt maar 1000 bezitten

Aan het einde van spel word de waarde bepaald door cash + (aandelen * waarde) te doen. Je hoeft dus niet alles aan het einde te verkopen.

Het spel loopt voor 2,5 seconden voor iedereen, als je niet binnen 2,5 seconden afgerond hebt stopt het spel gewoon. Je wordt wel meegenomen in de ranking, maar je houdt de aandelen die je had toen het spel stopte.
Het kan zijn dat dit tijdslimiet nog wordt opgerekt.

Het spel loopt per speler individueel er kan dus niet naar elkaar worden gekeken of op elkaar worden gereageerd.

### Bonus regeling
Als je een UI weet te maken die het spel kan afspelen aan het eind krijg je +2% bonus bovenop je eindresultaat per game.
Als je een UI weet te maken waarbij je zelf kan handelen krijg je +5% bonus bovenop je eindresultaat per game.
Voor de finale moet je met een Bot meedoen, je kan dan niet zelf handelen.

Je mag hiervoor de source aanpassen, maar spel moet nog wel normaal kunnen lopen.

## Hoe doe ik mee?
Maak een fork van dit project op github [How to fork](https://docs.github.com/en/pull-requests/collaborating-with-pull-requests/working-with-forks/fork-a-repo) 

Maak je bot door een kopie te pakken van de ExampleBot die staat in de [Bots map](https://github.com/CSHDJO/NasdaqTradeSystem/tree/master/Bots). 

Als je voor de komende run je bot wil updaten, maak dan een pull request [How to create pull request from fork](https://docs.github.com/en/pull-requests/collaborating-with-pull-requests/proposing-changes-to-your-work-with-pull-requests/creating-a-pull-request-from-a-fork) 
Zorg ervoor dat je project wel build!

Elke nacht loopt het spel opnieuw, de resultaten komen op: [Resultaten pagina](https://chipper-genie-b3874c.netlify.app/)


## Lokaal runnen
Voor het lokaal uitvoeren van je bot neem de volgende stappen:

- Build je project en zorg dat de output komt in (Build\Bots)[https://github.com/CSHDJO/NasdaqTradeSystem/tree/master/Build/Bots]
- Run via commandline of door dit in te stellen bij je run configuratie in visual studio (Build\NasdaqTrader.CLI.exe)[https://github.com/CSHDJO/NasdaqTradeSystem/blob/master/Build/NasdaqTrader.CLI.exe]
- Deze vraagt om instellingen voor het runnen, vul deze zoals je wilt of laat op default staan
- Nu runt het spel aan het eind toont deze op je lokale pc de resultaten pagina

ps. er zijn een aantal command arguments om te zorgen dat je niet altijd alles zelf hoeft in te stellen kijk in [Source\NasdaqTrader.CLI\Program.cse](https://github.com/CSHDJO/NasdaqTradeSystem/blob/master/Source/NasdaqTrader.CLI/Program.cs)  wat de mogelijkheden zijn.
