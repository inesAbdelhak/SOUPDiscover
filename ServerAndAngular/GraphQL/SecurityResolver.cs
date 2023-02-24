using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using GraphQL;
using GraphQL.Client.Abstractions;
using GraphQL.Client.Http;
using GraphQL.Client.Serializer.Newtonsoft;
using Newtonsoft.Json;
using SoupDiscover.ORM;
using GraphQL.Types; 

namespace SoupDiscover.GraphQL
{

    /*
    public class SecurityResolver 
    {
        private readonly HttpClient _httpClient;
        

        public SecurityResolver(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<object> SecurityVulnerabilities(ResolveFieldContext<object> context)
        {
            var package = context.GetArgument<string>("package");
            var query = @"
            query ($first: Int!, $orderBy: SecurityAdvisoryOrder, $severities: [SecurityAdvisorySeverity!]!, $package: String!) {
                securityAdvisories(first: $first, orderBy: $orderBy, severities: $severities, package: $package) {
                    nodes {
                        vulnerableVersionRange
                        package {
                            name
                        }
                        advisory {
                            identifiers {
                                type
                                value
                            }
                            permalink
                            severity
                            cvss {
                                score
                            }
                            publishedAt
                            updatedAt
                        }
                    }
                }
            }
        ";

            var variables = new
            {
                first = 100,
                orderBy = new
                {
                    field = "UPDATED_AT",
                    direction = "ASC"
                },
                severities = new string[] { "HIGH", "CRITICAL" },
                package = package
            };

            var request = new HttpRequestMessage(HttpMethod.Post, "https://api.github.com/graphql");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", "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");

            var json = JsonConvert.SerializeObject(new { query, variables });
            request.Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                throw new ExecutionError($"Failed to retrieve security vulnerabilities for package '{package}'.");
            }

            var responseJson = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<dynamic>(responseJson);

            var vulnerabilities = result.data.SecurityAdvisories.Nodes;
            
            var orderedVulnerabilities = vulnerabilities.OrderBy(v => v.advisory.updatedAt);

            var filteredVulnerabilities = orderedVulnerabilities.Where(v => v.advisory.severity == "HIGH" || v.advisory.severity == "CRITICAL");

            var output = filteredVulnerabilities.Select(v => new
            {
                vulnerableVersionRange = (string)v.vulnerableVersionRange,
                package = new { name = (string)v.package.name },
                advisory = new
                {
                    identifiers = ((IEnumerable<dynamic>)v.advisory.identifiers).Select(i => new
                    {
                        type = (string)i.type,
                        value = (string)i.value
                    }),
                    permalink = (string)v.advisory.permalink,
                    severity = (string)v.advisory.severity,
                    cvssScore = (float?)v.advisory.cvss.score,
                    publishedAt = (DateTime?)v.advisory.publishedAt,
                    updatedAt = (DateTime?)v.advisory.updatedAt
                }
            });

            return output;
        }
    }



        */






























