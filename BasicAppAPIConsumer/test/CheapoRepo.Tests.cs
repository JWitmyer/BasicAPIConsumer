using Azure.Data.Tables;
using Microsoft.Extensions.Logging;
using Moq;
using Services;
using Xunit;

namespace test
{
    //Starting with some functional tests for building. Maybe we'll get some moq tests in here depending on time --JSW 
    

    public class CheapoRepoTests
    {
        public CheapoRepoTests()
        {
              
        }

        [Fact]
        public async Task AddWords()
        {
            var cheapoRepo = new CheapoRepo(new Mock<ILogger>().Object, new Mock<TableServiceClient>().Object); // Will have to be updated

            await cheapoRepo.saveItAsync("Word1", "");
            await cheapoRepo.saveItAsync("Word2", "");
            await cheapoRepo.saveItAsync("Word3", "");


            Wordout[] x = await cheapoRepo.getAllWords();
                       


            Assert.NotEmpty(x);
        }



    }
}