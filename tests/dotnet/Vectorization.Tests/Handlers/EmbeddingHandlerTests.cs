using FakeItEasy;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.TextEmbedding;
using FoundationaLLM.Vectorization.Handlers;
using FoundationaLLM.Vectorization.Interfaces;
using FoundationaLLM.Vectorization.Models;
using FoundationaLLM.Vectorization.Models.Resources;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Vectorization.Tests.Handlers
{
    internal class EmbeddingMockServiceFactory : IVectorizationServiceFactory<ITextEmbeddingService>
    {
        ITextEmbeddingService IVectorizationServiceFactory<ITextEmbeddingService>.GetService(string serviceName)
        {
            ITextEmbeddingService mockTextEmbeddingService = A.Fake<ITextEmbeddingService>();

            A.CallTo(() => mockTextEmbeddingService.GetEmbeddingsAsync(A<IList<string>>._))
                .Returns((
                    Embeddings: new List<Embedding> {
                        new Embedding(new float[5]),
                        new Embedding(new float[5])
                    },
                    TokenCount: 10
                ));

            return mockTextEmbeddingService;
        }

        (ITextEmbeddingService Service, VectorizationProfileBase VectorizationProfile) IVectorizationServiceFactory<ITextEmbeddingService>.GetServiceWithProfile(string serviceName)
        {
            throw new NotImplementedException();
        }
    }

    public class EmbeddingHandlerTests
    {
        [Fact]
        public async void TestProcessRequest()
        {
            IConfigurationRoot configurationRoot = new ConfigurationBuilder().Build();
            IConfigurationSection stepsConfiguration = configurationRoot.GetSection("");

            // DI container configuration
            IVectorizationStateService stateService = A.Fake<IVectorizationStateService>();
            IServiceCollection serviceCollection = new ServiceCollection();
            serviceCollection.AddSingleton<IVectorizationServiceFactory<ITextEmbeddingService>, EmbeddingMockServiceFactory>();

            ILoggerFactory loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());

            EmbeddingHandler handler = new EmbeddingHandler(
                "Queue-Message-1",
                new Dictionary<string, string> { { "text_embedding_profile_name", "" } },
                stepsConfiguration,
                stateService,
                serviceCollection.BuildServiceProvider(),
                loggerFactory
            );

            ContentIdentifier contentIdentifier = new ContentIdentifier
            {
                MultipartId = new List<string> {
                    "https://somesa.blob.core.windows.net",
                    "vectorization-input",
                    "somedata.pdf"
                },
                ContentSourceProfileName = "SomePDFData",
                CanonicalId = "SomeBusinessUnit/SomePDFData"
            };
            VectorizationRequest request = new VectorizationRequest
            {
                Id = "d4669c9c-e330-450a-a41c-a4d6649abdef",
                ContentIdentifier = contentIdentifier,
                ProcessingType = VectorizationProcessingType.Synchronous,
                Steps = new List<VectorizationStep>
                {
                    new VectorizationStep { Id = "embed", Parameters = new Dictionary<string, string> { } }
                },
                CompletedSteps = new List<string> { },
                RemainingSteps = new List<string> { "embed" }
            };
            VectorizationState state = new VectorizationState
            {
                CurrentRequestId = "d4669c9c-e330-450a-a41c-a4d6649abdef",
                ContentIdentifier = contentIdentifier,
                Artifacts = new List<VectorizationArtifact> {
                    new VectorizationArtifact { Type = VectorizationArtifactType.TextPartition, Content = "This is Text Partition #1" },
                    new VectorizationArtifact { Type = VectorizationArtifactType.TextPartition, Content = "This is Text Partition #2" },
                    new VectorizationArtifact { Type = VectorizationArtifactType.ExtractedText, Content = "This is Extracted Text" }
                }
            };
            CancellationTokenSource tokenSource = new CancellationTokenSource();

            await handler.Invoke(request, state, tokenSource.Token);
            Assert.True(state.Artifacts.Count(artifact => artifact.Type == VectorizationArtifactType.TextEmbeddingVector) == 2);
        }
    }
}