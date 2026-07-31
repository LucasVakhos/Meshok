namespace LB.Libs;

/// <summary>Атрибут для привязки значений EditTypes к категориям и путям иконок.</summary>
public class MapAttribute : Attribute
{
    private static readonly string small_ext = "_16x16.png";
    private static readonly string large_ext = "_32x32.png";

    /// <summary>Категория действия.</summary>
    public string Category { get; }

    /// <summary>Базовый путь к иконкам.</summary>
    protected string Path { get; }

    /// <summary>Имя ресурса иконки.</summary>
    protected string ResourceName { get; }

    /// <summary>Тип действия (EditTypes).</summary>
    public EditTypes Button { get; set; }

    /// <summary>Полный путь к маленькой иконке.</summary>
    public string SmallFullPath => Path + ResourceName + small_ext;

    /// <summary>Полный путь к большой иконке.</summary>
    public string LargeFullPath => Path + ResourceName + large_ext;

    /// <summary>Создаёт атрибут Map с указанием категории, пути и имени ресурса.</summary>
    public MapAttribute(string category, string path, string resourceName, EditTypes button)
    {
        Category = category;
        Path = path.Replace(" ", "%20");
        ResourceName = resourceName;
        Button = button;
    }
}
