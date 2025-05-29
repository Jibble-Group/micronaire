// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Micronaire.LlmEvaluation.Models;
using Microsoft.SemanticKernel;

namespace Micronaire.LlmEvaluation;

public interface ILlmEvaluator
{
    public Task<LlmEvaluationReport> EvaluateAsync(
        Kernel kernel,
        string question,
        string context,
        string answer,
        string groundTruth,
        CancellationToken cancellationToken = default);

    public Task<LlmEvaluationReport> EvaluateQuestionAsync(
        Kernel kernel,
        string question,
        string context,
        string answer,
        CancellationToken cancellationToken = default);
}
