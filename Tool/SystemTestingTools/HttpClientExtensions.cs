﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;

namespace SystemTestingTools
{
    /// <summary>
    /// extensions to HttpClient to allow mocking and assertions
    /// </summary>
    public static class HttpClientExtensions
    {
        /// <summary>
        /// Create a new session, so logs and requests and responses can be tracked
        /// </summary>
        /// <param name="httpClient"></param>
        /// <returns>the SessionId, can be used to interact with other tools that might need a session</returns>
        public static string CreateSession(this HttpClient httpClient)
        {
            var sessionId = Guid.NewGuid().ToString();
            httpClient.DefaultRequestHeaders.Add(Constants.headerName, sessionId);
            MockInstrumentation.MockedEndpoints.Add(sessionId, new List<MockEndpoint>());
            MockInstrumentation.SessionLogs.Add(sessionId, new List<LoggedEvent>());
            MockInstrumentation.OutgoingRequests.Add(sessionId, new List<HttpRequestMessageWrapper>());
            return sessionId;
        }

        private static string GetSessionFromHeader(HttpClient httpClient)
        {
            var values = httpClient.DefaultRequestHeaders.GetValues(Constants.headerName);

            if (values.Count() != 1) throw new ApplicationException("You need to call 'CreateSession' first");

            return values.First();
        }

        /// <summary>
        /// Will return the response when a matching call gets fired, but only once
        /// if you expect this endpoint to be called X times, add X mock endpoints
        /// </summary>
        /// <param name="httpClient"></param>
        /// <param name="httpMethod"></param>
        /// <param name="Url"></param>
        /// <param name="response">You can create your response, or use ResponseFactory to create one for you</param>
        /// <param name="headerMatches">Optional headers that must match for the response to be returned</param>
        public static void AppendMockHttpCall(this HttpClient httpClient, HttpMethod httpMethod, System.Uri Url, HttpResponseMessage response, Dictionary<string, string> headerMatches = null)
        {
            var sessionId = GetSessionFromHeader(httpClient);

            MockInstrumentation.MockedEndpoints[sessionId].Add(new MockEndpoint(httpMethod, Url, response, headerMatches));
        }

        /// <summary>
        /// Will throw an exception when a matching call gets fired, but only once
        /// if you expect this endpoint to be called X times, add X mock endpoints
        /// </summary>
        /// <param name="httpClient"></param>
        /// <param name="httpMethod"></param>
        /// <param name="Url"></param>
        /// <param name="exception">The exception that will be throw when HttpClient.SendAsync gets called</param>
        /// <param name="headerMatches">Optional headers that must match for the response to be returned</param>
        public static void AppendMockHttpCall(this HttpClient httpClient, HttpMethod httpMethod, System.Uri Url, Exception exception, Dictionary<string, string> headerMatches = null)
        {
            var sessionId = GetSessionFromHeader(httpClient);

            MockInstrumentation.MockedEndpoints[sessionId].Add(new MockEndpoint(httpMethod, Url, exception, headerMatches));
        }

        /// <summary>
        /// Get all logs related to the current session
        /// </summary>
        /// <param name="httpClient"></param>
        /// <returns></returns>
        public static List<LoggedEvent> GetSessionLogs(this HttpClient httpClient)
        {
            var sessionId = GetSessionFromHeader(httpClient);

            return MockInstrumentation.SessionLogs[sessionId];
        }

        /// <summary>
        /// Get all outgoing Http requests related to the current session
        /// </summary>
        /// <param name="httpClient"></param>
        /// <returns></returns>
        public static List<HttpRequestMessageWrapper> GetSessionOutgoingRequests(this HttpClient httpClient)
        {
            var sessionId = GetSessionFromHeader(httpClient);

            return MockInstrumentation.OutgoingRequests[sessionId];
        }
    }
}
