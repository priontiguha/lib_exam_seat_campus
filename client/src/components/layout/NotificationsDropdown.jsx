import { Bell } from 'lucide-react';
import { useState, useRef, useEffect } from 'react';

export default function NotificationsDropdown({ notifications = [], unreadCount = 0, onMarkRead, onMarkAllRead }) {
  const [open, setOpen] = useState(false);
  const ref = useRef(null);

  useEffect(() => {
    function handleClick(e) {
      if (ref.current && !ref.current.contains(e.target)) setOpen(false);
    }
    window.addEventListener('click', handleClick);
    return () => window.removeEventListener('click', handleClick);
  }, []);

  return (
    <div className="notifications-dropdown" ref={ref}>
      <button type="button" className="icon-button" onClick={() => setOpen((v) => !v)} aria-label="Notifications">
        <Bell size={18} />
        {unreadCount > 0 && <span className="badge">{unreadCount}</span>}
      </button>

      {open && (
        <div className="dropdown-panel">
          <div className="dropdown-header">
            <strong>Notifications</strong>
            {notifications.length > 0 && <button type="button" className="link" onClick={onMarkAllRead}>Mark all read</button>}
          </div>
          <div className="dropdown-list">
            {notifications.length === 0 && <div className="empty">No notifications</div>}
            {notifications.map((n) => (
              <div key={n.id} className={`notification-item ${n.type || ''} ${n.read ? 'read' : 'unread'}`}>
                <div className="dot" />
                <div className="content">
                  <div className="message">{n.message}</div>
                  <div className="meta"><small>{n.actorRole ? n.actorRole : ''} {n.createdAt ? `· ${new Date(n.createdAt).toLocaleString()}` : ''}</small></div>
                </div>
                {!n.read && <button type="button" className="mark-read" onClick={() => onMarkRead(n.id)}>Mark</button>}
              </div>
            ))}
          </div>
        </div>
      )}
    </div>
  );
}
