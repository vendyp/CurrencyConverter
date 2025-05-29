using System.ComponentModel;
using NetEscapades.EnumGenerators;

// ReSharper disable InconsistentNaming

namespace CurrencyConverter.Domain.Enums;

public partial class Enums
{
    [EnumExtensions]
    public enum Currency
    {
        [Description("Unsupported Currency")] Undefined = 0,

        [Description("Australian Dollar")] AUD = 1,

        [Description("Bulgarian Lev")] BGN = 2,

        [Description("Brazilian Real")] BRL = 3,

        [Description("Canadian Dollar")] CAD = 4,

        [Description("Swiss Franc")] CHF = 5,

        [Description("Chinese Renminbi Yuan")] CNY = 6,

        [Description("Czech Koruna")] CZK = 7,

        [Description("Danish Krone")] DKK = 8,

        [Description("Euro")] EUR = 9,

        [Description("British Pound")] GBP = 10,

        [Description("Hong Kong Dollar")] HKD = 11,

        [Description("Indonesian Rupiah")] IDR = 12,

        [Description("Israeli New Sheqel")] ILS = 13,

        [Description("Indian Rupee")] INR = 14,

        [Description("Icelandic Króna")] ISK = 15,

        [Description("Japanese Yen")] JPY = 16,

        [Description("South Korean Won")] KRW = 17,

        [Description("Mexican Peso")] MXN = 18,

        [Description("Malaysian Ringgit")] MYR = 19,

        [Description("Norwegian Krone")] NOK = 20,

        [Description("New Zealand Dollar")] NZD = 21,

        [Description("Philippine Peso")] PHP = 22,

        [Description("Polish Zloty")] PLN = 23,

        [Description("Romanian Leu")] RON = 24,

        [Description("Swedish Krona")] SEK = 25,

        [Description("Singapore Dollar")] SGD = 26,

        [Description("Thai Baht")] THB = 27,

        [Description("Turkish Lira")] TRY = 28,

        [Description("United States Dollar")] USD = 29,

        [Description("South African Rand")] ZAR = 30
    }
}