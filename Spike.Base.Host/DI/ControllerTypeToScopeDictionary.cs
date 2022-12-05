namespace App.Base.DI
{

    /// <summary>
    /// A Singleton
    /// Dictionary to permit the 
    /// <see cref="MyServiceBasedControllerActivator"/>
    /// find the right DI scope to build a 
    /// late loaded controller.
    /// </summary>
    public class ControllerTypeToScopeDictionary : Dictionary<Type, ControllerToScopeDictionaryEntry>
    {
        static public ControllerTypeToScopeDictionary Instance { get; private set; } = new ControllerTypeToScopeDictionary();
    }
}
