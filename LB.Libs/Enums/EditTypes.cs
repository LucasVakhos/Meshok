using System.ComponentModel.DataAnnotations;
using static LB.Libs.EditTypesConst;

namespace LB.Libs;

/// <summary>Типы действий редактирования данных.</summary>
public enum EditTypes
{
    /// <summary>Добавить новую запись.</summary>
    [Map(EditCategory, EditPath, "add", Insert), Display(Name = "Добавить", Description = "Добавить новую запись")]
    Insert,

    /// <summary>Изменить запись.</summary>
    [Map(EditCategory, EditPath, "editname", Edit), Display(Name = "Изменить", Description = "Изменить запись")]
    Edit,

    /// <summary>Удалить запись.</summary>
    [Map(EditCategory, EditPath, "remove", Delete), Display(Name = "Удалить", Description = "Удалить запись")]
    Delete,

    /// <summary>Сохранить изменения.</summary>
    [Map(SaveCategory, EditPath, "apply", Save), Display(Name = "Сохранить", Description = "Сохранить изменения")]
    Save,

    /// <summary>Закрыть документ.</summary>
    [Map(SaveCategory, DocumentPath, "bofolder", Save), Display(Name = "Закрыть", Description = "Закрыть документ")]
    CloseDocument,

    /// <summary>Открыть документ.</summary>
    [Map(SaveCategory, DocumentPath, "bopermission", Save), Display(Name = "Открыть", Description = "Открыть документ")]
    OpenDocument,

    /// <summary>Отменить изменения.</summary>
    [Map(SaveCategory, EditPath, "cancel", Cancel), Display(Name = "Отменить", Description = "Отменить изменения")]
    Cancel,

    /// <summary>Обновить все записи.</summary>
    [Map(ViewCategory, EditPath, "convert", RefreshAll), Display(Name = "Обновить всё", Description = "Обновить все записи")]
    RefreshAll,

    /// <summary>Вывести для печати.</summary>
    [Map(ViewCategory, ViewPath, "exporttoxlsx", Preview), Display(Name = "Вывести для печати", Description = "Вывести для печати")]
    Preview,

    /// <summary>Дополнительное действие.</summary>
    [Map(AdditionCategory, AdditionPath, "initialstate", Additional), Display(Name = "Дополнительно", Description = "Создать Дополнительно")]
    Additional
}
