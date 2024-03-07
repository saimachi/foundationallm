using FluentValidation;
using FoundationaLLM.Common.Models.Agents;

namespace FoundationaLLM.Agent.Validation.Metadata
{
    /// <summary>
    /// Validator for the <see cref="InternalContextAgent"/> model.
    /// </summary>
    public class InternalContextAgentValidator : AbstractValidator<InternalContextAgent>
    {
        /// <summary>
        /// Configures the validation rules for the <see cref="InternalContextAgent"/> model.
        /// </summary>
        public InternalContextAgentValidator() => Include(new AgentBaseValidator());
    }
}
