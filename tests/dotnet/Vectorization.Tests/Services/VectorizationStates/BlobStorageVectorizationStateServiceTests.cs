using FakeItEasy;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.TextEmbedding;
using FoundationaLLM.Vectorization.Models;
using FoundationaLLM.Vectorization.Services.VectorizationStates;
using Microsoft.Extensions.Logging;

namespace Vectorization.Tests.Services.VectorizationStates
{
    public class BlobStorageVectorizationStateServiceTests
    {
        private readonly BlobStorageVectorizationStateService _blobStorageVectorizationStateService;
        private readonly IStorageService _storageService;

        public BlobStorageVectorizationStateServiceTests()
        {
            _storageService = A.Fake<IStorageService>();
            _blobStorageVectorizationStateService = new BlobStorageVectorizationStateService(
                _storageService,
                LoggerFactory.Create(configure => configure.AddConsole())
            );
        }

        [Fact]
        public async void TestSaveState()
        {
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
            // Only Artifacts #1 and #3 should be saved
            VectorizationState state = new VectorizationState
            {
                CurrentRequestId = "d4669c9c-e330-450a-a41c-a4d6649abdef",
                ContentIdentifier = contentIdentifier,
                Artifacts = new List<VectorizationArtifact> {
                    new VectorizationArtifact { Type = VectorizationArtifactType.TextPartition, Content = "This is Text Partition #1", IsDirty = true, Position = 1 },
                    new VectorizationArtifact { Type = VectorizationArtifactType.TextPartition, Content = "This is Text Partition #2", Position = 2 },
                    new VectorizationArtifact { Type = VectorizationArtifactType.ExtractedText, Content = "This is Extracted Text", IsDirty = true, Position = 3 }
                }
            };

            await _blobStorageVectorizationStateService.SaveState(state);

            // Three calls total (2 artifacts, 1 state file)
            A.CallTo(() => _storageService.WriteFileAsync(A<string>._, A<string>._, A<string>._, default, default))
                .MustHaveHappenedANumberOfTimesMatching(executionTimes => executionTimes == 3);
            A.CallTo(() => _storageService.WriteFileAsync(
                "vectorization-state",
                "SomeBusinessUnit/SomePDFData_state_ED58FB8030C071F5506903E0773C09DD_textpartition_000001.txt",
                "This is Text Partition #1",
                default,
                default)).MustHaveHappenedOnceExactly();
            A.CallTo(() => _storageService.WriteFileAsync(
                "vectorization-state",
                "SomeBusinessUnit/SomePDFData_state_ED58FB8030C071F5506903E0773C09DD_extractedtext_000003.txt",
                "This is Extracted Text",
                default,
                default)).MustHaveHappenedOnceExactly();
            A.CallTo(() => _storageService.WriteFileAsync(
                "vectorization-state",
                "SomeBusinessUnit/SomePDFData_state_ED58FB8030C071F5506903E0773C09DD.json",
                A<string>._, // Serialized JSON
                default,
                default)).MustHaveHappenedOnceExactly();
            
            // Canonical IDs should not be null after the SaveState() call
            Assert.Equal("SomeBusinessUnit/SomePDFData_state_ED58FB8030C071F5506903E0773C09DD_textpartition_000001.txt", state.Artifacts[0].CanonicalId);
            Assert.Equal("SomeBusinessUnit/SomePDFData_state_ED58FB8030C071F5506903E0773C09DD_extractedtext_000003.txt", state.Artifacts[2].CanonicalId);
        }

        [Fact]
        public async void TestLoadArtifacts()
        {
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
            // Only Artifact #3 should be loaded - #1 has a null CanonicalId
            VectorizationState state = new VectorizationState
            {
                CurrentRequestId = "d4669c9c-e330-450a-a41c-a4d6649abdef",
                ContentIdentifier = contentIdentifier,
                Artifacts = new List<VectorizationArtifact> {
                    new VectorizationArtifact { Type = VectorizationArtifactType.TextPartition },
                    new VectorizationArtifact { Type = VectorizationArtifactType.ExtractedText },
                    new VectorizationArtifact {
                        Type = VectorizationArtifactType.TextPartition,
                        CanonicalId = "SomeBusinessUnit/SomePDFData_state_ED58FB8030C071F5506903E0773C09DD_textpartition_000003.txt"
                    }
                }
            };

            await _blobStorageVectorizationStateService.LoadArtifacts(state, VectorizationArtifactType.TextPartition);

            A.CallTo(() => _storageService.ReadFileAsync(A<string>._, A<string>._, A<CancellationToken>._))
                .MustHaveHappenedOnceExactly();
            A.CallTo(() => _storageService.ReadFileAsync(
                "vectorization-state",
                "SomeBusinessUnit/SomePDFData_state_ED58FB8030C071F5506903E0773C09DD_textpartition_000003.txt",
                A<CancellationToken>._)).MustHaveHappenedOnceExactly();
            // Should be non-null
            Assert.NotNull(state.Artifacts[2].Content);
        }
    }
}
