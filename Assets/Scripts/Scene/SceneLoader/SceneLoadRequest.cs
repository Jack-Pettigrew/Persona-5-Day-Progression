namespace DD.Scene
{
    public class SceneLoadRequest
    {
        public readonly int sceneBuildIndex;
        public readonly SceneTransitionType sceneTransitionType;
        public readonly SceneLoadType loadType;
        public readonly SceneLoadHandle loadHandle;

        /// <summary>
        /// A new scene loading operation for queue storage.
        /// </summary>
        /// <param name="buildIndex">The scene build index of the scene to load.</param>
        /// <param name="sceneTransitionType">The transition type between scenes.</param>
        /// <param name="loadType">The type of load to perform.</param>
        /// <param name="loadHandle">The load handle for manual scene loader items (ignore if automatic load type).</param>
        public SceneLoadRequest(int buildIndex, SceneTransitionType sceneTransitionType, SceneLoadType loadType, SceneLoadHandle loadHandle = null)
        {
            this.sceneBuildIndex = buildIndex;
            this.sceneTransitionType = sceneTransitionType;
            this.loadType = loadType;
            this.loadHandle = loadHandle;
        }
    }
}