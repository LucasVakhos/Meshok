using Common;
using Dapper;
using FirebirdSql.Data.FirebirdClient;
using GH.Components;
using System.Collections.Generic;
using System.Linq;
using MeshokBrowser.Models;
namespace MeshokBrowser.Data
{
    public static class DapperLookupRepository
    {
        public static KeyValuePair<int, string>[] BaseStatuses() => Read("select cs_id as Id, cs_name as Name from z$base_statuses order by cs_id");
        public static KeyValuePair<int, string>[] SiteStatuses() => Read("select zs_id as Id, zs_status_name as Name from z$statuses order by zs_id");
        public static KeyValuePair<int, string>[] DeliveryModes() => Read("select md_id as Id, md_name as Name from mode_delivery order by md_id");
        public static List<User> LoadActiveUsers()
        {
            const string sql = @"select mn_id as id, mn_name as Name, mn_password as Password, mn_active as Active
                from managers where mn_active = @active order by mn_name";
            using (var connection = new FbConnection(IniHelper.Cfg<CfgIShop>().ConnectionString()))
                return connection.Query<User>(sql, new { active = true }).ToList();
        }
        public static List<MessagesSet> LoadMessageSettings()
        {
            const string sql = @"select zsc_id as id, zsc_cs_id, zsc_zs_id, zsc_md_id, zsc_case, zsc_message
                from z$statuses_cod order by zsc_id";
            using (var connection = new FbConnection(IniHelper.Cfg<CfgIShop>().ConnectionString()))
                return connection.Query<MessagesSet>(sql).ToList();
        }
        public static void SaveMessageSetting(MessagesSet item)
        {
            using (var connection = new FbConnection(IniHelper.Cfg<CfgIShop>().ConnectionString()))
            {
                if (item.id <= 0)
                {
                    const string insert = @"insert into z$statuses_cod
                        (zsc_cs_id, zsc_zs_id, zsc_md_id, zsc_case, zsc_message)
                        values (@zsc_cs_id, @zsc_zs_id, @zsc_md_id, @zsc_case, @zsc_message)
                        returning zsc_id";
                    item.id = connection.QueryFirst<int>(insert, item);
                }
                else
                {
                    const string update = @"update z$statuses_cod set zsc_cs_id=@zsc_cs_id, zsc_zs_id=@zsc_zs_id,
                        zsc_md_id=@zsc_md_id, zsc_case=@zsc_case, zsc_message=@zsc_message where zsc_id=@id";
                    connection.Execute(update, item);
                }
            }
        }
        public static void DeleteMessageSetting(int id)
        {
            using (var connection = new FbConnection(IniHelper.Cfg<CfgIShop>().ConnectionString()))
                connection.Execute("delete from z$statuses_cod where zsc_id=@id", new { id });
        }
        public static int ImportClient(int siteId, Client client)
        {
            const string sql = @"select c_id from z$import_clients(@siteId, @site_id, @md_id, @mp_id, @c_name,
                @c_phone, @c_email, @c_zipcode, @site_address, @change_address)";
            var parameters = new DynamicParameters(client);
            parameters.Add("siteId", siteId);
            using (var connection = new FbConnection(IniHelper.Cfg<CfgIShop>().ConnectionString()))
                return connection.QueryFirst<int>(sql, parameters);
        }
        public static TitleCallBack ImportTitle(Title title)
        {
            const string sql = @"select t_id as id, ts_id from z$import_t(@t_base_id, @t_bar_code, @t_artist,
                @t_title, @t_year, @l_name, @m_name, @ctr_name, @st_name, @ts_st_no, @ts_st_id, @ts_quality, @t_price)";
            using (var connection = new FbConnection(IniHelper.Cfg<CfgIShop>().ConnectionString()))
                return connection.QueryFirst<TitleCallBack>(sql, title);
        }
        public static int ImportOrder(Order order)
        {
            const string sql = "select co_id from z$import_co(@c_id, @co_date, @md_id, @mp_id, @co_curs)";
            using (var connection = new FbConnection(IniHelper.Cfg<CfgIShop>().ConnectionString()))
                return connection.QueryFirst<int>(sql, order);
        }
        public static void ImportOrderLine(int siteId, OrderLine line)
        {
            const string sql = @"select cod_id from z$import_cod(@siteId, @dealId, @co_id, @t_id, @ts_id,
                @qty, @price, @commition, @price_ru)";
            var parameters = new DynamicParameters(line);
            parameters.Add("siteId", siteId);
            parameters.Add("dealId", line.deal_id);
            using (var connection = new FbConnection(IniHelper.Cfg<CfgIShop>().ConnectionString()))
                connection.QueryFirst<int>(sql, parameters);
        }
        public static CheckClient FindClient(int siteId, int clientSiteId)
        {
            const string sql = @"select c_id as id, c_name as Name, c_md_id, c_mp_id, c_email, c_enabled,
                c_phone, c_zipcode, c_address from z$import_clients_inf(@siteId, @clientSiteId)";
            using (var connection = new FbConnection(IniHelper.Cfg<CfgIShop>().ConnectionString()))
                return connection.QueryFirstOrDefault<CheckClient>(sql, new { siteId, clientSiteId });
        }
        public static List<CheckMesage> FindMessages(int siteId, string dealId, int messageCase)
        {
            const string sql = @"select cod_id as id, c_id, c_name, c_email, md_id, md_name, mp_id, mp_name,
                cs_id, cs_name, dp_totalsumm, dp_totalsumm_info, dp_packed, md_address, md_treck_num,
                md_tracking_url, zsc_case, zsc_message as mess_text
                from z$check_message(@siteId, @dealId, @messageCase)";
            using (var connection = new FbConnection(IniHelper.Cfg<CfgIShop>().ConnectionString()))
                return connection.Query<CheckMesage>(sql, new { siteId, dealId, messageCase }).ToList();
        }
        public static CheckOrder FindOrder(int siteId, string dealId)
        {
            const string sql = @"select cod_id as id, co_id, co_c_id, co_creation_date, co_md_id, co_mp_id, co_status,
                dp_id, dp_c_id, dp_md_id, dp_mp_id, dp_totalsumm, dp_status, dp_packed, dp_creation_date
                from z$check_order(@siteId, @dealId)";
            using (var connection = new FbConnection(IniHelper.Cfg<CfgIShop>().ConnectionString()))
                return connection.QueryFirstOrDefault<CheckOrder>(sql, new { siteId, dealId });
        }
        public static bool HasOrder(int siteId, string dealId)
        {
            const string sql = "select z$s_id from z$site_id_s(@siteId, @dealId)";
            using (var connection = new FbConnection(IniHelper.Cfg<CfgIShop>().ConnectionString()))
                return connection.QueryFirstOrDefault<int?>(sql, new { siteId, dealId }).GetValueOrDefault() > 0;
        }
        public static List<BaseStatus> BaseStatusEntities()
        {
            using (var connection = new FbConnection(IniHelper.Cfg<CfgIShop>().ConnectionString()))
                return connection.Query<BaseStatus>("select cs_id as id, cs_name as Name from z$base_statuses order by cs_id").ToList();
        }
        public static void ImportStatuses(int siteId, IDictionary<int, string> statuses)
        {
            using (var connection = new FbConnection(IniHelper.Cfg<CfgIShop>().ConnectionString()))
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    foreach (var status in statuses)
                        connection.Execute("execute procedure z$import_statuses(@siteId, @statusId, @statusName)",
                            new { siteId, statusId = status.Key, statusName = status.Value }, transaction);
                    transaction.Commit();
                }
            }
        }        public static int? FindSiteId(string siteName)
        {
            using (var connection = new FbConnection(IniHelper.Cfg<CfgIShop>().ConnectionString()))
                return connection.QuerySingleOrDefault<int?>(
                    "select z$s_id from z$site_id where z$s_name = @siteName", new { siteName });
        }
        private static KeyValuePair<int, string>[] Read(string sql)
        {
            using (var connection = new FbConnection(IniHelper.Cfg<CfgIShop>().ConnectionString()))
                return connection.Query<LookupRow>(sql).Select(x => new KeyValuePair<int, string>(x.Id, x.Name)).ToArray();
        }
        private sealed class LookupRow { public int Id { get; set; } public string Name { get; set; } }
    }
}