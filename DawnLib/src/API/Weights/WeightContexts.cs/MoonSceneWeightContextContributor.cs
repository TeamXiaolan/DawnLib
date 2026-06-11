using System.Linq;

namespace Dawn;

public sealed class MoonSceneWeightContextContributor : IWeightContextContributor
{
    public void Contribute(WeightContextBuilder builder)
    {
        DawnMoonInfo? moon = builder.Query.Moon;

        if (moon == null)
            return;

        builder.Set(DawnWeightContextKeys.MoonScene, moon.Scenes.First(x => x.SceneName == moon.Level.sceneName));
    }
}