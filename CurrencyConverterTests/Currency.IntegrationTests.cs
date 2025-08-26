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


namespace Currency.IntegrationTests
{
    public class CurrencyIntegrationTests
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
        public async Task GetLatestRatesAsync_ShouldReturnCachedData_WhenCacheHit()
        {
            _mockClaimService.Setup(s => s.GetCurrencyProvider()).Returns("frankfurter");
            // Arrange
            var cachedData = new RateResponse
            {
                BaseCurrency = "USD",
                Date = new DateTime(2024, 8, 23, 23, 34, 33),
                Rates = new Dictionary<string, decimal> { { "EUR", 0.92m } }
            };

            _mockCache.Setup(c => c.GetAsync<object>(It.IsAny<string>()))
                      .ReturnsAsync(cachedData);

            _mockCache.Setup(c => c.GetAsync<RateResponse>(It.IsAny<string>())).ReturnsAsync(cachedData);

            var controller = CreateControllerWithUser("frankfurter");
            var request = new RatesRequest { BaseCurrency = "USD", StartDate = new DateTime(2024, 8, 23, 23, 34, 33) };

            // Act
            var result = await controller.GetLatestRatesAsync(request);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result);

            var api = Assert.IsType<RateResponse>(ok.Value);

            api.Should().BeEquivalentTo(cachedData);
        }

        [Fact]
        public async Task GetLatestRatesAsync_ShouldReturnBadRequest_WhenNoProviderClaim()
        {
            // Arrange
            var controller = CreateControllerWithUser(providerName: "");
            var request = new RatesRequest { BaseCurrency = "USD" };

            // Act
            var result = await controller.GetLatestRatesAsync(request);

            // Assert
            var badRequest = result as BadRequestObjectResult;
            badRequest.Should().NotBeNull();
            badRequest!.Value.Should().BeEquivalentTo(new
            {
                Error = "ProviderNotAssigned",
                Message = "Currency provider not assigned to user.",
                Timestamp = badRequest.Value.GetType().GetProperty("Timestamp")!.GetValue(badRequest.Value)
            });
        }
    }
}
