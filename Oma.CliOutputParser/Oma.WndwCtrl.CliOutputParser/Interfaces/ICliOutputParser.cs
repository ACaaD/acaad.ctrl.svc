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

    public IEnumerator<object> GetEnumerator() =>  ((IEnumerable<object>)Get().Select(i => i)).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => Get().Select(i => i).GetEnumerator();

    public IEnumerable<object> Get()
    {
        return _items.AsEnumerable();
    }
}

public interface ICliOutputParser
{
    Either<Error, ParserResult> Parse(string transformation, string text);
    
    Either<Error, ParserResult> Parse(string transformation, IEnumerable<object> values);
}