// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Micronaire.Claims;
using Micronaire.Claims.Models;
using Micronaire.RetrievalClaimEvaluation.Models;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;

namespace Micronaire.RetrievalClaimEvaluation;

/// <summary>
/// Evaluates retrieval using claims.
/// </summary>
public class RetrievalClaimEvaluator(ILogger<RetrievalClaimEvaluator> logger) : IRetrievalClaimEvaluator
{
    /// <inheritdoc/>
    public async Task<RetrievalClaimReport> EvaluateAsync(
        Kernel evaluator,
        List<Claim> groundTruthClaims,
        List<Claim> contextClaims,
        CancellationToken cancellationToken = default)
    {
        var totalGroundTruthClaims = groundTruthClaims.Count();
        var coveredClaims = 0;
        var relevantChunks = 0;
        foreach (var contextClaim in contextClaims)
        {
            var foundRelevantClaim = false;
            foreach (var groundTruthClaim in groundTruthClaims)
            {
                var score = await ClaimOperations.CalculateClaimSimilarityAsync(
                    evaluator,
                    contextClaim,
                    groundTruthClaim,
                    logger,
                    cancellationToken);
                if (score >= 3)
                {
                    coveredClaims++;
                    if (!foundRelevantClaim)
                    {
                        relevantChunks++;
                        foundRelevantClaim = true;
                    }
                }
            }
        }

        var claimRecall = totalGroundTruthClaims > 0 ? (double)coveredClaims / totalGroundTruthClaims : 0;
        logger.LogInformation("Claim Recall: {claimRecall}", claimRecall);

        var contextPrecision = contextClaims.Count > 0 ? (double)relevantChunks / contextClaims.Count : 0;
        logger.LogInformation("Context Precision: {contextPrecision}", contextPrecision);

        return new RetrievalClaimReport
        {
            ClaimRecall = claimRecall,
            ContextPrecision = contextPrecision,
        };
    }
}
