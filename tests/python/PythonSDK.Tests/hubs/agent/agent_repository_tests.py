import pytest
from unittest.mock import patch
from foundationallm.config import Configuration
from foundationallm.hubs.agent import (
    AgentRepository,
    AgentHubStorageManager,
    AgentMetadata,
    AgentType,
)
from azure.core.exceptions import ClientAuthenticationError

@pytest.fixture
def test_config():
    return Configuration()

@pytest.fixture
def data_source_repository(test_config):
    return AgentRepository(config=test_config)

class AgentRepositoryTests:
    """
    AgentRepositoryTests verifies that AgentRepository appropriately handles fetching agent metadata
        from storage providers, including gracefully recovering from errors.
        
    This is an integration test class and expects the following environment variable to be set:
        foundationallm-app-configuration-uri.
        
    This test class also expects a valid Azure credential (DefaultAzureCredential) session.
    """
    def test_get_metadata_values(self, data_source_repository):
        """
        get_metadata_values() queries all files matching the provided pattern and deserializes them.
        
        It must be robust to handle invalid file contents and file backend exceptions (e.g., Azure Blob Storage), returning an empty list.
        """
        with (
            patch.object(AgentHubStorageManager, "list_blobs") as list_blobs,
            patch.object(
                AgentHubStorageManager, "read_file_content"
            ) as read_file_content,
        ):
            list_blobs.return_value = ["Blob1", "Blob2", "Blob3"]
            # Valid JSON, Invalid JSON (Pydantic Exception), and Blob Storage Exception
            read_file_content.side_effect = [
                '{"name": "AnomalyAgent", "description": "Responds to anomalies from the SQL DB", "type": "sql"}',
                "{}",
                ClientAuthenticationError()
            ] * 2
            
            # `None` is an invalid input the Blob SDK - use empty string
            data_source_repository.get_metadata_values()
            list_blobs.assert_called_with(path="")

            metadata = data_source_repository.get_metadata_values("Blob*")

            # Only one valid JSON file should be returned
            assert len(metadata) == 1
            list_blobs.assert_called_with(path="Blob*")
            
    def test_get_metadata_by_name(self, data_source_repository):
        """
        get_metadata_by_name() fetches a specific metadata file from the storage backend and deserializes it.
        
        It should return `None` if the file could not be located, or any other errors occurred.
        """
        with (
            patch.object(AgentHubStorageManager, "file_exists") as file_exists,
            patch.object(
                AgentHubStorageManager, "read_file_content"
            ) as read_file_content,
        ):
            file_exists.side_effect = [True, True, False, True]
            
            # Invalid JSON, Blob Exception, and Valid JSON
            # Only three calls should be made to read_file_content()
            read_file_content.side_effect = [
                "{}",
                ClientAuthenticationError(),
                '{"name": "Default", "description": "Default Q/A agent", "type": "conversational"}'
            ]
            
            # Invalid JSON
            assert data_source_repository.get_metadata_by_name("Weather") is None
            file_exists.assert_called_with("Weather.json")

            # Blob Exception
            assert data_source_repository.get_metadata_by_name("SQ L") is None
            # No parsing of problematic file names
            file_exists.assert_called_with("SQ L.json")

            # File Not Found
            assert data_source_repository.get_metadata_by_name("DataFrame") is None
            
            # Successful Loading/Deserialization
            assert type(data_source_repository.get_metadata_by_name("Default")) == AgentMetadata
