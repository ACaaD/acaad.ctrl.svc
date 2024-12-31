using System.Collections;
using LanguageExt;
using LanguageExt.Common;
using Oma.WndwCtrl.CliOutputParser.Errors;

namespace Oma.WndwCtrl.CliOutputParser.Interfaces;

public class ParserResult : IEnumerable<object> 
{
    private readonly List<object> _items;
    
    public ParserResult()
    {
        _items = new List<object>();
    }

    public ParserResult(object item) : this()
    {
        _items.Add(item);
    }
    
    public ParserResult(IEnumerable<object> collection) : this()
    {
        _items.AddRange(collection);
    }

    public IEnumerator<object> GetEnumerator() =>  Get().GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() =>  Get().GetEnumerator();

    public IEnumerable<object> Get()
    {
        foreach (object item in _items)
        {
            yield return item;
        }
    }
}

public interface ICliOutputParser
{
    Either<Error, ParserResult> Parse(string transformation, string text);
    
    Either<Error, ParserResult> Parse(string transformation, IEnumerable<object> values);
}