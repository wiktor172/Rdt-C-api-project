namespace WorkExperienceOct2024.Server.Models;

// this tiny record is what the server hands to the client when it asks for stock data
// blazor serializes this shape into json, and the wasm front-end has a matching record
// keeping it dumb like this makes it super obvious what the API contract looks like
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
