namespace CurrencyConverter.WebApi;

internal partial class Constants
{
    public partial class RedisKey
    {
        public const string LatestExchangeRatesKeyFormat = "latest_exchange_rates_{0}";
        public const string HistoryOf3MonthsFromNowExchangeRatesKeyFormat = "history_exchange_rates_{0}";

        /// <summary>
        /// {0} represent ID, and {1} represent path
        /// </summary>
        public const string RateLimitingKeyFormat = "rate-limiting_{0}_{1}";
    }
}