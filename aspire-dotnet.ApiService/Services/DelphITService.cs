using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Microsoft.Extensions.Configuration;
using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace api.Services.DelphITService
{
    public class DelphITService : IDelphITService
    {
        private readonly IAmazonDynamoDB _ddbClient;
        private readonly string _delphiTableName;

        public DelphITService(IAmazonDynamoDB ddbClient, IConfiguration configuration)
        {
            _ddbClient = ddbClient;
            _delphiTableName = configuration["AWS:Resources:DelphITable"] ?? throw new ArgumentNullException(nameof(configuration), "DelphITable configuration value is missing or null.");
        }

        public async Task<DelphIT?> GetByIdAsync(int id)
        {
            var response = await _ddbClient.GetItemAsync(new GetItemRequest
            {
                TableName = _delphiTableName,
                Key = new Dictionary<string, AttributeValue>
                {
                    {"Id", new AttributeValue{N = id.ToString()}}
                }
            });

            if (response.Item == null || !response.Item.Any())
            {
                return null; 
            }

            return ConvertItemToDelphIT(response.Item);
        }

        public async Task<IEnumerable<DelphIT>> GetAllAsync()
        {
            var scanRequest = new ScanRequest
            {
                TableName = _delphiTableName
            };

            var response = await _ddbClient.ScanAsync(scanRequest);

            return response.Items.Select(ConvertItemToDelphIT).ToList();
        }

        private DelphIT ConvertItemToDelphIT(Dictionary<string, AttributeValue> item)
        {
            return new DelphIT
            {
                Id = int.Parse(item["Id"].N),
                Name = item.ContainsKey("Name") ? item["Name"].S : null,
                Bio = item.ContainsKey("Bio") ? item["Bio"].S : null
            };
        }
    }
}
