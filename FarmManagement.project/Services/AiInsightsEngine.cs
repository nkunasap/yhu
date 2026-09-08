namespace FarmManagement.API.Services
{
    /// <summary>
    /// FarmSense AI — a lightweight, explainable decision-support engine.
    ///
    /// It is intentionally NOT a black-box LLM call: every insight below is
    /// produced by an inspectable rule against live weather + the farmer's
    /// own data (growing-zone targets, inventory, tasks, crops). That makes
    /// it fast, free to run, works completely offline/without any API key,
    /// and every recommendation can be explained in one sentence — which
    /// matters a lot more to a farmer than a vague AI-sounding paragraph.
    /// </summary>
    public interface IAiInsightsEngine
    {
        List<AiInsight> GenerateInsights(FarmSnapshot snapshot);
        AiInsightsResponse BuildResponse(FarmSnapshot snapshot);
    }

    public class AiInsightsEngine : IAiInsightsEngine
    {
        public AiInsightsResponse BuildResponse(FarmSnapshot snapshot)
        {
            var insights = GenerateInsights(snapshot);

            var critical = insights.Count(i => i.Severity == "critical");
            var warning  = insights.Count(i => i.Severity == "warning");
            var advisory = insights.Count(i => i.Severity == "advisory");

            var score = 100 - (critical * 10) - (warning * 5) - (advisory * 2);
            score = Math.Clamp(score, 5, 100);

            string headline;
            if (critical > 0)
                headline = $"{critical} urgent item{(critical == 1 ? "" : "s")} need attention on {snapshot.FarmName}";
            else if (warning > 0)
                headline = $"{warning} thing{(warning == 1 ? "" : "s")} worth a look on {snapshot.FarmName} today";
            else if (advisory > 0)
                headline = $"{snapshot.FarmName} looks steady — a few small tips below";
            else
                headline = $"{snapshot.FarmName} is running smoothly. No action needed.";

            return new AiInsightsResponse
            {
                FarmId = snapshot.FarmId == 0 ? null : snapshot.FarmId,
                FarmName = snapshot.FarmName,
                GeneratedAt = DateTime.UtcNow,
                Headline = headline,
                OverallScore = score,
                Insights = insights
            };
        }

        public List<AiInsight> GenerateInsights(FarmSnapshot s)
        {
            var insights = new List<AiInsight>();
            var w = s.Weather;

            // ── Weather-driven rules ──────────────────────────────────
            if (w != null)
            {
                if (w.TodayMinC <= 2)
                {
                    insights.Add(new AiInsight
                    {
                        Category = "weather", Severity = "critical", Icon = "🥶",
                        Title = "Frost risk tonight",
                        Message = $"Overnight low is forecast at {w.TodayMinC:0.#}°C near {w.LocationLabel}. Cover sensitive seedlings or bring potted crops under shelter.",
                        Confidence = 91
                    });
                }
                else if (w.TodayMinC <= 5)
                {
                    insights.Add(new AiInsight
                    {
                        Category = "weather", Severity = "warning", Icon = "❄️",
                        Title = "Cold night ahead",
                        Message = $"Temperatures are set to dip to {w.TodayMinC:0.#}°C tonight. Keep an eye on frost-sensitive crops.",
                        Confidence = 84
                    });
                }

                if (w.TodayMaxC >= 38)
                {
                    insights.Add(new AiInsight
                    {
                        Category = "weather", Severity = "critical", Icon = "🔥",
                        Title = "Extreme heat warning",
                        Message = $"A high of {w.TodayMaxC:0.#}°C is expected. Increase irrigation frequency and shade heat-sensitive zones to prevent stress.",
                        Confidence = 90
                    });
                }
                else if (w.TodayMaxC >= 32)
                {
                    insights.Add(new AiInsight
                    {
                        Category = "weather", Severity = "warning", Icon = "🌡️",
                        Title = "Hot day ahead",
                        Message = $"Expect a high of {w.TodayMaxC:0.#}°C. Water early morning or late afternoon to reduce evaporation loss.",
                        Confidence = 80
                    });
                }

                if (w.WindGustKmh >= 55 || w.WindSpeedKmh >= 40)
                {
                    insights.Add(new AiInsight
                    {
                        Category = "weather", Severity = "warning", Icon = "💨",
                        Title = "Strong winds expected",
                        Message = $"Gusts up to {Math.Max(w.WindGustKmh, w.WindSpeedKmh):0} km/h are forecast. Secure greenhouse vents, covers and loose structures.",
                        Confidence = 78
                    });
                }

                if (w.PrecipitationProbabilityPct >= 65)
                {
                    insights.Add(new AiInsight
                    {
                        Category = "irrigation", Severity = "advisory", Icon = "🌧️",
                        Title = "Rain likely — hold off irrigation",
                        Message = $"There's a {w.PrecipitationProbabilityPct}% chance of rain today. Consider skipping scheduled irrigation to avoid waterlogging.",
                        Confidence = 82
                    });
                }
                else
                {
                    var dryDays = s.Weather!.Daily.Take(3).Count(d => d.PrecipitationSumMm < 1);
                    if (dryDays >= 3 && w.TemperatureC >= 22)
                    {
                        insights.Add(new AiInsight
                        {
                            Category = "irrigation", Severity = "advisory", Icon = "💧",
                            Title = "Dry spell forming",
                            Message = "No meaningful rain is forecast for the next 3 days and temperatures are warm. Plan an irrigation pass in the next day or two.",
                            Confidence = 74
                        });
                    }
                }

                if (w.HumidityPct >= 85 && w.TemperatureC is >= 17 and <= 28)
                {
                    insights.Add(new AiInsight
                    {
                        Category = "pest", Severity = "warning", Icon = "🦠",
                        Title = "High fungal-disease risk",
                        Message = $"Humidity is at {w.HumidityPct:0}% with mild temperatures — ideal conditions for blight and mildew. Inspect leaves and improve airflow where possible.",
                        Confidence = 71
                    });
                }

                if (w.UvIndex >= 8)
                {
                    insights.Add(new AiInsight
                    {
                        Category = "weather", Severity = "advisory", Icon = "🕶️",
                        Title = "Very high UV today",
                        Message = $"UV index is {w.UvIndex:0.#}. Schedule outdoor field work for morning or late afternoon and stay hydrated.",
                        Confidence = 76
                    });
                }
            }

            // ── Growing-zone climate rules (compares live weather to each zone's AI targets) ──
            if (w != null)
            {
                foreach (var z in s.Zones)
                {
                    if (z.HasClimateControl) continue; // controlled zones ride out ambient swings

                    if (w.TemperatureC < (double)z.TargetTempMinC - 2)
                    {
                        insights.Add(new AiInsight
                        {
                            Category = "climate", Severity = "warning", Icon = "📉",
                            Title = $"{z.Name} running cold",
                            Message = $"Outdoor temperature ({w.TemperatureC:0.#}°C) is below the {z.TargetTempMinC:0.#}–{z.TargetTempMaxC:0.#}°C target for {(string.IsNullOrWhiteSpace(z.CurrentCrop) ? "this zone" : z.CurrentCrop)}. Consider supplemental heating.",
                            Confidence = 83
                        });
                    }
                    else if (w.TemperatureC > (double)z.TargetTempMaxC + 2)
                    {
                        insights.Add(new AiInsight
                        {
                            Category = "climate", Severity = "warning", Icon = "📈",
                            Title = $"{z.Name} running hot",
                            Message = $"Outdoor temperature ({w.TemperatureC:0.#}°C) exceeds the {z.TargetTempMinC:0.#}–{z.TargetTempMaxC:0.#}°C target for {(string.IsNullOrWhiteSpace(z.CurrentCrop) ? "this zone" : z.CurrentCrop)}. Increase ventilation or shading.",
                            Confidence = 83
                        });
                    }

                    if (z.ExpectedHarvestDate.HasValue)
                    {
                        var daysOut = (z.ExpectedHarvestDate.Value.Date - DateTime.UtcNow.Date).Days;
                        if (daysOut is >= 0 and <= 7)
                        {
                            insights.Add(new AiInsight
                            {
                                Category = "growth", Severity = "advisory", Icon = "🌾",
                                Title = $"{z.Name} nearing harvest",
                                Message = $"Expected harvest in {daysOut} day{(daysOut == 1 ? "" : "s")} ({z.ExpectedHarvestDate:MMM d}). Start planning labour and storage.",
                                Confidence = 88
                            });
                        }
                    }
                }
            }

            // ── Crop growth-stage rules ───────────────────────────────
            foreach (var c in s.Crops)
            {
                var daysSincePlanted = (DateTime.UtcNow.Date - c.PlantedDate.Date).Days;
                var earlyStage = c.GrowthStage.Equals("Seed", StringComparison.OrdinalIgnoreCase)
                                  || c.GrowthStage.Equals("Seedling", StringComparison.OrdinalIgnoreCase);

                if (earlyStage && daysSincePlanted >= 60)
                {
                    insights.Add(new AiInsight
                    {
                        Category = "growth", Severity = "advisory", Icon = "🌱",
                        Title = $"{c.Name} growth may be stalling",
                        Message = $"{c.Name} at {c.FieldLocation} was planted {daysSincePlanted} days ago but is still logged as \"{c.GrowthStage}\". Worth a field check, or update its stage if it has moved on.",
                        Confidence = 62
                    });
                }
            }

            // ── Inventory rules ───────────────────────────────────────
            foreach (var item in s.Inventory)
            {
                if (item.Quantity <= 0)
                {
                    insights.Add(new AiInsight
                    {
                        Category = "inventory", Severity = "critical", Icon = "📦",
                        Title = $"{item.Name} out of stock",
                        Message = $"{item.Name} ({item.Category}) is at 0 {item.Unit}. Reorder soon to avoid disrupting operations.",
                        Confidence = 95
                    });
                }
                else if (item.Quantity <= 5)
                {
                    insights.Add(new AiInsight
                    {
                        Category = "inventory", Severity = "warning", Icon = "📦",
                        Title = $"{item.Name} running low",
                        Message = $"Only {item.Quantity} {item.Unit} of {item.Name} left. Consider restocking this week.",
                        Confidence = 85
                    });
                }
            }

            // ── Task rules ─────────────────────────────────────────────
            var overdue = s.Tasks.Where(t =>
                t.DueDate.Date < DateTime.UtcNow.Date &&
                !t.Status.Equals("Completed", StringComparison.OrdinalIgnoreCase) &&
                !t.Status.Equals("Done", StringComparison.OrdinalIgnoreCase)).ToList();

            if (overdue.Count > 0)
            {
                insights.Add(new AiInsight
                {
                    Category = "tasks", Severity = overdue.Count >= 3 ? "warning" : "advisory", Icon = "✅",
                    Title = $"{overdue.Count} overdue task{(overdue.Count == 1 ? "" : "s")}",
                    Message = overdue.Count == 1
                        ? $"\"{overdue[0].Title}\" was due {overdue[0].DueDate:MMM d} and is still open."
                        : $"Tasks including \"{overdue[0].Title}\" are past their due date. Review the Tasks section to reprioritise.",
                    Confidence = 96
                });
            }

            // ── All-clear fallback ────────────────────────────────────
            if (insights.Count == 0)
            {
                insights.Add(new AiInsight
                {
                    Category = "general", Severity = "info", Icon = "✅",
                    Title = "Everything looks good",
                    Message = "No weather, climate, inventory or task risks detected right now. Keep up the good work!",
                    Confidence = 70
                });
            }

            // Most urgent first
            var order = new Dictionary<string, int> { ["critical"] = 0, ["warning"] = 1, ["advisory"] = 2, ["info"] = 3 };
            return insights.OrderBy(i => order.GetValueOrDefault(i.Severity, 9)).ToList();
        }
    }
}
