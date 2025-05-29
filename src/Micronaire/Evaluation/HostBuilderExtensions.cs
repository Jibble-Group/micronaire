// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
using Micronaire.Claims;
using Micronaire.LlmEvaluation;
using Micronaire.OverallClaimEvaluation;
using Micronaire.RetrievalClaimEvaluation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Micronaire.Evaluation;

public static class HostBuilderExtensions
{
    public static IServiceCollection AddMicronaire(this IServiceCollection services)
    {
        return services
            .AddSingleton<IEvaluator, Evaluator>()
            .AddSingleton<ILlmEvaluator, LlmEvaluator>()
            .AddSingleton<IClaimExtractor, ClaimExtractor>()
            .AddSingleton<IOverallClaimEvaluator, OverallClaimEvaluator>()
            .AddSingleton<IRetrievalClaimEvaluator, RetrievalClaimEvaluator>();
    }

    public static IHostApplicationBuilder AddMicronaire(this IHostApplicationBuilder builder)
    {
        builder.Services.AddMicronaire();
        return builder;
    }
}