using System.Collections.Generic;
using FubuMVC.Core;
using FubuMVC.Core.Diagnostics.Instrumentation;
using FubuMVC.Core.Diagnostics.Runtime;
using HtmlTags;
using StoryTeller.Results;

namespace Serenity
{
    public class RequestReporter : Report
    {
        private readonly FubuRuntime _runtime;
        private readonly List<ChainExecutionLog> _logs = new List<ChainExecutionLog>();

        public RequestReporter(FubuRuntime runtime)
        {
            _runtime = runtime;
        }

        public string ShortTitle
        {
            get { return "FubuMVC"; }
        }

        public int Count
        {
            get { return _logs.Count; }
        }


        public string ToHtml()
        {
            var table = new TableTag();
            table.AddClass("table");
            table.AddClass("table-striped");
            table.AddHeaderRow(row =>
            {
                row.Header("Details");
                row.Header("Duration (ms)");
                row.Header("Method");
                row.Header("Endpoint");
                row.Header("Status");
                row.Header("Content Type");
            });

            _logs.Each(log =>
            {
                var url = _runtime.BaseAddress.TrimEnd('/') + "/_fubu/#/fubumvc/request-details/" + log.Id;

                table.AddBodyRow(row =>
                {
                    row.Cell().Add("a").Text("Details").Attr("href", url).Attr("target", "_blank");
                    row.Cell(log.ExecutionTime.ToString()).Attr("align", "right");
                    
                    /*
                    row.Cell(log.HttpMethod);
                    row.Cell(log.Endpoint);
                    row.Cell(log.StatusCode.ToString());
                    row.Cell(log.ContentType);
                     */
                });
            });


            return table.ToString();
        }

        public string Title
        {
            get { return "FubuMVC Requests During the Specification Execution"; }
        }

        public void Append(ChainExecutionLog[] requests)
        {
            _logs.AddRange(requests);
        }
    }
}