import { useEffect, useState, useCallback, type CSSProperties } from 'react'
import { getAiInsights, type AiInsightsResponse } from '../services/farmAi'

export default function AiInsightsPanel() {
  const [data, setData] = useState<AiInsightsResponse | null>(null)
  const [error, setError] = useState<string | null>(null)
  const [loading, setLoading] = useState(true)

  const load = useCallback(() => {
    setLoading(true)
    setError(null)
    getAiInsights()
      .then(setData)
      .catch(err => setError(err.message ?? 'AI engine is unavailable right now.'))
      .finally(() => setLoading(false))
  }, [])

  useEffect(() => { load() }, [load])

  return (
    <div className="ai-card">
      <div className="ai-card-inner">
        <div className="ai-card-head">
          <div className="ai-orb">🤖</div>
          <div>
            <h3>FarmSense AI</h3>
            <span>Live insights engine</span>
          </div>
        </div>

        {loading && (
          <div className="ai-loading">
            <span>Analyzing farm &amp; weather data</span>
            <span className="ai-dot-flicker"><span /><span /><span /></span>
          </div>
        )}

        {!loading && error && (
          <div className="ai-error">
            <p>{error}</p>
            <button className="ai-retry" onClick={load}>Retry</button>
          </div>
        )}

        {!loading && !error && data && (
          <>
            <div className="ai-score-row">
              <div
                className="ai-score-ring"
                style={{ '--pct': data.overallScore } as CSSProperties}
              >
                <span>{data.overallScore}</span>
              </div>
              <p className="ai-headline">{data.headline}</p>
            </div>

            <div className="ai-insight-list">
              {data.insights.slice(0, 5).map((insight, i) => (
                <div
                  key={insight.id}
                  className={`ai-insight sev-${insight.severity}`}
                  style={{ animationDelay: `${i * 90}ms` }}
                >
                  <span className="ai-insight-icon">{insight.icon}</span>
                  <div>
                    <div className="ai-insight-title">{insight.title}</div>
                    <div className="ai-insight-msg">{insight.message}</div>
                  </div>
                </div>
              ))}
            </div>
          </>
        )}
      </div>
    </div>
  )
}
