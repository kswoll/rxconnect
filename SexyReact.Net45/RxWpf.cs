namespace SexyReact
{
    public class RxWpf
    {
        static RxWpf()
        {
            RxApp.UiScheduler = WpfUiScheduler.Instance;
        } 

        public static void RegisterDependency()
        {
        }
    }
}