using System.Linq;

namespace Dawn;

public sealed class MoonSceneWeightContextContributor : IWeightContextContributor
{
    public void Contribute(WeightContextBuilder builder)
    {
        DawnMoonInfo? moon = builder.Query.Moon;

        if (moon == null)
            return;

        IMoonSceneInfo sceneInfo = moon.Scenes.FirstOrDefault(x => x.SceneName == moon.Level.sceneName) ?? moon.Scenes.First();
        builder.Set(DawnWeightContextKeys.MoonScene, sceneInfo);
    }
}