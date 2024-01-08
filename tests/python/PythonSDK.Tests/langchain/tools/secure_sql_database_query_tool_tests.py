import pytest
import os
from unittest.mock import MagicMock
from foundationallm.config import Configuration
from foundationallm.models.language_models import (
    LanguageModelType,
    LanguageModel,
    LanguageModelProvider,
)
from foundationallm.langchain.language_models import LanguageModelFactory
from foundationallm.langchain.tools import SecureSQLDatabaseQueryTool
from foundationallm.langchain.data_sources.sql import SQLDatabaseFactory
from foundationallm.langchain.data_sources.sql import SQLDatabaseConfiguration


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
def test_tool_llm(test_config):
    language_model = LanguageModel(
        type=LanguageModelType.OPENAI,
        provider=LanguageModelProvider.MICROSOFT,
        temperature=0,
        use_chat=True,
    )
    llm = LanguageModelFactory(
        language_model=language_model,
        config=test_config,
    ).get_llm()
    return llm.get_completion_model(language_model)


@pytest.fixture
def sql_data_source_config():
    return SQLDatabaseConfiguration(
        dialect="mssql",
        password_secret_setting_key_name="TEST_MSSQL_PW_KEY",
        host=os.environ["TEST_MSSQL_DB_HOST"],
        port=os.getenv("TEST_MSSQL_DB_PORT", 1433),
        database_name=os.environ["TEST_MSSQL_DB_NAME"],
        username=os.environ["TEST_MSSQL_DB_USERNAME"],
        include_tables=["DailyPrecipReport"],
        schema="dbo",
        use_row_level_security=False,
    )


@pytest.fixture
def sql_db(sql_data_source_config, test_config):
    return SQLDatabaseFactory(
        sql_db_config=sql_data_source_config, config=test_config
    ).get_sql_database()


@pytest.fixture
def sql_db_tool(sql_db):
    return SecureSQLDatabaseQueryTool(
        db=sql_db,
        description="Weather Database with RLS",
        username="RLSUser",
        use_row_level_security=True,
    )


class SecureSQLDatabaseQueryToolTests:
    def test_row_level_security(self, sql_db_tool):
        query_result = sql_db_tool._run(
            "SELECT COUNT(*) FROM [dbo].[DailyPrecipReport];"
        )
        # Result should be [(1,)]
        assert query_result[2] == "1"