using LB.Libs;
using MeshokBrowser.Models;
using System.Collections;
using System.Threading;
using System.Threading.Tasks;

namespace MeshokBrowser.Data;

/// <summary>
/// Repository для работы с настройками сообщений, совместимый с LB.Libs IDataRepository
/// </summary>
public sealed class DapperMessageSettingsRepository : IMessageSettingsRepository
{
    // Legacy LB.Libs IDataRepository properties
    public Type ConcreteType => typeof(MessagesSet);
    public Func<SqlTypes, BaseEntity, string> GetSQL { get; set; }
    public Action<object> PostFinish { get; set; }
    public Action<BaseEntity> DeleteFinish { get; set; }
    public Action<object> CloseOpenDocFinish { get; set; }
    public Func<Dictionary<string, bool>> GetSorting { get; set; }
    public Func<Dictionary<string, object>> GetParams { get; set; }
    public Control Control { get; set; }
    public bool RefreshAfterPost { get; set; }
    public bool NeedLoadingAnimate { get; set; }

    // Legacy sync methods (preserved for backward compatibility)
    public KeyValuePair<int, string>[] KeyIntLookupList() => Array.Empty<KeyValuePair<int, string>>();
    public KeyValuePair<BaseEntity, string>[] KeyEntityLookupList() => Array.Empty<KeyValuePair<BaseEntity, string>>();

    public BaseEntity Get(object id) => DapperLookupRepository.LoadMessageSettings().FirstOrDefault(x => Equals(x.id, id));

    public void Save(object entity) => SaveOrUpdate(entity);

    public void SaveOrUpdate(object entity)
    {
        DapperLookupRepository.SaveMessageSetting((MessagesSet)entity);
        PostFinish?.Invoke(entity);
    }

    public void Refresh(object entity) { }

    public void Delete(object entity)
    {
        var item = (MessagesSet)entity;
        DapperLookupRepository.DeleteMessageSetting(item.id);
        DeleteFinish?.Invoke(item);
    }

    public void CloseOpenDoc(object entity) => CloseOpenDocFinish?.Invoke(entity);

    public IList SelectAll() => DapperLookupRepository.LoadMessageSettings();

    public BaseEntity SelectOne() => DapperLookupRepository.LoadMessageSettings().FirstOrDefault();

    public BaseEntity SelectFormProcedure(BaseEntity entity, string sql) => entity;

    public void ExequteQuery(string[] sql) => throw new NotSupportedException("Raw SQL execution is not supported by the Dapper message repository.");

    #region Modern Async API

    /// <summary>
    /// Асинхронная загрузка настройки сообщения по ID
    /// </summary>
    public async Task<MessagesSet?> GetAsync(int id, CancellationToken cancellationToken = default)
    {
        var items = await DapperLookupRepository.LoadMessageSettingsAsync(cancellationToken);
        return items.FirstOrDefault(x => x.id == id);
    }

    /// <summary>
    /// Асинхронная загрузка всех настроек сообщений
    /// </summary>
    public async Task<List<MessagesSet>> SelectAllAsync(CancellationToken cancellationToken = default)
    {
        return await DapperLookupRepository.LoadMessageSettingsAsync(cancellationToken);
    }

    /// <summary>
    /// Асинхронное сохранение настройки сообщения
    /// </summary>
    public async Task SaveAsync(MessagesSet entity, CancellationToken cancellationToken = default)
    {
        await DapperLookupRepository.SaveMessageSettingAsync(entity, cancellationToken);
        PostFinish?.Invoke(entity);
    }

    /// <summary>
    /// Асинхронное удаление настройки сообщения
    /// </summary>
    public async Task DeleteAsync(MessagesSet entity, CancellationToken cancellationToken = default)
    {
        await DapperLookupRepository.DeleteMessageSettingAsync(entity.id, cancellationToken);
        DeleteFinish?.Invoke(entity);
    }

    /// <summary>
    /// Асинхронное удаление настройки сообщения по ID
    /// </summary>
    public async Task DeleteByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        await DapperLookupRepository.DeleteMessageSettingAsync(id, cancellationToken);
    }

    #endregion
}
