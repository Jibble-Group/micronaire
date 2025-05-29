// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
using Micronaire.Claims.Models;
using Micronaire.OverallClaimEvaluation.Models;
using Microsoft.SemanticKernel;

namespace Micronaire.OverallClaimEvaluation;

/// <summary>
/// Evaluates overall metrics based on claim analysis.
/// </summary>
public interface IOverallClaimEvaluator
{
    /// <summary>
    /// Evaluates overall metrics based on claim analysis of the given answers.
    /// </summary>
    /// <param name="evaluator">The evaluator kernel.</param>
    /// <param name="generatedClaims">The generated claims.</param>
    /// <param name="groundTruthClaims">The ground truth claims.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The metrics report.</returns>
    public Task<OverallClaimReport> EvaluateAsync(
        Kernel evaluator,
        List<Claim> generatedClaims,
        List<Claim> groundTruthClaims,
        CancellationToken cancellationToken = default);
}
