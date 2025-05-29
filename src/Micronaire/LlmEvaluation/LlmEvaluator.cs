// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Micronaire.LlmEvaluation.Models;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;

namespace Micronaire.LlmEvaluation;

public class LlmEvaluator(ILogger<LlmEvaluator> logger) : ILlmEvaluator
{
    public async Task<LlmEvaluationReport> EvaluateAsync(
        Kernel kernel,
        string question,
        string context,
        string answer,
        string groundTruth,
        CancellationToken cancellationToken = default)
    {
        var ctx = new KernelArguments
        {
            { "question", question },
            { "context", context },
            { "answer", answer },
            { "ground_truth", groundTruth },
        };

        var groundednessResult = (await kernel
            .InvokeAsync("Plugins", "Groundedness", ctx, cancellationToken))
            .GetValue<string>();
        logger.LogInformation("Got groundedness: {res}", groundednessResult);
        var groundedness = (groundednessResult?.FirstOrDefault(c => "12345".Contains(c), '1') ?? '1') - '0';

        var relevanceResult = (await kernel
            .InvokeAsync("Plugins", "Relevance", ctx, cancellationToken))
            .GetValue<string>();
        logger.LogInformation("Got relevance: {res}", relevanceResult);
        var relevance = (relevanceResult?.FirstOrDefault(c => "12345".Contains(c), '1') ?? '1') - '0';

        var coherenceResult = (await kernel
            .InvokeAsync("Plugins", "Coherence", ctx, cancellationToken))
            .GetValue<string>();
        logger.LogInformation("Got coherence: {res}", coherenceResult);
        var coherence = (coherenceResult?.FirstOrDefault(c => "12345".Contains(c), '1') ?? '1') - '0';

        var fluencyResult = (await kernel.InvokeAsync("Plugins", "Fluency", ctx, cancellationToken)).GetValue<string>();
        logger.LogInformation("Got fluency: {res}", fluencyResult);
        var fluency = (fluencyResult?.FirstOrDefault(c => "12345".Contains(c), '1') ?? '1') - '0';

        var retrievalResult = (await kernel
            .InvokeAsync("Plugins", "RetrievalScore", ctx, cancellationToken))
            .GetValue<string>();
        logger.LogInformation("Got retrieval score: {res}", retrievalResult);
        var retrieval = (retrievalResult?.FirstOrDefault(c => "12345".Contains(c), '1') ?? '1') - '0';

        var similarityResult = (await kernel
            .InvokeAsync("Plugins", "Similarity", ctx, cancellationToken))
            .GetValue<string>();
        logger.LogInformation("Got similarity: {res}", similarityResult);
        var similarity = (similarityResult?.FirstOrDefault(c => "12345".Contains(c), '1') ?? '1') - '0';

        return new LlmEvaluationReport
        {
            Groundedness = (double)(groundedness - 1) / 4,
            Relevance = (double)(relevance - 1) / 4,
            Coherence = (double)(coherence - 1) / 4,
            Fluency = (double)(fluency - 1) / 4,
            RetrievalScore = (double)(retrieval - 1) / 4,
            Similarity = (double)(similarity - 1) / 4,
        };
    }

    public async Task<LlmEvaluationReport> EvaluateQuestionAsync(
        Kernel kernel,
        string question,
        string context,
        string answer,
        CancellationToken cancellationToken = default)
    {
        var ctx = new KernelArguments
        {
            { "question", question },
            { "context", context },
            { "answer", answer },
        };

        var groundednessResult = (await kernel
            .InvokeAsync("Plugins", "Groundedness", ctx, cancellationToken))
            .GetValue<string>();
        logger.LogInformation("Got groundedness: {res}", groundednessResult);
        var groundedness = (groundednessResult?.FirstOrDefault(c => "12345".Contains(c), '1') ?? '1') - '0';

        var relevanceResult = (await kernel
            .InvokeAsync("Plugins", "Relevance", ctx, cancellationToken))
            .GetValue<string>();
        logger.LogInformation("Got relevance: {res}", relevanceResult);
        var relevance = (relevanceResult?.FirstOrDefault(c => "12345".Contains(c), '1') ?? '1') - '0';

        var coherenceResult = (await kernel
            .InvokeAsync("Plugins", "Coherence", ctx, cancellationToken))
            .GetValue<string>();
        logger.LogInformation("Got coherence: {res}", coherenceResult);
        var coherence = (coherenceResult?.FirstOrDefault(c => "12345".Contains(c), '1') ?? '1') - '0';

        var fluencyResult = (await kernel.InvokeAsync("Plugins", "Fluency", ctx, cancellationToken)).GetValue<string>();
        logger.LogInformation("Got fluency: {res}", fluencyResult);
        var fluency = (fluencyResult?.FirstOrDefault(c => "12345".Contains(c), '1') ?? '1') - '0';

        var retrievalResult = (await kernel
            .InvokeAsync("Plugins", "RetrievalScore", ctx, cancellationToken))
            .GetValue<string>();
        logger.LogInformation("Got retrieval score: {res}", retrievalResult);
        var retrieval = (retrievalResult?.FirstOrDefault(c => "12345".Contains(c), '1') ?? '1') - '0';

        return new LlmEvaluationReport
        {
            Groundedness = (double)(groundedness - 1) / 4,
            Relevance = (double)(relevance - 1) / 4,
            Coherence = (double)(coherence - 1) / 4,
            Fluency = (double)(fluency - 1) / 4,
            RetrievalScore = (double)(retrieval - 1) / 4,
        };
    }
}
