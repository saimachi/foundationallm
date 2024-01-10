"""
Class name: SearchServiceConfiguration
Description: Azure AI Search index information.
"""
from typing import Optional
from foundationallm.langchain.data_sources import DataSourceConfiguration

class SearchServiceConfiguration(DataSourceConfiguration):
    """
    Connection information indicating the index information
    and the number of results to return in the RAG pattern
    """
    endpoint: str
    key_secret: str
    index_name: str
    embedding_field_name: Optional[str] = "Embedding"
    text_field_name: Optional[str] = "Text"
    top_n: Optional[int] = 3
