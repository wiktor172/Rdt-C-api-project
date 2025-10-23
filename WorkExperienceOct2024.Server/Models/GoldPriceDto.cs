namespace WorkExperienceOct2024.Server.Models;

// same story as QuoteDto but for the gold endpoint
// whatever we send from the api must match the struct in the wasm client one-for-one
public record GoldPriceDto(
    string Pair,
    decimal Price,
    decimal Bid,
    decimal Ask,
    string LastRefreshed,
    long TsUnixMs
);
