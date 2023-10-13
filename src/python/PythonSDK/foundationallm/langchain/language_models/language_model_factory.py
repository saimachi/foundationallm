from langchain.base_language import BaseLanguageModel
from foundationallm.config import Configuration
from foundationallm.langchain.language_models import (
    AzureChatModel,
    AzureTextCompletionModel,
    OpenAIChatModel,
    OpenAITextCompletionModel,
    LanguageModelProviders,
    LanguageModelTypes
)

from foundationallm.models.orchestration import CompletionRequest

class LanguageModelFactory:
    """
    Factory class for determine which language models to use.
    """
    
    def __init__(self, completion_request: CompletionRequest, config: Configuration):
        self.config = config
        self.language_model = completion_request.language_model
        
    def get_llm(self) -> BaseLanguageModel:
        """
        Retrieves the language model to use for completion requests.
        
        Returns
        -------
        BaseLanguageModel
            Returns the language model to use for completion requests.
        """
        if self.language_model.type == LanguageModelTypes.OPENAI:
            match self.language_model.provider:
                case LanguageModelProviders.MICROSOFT:
                    if self.language_model.use_chat:
                        return AzureChatModel(config = self.config)
                    else:
                        return AzureTextCompletionModel(config = self.config)
                case LanguageModelProviders.OPENAI:
                    if self.language_model.use_chat:
                        return OpenAIChatModel(config = self.config)
                    else:
                        return OpenAITextCompletionModel(config = self.config)
                