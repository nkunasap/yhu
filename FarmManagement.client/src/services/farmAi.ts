const API_BASE = '/api'

function authHeaders(): HeadersInit {
  try {
    const raw = localStorage.getItem('farm_auth')
    const token = raw ? (JSON.parse(raw)?.token as string | undefined) : undefined
    return token ? { Authorization: `Bearer ${token}` } : {}
  } catch {
    return {}
  }
}

async function get<T>(path: string): Promise<T> {
  const res = await fetch(`${API_BASE}${path}`, { headers: { ...authHeaders() } })
  if (!res.ok) {
    const body = await res.json().catch(() => ({ message: 'Request failed' }))
    throw new Error(body.message ?? `Request failed (${res.status})`)
  }
  return res.json() as Promise<T>
}

/* ── AI insights engine ─────────────────────────────────── */
export type InsightSeverity = 'info' | 'advisory' | 'warning' | 'critical'

export interface AiInsight {
  id: string
  category: string
  severity: InsightSeverity
  icon: string
  title: string
  message: string
  confidence: number
}

export interface AiInsightsResponse {
  engineName: string
  engineVersion: string
  generatedAt: string
  farmId: number | null
  farmName: string
  headline: string
  overallScore: number
  insights: AiInsight[]
}

export function getAiInsights(farmId?: number): Promise<AiInsightsResponse> {
  const qs = farmId ? `?farmId=${farmId}` : ''
  return get<AiInsightsResponse>(`/aiinsights${qs}`)
}

/* ── Live weather ────────────────────────────────────────── */
export interface DailyForecast {
  date: string
  maxTempC: number
  minTempC: number
  precipitationProbabilityPct: number
  precipitationSumMm: number
  weatherCode: number
  condition: string
  icon: string
}

export interface LiveWeatherResult {
  latitude: number
  longitude: number
  locationLabel: string
  temperatureC: number
  feelsLikeC: number
  humidityPct: number
  windSpeedKmh: number
  windGustKmh: number
  precipitationMm: number
  precipitationProbabilityPct: number
  uvIndex: number
  isDay: boolean
  weatherCode: number
  condition: string
  icon: string
  todayMaxC: number
  todayMinC: number
  daily: DailyForecast[]
  fetchedAtUtc: string
  source: string
}

export function getLiveWeather(lat?: number, lon?: number): Promise<LiveWeatherResult> {
  const qs = lat != null && lon != null ? `?lat=${lat}&lon=${lon}` : ''
  return get<LiveWeatherResult>(`/liveweather${qs}`)
}

/** Best-effort browser geolocation; resolves to null instead of throwing if unavailable/denied. */
export function tryGetBrowserLocation(): Promise<{ lat: number; lon: number } | null> {
  return new Promise(resolve => {
    if (!('geolocation' in navigator)) {
      resolve(null)
      return
    }
    navigator.geolocation.getCurrentPosition(
      pos => resolve({ lat: pos.coords.latitude, lon: pos.coords.longitude }),
      () => resolve(null),
      { timeout: 4000, maximumAge: 10 * 60 * 1000 }
    )
  })
}
