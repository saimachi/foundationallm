"""
Storage manager for the AgentHub.
"""
from typing import List
from foundationallm.config import Configuration
from foundationallm.storage import BlobStorageManager

class AgentHubStorageManager(BlobStorageManager):
    """ The AgentHubStorageManager class is responsible for fetching
        available agent values from Azure Blob Storage."""
    def __init__(self, prefix:str = None, config: Configuration = None):
        connection_string = config.get_value(
            "FoundationaLLM:AgentHub:StorageManager:BlobStorage:ConnectionString")
        container_name = config.get_value("FoundationaLLM:AgentHub:AgentMetadata:StorageContainer")
        if prefix is not None:
            container_name = f"{prefix}/{container_name}"

        super().__init__(blob_connection_string=connection_string,
                            container_name=container_name)

    def read_file_content(self, path) -> str:
        return super().read_file_content(path).decode()

    def list_blobs(self, path):
        blob_list = []
        try:
            blob_list = list(super().list_blobs(path=path))
        except Exception as err:
            return []
        blob_names = [blob["name"].split('/')[-1] for blob in blob_list]
        return blob_names
