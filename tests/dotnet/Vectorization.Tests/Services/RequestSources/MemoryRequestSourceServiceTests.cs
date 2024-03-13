﻿using FakeItEasy;
using FoundationaLLM.Vectorization.Interfaces;
using FoundationaLLM.Vectorization.Models;
using FoundationaLLM.Vectorization.Models.Configuration;
using FoundationaLLM.Vectorization.Services.RequestSources;
using Microsoft.Extensions.Logging;

namespace Vectorization.Tests.Services.RequestSources
{
    public class MemoryRequestSourceServiceTests
    {
        [Fact]
        public async void TestMemoryRequestSourceService()
        {
            RequestSourceServiceSettings requestSourceServiceSettings = A.Fake<RequestSourceServiceSettings>();
            ILogger<MemoryRequestSourceService> logger = A.Fake<ILogger<MemoryRequestSourceService>>();
            VectorizationRequest vectorizationRequest = A.Fake<VectorizationRequest>();            
            requestSourceServiceSettings.Name = "MemorySource";

            MemoryRequestSourceService memoryRequestSourceService = new MemoryRequestSourceService(
                requestSourceServiceSettings,
                logger
            );

            await memoryRequestSourceService.SubmitRequest(vectorizationRequest);

            // Though we requested 5 vectorization requests, only 1 should be returned
            var vectorizationRequests = await memoryRequestSourceService.ReceiveRequests(5);
            Assert.True(vectorizationRequests.Count() == 1);

            Assert.False(await memoryRequestSourceService.HasRequests());

            Assert.Equal("MemorySource", memoryRequestSourceService.SourceName);
        }
    }
}
