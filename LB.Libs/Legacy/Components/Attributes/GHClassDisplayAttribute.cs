namespace LB.Libs;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public sealed class GHClassDisplayAttribute : Attribute
{
    public string Caption { get; set; }

    public string ToolTip { get; set; }
}
