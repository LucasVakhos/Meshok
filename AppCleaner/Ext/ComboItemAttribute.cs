#nullable disable
namespace AppCleaner
{
    public enum OperationTypes
    {
        ProcessContent,
        ProcessFiles,
        ProcessOther
    }
    public class ComboItemAttribute : Attribute
    {
        public string Name { get; set; } = string.Empty;
        public string Pattern { get; set; } = string.Empty;
        public string SearchLabel { get; set; } = "Cканировать папку:";
        public string PlaceLabel { get; set; } = "Папка для найденного:";
        public OperationTypes OperationTypes { get; set; } = OperationTypes.ProcessContent;  
    }
}
