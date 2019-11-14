﻿namespace Spectre.Client
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Refit;

    /// <summary>
    /// Extensions that simplify calling the API.
    /// </summary>
    public static class SaltEdgeClientExtensions
    {
        /// <summary>
        /// Get all connections for a customer.
        /// </summary>
        /// <param name="client">API client.</param>
        /// <param name="customerId">Customer ID.</param>
        /// <returns>An enumerable of all the connections.</returns>
        public static async Task<IEnumerable<GetConnectionsResponse>> GetConnections(this ISaltEdgeClient client, string customerId)
        {
            return await Wrap(async () =>
            {
                var result = await client.GetConnectionsCall(customerId).ConfigureAwait(false);
                return result.data;
            }).ConfigureAwait(false);
        }

        /// <summary>
        /// Request a connect session with scopes [account_details, transactions_details] with access from a given start date.
        /// </summary>
        /// <param name="client">API client.</param>
        /// <param name="customerId">Customer ID.</param>
        /// <param name="returnUri">Redirect callback URI.</param>
        /// <param name="accessStart">Start date of requested access.</param>
        /// <returns>CreateConnectSessionResponse.</returns>
        public static async Task<CreateConnectSessionResponse> CreateConnectSession(this ISaltEdgeClient client, string customerId, Uri returnUri, DateTime accessStart)
        {
            return await Wrap(async () =>
            {
                var result = await client.CreateConnectSessionCall(new ParamWrapper<CreateConnectSessionRequest>
                {
                    data = new CreateConnectSessionRequest
                    {
                        attempt = new Attempt
                        {
                            return_to = returnUri,
                        },
                        consent = new Consent
                        {
                            from_date = accessStart,
                            scopes = new[]
                            {
                                "account_details",
                                "transactions_details",
                            },
                        },
                        customer_id = customerId,
                    },
                }).ConfigureAwait(false);

                return result.data;
            }).ConfigureAwait(false);
        }

        /// <summary>
        /// Create a customer just by specifying the desired identifier.
        /// </summary>
        /// <param name="client">API client.</param>
        /// <param name="desiredIdentifier">Identifier.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        public static async Task<CreateCustomerResponse> CreateCustomer(this ISaltEdgeClient client, string desiredIdentifier)
        {
            return await Wrap(async () =>
            {
                var result = await client.CreateCustomerCall(new ParamWrapper<CreateCustomerRequest>
                {
                    data = new CreateCustomerRequest
                    {
                        identifier = desiredIdentifier,
                    },
                }).ConfigureAwait(false);

                return result.data;
            }).ConfigureAwait(false);
        }

        internal static async Task<T> Wrap<T>(Func<Task<T>> func)
        {
            try
            {
                return await func().ConfigureAwait(false);
            }
            catch (ApiException e)
            {
                throw new SaltEdgeException(await e.GetContentAsAsync<ErrorResponse>().ConfigureAwait(false));
            }
        }
    }
}