using FakeItEasy;
using FoundationaLLM.Vectorization.Interfaces;
using FoundationaLLM.Vectorization.Models;
using FoundationaLLM.Vectorization.Models.Configuration;

namespace Vectorization.Tests.Services
{
    internal class MockedRequestSourceService : IRequestSourceService
    {
        public string SourceName => throw new NotImplementedException();

        private SynchronizedCollection<VectorizationRequest> _vectorizationRequests = new SynchronizedCollection<VectorizationRequest>();

        public Task DeleteRequest(string messageId, string popReceipt)
        {
            _vectorizationRequests.Remove(_vectorizationRequests.First(request => request.Id == messageId));
            return Task.CompletedTask;
        }

        public Task<bool> HasRequests()
        {
            return Task.FromResult(_vectorizationRequests.Count > 0);
        }

        public Task<IEnumerable<(VectorizationRequest Request, string MessageId, string PopReceipt)>> ReceiveRequests(int count)
        {
            return Task.FromResult(_vectorizationRequests.Take(count).Select(result => (result, "", "")));
        }

        public Task SubmitRequest(VectorizationRequest request)
        {
            _vectorizationRequests.Add(request);
            return Task.CompletedTask;
        }
    }

    public class RequestManagerServiceTests
    {
        public RequestManagerServiceTests()
        {
            RequestManagerServiceSettings settings = new RequestManagerServiceSettings
            {
                RequestSourceName = "mocked-requests",
                MaxHandlerInstances = 5
            };
            Dictionary<string, IRequestSourceService> requestSourceServices = new Dictionary<string, IRequestSourceService>
            {
                { "mocked-requests", new MockedRequestSourceService() }
            };
            IVectorizationStateService vectorizationStateService = A.Fake<IVectorizationStateService>();
        }
    }
}
