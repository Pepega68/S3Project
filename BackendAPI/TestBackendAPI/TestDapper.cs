using Xunit;
using System.Data.SqlClient;
using Dapper;
using BackendAPI.Model;
using BackendAPI.Model.DTO;
using BackendAPI.Service;
using Microsoft.Extensions.Configuration;

namespace TestBackendAPI;

public class TestDapper
{
    private string _dbConnectionString;

    public TestDapper()
    {
        IConfiguration config = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .AddEnvironmentVariables()
            .Build();

        _dbConnectionString = config.GetConnectionString("ConnectMsSqlString");
    }

    [Fact]
    public void TestLocalConnection()
    {
        //arrange
        using (SqlConnection connection = new SqlConnection(_dbConnectionString))
        {
            //act
            string version = connection.ExecuteScalar<string>("select @@version");

            //assert
            Assert.NotNull(version);
            System.Console.WriteLine(version);
        }
    }

    [Fact]
    public void TestPurchasableDto()
    {
        //arrange
        using (SqlConnection connection = new SqlConnection(_dbConnectionString))
        {
            //act
            //start with test insert
            PurchasableDto purchasable = new PurchasableDto
            {
                Name = "TestItem",
                Price = 69420
            };
            string sqlInsert = "insert into Purchasable(name, price) values (@Name, @Price)";
            int rowsInserted = connection.Execute(sqlInsert, purchasable);
            //assert
            Assert.Equal(1, rowsInserted);
            if (rowsInserted >= 1)
            {
                //testDelete
                string sqlDelete = "delete from Purchasable where name = @Name";
                int rowsDeleted = connection.Execute(sqlDelete, purchasable);
                Assert.True(rowsDeleted > 0);
            }

        }
    }
    [Fact]
    public void TestGameInstanceDto()
    {
        //arrange
        using (SqlConnection connection = new SqlConnection(_dbConnectionString))
        {
            //act
            //start with test insert
            GameInstanceDto gameInstanceDto = new GameInstanceDto
            {
                Balance = 1,
                HostIp = "Jeg er ikke en ip"
            };
            string sqlInsert = "insert into GameInstance(balance, hostIp) values (@Balance, @HostIp)";
            int rowsInserted = connection.Execute(sqlInsert, gameInstanceDto);
            //assert
            Assert.Equal(1, rowsInserted);
            if (rowsInserted >= 1)
            {
                //testDelete
                string sqlDelete = "delete from GameInstance where hostIp = @HostIp";
                int rowsDeleted = connection.Execute(sqlDelete, gameInstanceDto);
                Assert.True(rowsDeleted > 0);
            }
        }
    }

    [Fact]
    public void TestCreateGameInstance()
    {
        //Arrange 
        SQLGameDataService sqlGameDataService = new SQLGameDataService(_dbConnectionString);
        //Act 
        GameInstance gameInstance = sqlGameDataService.CreateGameInstance();
        //Assert
        Assert.NotNull(gameInstance);
        Assert.NotEqual(0, gameInstance.Id);

    }
    [Fact]
    public void TestSaveGameInstance()
    {
        //Arrange 
        SQLGameDataService sqlGameDataService = new SQLGameDataService(_dbConnectionString);

        //Act 
        GameInstance gameInstance = sqlGameDataService.CreateGameInstance();
        gameInstance.HostIp = "Jeg er ikke en ip";

        bool result = sqlGameDataService.SaveGameInstance(gameInstance);

        //Assert
        Assert.True(result);
    }

    [Fact]
    public void TestSaveGameInstanceWithPurchases()
    {
        //Arrange 
        SQLGameDataService sqlGameDataService = new SQLGameDataService(_dbConnectionString);
        //Act 
        GameInstance gameInstance = sqlGameDataService.CreateGameInstance();
        gameInstance.Purchases.Add(1, 5);
        gameInstance.Purchases.Add(2, 5);

        bool result = sqlGameDataService.SaveGameInstance(gameInstance);
        //Assert

        Assert.True(result);
    }

    [Fact]
    public void TestLoadData()
    {
        //Arrange
        SQLGameDataService sqlGameDataService = new SQLGameDataService(_dbConnectionString);

        GameInstance gameInstance = sqlGameDataService.CreateGameInstance();
        gameInstance.Purchases.Add(1, 5);
        gameInstance.Purchases.Add(2, 5);

        sqlGameDataService.SaveGameInstance(gameInstance);

        //Act
        GameInstance loadedGameInstance = sqlGameDataService.GetGameInstance(gameInstance.Id);

        //Assert
        Assert.NotNull(loadedGameInstance);
        Assert.Equal(2, loadedGameInstance.Purchases.Count);
        Assert.Equal(5, loadedGameInstance.Purchases[1]);
        Assert.Equal(5, loadedGameInstance.Purchases[2]);
    }

    [Fact]
    public void TestLoadDataWithoutPurchases()
    {
        //Arrange
        SQLGameDataService sqlGameDataService = new SQLGameDataService(_dbConnectionString);

        GameInstance gameInstance = sqlGameDataService.CreateGameInstance();

        sqlGameDataService.SaveGameInstance(gameInstance);

        //Act
        GameInstance loadedGameInstance = sqlGameDataService.GetGameInstance(gameInstance.Id);

        //Assert
        Assert.NotNull(loadedGameInstance);
        Assert.Equal(0, loadedGameInstance.Purchases.Count);
    }

    [Fact]
    public void TestLoadGameInstanceBadId()
    {
        SQLGameDataService sqlGameDataService = new SQLGameDataService(_dbConnectionString);

        GameInstance gameInstance = sqlGameDataService.GetGameInstance(-1);

        //Assert 
        Assert.Null(gameInstance);
    }
}
