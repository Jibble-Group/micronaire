// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
namespace Micronaire.LlmEvaluation.Models;

/// <summary>
/// Evaluation report for the LLM evaluator.
/// </summary>
public class LlmEvaluationReport
{
    /// <summary>
    /// Gets or sets the score of groundedness as evaluated by the LLM.
    /// </summary>
    public double Groundedness { get; set; }

    /// <summary>
    /// Gets or sets the score of relevance as evaluated by the LLM.
    /// </summary>
    public double Relevance { get; set; }

    /// <summary>
    /// Gets or sets the score of coherence as evaluated by the LLM.
    /// </summary>
    public double Coherence { get; set; }

    /// <summary>
    /// Gets or sets the score of fluency as evaluated by the LLM.
    /// </summary>
    public double Fluency { get; set; }

    /// <summary>
    /// Gets or sets the score of retrieval as evaluated by the LLM.
    /// </summary>
    public double RetrievalScore { get; set; }

    /// <summary>
    /// Gets or sets the score of similarity between the ground truth and the generated question as evaluated by the LLM.
    /// </summary>
    public double Similarity { get; set; }
}
