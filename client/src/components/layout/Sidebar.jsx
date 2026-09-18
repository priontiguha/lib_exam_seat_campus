import {
  Bell,
  BookOpen,
  BriefcaseBusiness,
  ClipboardCheck,
  ChevronLeft,
  ChevronRight,
  LayoutGrid,
  Users,
} from 'lucide-react';

const baseItems = [
  { id: 'overview', label: 'Overview', icon: LayoutGrid },
  {
    id: 'library',
    label: 'Library',
    icon: BookOpen,
    children: [
      { id: 'library-books', label: 'Books', icon: BookOpen },
      { id: 'library-issues', label: 'Issues', icon: ClipboardCheck },
    ],
  },
  {
    id: 'exams',
    label: 'Exams',
    icon: ClipboardCheck,
    children: [
      { id: 'exam-rooms', label: 'Rooms', icon: LayoutGrid },
      { id: 'exam-invigilators', label: 'Invigilators', icon: Users },
      { id: 'exam-schedule', label: 'Schedule', icon: ClipboardCheck },
      { id: 'exam-allocations', label: 'Allocations', icon: BriefcaseBusiness },
    ],
  },
  { id: 'notifications', label: 'Notifications', icon: Bell },
];

export default function Sidebar({ isAdmin, activeSection, collapsed, onToggle, onNavigate, currentRole }) {
  const items = currentRole === 'Student'
    ? [{ id: 'overview', label: 'Student Home', icon: LayoutGrid }]
    : isAdmin
      ? [...baseItems, { id: 'users', label: 'Users', icon: Users }]
      : currentRole === 'Librarian'
        ? baseItems.filter((item) => item.id !== 'exams')
        : baseItems;

  return (
    <aside className={`sidebar ${collapsed ? 'collapsed' : ''}`}>
      <div className="sidebar-top">
        <div className="sidebar-brand">
          <div className="brand-mark">L</div>
          {!collapsed && <div className="brand-copy"><strong>LibSeat</strong><span>Campus</span></div>}
        </div>
        <button type="button" className="collapse-toggle" onClick={onToggle} aria-label={collapsed ? 'Expand sidebar' : 'Collapse sidebar'}>
          {collapsed ? <ChevronRight size={16} /> : <ChevronLeft size={16} />}
        </button>
      </div>

      <nav className="sidebar-nav" aria-label="Sidebar navigation">
        {items.map((item) => item.children ? (
          <div className="nav-group" key={item.id}>
            <button type="button" className={`nav-item nav-group-title ${activeSection.startsWith(`${item.id}-`) ? 'active' : ''}`} onClick={() => onNavigate(item.children[0].id)} title={item.label}>
              <span className="nav-icon"><item.icon size={18} /></span><span>{item.label}</span>
            </button>
            {!collapsed && <div className="nav-submenu">
              {item.children.map((child) => (
                <button key={child.id} type="button" className={`nav-item nav-subitem ${activeSection === child.id ? 'active' : ''}`} onClick={() => onNavigate(child.id)}>
                  <span className="nav-icon"><child.icon size={16} /></span><span>{child.label}</span>
                </button>
              ))}
            </div>}
          </div>
        ) : (
          <button key={item.id} type="button" className={`nav-item ${activeSection === item.id ? 'active' : ''}`} onClick={() => onNavigate(item.id)} title={item.label}>
            <span className="nav-icon"><item.icon size={18} /></span><span>{item.label}</span>
          </button>
        ))}
      </nav>

      {!collapsed && <div className="sidebar-card"><span>Access level</span><strong>{currentRole}</strong></div>}
    </aside>
  );
}
