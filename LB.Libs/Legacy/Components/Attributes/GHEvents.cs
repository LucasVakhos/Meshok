using System.ComponentModel;
namespace LB.Libs;

[AttributeUsage(AttributeTargets.Event)]
public class GHEventsAttribute : CategoryAttribute
{
    public GHEventsAttribute() : base("GH Events")
    {
    }
}
