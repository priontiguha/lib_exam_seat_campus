export default function NotificationsSection({
  active,
  isAdmin,
  notifications,
  liveNotifications = [],
  socketStatus,
  notificationMessage,
  onMessageChange,
  onBroadcast,
  status,
  isSubmitting,
}) {
  if (!active) return null;

  const socketBanner = {
    connected:    { cls: 'success', text: '🟢 Realtime connection active' },
    reconnecting: { cls: 'warning', text: '🟡 Reconnecting to realtime server…' },
    disconnected: { cls: 'danger',  text: '🔴 Realtime updates unavailable — changes may be delayed' },
  }[socketStatus];

  return (
    <section className="section active">
      <div className="section-header">
        <h1>System Notifications</h1>
        <p>View and broadcast system messages</p>
      </div>

      {status.message && (
        <div className={`message ${status.type}`}>{status.message}</div>
      )}

      {socketBanner && (
        <div className={`message ${socketBanner.cls}`} style={{ marginBottom: '1rem' }}>
          {socketBanner.text}
        </div>
      )}

      {/* Live realtime notifications */}
      {liveNotifications.length > 0 && (
        <div className="card" style={{ marginBottom: '1rem' }}>
          <div className="card-header">
            <h3>Live updates</h3>
          </div>
          <div className="card-body">
            {liveNotifications.map((item, index) => (
              <div
                key={`live-${index}`}
                className={`message ${item.type || 'live'}`}
                style={{ marginBottom: '0.5rem' }}
              >
                {item.message}
              </div>
            ))}
          </div>
        </div>
      )}

      {/* REST summary notifications */}
      <div className="card">
        <div className="card-header">
          <h3>System summary</h3>
        </div>
        <div className="card-body">
          {notifications.length > 0
            ? notifications.map((item, index) => (
                <div
                  key={`summary-${item.type}-${index}`}
                  className={`message ${item.type}`}
                  style={{ marginBottom: '0.5rem' }}
                >
                  {item.message}
                </div>
              ))
            : <p style={{ color: 'var(--muted-foreground)' }}>No system notifications</p>}
        </div>
      </div>

      {isAdmin && (
        <div className="form-card">
          <h2>Broadcast message</h2>
          <form onSubmit={onBroadcast} className="form-group">
            <label>
              Message
              <textarea
                value={notificationMessage}
                onChange={(event) => onMessageChange(event.target.value)}
                placeholder="Type your message here…"
                rows="4"
              />
            </label>
            <button type="submit" className="btn btn-primary" disabled={isSubmitting}>
              Send broadcast
            </button>
          </form>
        </div>
      )}
    </section>
  );
}
