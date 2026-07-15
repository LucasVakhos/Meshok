using System.ComponentModel;
namespace LB.Libs
{
    [AttributeUsage(AttributeTargets.Property)]
    public class GHPropertyAttribute : CategoryAttribute
    {
        public GHPropertyAttribute() : base("GH Propertys")
        {
        }
    }
}
