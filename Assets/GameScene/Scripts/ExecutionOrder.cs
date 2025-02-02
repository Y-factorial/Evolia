namespace Evolia.GameScene
{

    public class ExecutionOrder
    {
        public const int CONTROLLER = 0;

        public const int AFTER_CONTROLLER = CONTROLLER+1;

        public const int SIMULATOR = CONTROLLER+1;

        public const int AFTER_SIMULATOR = SIMULATOR + 1;

        public const int WORLDMAP = SIMULATOR+1;

        public const int AFTER_WORLDMAP = WORLDMAP+1;
    }
}