using SmartAc.Application.Extensions;
using System.Text.Json.Serialization;

namespace SmartAc.Application.Abstractions.Messaging;

public class IdempotentCommand<TResult> : IIdempotentCommand<TResult>
{
    [JsonIgnore]
    public string HashString => this.GetHashString();
}