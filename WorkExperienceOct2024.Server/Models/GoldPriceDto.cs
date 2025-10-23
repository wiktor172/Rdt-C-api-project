namespace WorkExperienceOct2024.Server.Models;

public record GoldPriceDto(
    string Pair,
    decimal Price,
    decimal Bid,
    decimal Ask,
    string LastRefreshed,
    long TsUnixMs
);

