import { useState } from 'react'
import { useAuth } from '../context/AuthContext'
import { useNavigate } from 'react-router-dom'
import AiInsightsPanel from '../components/AiInsightsPanel'
import LiveWeatherPanel from '../components/LiveWeatherPanel'

type Section = 'overview' | 'animals' | 'crops' | 'inventory' | 'tasks'

const NAV_ITEMS: { id: Section; label: string; icon: string }[] = [
  { id: 'overview',   label: 'Overview',   icon: '🏠' },
  { id: 'animals',    label: 'Animals',    icon: '🐄' },
  { id: 'crops',      label: 'Crops',      icon: '🌽' },
  { id: 'inventory',  label: 'Inventory',  icon: '📦' },
  { id: 'tasks',      label: 'Tasks',      icon: '✅' },
]

export default function Dashboard() {
  const { user, logout } = useAuth()
  const navigate = useNavigate()
  const [active, setActive] = useState<Section>('overview')

  function handleLogout() {
    logout()
    navigate('/login')
  }

  const initials = user?.email?.slice(0, 2).toUpperCase() ?? '??'

  return (
    <div className="layout">
      {/* ── Sidebar ── */}
      <aside className="sidebar">
        <div className="sidebar-brand">
          <h2>🌾 Farm Manager</h2>
          <p>Management System</p>
        </div>

        <nav className="sidebar-nav">
          {NAV_ITEMS.map(item => (
            <div
              key={item.id}
              className={`nav-item${active === item.id ? ' active' : ''}`}
              onClick={() => setActive(item.id)}
            >
              <span>{item.icon}</span>
              <span>{item.label}</span>
            </div>
          ))}
        </nav>

        <div className="sidebar-footer">
          <div className="user-badge">
            <div className="user-avatar">{initials}</div>
            <div className="user-info">
              <p>{user?.email}</p>
              <span>{user?.role}</span>
            </div>
          </div>
          <button className="btn-logout" onClick={handleLogout}>Sign out</button>
        </div>
      </aside>

      {/* ── Main ── */}
      <main className="main-content">
        {active === 'overview'  && <Overview />}
        {active === 'animals'   && <PlaceholderSection title="Animals"   icon="🐄" description="Track and manage your livestock." />}
        {active === 'crops'     && <PlaceholderSection title="Crops"     icon="🌽" description="Monitor crop growth and field locations." />}
        {active === 'inventory' && <PlaceholderSection title="Inventory" icon="📦" description="Manage farm supplies and stock levels." />}
        {active === 'tasks'     && <PlaceholderSection title="Tasks"     icon="✅" description="View and complete farm tasks." />}
      </main>
    </div>
  )
}

/* ── Overview section ───────────────────────────────── */
function Overview() {
  const { user } = useAuth()

  const stats = [
    { icon: '🐄', label: 'Animals',        value: '—' },
    { icon: '🌽', label: 'Crops',          value: '—' },
    { icon: '📦', label: 'Inventory items', value: '—' },
    { icon: '✅', label: 'Open tasks',      value: '—' },
  ]

  const activities = [
    'Welcome to Farm Management!',
    'Register your animals in the Animals section.',
    'Add your crops in the Crops section.',
    'Track supplies in the Inventory section.',
    'Manage your to-dos in the Tasks section.',
  ]

  return (
    <>
      <div className="page-header">
        <h1>Good day 👋</h1>
        <p>Welcome back, {user?.email}. Here's an overview of your farm.</p>
      </div>

      <div className="overview-grid">
        {/* ── Left rail: AI engine + live weather ── */}
        <div className="overview-rail">
          <AiInsightsPanel />
          <LiveWeatherPanel />
        </div>

        {/* ── Main column: existing stats + activity ── */}
        <div className="overview-main">
          <div className="stats-grid">
            {stats.map(s => (
              <div className="stat-card" key={s.label}>
                <div className="stat-icon">{s.icon}</div>
                <div className="stat-info">
                  <p>{s.label}</p>
                  <h3>{s.value}</h3>
                </div>
              </div>
            ))}
          </div>

          <div className="card">
            <h2>Getting started</h2>
            <ul className="activity-list">
              {activities.map((a, i) => (
                <li className="activity-item" key={i}>
                  <span className="activity-dot" />
                  <span>{a}</span>
                </li>
              ))}
            </ul>
          </div>
        </div>
      </div>
    </>
  )
}

/* ── Generic placeholder ────────────────────────────── */
function PlaceholderSection({ title, icon, description }: { title: string; icon: string; description: string }) {
  return (
    <>
      <div className="page-header">
        <h1>{icon} {title}</h1>
        <p>{description}</p>
      </div>
      <div className="card">
        <h2>{title}</h2>
        <p style={{ color: 'var(--gray-600)', fontSize: '.9rem' }}>
          This section is ready to be connected to the {title.toLowerCase()} API endpoints once they are added to the backend.
        </p>
      </div>
    </>
  )
}
