using SmartAc.Application.Extensions;
using System.Text.Json.Serialization;

namespace SmartAc.Application.Abstractions.Messaging;

public class CommandBase<TResult> : IIdempotentCommand<TResult>
{
    [JsonIgnore]
    public string HashCode => this.GetHashString();
}