export default function NotificationsSection({ active, isAdmin, notifications, notificationMessage, onMessageChange, onBroadcast, status, isSubmitting }) {
  if (!active) return null;
  return <section className="section active">
    <div className="section-header"><h1>System Notifications</h1><p>View and broadcast system messages</p></div>
    {status.message && <div className={`message ${status.type}`}>{status.message}</div>}
    <div className="card"><div className="card-header"><h3>Recent notifications</h3></div><div className="card-body">
      {notifications.length > 0 ? notifications.map((item, index) => <div key={`${item.type}-${index}`} className={`message ${item.type}`} style={{ marginBottom: '0.5rem' }}>{item.message}</div>) : <p style={{ color: 'var(--muted-foreground)' }}>No notifications yet</p>}
    </div></div>
    {isAdmin && <div className="form-card"><h2>Broadcast message</h2><form onSubmit={onBroadcast} className="form-group"><label>Message<textarea value={notificationMessage} onChange={(event) => onMessageChange(event.target.value)} placeholder="Type your message here..." rows="4" /></label><button type="submit" className="btn btn-primary" disabled={isSubmitting}>Send broadcast</button></form></div>}
  </section>;
}
