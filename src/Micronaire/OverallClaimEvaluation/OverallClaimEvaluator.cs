// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Micronaire.Claims;
using Micronaire.Claims.Models;
using Micronaire.OverallClaimEvaluation.Models;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;

namespace Micronaire.OverallClaimEvaluation;

/// <inheritdoc/>
public class OverallClaimEvaluator(ILogger<OverallClaimEvaluator> logger) : IOverallClaimEvaluator
{
    /// <inheritdoc/>
    public async Task<OverallClaimReport> EvaluateAsync(
        Kernel evaluator,
        List<Claim> generatedClaims,
        List<Claim> groundTruthClaims,
        CancellationToken cancellationToken = default)
    {
        var correctClaims = 0;
        var pairs = generatedClaims.Zip(groundTruthClaims);
        foreach (var (generatedClaim, groundTruthClaim) in pairs)
        {
            var score = await ClaimOperations.CalculateClaimSimilarityAsync(
                evaluator,
                generatedClaim,
                groundTruthClaim,
                logger,
                cancellationToken);
            if (score >= 3)
            {
                correctClaims++;
            }
        }

        // Precision: proportion of correct claims in all response claims
        var precision = (double)correctClaims / generatedClaims.Count;

        // Recall: proportion of correct claims in all ground truth claims
        var recall = (double)correctClaims / groundTruthClaims.Count;

        // F1 score: harmonic mean of precision and recall
        var f1Score = 2 / ((precision == 0 ? 0 : 1 / precision) + (recall == 0 ? 0 : 1 / recall));

        return new OverallClaimReport
        {
            Precision = precision,
            Recall = recall,
            F1Score = f1Score,
        };
    }
}
