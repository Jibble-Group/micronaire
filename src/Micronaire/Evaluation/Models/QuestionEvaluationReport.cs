// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Micronaire.LlmEvaluation.Models;

namespace Micronaire.Evaluation.Models;

/// <summary>
/// Evaluation Report for a single question.
/// </summary>
public class QuestionEvaluationReport
{
    /// <summary>
    /// Gets the report from the LLM evaluator.
    /// </summary>
    public required LlmEvaluationReport LlmReport { get; init; }
}