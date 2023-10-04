from foundationallm.hubs.agent import AgentMetadata
from foundationallm.hubs import Resolver
from typing import List

class AgentResolver(Resolver):
    def resolve(self, request, metadata_values)->List[AgentMetadata]:       
        # This should use some logic to resolve the request to a metadata
        # For simplicity, returning the first metadata value
        return [metadata_values[0]]