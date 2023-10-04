using Dunet;

namespace SmartHomeWWW.Core.Utils.Functional;

[Union]
public partial record Option<T>
{
    public partial record Some(T Value);
    public partial record None();
}
