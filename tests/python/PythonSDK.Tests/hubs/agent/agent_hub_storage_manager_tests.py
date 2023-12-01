import pytest
from unittest.mock import patch
from foundationallm.config import Configuration
from foundationallm.hubs.agent import AgentHubStorageManager
from foundationallm.storage import BlobStorageManager
from azure.core.exceptions import ClientAuthenticationError
from azure.storage.blob import BlobProperties

@pytest.fixture
def test_config():
    return Configuration()

@pytest.fixture
def agent_hub_storage_manager(test_config):
    return AgentHubStorageManager(config=test_config)

class AgentHubStorageManagerTests:
    """
    AgentHubStorageManagerTests validates AgentHubStorageManager's behavior and ensures
        that it is resilient under Blob Storage errors.
        
    This is an integration test class and expects the following environment variable to be set:
        foundationallm-app-configuration-uri.
        
    This test class also expects a valid Azure credential (DefaultAzureCredential) session.
    """
    def test_list_blobs(self, agent_hub_storage_manager):
        """
        list_blobs() queries the files in the provided Blob Storage virtual path.
        
        It truncates the file path from the returned blob names, and it should return an empty list if an error occurs.
        
        This function assumes that a valid Blob Storage path is provided; it will handle any exceptions raised due to an invalid path.
        """
        with patch.object(BlobStorageManager, "list_blobs") as list_blobs:
            list_blobs.side_effect = [
                [BlobProperties(name="/agents/default.json")],
                ClientAuthenticationError()
            ]
            assert agent_hub_storage_manager.list_blobs("/agents") == ["default.json"]
            list_blobs.assert_called_once_with(path="/agents")
            # Blob Storage Exception
            assert agent_hub_storage_manager.list_blobs("/agents") == []
