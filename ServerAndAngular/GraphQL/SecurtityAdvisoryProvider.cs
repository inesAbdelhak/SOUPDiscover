using GraphQL.Client.Abstractions;
using GraphQL.Client.Http;
using GraphQL.Client.Serializer.Newtonsoft;
using GraphQL;
using System.Net.Http.Headers;
using System.Net.Http;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SoupDiscover.ORM;
using Newtonsoft.Json;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using SoupDiscover.Controllers;
using SoupDiscover.Core;
using System.Linq;

namespace SoupDiscover.GraphQL
{
    public class SecurtityAdvisoryProvider
    {

        public static async Task<string> RequestSecurityAdvisoryDetails(IReadOnlyList<string> ghsaIds)
        {
            var client = new HttpClient();
            var options = new GraphQLHttpClientOptions() { EndPoint = new Uri("https://api.github.com/graphql") };
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "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");
            var graphQLClient = new GraphQLHttpClient(options, new NewtonsoftJsonSerializer(), client);

            var vulnerabilities = new List<VulnerabilitiesDetails>();

            foreach (var ghsaId in ghsaIds)
            {
                var partOne = @"
        query {
  securityAdvisory(ghsaId: ";
                var partTwo = $"\"{ghsaId}\"";
                var lastPart = @") {
        classification
        cvss {
            score
            vectorString
        }
        cwes(first: 1) {
            nodes {
                cweId
                description
                id
                name
            }
        }
        databaseId
        description
        ghsaId
    }
}";

                string query = partOne + partTwo + lastPart;
                var request = new GraphQLRequest
                {
                    Query = query,
                    Variables = new { ghsaId }
                };

                var response = await graphQLClient.SendQueryAsync<dynamic>(request);
                var resp = response as GraphQLHttpResponse<object>;
                if (resp != null && resp.Data != null)
                {
                    var vulDetails = JsonConvert.DeserializeObject<VulnerabilitiesDetails>(resp.Data.ToString());
                    if (vulDetails != null)
                    {
                        vulnerabilities.Add(vulDetails);
                    }
                }
            }

            var serializedVulnerabilities = JsonConvert.SerializeObject(vulnerabilities);
            return serializedVulnerabilities;
        }

        // Pour mapper les données en une liste 
        /* string responseJson = await RequestSecurityAdvisoryDetails(ghsaIds);
        var securityVulnerabilities = JsonConvert.DeserializeObject<VulnerabilitiesDetails>(responseJson)?.SecurityVulnerabilities;
        var vulnerabilitiesMetaData = securityVulnerabilities?.SecurityAdvisory?.Cwes?.Nodes?.Select(node =>
            new VulnerabilityMetaData(
                new Uri(securityVulnerabilities.SecurityAdvisory.AdvisoryUrl),
                securityVulnerabilities.SecurityAdvisory.Cvss?.Score ?? 0,
                securityVulnerabilities.SecurityAdvisory.GhsaId
            )
        ).ToList();
        */ 


        public partial class VulnerabilitiesDetails
        {
            [JsonProperty("securityVulnerabilities")]
            public SecurityVulnerabilities SecurityVulnerabilities { get; set; }
        }

        public partial class SecurityVulnerabilities
        {
            [JsonProperty("securityAdvisory")]
            public SecurityAdvisory SecurityAdvisory { get; set; }
        }

        public partial class SecurityAdvisory
        {
            [JsonProperty("classification")]
            public string Classification { get; set; }

            [JsonProperty("cvss")]
            public Cvss Cvss { get; set; }

            [JsonProperty("cwes")]
            public Cwes Cwes { get; set; }

            [JsonProperty("databaseId")]
            public long DatabaseId { get; set; }

            [JsonProperty("description")]
            public string Description { get; set; }

            [JsonProperty("ghsaId")]
            public string GhsaId { get; set; }
        }

        public partial class Cvss
        {
            [JsonProperty("score")]
            public double Score { get; set; }

            [JsonProperty("vectorString")]
            public string VectorString { get; set; }
        }

        public partial class Cwes
        {
            [JsonProperty("nodes")]
            public List<Node> Nodes { get; set; }
        }

        public partial class Node
        {
            [JsonProperty("cweId")]
            public string CweId { get; set; }

            [JsonProperty("description")]
            public string Description { get; set; }

            [JsonProperty("id")]
            public string Id { get; set; }

            [JsonProperty("name")]
            public string Name { get; set; }
        }
    }
}






















    }
}



