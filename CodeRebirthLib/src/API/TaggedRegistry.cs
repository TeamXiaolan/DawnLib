using System.Collections.Generic;

namespace CodeRebirthLib;
public class TaggedRegistry<T> : Registry<T> where T : CRBaseInfo<T>
{
    private List<IAutoTagger<T>> _autoTaggers = [];
    public void AddAutoTaggers(params IAutoTagger<T>[] taggers)
    {
        foreach (IAutoTagger<T> tagger in taggers)
        {
            AddAutoTagger(tagger);
        }
    }
    
    public void AddAutoTagger(IAutoTagger<T> tagger)
    {
        _autoTaggers.Add(tagger);
        foreach (T value in Values)
        {
            if(tagger.ShouldApply(value)) 
                value.Internal_AddTag(tagger.Tag);
        }
    }

    override internal void Register(T value)
    {
        base.Register(value);
        foreach (IAutoTagger<T> tagger in _autoTaggers)
        {
            if (tagger.ShouldApply(value))
            {
                value.Internal_AddTag(tagger.Tag);
            }
        }
    }
}