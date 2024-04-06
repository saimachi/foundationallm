﻿namespace FoundationaLLM.SemanticKernel.Core.Exceptions
{
    /// <summary>
    /// Represents an error generated by the Semantic Kernel service.
    /// </summary>
    public class SemanticKernelException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SemanticKernelException"/> class with a default message.
        /// </summary>
        public SemanticKernelException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SemanticKernelException"/> class with its message set to <paramref name="message"/>.
        /// </summary>
        /// <param name="message">A string that describes the error.</param>
        public SemanticKernelException(string? message) : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SemanticKernelException"/> class with its message set to <paramref name="message"/>.
        /// </summary>
        /// <param name="message">A string that describes the error.</param>
        /// <param name="innerException">The exception that is the cause of the current exception.</param>
        public SemanticKernelException(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }
}
