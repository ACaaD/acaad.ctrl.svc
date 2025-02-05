using System.Text.Json.Serialization;
using JetBrains.Annotations;
using Oma.WndwCtrl.Abstractions;
using Oma.WndwCtrl.Core.Interfaces;
using ValueType = Oma.WndwCtrl.Abstractions.Model.ValueType;

namespace Oma.WndwCtrl.Core.Model;

/// <summary>
/// A read+write control indicating on/off.
/// Can define a GET endpoint to query the _current_ state (ad-hoc execution)
/// Additionally, a POST:/on and POST:/off endpoint is hosted (potentially flip as well)
/// </summary>
public class Switch : Component, IStateQueryable
{
  public delegate (bool Is, object? instance) TryParse(object obj);

  [PublicAPI]
  public static readonly IReadOnlyDictionary<ValueType, TryParse> ValueTypeValidators =
    new Dictionary<ValueType, TryParse>
    {
      [ValueType.Boolean] = TryParseBool,
      [ValueType.Long] = TryParseLong,
      [ValueType.Decimal] = TryParseDecimal,
      [ValueType.String] = TryParseString,
    };

  private object _onIff = true;

  [JsonIgnore]
  public override string Type => "switch";

  [JsonInclude]
  [JsonRequired]
  public ICommand OnCommand { get; internal set; } = null!;

  [JsonInclude]
  [JsonRequired]
  public ICommand OffCommand { get; internal set; } = null!;

  [JsonIgnore]
  public override IEnumerable<ICommand> Commands => [QueryCommand,];

  [JsonInclude]
  public object OnIff
  {
    get => _onIff;
    set
    {
      foreach ((ValueType valueType, TryParse validator) in ValueTypeValidators)
      {
        (bool typeMatch, object? instance) = validator(value);

        if (!typeMatch)
        {
          continue;
        }

        InferredComparisonType = valueType;
        _onIff = instance!;
        break;
      }

      if (InferredComparisonType is null)
      {
        throw new InvalidOperationException(
          $"Unsupported type for OnIff: {value}. Allowed value types are [{string.Join(", ", ValueTypeValidators.Keys.Select(vt => vt.ToString()))}]"
        );
      }
    }
  }

  [JsonIgnore]
  public ValueType? InferredComparisonType { get; private set; }

  [JsonInclude]
  [JsonRequired]
  public ICommand QueryCommand { get; internal set; } = null!;

  private static (bool, object?) TryParseBool(object obj)
  {
    if (obj is bool res)
    {
      return (true, res);
    }

    if (bool.TryParse(obj.ToString(), out bool resParsed))
    {
      return (true, resParsed);
    }

    return (false, null);
  }

  private static (bool, object?) TryParseString(object obj)
  {
    if (obj is string res)
    {
      return (true, res);
    }

    string? asString = obj.ToString();

    return !string.IsNullOrEmpty(asString)
      ? (true, asString)
      : (false, null);
  }

  private static (bool, object?) TryParseLong(object obj)
  {
    if (obj is long res)
    {
      return (true, res);
    }

    if (long.TryParse(obj.ToString(), out long resParsed))
    {
      return (true, resParsed);
    }

    return (false, null);
  }

  private static (bool, object?) TryParseDecimal(object obj)
  {
    if (obj is decimal res)
    {
      return (true, res);
    }

    if (decimal.TryParse(obj.ToString(), out decimal resParsed))
    {
      return (true, resParsed);
    }

    return (false, null);
  }
}