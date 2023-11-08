using Azure;
using Azure.Data.Tables;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Services 
{
    public interface ICheapoRepo
    {
        Task saveItAsync(string word, string definition);
        Task<Wordout[]> getAllWords();        

    }

    // Per the name this will be a very basic repo. Usally I like to use a generic form on this. But time constraints and all -JSW
    public class CheapoRepo : ICheapoRepo
    {
        //private readonly string conn = @"DefaultEndpointsProtocol=http;AccountName=devstoreaccount1;AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==;BlobEndpoint=http://127.0.0.1:10000/devstoreaccount1;QueueEndpoint=http://127.0.0.1:10001/devstoreaccount1;TableEndpoint=http://127.0.0.1:10002/devstoreaccount1;";
        //private readonly Uri storageUri = new Uri(@"http://127.0.0.1:10002/devstoreaccount1");
        //private readonly string ActName = "devstoreaccount1";
        //private readonly string ActKey = "Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==";

        //If taken to a larger level datawise will have to design a better partition key - JSW
        private readonly string PartitionKey = "f60710cc-084b-4fd0-9291-d8f37d682121";


        //The following will be configurable eventually - JSW
        private readonly string TableName = "Words";
        private readonly TableServiceClient _tableSrvCli;
        private readonly TableClient _tableCli;
        private readonly ILogger _logger;


        public CheapoRepo(ILogger logger, TableServiceClient tableServiceClient)
        {

            _logger = logger; // could do a null ref exception here.
            // TODO : will inject the client
            _tableSrvCli = tableServiceClient;


            _tableCli = _tableSrvCli.GetTableClient(TableName);


        }

        public async Task saveItAsync(string word, string definition) 
        {
            try
            {
                var table = await _tableSrvCli.CreateTableIfNotExistsAsync(TableName);


                var ent = new TableEntity(PartitionKey, Guid.NewGuid().ToString())// PartionKey/RowKey here needs some work for if large data.
                {
                    { "word", word },
                    { "definition", definition}
                };

                var entAddRes = await _tableCli.AddEntityAsync(ent);
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                throw;
            }
        }

        public async Task<Wordout[]> getAllWords()
        {
            List<Wordout> retVal = new List<Wordout>();
            try
            {
                AsyncPageable<TableEntity> queryResultsMaxPerPage = _tableCli.QueryAsync<TableEntity>(filter: $"PartitionKey eq '{PartitionKey}'", maxPerPage: 10) ;

                await foreach (Page<TableEntity> page in queryResultsMaxPerPage.AsPages())
                {
                    foreach (TableEntity qEntity in page.Values)
                    {
                        retVal.Add(
                            new Wordout
                            {
                                word = qEntity.GetString("word"),
                                def = qEntity.GetString("definition")
                            });                           
                    }
                }

            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                throw;
            }

            return retVal.ToArray();
        } 

    }
    public class Wordout //probably should be in DTO's
    {
        public string word { get; set; }

        public string def { get; set; }
    }
    
}



//Here is a better generic repo. TODO implement

//public class TableClientRepository<T> : IRepository<T> where T : class, ITableEntity
//{
//    readonly TableClient _tableClient;

//    public TableClientRepository(TableServiceClient tableServiceClient)
//    {
//        _tableClient = tableServiceClient.GetTableClient(typeof(T).Name);
//    }

//    public async Task AddAsync(T item)
//    {
//        var entity = item.ValidateTableStorageEntity();

//        await _tableClient.AddEntityAsync(entity);
//    }

//    public async Task UpdateAsync(T item)
//    {
//        var entity = item.ValidateTableStorageEntity();

//        await _tableClient.UpdateEntityAsync(entity, Azure.ETag.All, TableUpdateMode.Merge);
//    }

//    public async Task Remove(T item)
//    {
//        await _tableClient.DeleteEntityAsync(item.PartitionKey, item.RowKey);
//    }
//}
