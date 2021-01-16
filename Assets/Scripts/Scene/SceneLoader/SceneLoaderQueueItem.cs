namespace DD.Scene
{
    public class SceneLoaderQueueItem
    {
        public readonly int sceneBuildIndex;
        public readonly SceneTransitionType sceneTransitionType;

        public SceneLoaderQueueItem(int buildIndex, SceneTransitionType sceneTransitionType)
        {
            this.sceneBuildIndex = buildIndex;
            this.sceneTransitionType = sceneTransitionType;
        }
    }
}