using Common;
using GH.Components;
using MeshokBrowser.Data;
using MeshokBrowser.Helpers;
using MeshokBrowser.Models;
using MySql.Data.MySqlClient;

namespace MeshokBrowser
{
    public static class FbHelper
    {
        private static int _siteNo = -1;

        public static void Init()
        {
            if (_siteNo > 0)
                return;
            SetSiteNo();
            AddSiteStatusesToBase();
            GetRelationsStatusesFromBase();
        }

        private static void SetSiteNo()
        {
            _siteNo = DapperLookupRepository.FindSiteId(LB.Libs.IniHelper.Cfg<CfgMeshok>().SiteName)
                ?? throw new InvalidOperationException("Site is not registered in the database.");
        }

        private static void GetRelationsStatusesFromBase() =>
            StatusRelation.SetStatusRels(DapperLookupRepository.BaseStatusEntities());

        private static void AddSiteStatusesToBase()
        {
            var statuses = Enum.GetValues<OrderStatus>()
                .ToDictionary(item => (int)item, item => GH.Components.EnumExtensions.GetDisplayValue(item));
            DapperLookupRepository.ImportStatuses(_siteNo, statuses);
        }

        public static void AddOrderLine(OrderLine orderLine)
        {
            var check = DapperLookupRepository.FindOrder(_siteNo, orderLine.deal_id)
                ?? CheckOrder.NewCheckOrder(orderLine);
            orderLine.Check = check;
            try
            {
                DeliveryPacket packet = AllPackets.GetPacket(orderLine, check);
                packet.AddOrderLine(orderLine, check);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
        }

        public static void SetOtherInfos(ScanStatus scanStatus)
        {
            switch (scanStatus)
            {
                case ScanStatus.ScanLostDeals:
                case ScanStatus.ScanNew:
                    SetClients();
                    break;
                case ScanStatus.ScanNotNew:
                case ScanStatus.ScanNewMess:
                    foreach (var orderLine in AllPackets.OrderLines)
                        SetDealMessage(orderLine);
                    break;
            }
        }

        private static void SetClients()
        {
            foreach (Client client in AllPackets.ForPostClients)
            {
                try
                {
                    client.base_info = DapperLookupRepository.FindClient(_siteNo, client.site_id);
                }
                catch (Exception ex)
                {
                    Logger.Error(ex);
                }
            }
        }

        public static void SetDealMessage(OrderLine orderLine)
        {
            MessageCase messageCase = orderLine.CurrStatus is OrderStatus.New or OrderStatus.Discution
                ? MessageCase.EmailCheck
                : orderLine.Check.dp_packed ? MessageCase.DocClosed : MessageCase.AnyCase;
            foreach (var message in DapperLookupRepository.FindMessages(_siteNo, orderLine.deal_id, (int)messageCase))
            {
                try
                {
                    message.SetDealMessagesFor(orderLine);
                }
                catch (Exception ex)
                {
                    Logger.Error(ex);
                }
            }
        }

        public static bool AddToDatabase(ScanHelper scanHelper)
        {
            try
            {
                foreach (Client client in AllPackets.ForPostClients)
                    client.base_id = DapperLookupRepository.ImportClient(_siteNo, client);
                foreach (Title title in AllPackets.ForPostTitles)
                {
                    SetTitleToRelist(title);
                    var result = DapperLookupRepository.ImportTitle(title);
                    title.t_id = result.id;
                    title.ts_id = result.ts_id;
                }
                foreach (Order order in AllPackets.ForPostOrders)
                {
                    order.RetriveFields();
                    order.base_id = DapperLookupRepository.ImportOrder(order);
                }
                foreach (OrderLine line in AllPackets.ForPostOrderLines)
                    DapperLookupRepository.ImportOrderLine(_siteNo, line);
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return false;
            }
        }

        public static void SetIdToRelist(int stNo, int baseId, int stId) =>
            SetTitleToRelist(new Title(stNo, baseId, stId));

        private static void SetTitleToRelist(Title title)
        {
            string sql = $"CALL meshok_relist_set({title.ts_st_no}, {title.t_base_id}, {title.ts_st_id})";
            try
            {
                using var connection = CfgBridgeNote.CreateConnection();
                connection.Open();
                using var transaction = connection.BeginTransaction();
                using var command = new MySqlCommand(sql, connection, transaction);
                command.ExecuteNonQuery();
                transaction.Commit();
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
        }

        public static bool HasInBase(string dealId)
        {
            try
            {
                return DapperLookupRepository.HasOrder(_siteNo, dealId);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return false;
            }
        }
    }
}
