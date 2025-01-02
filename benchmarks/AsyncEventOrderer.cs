using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Order;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;

namespace OoLunar.AsyncEvents.Benchmarks
{
    public sealed class AsyncEventOrderer : IOrderer
    {
        private readonly DefaultOrderer _defaultOrderer = new(SummaryOrderPolicy.FastestToSlowest);

        public bool SeparateLogicalGroups => _defaultOrderer.SeparateLogicalGroups;

        public IEnumerable<BenchmarkCase> GetExecutionOrder(ImmutableArray<BenchmarkCase> benchmarksCase, IEnumerable<BenchmarkLogicalGroupRule>? order = null) => _defaultOrderer.GetExecutionOrder(benchmarksCase, order);
        public string? GetHighlightGroupKey(BenchmarkCase benchmarkCase) => _defaultOrderer.GetHighlightGroupKey(benchmarkCase);
        public string? GetLogicalGroupKey(ImmutableArray<BenchmarkCase> allBenchmarksCases, BenchmarkCase benchmarkCase) => _defaultOrderer.GetLogicalGroupKey(allBenchmarksCases, benchmarkCase);
        public IEnumerable<IGrouping<string, BenchmarkCase>> GetLogicalGroupOrder(IEnumerable<IGrouping<string, BenchmarkCase>> logicalGroups, IEnumerable<BenchmarkLogicalGroupRule>? order = null) => _defaultOrderer.GetLogicalGroupOrder(logicalGroups, order);
        public IEnumerable<BenchmarkCase> GetSummaryOrder(ImmutableArray<BenchmarkCase> benchmarkCases, Summary summary) =>
            // Order by the mean, then by the async name column, then by the handler count column
            // If any of the columns aren't there, use the default orderer
            _defaultOrderer.GetSummaryOrder(benchmarkCases, summary)
                    .OrderBy(benchmarkCase => benchmarkCase.Parameters.Items.FirstOrDefault(item => item.Name == "asyncEventName")?.Value ?? string.Empty)
                    .ThenBy(benchmarkCase => benchmarkCase.Parameters.Items.FirstOrDefault(item => item.Name == "handlerCount")?.Value ?? string.Empty);
    }
}
