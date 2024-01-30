﻿using FoundationaLLM.Common.Models.Vectorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoundationaLLM.Common.Interfaces
{
    /// <summary>
    /// Creates typed service instances.
    /// </summary>
    public interface IVectorizationServiceFactory<T>

    {
        /// <summary>
        /// Retrieves a service instance of type T specified by name.
        /// </summary>
        /// <param name="serviceName">The name of the service instance to create.</param>
        /// <returns>The service instance created by name.</returns>
        T GetService(string serviceName);

        /// <summary>
        /// Retrieves a service instance of type T specified by name and its associated vectorizaiton profile.
        /// </summary>
        /// <param name="serviceName">The name of the service instance to create.</param>
        /// <returns>The service instance and its associated vectorization profile.</returns>
        (T Service, VectorizationProfileBase VectorizationProfile) GetServiceWithProfile(string serviceName);
    }
}