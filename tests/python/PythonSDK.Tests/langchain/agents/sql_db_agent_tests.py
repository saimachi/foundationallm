import pytest
import os
from unittest.mock import MagicMock
from foundationallm.config import Configuration
from foundationallm.models.orchestration import CompletionRequest
from foundationallm.models.metadata import Agent, DataSource
from foundationallm.models.language_models import (
    LanguageModelType,
    LanguageModelProvider,
    LanguageModel,
)
from foundationallm.langchain.language_models import LanguageModelFactory
from foundationallm.langchain.agents import SqlDbAgent
from foundationallm.langchain.data_sources.sql import SQLDatabaseConfiguration
from foundationallm.config import Context
from azure.storage.blob import BlobServiceClient
from azure.identity import DefaultAzureCredential


@pytest.fixture
def test_config():
    mock_config = MagicMock()
    configuration = Configuration()
    mock_config.get_value.side_effect = (
        lambda key: "Password.1!!"
        if key == "TEST_MSSQL_PW_KEY"
        else configuration.get_value(key)
    )
    return mock_config


@pytest.fixture
def sql_data_source_config():
    return SQLDatabaseConfiguration(
        dialect="mssql",
        password_secret_setting_key_name="TEST_MSSQL_PW_KEY",
        host=os.environ["TEST_MSSQL_DB_HOST"],
        port=os.getenv("TEST_MSSQL_DB_PORT", 1433),
        database_name=os.environ["TEST_MSSQL_DB_NAME"],
        username=os.environ["TEST_MSSQL_DB_USERNAME"],
        include_tables=["StationKeyLookup", "DailyPrecipReport"],
        schema="dbo",
        use_row_level_security=False,
    )


@pytest.fixture
def blob_client():
    return BlobServiceClient(
        account_url=f'https://{os.environ["INTEGRATION_TESTS_SA"]}.blob.core.windows.net',
        credential=DefaultAzureCredential(),
    )


@pytest.fixture
def prompt_prefix(blob_client):
    blob_client = blob_client.get_blob_client(
        container=os.environ["INTEGRATION_TESTS_CONTAINER"], blob="weather.txt"
    )
    return blob_client.download_blob().readall().decode("utf-8")


@pytest.fixture
def prompt_suffix(blob_client):
    blob_client = blob_client.get_blob_client(
        container=os.environ["INTEGRATION_TESTS_CONTAINER"], blob="weather_suffix.txt"
    )
    return blob_client.download_blob().readall().decode("utf-8")


@pytest.fixture
def test_sql_completion_request(sql_data_source_config, prompt_prefix, prompt_suffix):
    req = CompletionRequest(
        user_prompt="How many daily precipitation reports were prepared by station number CO-LR-118 in June 1998?",
        agent=Agent(
            name="weather-sql",
            type="sql",
            description="Answers questions based on the provided SQL Database",
            prompt_prefix=prompt_prefix,
            prompt_suffix=prompt_suffix,
        ),
        language_model=LanguageModel(
            type=LanguageModelType.OPENAI,
            provider=LanguageModelProvider.MICROSOFT,
            temperature=0,
            use_chat=True,
        ),
        data_source=DataSource(
            name="weather-sql",
            type="sql",
            description="Useful for when you need to answer questions about precipitation and hail patterns.",
            data_description="Weather SQL Database",
            configuration=sql_data_source_config,
        ),
        message_history=[],
    )
    return req


@pytest.fixture
def test_sql_llm(test_sql_completion_request, test_config):
    model_factory = LanguageModelFactory(
        language_model=test_sql_completion_request.language_model, config=test_config
    )
    return model_factory.get_llm()


class SqlDbAgentTests:
    def test_sql_db_qa(self, test_sql_completion_request, test_sql_llm, test_config):
        """
        This test verifies the functionality of SqlDbAgent on a sample Microsoft SQL Server database.

        The test is limited to the StationKeyLookup and DailyPrecipReport tables in the dbo schema.
        """
        agent = SqlDbAgent(
            completion_request=test_sql_completion_request,
            llm=test_sql_llm,
            config=test_config,
            context=Context(),
        )
        completion_response = agent.run(prompt=test_sql_completion_request.user_prompt)
        # Split by spaces - that way, if another answer is returned containing 5 (e.g., 15)
        # It is flagged as incorrect
        assert "5" in completion_response.completion.split(" ")