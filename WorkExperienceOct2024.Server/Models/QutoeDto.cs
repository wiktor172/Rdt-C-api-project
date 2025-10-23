namespace WorkExperienceOct2024.Server.Models;

public record QuoteDto(
    string Symbol,
    decimal Price,
    decimal Open,
    decimal High,
    decimal Low,
    decimal PrevClose,
    decimal Change,
    decimal ChangePercent,
    long TsUnixMs
);

