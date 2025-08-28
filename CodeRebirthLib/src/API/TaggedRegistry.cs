using System;
using System.Collections.Generic;

namespace CodeRebirthLib;
public class TaggedRegistry<T> : Registry<T> where T : CRBaseInfo<T>
{
    public event Action AfterTagging = delegate { };
    
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
    }

    override internal void Freeze()
    {
        base.Freeze();
        foreach (T value in Values)
        {
            foreach (IAutoTagger<T> tagger in _autoTaggers)
            {
                if(!tagger.ShouldApply(value))
                    continue;
                
                value.Internal_AddTag(tagger.Tag);
            }
        }
        AfterTagging();
    }
}