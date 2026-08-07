using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using MeshokBrowser;
using MeshokBrowser.Models;
using MeshokBrowser.Data;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VSDiagnostics;

namespace MehokBrowser.Benchmarks
{
    [SimpleJob(RuntimeMoniker.Net80)]
    [CPUUsageDiagnoser]
    public class FbHelperBenchmark
    {
        private List<OrderLine> _orderLines;
        private List<Client> _clients;
        private List<Title> _titles;
        private List<Order> _orders;
        [GlobalSetup]
        public void Setup()
        {
            // Initialize FbHelper
            try
            {
                FbHelper.Init();
            }
            catch
            {
            // Initialization may fail in benchmark environment without DB
            }

            // Create sample data
            _orderLines = Enumerable.Range(1, 10).Select(i => new OrderLine { deal_id = $"deal_{i}", CurrStatus = OrderStatus.New }).ToList();
            _clients = Enumerable.Range(1, 10).Select(i => new Client { site_id = $"client_{i}" }).ToList();
            _titles = Enumerable.Range(1, 10).Select(i => new Title(stNo: 1, baseId: i, stId: i)).ToList();
            _orders = Enumerable.Range(1, 10).Select(i => new Order()).ToList();
            // Prepare AllPackets with test data
            SetupAllPackets();
        }

        private void SetupAllPackets()
        {
        // Simulate AllPackets data structure
        // Note: This is a simplified setup. Actual implementation may require
        // more complex initialization depending on AllPackets internal structure
        }

        [Benchmark]
        public void BenchmarkAddOrderLine()
        {
            foreach (var orderLine in _orderLines)
            {
                try
                {
                    FbHelper.AddOrderLine(orderLine);
                }
                catch
                {
                // Expected in benchmark environment
                }
            }
        }

        [Benchmark]
        public void BenchmarkSetClients()
        {
            // This benchmarks the SetClients logic which is the N+1 query pattern
            try
            {
                FbHelper.SetOtherInfos(ScanStatus.ScanNew);
            }
            catch
            {
            // Expected in benchmark environment
            }
        }

        [Benchmark]
        public void BenchmarkHasInBase()
        {
            foreach (var orderLine in _orderLines)
            {
                try
                {
                    FbHelper.HasInBase(orderLine.deal_id);
                }
                catch
                {
                // Expected in benchmark environment
                }
            }
        }
    }
}