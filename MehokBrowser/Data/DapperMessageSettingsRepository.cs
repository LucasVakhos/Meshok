using GH.Components;
using MeshokBrowser.Models;
using System.Collections;

namespace MeshokBrowser.Data
{
    public sealed class DapperMessageSettingsRepository : IDataRepository
    {
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
    }
}