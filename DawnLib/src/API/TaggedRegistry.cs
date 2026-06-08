using System;
using System.Collections.Generic;

namespace Dawn;

public class TaggedRegistry<T> : Registry<T> where T : DawnBaseInfo<T>
{
    [Obsolete("Use AfterTaggingWithContext instead")]
    public event Action AfterTagging
    {
        add
        {
            AfterTaggingWithContext += _ =>
            {
                value();
            };
        }
        remove => DawnPlugin.Logger.LogError("Registry.AfterTagging -= is not supported.");
    }

    public event Action<NamespacedKeyResolver<T>> AfterTaggingWithContext
    {
        add
        {
            _afterTaggingWithContext += resolver =>
            {
                try
                {
                    value(resolver);
                }
                catch (Exception exception)
                {
                    DawnPlugin.Logger.LogError($"(AfterTaggingWithContext) An exception occured in firing an event for a registry:\n{exception}");
                }
            };
        }
        remove => DawnPlugin.Logger.LogError("Registry.AfterTaggingWithContext -= is not supported.");
    }

    private event Action<NamespacedKeyResolver<T>> _afterTaggingWithContext = delegate { };

    private List<IAutoTagger<T>> _autoTaggers = [new VanillaAutoTagger<T>(), new CustomAutoTagger<T>(), new AllAutoTagger<T>()];
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

        using (NamespacedKeyResolver<T> afterTaggingResolver = new(Values))
        {
            _afterTaggingWithContext(afterTaggingResolver);
        }
    }
}