using System;
using System.Collections.Generic;

namespace Dawn;
public class TaggedRegistry<T> : Registry<T> where T : DawnBaseInfo<T>
{
    public event Action AfterTagging = delegate { };

    private List<IAutoTagger<T>> _autoTaggers = [new VanillaAutoTagger<T>(), new CustomAutoTagger<T>()];
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
                try
                {
                    if (!tagger.ShouldApply(value))
                        continue;
                    
                    value.Internal_AddTag(tagger.Tag);
                }
                catch (Exception exception)
                {
                    DawnPlugin.Logger.LogError($"Exception while applying tag: {tagger.Tag}\n{exception}");
                }
            }
        }
        AfterTagging();
    }
}