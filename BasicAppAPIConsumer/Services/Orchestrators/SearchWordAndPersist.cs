using MerriamWebster.NET;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Services
{
    public interface ISearchWordAndPersist
    {
        Task<bool> Go(SearchRequestModel model);
        Task<Wordout[]> GetAll();
    }

    public class SearchWordAndPersist : ISearchWordAndPersist
    {
        private readonly MerriamWebsterSearch _search;
        private readonly ICheapoRepo _repo;
        private readonly ILogger _logger;

        public SearchWordAndPersist(MerriamWebsterSearch search,ICheapoRepo repo, ILogger logger)
        {
                _repo = repo;
                _search = search;
                _logger = logger;
        }
        public async Task<bool> Go(SearchRequestModel model)
        {
            try
            {
                var result = await _search.Search(model.Api, model.SearchTerm, model.ApiKey);
                model.Result = result;


                await _repo.saveItAsync(model.SearchTerm,model.Result.Summary );

                return true; 
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                throw e;
            }
        }

        public async Task<Wordout[]> GetAll()
        {
            try
            {
               return await _repo.getAllWords();
                 
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                throw e;
            }
        }
    }
}
