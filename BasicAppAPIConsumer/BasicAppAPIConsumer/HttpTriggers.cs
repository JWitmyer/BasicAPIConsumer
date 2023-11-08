using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Services;
using Microsoft.Extensions.Configuration;
using System.Linq;
using Microsoft.Extensions.Logging.Abstractions;
using System.Text.Json;

namespace BasicAppAPIConsumer
{
    public class Wordlyer
    {
        private readonly ISearchWordAndPersist _swap;
        private readonly IConfiguration _config;
        private readonly string _api;
        private readonly string _apiKey;

        public Wordlyer(ISearchWordAndPersist swap, IConfiguration config, WordlyerOptions opt)
        {
            _config = config;
            _swap = swap;
            _api = "collegiate";  //new SearchRequestModel().Apis.Where(x => x.Text == "Collegiate dictionary").FirstOrDefault().Value; // Don't need this atm 

            _apiKey = opt.DictonaryKey;
                
                
        }

        [FunctionName("EnterWord")]
        public async Task<IActionResult> EnterWord(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            try
            {
                log.LogInformation($"Entry {nameof(EnterWord)}");

                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                //string word = JsonConvert.DeserializeObject<string>(requestBody);

                //This is a MVP choice. I would not accept this through a design sesh - JSW
                string word = requestBody;


                await _swap.Go(new SearchRequestModel
                {
                   SearchTerm = word,
                   Api = _api,
                   ApiKey = _apiKey                   
                });
            }
            catch (Exception e) // At first we'll rely on the bubbling of exceptions would be nice to have custom exceptions -JSW
            {
                return new BadRequestObjectResult(e.Message);// could reuturn a 500 here. Effectively if it's in managed code it isn't a 500
                 
            }

            return new OkResult();
        }

        [FunctionName("GetAllWordsAndDefs")]
        public async Task<IActionResult> GetAllWordsAndDefs(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            try
            {
                log.LogInformation($"Entry {nameof(GetAllWordsAndDefs)}");

                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                return new OkObjectResult(JsonSerializer.Serialize(_swap.GetAll()));
            }
            catch (Exception e) // At first we'll rely on the bubbling of exceptions would be nice to have custom exceptions -JSW
            {
                return new BadRequestObjectResult(e.Message);// could reuturn a 500 here. Effectively if it's in managed code it isn't a 500

            }

            return new OkResult();
        }

    }
}
