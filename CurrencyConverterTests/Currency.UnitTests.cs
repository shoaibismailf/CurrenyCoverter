using Currency.Application.Interfaces;
using Currency.Application.Interfaces.Redis;
using Currency.Application.Models;
using Currency.Application.Models.Request;
using Currency.Application.Models.Response;
using Currency.Application.Services;
using Currency.WebApi.Controllers;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;


namespace Currency.UnitTests
{
    public class CurrencyUnitTests
    {
        private readonly Mock<IProviderFactoryService> _mockProviderFactory = new();
        private readonly Mock<IRedisCacheService> _mockCache = new();
        private readonly Mock<ILogger<RatesController>> _mockLogger = new();
        private readonly Mock<IClaimService> _mockClaimService = new();

        private RatesController CreateControllerWithUser(string providerName = "frankfurter")
        {
            var controller = new RatesController(
                _mockProviderFactory.Object,
                _mockCache.Object,
                _mockLogger.Object,
                _mockClaimService.Object);

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
                    {
                        new Claim("currency_provider", providerName)
                    }))
                }
            };

            return controller;
        }


        [Fact]
        public async Task GetLatestRatesAsync_ReturnsResponse_WhenDataIsAvailable()
        {
            _mockClaimService.Setup(s => s.GetCurrencyProvider()).Returns("frankfurter");
            // Arrange
            var ratesRequest = new RatesRequest
            {
                StartDate = new DateTime(2025, 08, 26),
                BaseCurrency = "USD",
                Symbols = "EUR,GBP"
            };

            var rateResponse = new RateResponse
            {
                BaseCurrency = "USD",
                Rates = new Dictionary<string, decimal>
                {
                    { "EUR", 0.91m },
                    { "GBP", 0.79m }
                },

                Date = (DateTime)ratesRequest.StartDate
            };

            // Mock HttpClient
            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = JsonContent.Create(rateResponse)
                });

            var httpClient = new HttpClient(handlerMock.Object);

            var loggerMock = new Mock<ILogger<FrankFurterProviderService>>();
            var httpContextAccessorMock = new Mock<IHttpContextAccessor>();
            var claimServiceMock = new Mock<IClaimService>();

            httpContextAccessorMock.Setup(a => a.HttpContext).Returns(new DefaultHttpContext());

            var service = new FrankFurterProviderService(
                httpClient,
                loggerMock.Object,
                claimServiceMock.Object,
                _mockCache.Object
            );

            // Act
            var api = await service.GetLatestRatesAsync(ratesRequest);

            api.Data.Should().NotBeNull();

            var data = Assert.IsType<RateResponse>(api.Data);
            data.BaseCurrency.Should().Be(ratesRequest.BaseCurrency);
            data.Rates.Should().HaveCount(2);
            data.Rates["EUR"].Should().Be(0.91m);
            data.Rates["GBP"].Should().Be(0.79m);
            data.Date.Should().Be(ratesRequest.StartDate);
        }

        [Fact]
        public async Task GetLatestRatesAsync_ThrowsException_WhenResponseIsNull()
        {
            _mockClaimService.Setup(s => s.GetCurrencyProvider()).Returns("frankfurter");
            // Arrange
            var ratesRequest = new RatesRequest
            {

            };

            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = JsonContent.Create(new RateResponse()),
                });

            var httpClient = new HttpClient(handlerMock.Object);

            var loggerMock = new Mock<ILogger<FrankFurterProviderService>>();
            var httpContextAccessorMock = new Mock<IHttpContextAccessor>();
            var claimServiceMock = new Mock<IClaimService>();

            httpContextAccessorMock.Setup(a => a.HttpContext).Returns(new DefaultHttpContext());

            var service = new FrankFurterProviderService(
                httpClient,
                loggerMock.Object,
                claimServiceMock.Object,
                _mockCache.Object
            );

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => service.GetLatestRatesAsync(ratesRequest));
        }

        [Fact]
        public async Task GetHistoricalExchangeRates()
        {
            _mockClaimService.Setup(s => s.GetCurrencyProvider()).Returns("frankfurter");
            // Arrange
            var ratesRequest = new HistoricalRequest
            {
                StartDate = new DateTime(2025, 08, 01),
                EndDate = new DateTime(2025, 08, 05),
                BaseCurrency = "USD",
                Symbols = "EUR,GBP",
                PageNumber = 1,
                PageSize = 2
            };

            var historicalRates = new Dictionary<string, Dictionary<string, decimal>>
            {
                { "2025-08-05", new Dictionary<string, decimal> { { "EUR", 0.91m }, { "GBP", 0.79m } } },
                { "2025-08-04", new Dictionary<string, decimal> { { "EUR", 0.92m }, { "GBP", 0.80m } } },
                { "2025-08-03", new Dictionary<string, decimal> { { "EUR", 0.90m }, { "GBP", 0.78m } } },
            };

            var historicalResponse = new HistoricalRateResponse
            {
                BaseCurrency = "USD",
                HistoricalRates = historicalRates
            };

            // Mock HttpClient
            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = JsonContent.Create(historicalResponse)
                });

            var httpClient = new HttpClient(handlerMock.Object);

            var loggerMock = new Mock<ILogger<FrankFurterProviderService>>();
            var httpContextAccessorMock = new Mock<IHttpContextAccessor>();
            var claimServiceMock = new Mock<IClaimService>();

            var service = new FrankFurterProviderService(httpClient, loggerMock.Object, claimServiceMock.Object, _mockCache.Object);

            // Act
            var api = await service.GetHistoricalExchangeRates(ratesRequest);

            api.Data.Should().NotBeNull();

            Assert.NotNull(api);

            // Use the *actual* type name/namespace for your paged model:
            var result = Assert.IsType<HistoricalRateApiResponse>(api.Data);

            Assert.Equal(ratesRequest.PageNumber, result.Page);
            Assert.Equal(ratesRequest.PageSize, result.PageSize);
            Assert.Equal(3, result.TotalItems);
            Assert.Equal(2, result.TotalPages);

            Assert.Equal(new DateTime(2025, 08, 05), result.EndDate);
        }


    }
}
