using FoundationaLLM.Vectorization.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace FoundationaLLM.Vectorization.Services.VectorizationStates
{
    public abstract class VectorizationStateServiceBase
    {
        protected abstract string GetPersistenceIdentifier(VectorizationContentIdentifier contentIdentifier);

        protected static string HashContentIdentifier(VectorizationContentIdentifier contentIdentifier)
        {
            var byteHash = MD5.HashData(
                Encoding.UTF8.GetBytes(
                    contentIdentifier.CanonicalId + "|" + contentIdentifier.UniqueId));

            return BitConverter.ToString(byteHash).Replace("-", string.Empty);
        }
    }
}
