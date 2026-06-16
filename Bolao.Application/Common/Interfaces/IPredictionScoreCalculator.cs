using Bolao.Application.Predictions.CalculatePredictionPoints;
using Bolao.Domain.Entities;

namespace Bolao.Application.Common.Interfaces;

public interface IPredictionScoreCalculator
{
    PredictionScoreResult Calculate(
        Prediction prediction,
        FootballMatch match,
        BolaoRules rules);
}