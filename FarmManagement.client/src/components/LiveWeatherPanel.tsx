import { useEffect, useState, useCallback } from 'react'
import { getLiveWeather, tryGetBrowserLocation, type LiveWeatherResult } from '../services/farmAi'

export default function LiveWeatherPanel() {
  const [data, setData] = useState<LiveWeatherResult | null>(null)
  const [error, setError] = useState<string | null>(null)
  const [loading, setLoading] = useState(true)

  const load = useCallback(async () => {
    setLoading(true)
    setError(null)
    try {
      const loc = await tryGetBrowserLocation()
      const result = loc ? await getLiveWeather(loc.lat, loc.lon) : await getLiveWeather()
      setData(result)
    } catch (err: any) {
      setError(err.message ?? 'Live weather is unavailable right now.')
    } finally {
      setLoading(false)
    }
  }, [])

  useEffect(() => { load() }, [load])

  return (
    <div className="weather-card">
      <div className="weather-card-inner">
        <div className="weather-head">
          <div>
            <h3>Live Weather</h3>
            <span>{data?.locationLabel ?? 'Locating…'}</span>
          </div>
          <div className="weather-live-badge">
            <span className="weather-live-dot" />
            LIVE
          </div>
        </div>

        {loading && (
          <>
            <div className="skel-line" style={{ width: '60%', marginTop: '1rem' }} />
            <div className="skel-line" style={{ width: '85%' }} />
            <div className="skel-line" style={{ width: '40%' }} />
          </>
        )}

        {!loading && error && (
          <div className="weather-error">
            <p>{error}</p>
            <button className="weather-retry" onClick={load}>Retry</button>
          </div>
        )}

        {!loading && !error && data && (
          <>
            <div className="weather-main">
              <div className="weather-icon">{data.icon}</div>
              <div>
                <div className="weather-temp">{Math.round(data.temperatureC)}°C</div>
                <div className="weather-condition">{data.condition} · feels like {Math.round(data.feelsLikeC)}°C</div>
              </div>
            </div>

            <div className="weather-stats">
              <div className="weather-stat"><span>Humidity</span><b>{Math.round(data.humidityPct)}%</b></div>
              <div className="weather-stat"><span>Wind</span><b>{Math.round(data.windSpeedKmh)} km/h</b></div>
              <div className="weather-stat"><span>Rain chance</span><b>{data.precipitationProbabilityPct}%</b></div>
              <div className="weather-stat"><span>UV index</span><b>{data.uvIndex.toFixed(1)}</b></div>
            </div>

            <div className="weather-days">
              {data.daily.slice(0, 5).map(d => (
                <div className="weather-day" key={d.date}>
                  <div>{new Date(d.date).toLocaleDateString(undefined, { weekday: 'short' })}</div>
                  <div className="wd-icon">{d.icon}</div>
                  <div className="wd-max">{Math.round(d.maxTempC)}°</div>
                  <div className="wd-min">{Math.round(d.minTempC)}°</div>
                </div>
              ))}
            </div>
          </>
        )}
      </div>
    </div>
  )
}
