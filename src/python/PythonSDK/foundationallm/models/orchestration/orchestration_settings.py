from typing import Optional
from pydantic import BaseModel

class OrchestrationSettings(BaseModel):
    """
        Represents settings for the orchestration.
        
        Parameters
        ----------
        agent_parameters : dict
            Parameters to override the behavior of the agent.
    """
    agent_parameters: Optional[dict] = None
