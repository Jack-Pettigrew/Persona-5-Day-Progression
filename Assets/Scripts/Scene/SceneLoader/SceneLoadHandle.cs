/// <summary>
/// A handle to the associated with the SceneLoadRequest. Use this to know when the scene has loaded and to tell the SceneLoader when to activate it.
/// </summary>
public class SceneLoadHandle
{
    public bool hasSceneLoaded = false;
    public bool canSceneActivate = false;
}
