﻿using DesktopHostingClient.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Microsoft.AspNetCore.SignalR.Client;
using DesktopHostingClient.Model;

namespace TestDesktopHostingClient;

public class GameHostingWithIp : IDisposable
{
    private HostingManager _hostingManager;
    public GameHostingWithIp()
    {
        _hostingManager = new HostingManager();
        _hostingManager.SetupSignalRHost();
        _hostingManager.StartHosting().Wait();
    }

    [Fact]
    public void TestGetSignalRConnection()
    {
        //Arrange 
        
        //Act
        HubConnectionBuilder builder = new HubConnectionBuilder();
        builder.WithUrl("http://localhost:5100/GameHub");
        HubConnection connection = builder.Build();
        connection.StartAsync().Wait();


        //Assert
        Assert.Equal(HubConnectionState.Connected, connection.State);

        connection.DisposeAsync();
    }

    [Fact]
    public void TestGetGameReference()
    {
        //Arrange
        GameDataManager gameDataManager = GameDataManager.GetInstance();

        //Act
        HubConnectionBuilder builder = new HubConnectionBuilder();
        builder.WithUrl("http://localhost:5100/GameHub");
        HubConnection connection = builder.Build();
        connection.StartAsync().Wait();
        gameDataManager.CreateGameData();

        Task<GameData> gameDataTask = connection.InvokeAsync<GameData>("GetCurrentGameData");
        gameDataTask.Wait();
        GameData gameData = gameDataTask.Result;

        //Assert
        Assert.NotNull(gameData);

        connection.DisposeAsync();
    }

    [Fact]
    public void RecieveRPCfromHost()
    {
        //Arrange

        //Act
        HubConnectionBuilder builder = new HubConnectionBuilder();
        builder.WithUrl("http://localhost:5100/GameHub");
        HubConnection connection = builder.Build();
        
        bool ponged = false;

        connection.On("Pong", () =>
        {
            ponged = true;
        });

        connection.StartAsync().Wait();
        connection.InvokeAsync("Ping").Wait();

        //Assert
        Assert.True(ponged);

        connection.DisposeAsync();
    }

    public void Dispose()
    {
        _hostingManager.DisposeHost();
    }

}
