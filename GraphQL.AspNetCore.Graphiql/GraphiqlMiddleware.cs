﻿using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace GraphQL.AspNetCore.Graphiql
{
    public class GraphiqlMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly GraphiqlMiddlewareOptions _options;

        public GraphiqlMiddleware(
            RequestDelegate next,
            GraphiqlMiddlewareOptions options)
        {
            _next = next;
            _options = options;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            var result = RenderGraphiqlUi();
            await httpContext.Response.WriteAsync(result);
        }

        private string RenderGraphiqlUi()
        {
            return @"<!DOCTYPE html>
<html lang=""en"" xmlns=""http://www.w3.org/1999/xhtml"">
<head>
    <meta charset=""utf-8"">
    <title>GraphiQL</title>
    <meta name=""robots"" content=""noindex"">
    <style>
        html, body {
            height: 100%;
            margin: 0;
            overflow: hidden;
            width: 100%;
        }
        #graphiql {
            height: 100vh;

        .jwt-token {
                background: linear-gradient(#f7f7f7, #e2e2e2);
                border-bottom: 1px solid #d0d0d0;
                font-family: system, -apple-system, 'San Francisco', '.SFNSDisplay-Regular', 'Segoe UI', Segoe, 'Segoe WP', 'Helvetica Neue', helvetica, 'Lucida Grande', arial, sans-serif;
                padding: 7px 14px 6px;
                font-size: 14px;
              }
      }
    </style>
    <link href=""//unpkg.com/graphiql@0.11.2/graphiql.css"" rel=""stylesheet"">
    <script src=""//unpkg.com/react@15.6.1/dist/react.min.js""></script>
    <script src=""//unpkg.com/react-dom@15.6.1/dist/react-dom.min.js""></script>
    <script src=""//unpkg.com/graphiql@0.11.2/graphiql.min.js""></script>
    <script src=""//cdn.jsdelivr.net/fetch/2.0.1/fetch.min.js""></script>
</head>
<body>
    <div>JWT Token <input id=""jwt-token"" placeholder=""JWT Token goes here""></div>
    <div id=""graphiql"">Loading...</div>
    
    <script>
        // Collect the URL parameters
        var parameters = {};

        document.getElementById('jwt-token').value = localStorage.getItem('graphiql:jwtToken');

        window.location.search.substr(1).split('&').forEach(function (entry) {
            var eq = entry.indexOf('=');
            if (eq >= 0) {
                parameters[decodeURIComponent(entry.slice(0, eq))] =
                    decodeURIComponent(entry.slice(eq + 1));
            }
        });
        // Produce a Location query string from a parameter object.
        function locationQuery(params, location) {
            return (location ? location : '') + '?' + Object.keys(params).map(function (key) {
                return encodeURIComponent(key) + '=' +
                    encodeURIComponent(params[key]);
            }).join('&');
        }
        // Derive a fetch URL from the current URL, sans the GraphQL parameters.
        var graphqlParamNames = {
            query: true,
            variables: true,
            operationName: true
        };
        var otherParams = {};
        for (var k in parameters) {
            if (parameters.hasOwnProperty(k) && graphqlParamNames[k] !== true) {
                otherParams[k] = parameters[k];
            }
        }

        // We don't use safe-serialize for location, because it's not client input.
        var fetchURL = locationQuery(otherParams, '" + _options.GraphQlEndpoint + @"');

        // Defines a GraphQL fetcher using the fetch API.
        function graphQLHttpFetcher(graphQLParams) {
        const jwtToken = document.getElementById('jwt-token').value;
              let headers = {
                'Accept': 'application/json',
                'Content-Type': 'application/json'
              };
              if (jwtToken) {
                localStorage.setItem('graphiql:jwtToken', jwtToken);
                headers = {
                  'Accept': 'application/json',
                  'Content-Type': 'application/json',
                  'Authorization': jwtToken ? `Bearer ${jwtToken}` : null
                };
              }

            return fetch(fetchURL, {
                method: 'post',
                headers,
                body: JSON.stringify(graphQLParams),
                credentials: 'include',
            }).then(function (response) {
                return response.text();
            }).then(function (responseBody) {
                try {
                    return JSON.parse(responseBody);
                } catch (error) {
                    return responseBody;
                }
            });
        }

        var fetcher = graphQLHttpFetcher;

        // When the query and variables string is edited, update the URL bar so
        // that it can be easily shared.
        function onEditQuery(newQuery) {
            parameters.query = newQuery;
            updateURL();
        }
        function onEditVariables(newVariables) {
            parameters.variables = newVariables;
            updateURL();
        }
        function onEditOperationName(newOperationName) {
            parameters.operationName = newOperationName;
            updateURL();
        }
        function updateURL() {
            var cleanParams = Object.keys(parameters).filter(function (v) {
                return parameters[v] !== undefined;
            }).reduce(function (old, v) {
                old[v] = parameters[v];
                return old;
            }, {});

            history.replaceState(null, null, locationQuery(cleanParams) + window.location.hash);
        }
        // Render <GraphiQL /> into the body.
        ReactDOM.render(
            React.createElement(GraphiQL, {
                fetcher: fetcher,
                onEditQuery: onEditQuery,
                onEditVariables: onEditVariables,
                onEditOperationName: onEditOperationName,
                query: null,
                response: null,
                variables: null,
                operationName: null,
                editorTheme: null,
                websocketConnectionParams: null,
            }),
            document.getElementById('graphiql')
        );
    </script>

</body>
</html>";
        }
    }
}
