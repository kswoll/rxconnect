# sexy-react
A UI binding framework built on top of the `System.Reactive`.  

## Installation

To install, use Nuget:

> Install-Package SexyReact

To get automatic reactivity for your properties (via the `[Rx]` attribute), you also need to install Fody:

> Install-Package Fody

After that finishes, it will create a `FodyWeavers.xml` file in your project's root folder.  Edit it and add the line:

    <SexyReact />
    
To its body so that it looks like:

    <?xml version="1.0" encoding="utf-8" ?>
    <Weavers>
      <SexyReact />
    </Weavers>

## A simple view model
    
    public class MyViewModel : RxObject
    {
        [Rx]public string MyStringProperty { get; set; }
    }
    
## Obtaining an observable for a property

    var model = new MyViewModel();
    IObservable<string> observable = model.ObserveProperty(x => x.MyStringProperty);


## FAQ

Q: Why aren't my `[Rx]` properties working?

A: If you've applied the `[Rx]` attribute to your properties but they don't seem to be reactive (no change notifications), then you
have probably not installed the Fody plugin or added `<SexyReact />` to `FodyWeavers.xml`.

