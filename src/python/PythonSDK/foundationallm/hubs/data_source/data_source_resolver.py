from foundationallm.hubs.data_source import DataSourceMetadata
from foundationallm.hubs import Resolver
from typing import List


class DataSourceResolver(Resolver):
    def resolve(self, request, metadata_values) -> List[DataSourceMetadata]:
        # This should use some logic to resolve the request to a metadata
        # For simplicity, returning the first metadata value
        return [metadata_values[0]]
