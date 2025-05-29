namespace CurrencyConverter.WebApi;

internal partial class Constants
{
    public partial class RedisKey
    {
        public const string LatestExchangeRatesKeyFormat = "latest_exchange_rates_{0}";
        public const string HistoryOf3MonthsFromNowExchangeRatesKeyFormat = "history_exchange_rates_{0}";
    }
}